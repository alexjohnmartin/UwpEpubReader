﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace eReader
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            Chapters = new ObservableCollection<Chapter>();
            this.InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // TODO: scan for ebooks

            // TODO: populate library

            // TODO: load settings, suggest resuming last read book

            // TODO: start auto-save-position timer
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            // TODO: open a book
            var openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.Downloads;
            openPicker.FileTypeFilter.Add(".epub");

            var bookFile = await openPicker.PickSingleFileAsync();
            if (bookFile == null) return;

            Textbox.Text = "(loading)";
            try
            {
                Chapters.Clear();
                await OpenBook(bookFile);
                Textbox.Text = "(done)";
            }
            catch (Exception ex)
            {
                Textbox.Text = "Error: " + ex.Message;
            }
        }

        private async Task OpenBook(StorageFile bookFile)
        {
            //https://github.com/Microsoft/Windows-universal-samples/blob/master/Samples/XmlDocument/cs/Scenario2_MarkHotProducts.xaml.cs

            //copy book to temp
            var bookname = Guid.NewGuid().ToString();
            var tempFolder = ApplicationData.Current.TemporaryFolder;
            var tempFile = await bookFile.CopyAsync(tempFolder, bookname + bookFile.FileType, NameCollisionOption.ReplaceExisting);

            // extract book to a subfolder
            var tempSubFolder = await tempFolder.CreateFolderAsync(bookname, CreationCollisionOption.ReplaceExisting);
            await Task.Run(() => {
                System.IO.Compression.ZipFile.ExtractToDirectory(tempFile.Path, tempSubFolder.Path);
            });

            // get meta-data container
            var doc = new XmlDocument();
            var metaFolder = await tempSubFolder.GetFolderAsync("META-INF");
            var metaFileText = "";
            var metaFile = await metaFolder.GetFileAsync("container.xml");
            metaFileText = await FileIO.ReadTextAsync(metaFile);
            doc.LoadXml(metaFileText);
            var contentFilePath = doc.SelectSingleNode("//*[local-name()='rootfile']/@*[local-name()='full-path']").NodeValue.ToString();

            var contentFolder = await GetContentFolder(contentFilePath, tempSubFolder);
            contentFilePath = StripPathFromContentFilePath(contentFilePath);

            // get content file
            var contentFileText = "";
            var contentFile = await contentFolder.GetFileAsync(contentFilePath); // TODO: does this work with sub-folders?
            contentFileText = await FileIO.ReadTextAsync(contentFile);
            doc.LoadXml(contentFileText);
            var tocFilePath = string.Empty;
            var contentPaths = doc.SelectNodes("//*[local-name()='item']/@*[local-name()='href']");
            for (uint index = 0; index < contentPaths.Length; index++)
            {
                tocFilePath = contentPaths.Item(index).NodeValue.ToString();
                if (tocFilePath.EndsWith(".ncx")) break;
            }

            // get table-of-contents file text
            var tocFileText = "";
            var tocFile = await contentFolder.GetFileAsync(tocFilePath);
            tocFileText = await FileIO.ReadTextAsync(tocFile);
            // add all chapters from the tocs list to the XAML bindable collection            
            doc.LoadXml(UpdateXmlHeader(tocFileText));
            var xpath = "//*[local-name()='content']/@*[local-name()='src']";
            var srcAttributes = doc.SelectNodes(xpath);
            for (uint index = 0; index < srcAttributes.Length; index++)
            {
                var chapterFilePath = srcAttributes.Item(index).NodeValue.ToString();
                chapterFilePath = StripParametersOffFilePath(chapterFilePath); // TODO: propogate parameters to the web-view to jump to sections
                var contentFolderShortPath = StripLocalFileStructureFromPath(contentFolder.Path);
                var uri = new Uri($"ms-appdata:///temp/{contentFolderShortPath}/{chapterFilePath.ToString()}");
                var chapterFile = await contentFolder.GetFileAsync(chapterFilePath.ToString());
                Chapters.Add(new Chapter((index+1).ToString(), uri, chapterFile));
            }
        }

        private string StripParametersOffFilePath(string chapterFilePath)
        {
            if (!chapterFilePath.Contains("#")) return chapterFilePath;

            return chapterFilePath.Substring(0, chapterFilePath.IndexOf('#'));
        }

        private string StripLocalFileStructureFromPath(string path)
        {
            //C:\Users\AlexM\AppData\Local\Packages\285cf3d9-4f0d-4a62-b900-07c582c30352_r6qrfn6sm5er0\TempState\84e7ddc7-0761-4d1f-ad8e-d8eda7fc8a18\OEBPS
            var startIndex = path.IndexOf("\\TempState\\");
            return path.Substring(startIndex + 11).Replace('\\', '/');
        }

        private string UpdateXmlHeader(string text)
        {
            var xml = text;
            //xml = xml.Replace("<?xml version=\"1.0\"?>", "<?xml version='1.0' encoding='utf-8'?>");
            var startIndex = xml.IndexOf("<docTitle");
            xml = "<?xml version='1.0' encoding='utf-8'?><ncx>" + xml.Substring(startIndex);
            return xml;
        }
        
        private async Task<IStorageFolder> GetContentFolder(string contentFilePath, StorageFolder tempSubFolder)
        {
            var contentFolder = tempSubFolder;

            if (contentFilePath.Contains("/"))
            {
                var parts = contentFilePath.Split('/');
                for (var i = 0; i < parts.Length - 1; i++)
                {
                    contentFolder = await contentFolder.GetFolderAsync(parts[i]);
                }
            }

            return contentFolder;
        }

        private string StripPathFromContentFilePath(string contentFilePath)
        {
            var stripped = contentFilePath;

            if (contentFilePath.Contains("/"))
            {
                var parts = contentFilePath.Split('/');
                stripped = parts[parts.Length - 1];                
            }

            return stripped;
        }

        public ObservableCollection<Chapter> Chapters { get; private set; }

        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pivot = sender as Pivot;
            var chapter = pivot.SelectedItem as Chapter;
            //PivotItemWebView.Navigate(chapter.Uri);

            if (chapter?.File == null) return;

            var fileText = await FileIO.ReadTextAsync(chapter.File);

            // TODO: update any relative paths in the HTML to make images work
            var regex = new Regex("src=[\"\']{1}[a-zA-Z0-9-_./]+[\"\']{1}");
            var matches = regex.Matches(fileText);
            foreach (var match in matches)
            {
                var oldLink = match.ToString();
                //var chapterPath = chapter.File.Path.Replace(chapter.File.Name, "").Replace('\\', '/');
                var oldLinkConverted = oldLink.Substring(5, oldLink.Length - 6); //.Replace('/', '\\');
                //var newLink = "src=\"file://" + Path.Combine(chapterPath, oldLinkConverted) + "\"";
                var newLink = "src=\"" + chapter.Uri.AbsoluteUri.Replace(chapter.File.Name, oldLinkConverted) + "\"";
                fileText = fileText.Replace(oldLink, newLink);
                //fileText = fileText.Replace(oldLink, oldLinkConverted);
            }

            PivotItemWebView.NavigateToString(fileText);
        }
    }

    public class Chapter
    {
        public Chapter(string name, Uri uri, IStorageFile bookFile)
        {
            this.Name = name;
            this.Uri = uri;
            this.File = bookFile;
        }

        public string Name { get; private set; }
        public Uri Uri { get; private set; }
        public IStorageFile File { get; private set; }
    }
}