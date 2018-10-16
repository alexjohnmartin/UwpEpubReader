using Windows.Storage;

namespace eReader
{
    public class BookItem
    {
        public StorageFolder Folder { get; private set; }
        public StorageFile File { get; private set; }
        public string Name { get; private set; }
        public bool IsFolder => Folder != null;

        public BookItem(StorageFile file)
        {
            this.File = file;
            this.Name = file.Name;
        }

        public BookItem(StorageFolder folder, string name = "")
        {
            this.Folder = folder;
            this.Name = string.IsNullOrEmpty(name) ? "(" + folder.Name + ")" : name;
        }
    }
}