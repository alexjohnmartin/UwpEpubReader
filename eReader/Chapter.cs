using System;
using Windows.Storage;

namespace eReader
{
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
