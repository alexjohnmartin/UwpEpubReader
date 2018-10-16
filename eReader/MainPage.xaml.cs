using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace eReader
{
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<Book> Books { get; private set; }

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
            this.Books = new ObservableCollection<Book>();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // TODO: scan for ebooks

            // TODO: populate library

            // TODO: load settings, suggest resuming last read book
        }

        private void BookButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var bookFile = button.CommandParameter as StorageFile;
            var bookDetails = new BookDetails(bookFile);
            this.Frame.Navigate(typeof(ReaderPage), bookDetails);
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var openpicker = new FolderPicker();
            openpicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openpicker.FileTypeFilter.Add("*");
            var folder = await openpicker.PickSingleFolderAsync();
            if (folder == null) return;

            await AddBooksInFolder(folder);
        }

        private async Task AddBooksInFolder(StorageFolder folder)
        {
            var files = await folder.GetFilesAsync();
            foreach(var file in files)
            {
                if (file.FileType.ToLower() == ".epub")
                {
                    Books.Add(new Book(file));
                }
            }

            var subfolders = await folder.GetFoldersAsync();
            foreach(var subfolder in subfolders)
            {
                await AddBooksInFolder(subfolder);
            }
        }
    }
}
