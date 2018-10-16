using System;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace eReader
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
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

            this.Frame.Navigate(typeof(ReaderPage), bookFile);
        }
    }
}
