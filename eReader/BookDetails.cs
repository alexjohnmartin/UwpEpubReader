using Windows.Storage;

namespace eReader
{
    internal class BookDetails
    {
        public StorageFile BookFile { get; private set; }
        public int Chapter { get; private set; }
        public int Position { get; private set; }

        public BookDetails(StorageFile bookFile, int chapter = 0, int position = 0)
        {
            this.BookFile = bookFile;
            this.Position = position;
            this.Chapter = chapter;
        }
    }
}