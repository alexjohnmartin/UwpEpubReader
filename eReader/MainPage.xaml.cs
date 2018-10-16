using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace eReader
{
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<BookItem> Books { get; private set; }

        private StorageFolder currentFolder;
        private StorageFolder rootFolder;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
            this.Books = new ObservableCollection<BookItem>();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            // TODO: scan for ebooks

            // TODO: populate library

            // TODO: load settings, suggest resuming last read book
        }

        private async void BookButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var bookItem = button.CommandParameter as BookItem;
            if (bookItem.IsFolder)
            {
                Books.Clear();

                // show 'back' button to navigate back up directory tree
                if (currentFolder != rootFolder)
                    Books.Add(new BookItem(currentFolder, "BACK"));

                await AddBooksInFolder(bookItem.Folder);
            }
            else
            {
                var bookDetails = new BookDetails(bookItem.File);
                this.Frame.Navigate(typeof(ReaderPage), bookDetails);
            }
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var openpicker = new FolderPicker();
            openpicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openpicker.FileTypeFilter.Add("*");
            var folder = await openpicker.PickSingleFolderAsync();
            if (folder == null) return;

            Books.Clear();
            rootFolder = folder;
            await AddBooksInFolder(folder);
        }

        private async Task AddBooksInFolder(StorageFolder folder)
        {
            this.currentFolder = folder;
            var files = await folder.GetFilesAsync();
            foreach(var file in files)
            {
                if (file.FileType.ToLower() == ".epub")
                {
                    Books.Add(new BookItem(file));
                }
            }

            var subfolders = await folder.GetFoldersAsync();
            foreach(var subfolder in subfolders)
            {
                //recursively add sub-folders
                //await AddBooksInFolder(subfolder);

                // add sub-folder as a selectable item
                Books.Add(new BookItem(subfolder)); // TODO: only add subfolder if books exist in it
            }
        }
    }
}
