// SimpleFS.cs
// Pete Myers
// Spring 2018-2020
//
// NOTE: Implement the methods and classes in this file
//
// Kate LaFrance
// 5/29/2020
//
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleFileSystem
{
    public class SimpleFS : FileSystem
    {
        #region filesystem

        //
        // File System
        //

        private const char PATH_SEPARATOR = FSConstants.PATH_SEPARATOR;
        private const int MAX_FILE_NAME = FSConstants.MAX_FILENAME;
        // Part of extra credit
        //private const int BLOCK_SIZE = 500;     // 500 bytes... 2 sectors of 256 bytes each (minus sector overhead)

        private VirtualFS virtualFileSystem;

        public SimpleFS()
        {
            virtualFileSystem = new VirtualFS();
        }

        public void Mount(DiskDriver disk, string mountPoint)
        {
            virtualFileSystem.Mount(disk, mountPoint);
        }

        public void Unmount(string mountPoint)
        {
            virtualFileSystem.Unmount(mountPoint);
        }

        public void Format(DiskDriver disk)
        {
            virtualFileSystem.Format(disk);
        }

        public Directory GetRootDirectory()
        {
            return new SimpleDirectory(virtualFileSystem.RootNode);
        }

        public FSEntry Find(string path)
        {
            // Follow the path down and return a ne directory or file
            // wrapping the node
            // Path must be an absolute path, including the root /
            // Simplifying assumption... not allowing ".." or "." in our paths
            // good:  /foo/bar, /foo/bar/
            // bad:  foo, foo/bar, //foo/bar, /foo//bar, /foo/../foo/bar
            if (String.IsNullOrEmpty(path) || path[0] != '/')
                throw new Exception("Hey! Invalid path!");

            // If path ends in a trailing slash, then...
            bool mustBeDirectory = false;
            if (path.EndsWith(PATH_SEPARATOR.ToString()))
            {
                // Remove the trailing slash and remember that we expect a directory
                path = path.Substring(0, path.Length - 1);
                mustBeDirectory = true;
            }

            // Walk through path and find the result
            string[] elements = path.Split(PATH_SEPARATOR);
            VirtualNode current = virtualFileSystem.RootNode;
            foreach (string element in elements.Skip(1))    // Skips the first blank
            {
                // We need to dig deeper, but we do not have a directory, so return null
                if (!current.IsDirectory)
                    return null;

                try
                {
                    // Get the named child from the directory
                    VirtualNode child = current.GetChild(element);
                    current = child;
                }
                catch (Exception)
                {
                    // If we can't find the child, return null
                    return null;
                }
            }

            if (mustBeDirectory && !current.IsDirectory)
                return null;

            return (current.IsDirectory ? new SimpleDirectory(current) as FSEntry : new SimpleFile(current));
        }

        public char PathSeparator { get { return PATH_SEPARATOR; } }
        public int MaxNameLength { get { return MAX_FILE_NAME; } }

        #endregion filesystem

        #region implementation

        //
        // FSEntry
        //

        abstract private class SimpleEntry : FSEntry
        {
            protected VirtualNode node;

            protected SimpleEntry(VirtualNode node)
            {
                this.node = node;
            }

            public string Name => node.Name;
            public Directory Parent => node.Parent == null ? null : new SimpleDirectory(node.Parent);

            public string FullPathName
            {
                get
                {
                    // Return a full path name to this entry, starting from the root
                    if (Parent == null)
                        return Name;
                    else if (Parent.Name == FSConstants.ROOT_DIR_NAME)
                        return $"{Parent.Name}{Name}";
                    else
                        return $"{Parent.FullPathName}{PATH_SEPARATOR}{ Name}";
                }
            }

            // override in derived classes
            public virtual bool IsDirectory => node.IsDirectory;

            public virtual bool IsFile => node.IsFile;

            public void Rename(string name)
            {
                node.Rename(name);
            }

            public void Move(Directory destination)
            {
                if (destination == null)
                    throw new Exception("Invalid Destination!");

                node.Move((destination as SimpleDirectory).node);
            }

            public void Delete()
            {
                node.Delete();
                node = null;
                // TODO: Deal with null nodes in the rest f the logial file systems objects
                // e.g. Move()
            }
        }

        //
        // Directory
        //

        private class SimpleDirectory : SimpleEntry, Directory
        {
            public SimpleDirectory(VirtualNode node) : base(node)
            {
            }

            public IEnumerable<Directory> GetSubDirectories()
            {
                List<Directory> result = new List<Directory>();
                foreach (VirtualNode child in node.GetChildren().Where(vn => vn.IsDirectory))
                {
                    result.Add(new SimpleDirectory(child));
                }
                return result;
            }

            public IEnumerable<File> GetFiles()
            {
                List<File> result = new List<File>();
                foreach (VirtualNode child in node.GetChildren().Where(vn => vn.IsFile))
                {
                    result.Add(new SimpleFile(child));
                }
                return result;
            }

            public Directory CreateDirectory(string name)
            {
                // Example of adapter pattern
                return new SimpleDirectory(node.CreateDirectoryNode(name));
            }

            public File CreateFile(string name)
            {
                return new SimpleFile(node.CreateFileNode(name));
            }
        }

        //
        // File
        //

        private class SimpleFile : SimpleEntry, File
        {
            public SimpleFile(VirtualNode node) : base(node)
            {
            }

            public int Length => node.FileLength;

            public FileStream Open()
            {
                return new SimpleStream(node);
            }
        }

        //
        // FileStream
        //

        private class SimpleStream : FileStream
        {
            private VirtualNode node;

            public SimpleStream(VirtualNode node)
            {
                this.node = node;
            }

            public void Close()
            {
                // Detatch the stream from the underlying file VirtualNode
                node = null;
            }

            public byte[] Read(int index, int length)
            {
                if (node == null)
                    throw new Exception("Stream closed for Read()!");

                return node.Read(index, length);
            }

            public void Write(int index, byte[] data)
            {
                if (node == null)
                    throw new Exception("Stream closed for Write()!");

                node.Write(index, data);
            }
        }

        #endregion implementation
    }
}