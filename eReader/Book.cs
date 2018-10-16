using Windows.Storage;

namespace eReader
{
    public class Book
    {
        public StorageFile BookFile { get; private set; }
        public string Name { get; private set; }

        public Book(StorageFile file)
        {
            this.BookFile = file;
            this.Name = file.Name;
        }
    }
}