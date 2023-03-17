using System;
using System.IO;

namespace BardAfar
{
    /// <summary>
    /// The MainWindow uses a ListBox to show files and directories.
    /// This class represents the information that may be stored in
    /// an item in this list box.
    /// </summary>
    internal class ListBoxItem
    {
        // The string to show for "next level up":
        public const string DirectoryUp = "..";

        // The full path to the thing this item represents:
        public string FullPath { get; private set; } = String.Empty;

        // Whether this item represents a directory:
        public bool isDirectory { get; private set; }

        // Whether this item represents a file:
        public bool isFile { get; private set; }

        // Whether this item represents the parent of the current directory:
        private bool isDirectoryUp;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">
        /// The full path to the file or directory.
        /// If this is the parent directory then this should
        /// be the full path to that parent directory.
        /// </param>
        /// <param name="isDirectoryUp">True if this is the parent directory.</param>
        public ListBoxItem(string path, bool isDirectoryUp = false)
        {
            if (path == null) throw new ArgumentNullException("path");
            FullPath = path;

            this.isDirectoryUp = isDirectoryUp;
            isDirectory = Directory.Exists(path);
            isFile = File.Exists(path);
        }

        /// <summary>
        /// The text the user sees on the item.
        /// </summary>
        public override string ToString()
        {
            string r;
            if (isDirectoryUp)
            {
                r = DirectoryUp;
            }
            else if (isDirectory)
            {
                DirectoryInfo d = new DirectoryInfo(FullPath);
                r = @"\" + d.Name;
            }
            else
            {
                FileInfo f = new FileInfo(FullPath);
                r = f.Name;
            }
            return r;
        }

        /// <summary>
        /// Gets a relative path to the item represented by this
        /// item relative to another path.
        /// </summary>
        /// <param name="serverBasePath">
        /// A full path to a directory.
        /// This must be a parent directory (direct or indirect)
        /// of of the file represented by this item.
        /// </param>
        public string GetServerPath(string serverBasePath)
        {
            return Path.GetRelativePath(serverBasePath, FullPath);
        }
    }
}
