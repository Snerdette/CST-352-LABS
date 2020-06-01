// SimpleVirtualFS.cs
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
    // NOTE:  Blocks are used for file data, directory contents are just stored in linked sectors (not blocks)

    public class VirtualFS
    {
        private const int DRIVE_INFO_SECTOR = 0;
        private const int ROOT_DIR_SECTOR = 1;
        private const int ROOT_DATA_SECTOR = 2;

        private Dictionary<string, VirtualDrive> drives;    // mountPoint --> drive
        private VirtualNode rootNode;

        public VirtualFS()
        {
            this.drives = new Dictionary<string, VirtualDrive>();
            this.rootNode = null;
        }

        public void Format(DiskDriver disk)
        {
            // wipe all sectors of disk and create minimum required DRIVE_INFO, DIR_NODE and DATA_SECTOR

            // Wipe all sectors (replace with "zeroes" are FREE_SECTOR)
            int bps = disk.BytesPerSector;
            FREE_SECTOR free = new FREE_SECTOR(bps);
            for (int i = 0; i < disk.SectorCount; i++)
            {
                disk.WriteSector(i, free.RawBytes);
            }

            // Create DRIVE_INFO
            DRIVE_INFO di = new DRIVE_INFO(bps, ROOT_DIR_SECTOR);
            disk.WriteSector(DRIVE_INFO_SECTOR, di.RawBytes);

            // Create and write the DIR_NODE for the root node...
            DIR_NODE dn = new DIR_NODE(bps, ROOT_DATA_SECTOR, FSConstants.ROOT_DIR_NAME, 0);
            disk.WriteSector(ROOT_DIR_SECTOR, dn.RawBytes);

            // ... and an empty DATA_SECTOR
            DATA_SECTOR ds = new DATA_SECTOR(bps, 0, null); // 0 = no next data sector, nul = empty set
            disk.WriteSector(ROOT_DATA_SECTOR, ds.RawBytes);

        }

        public void Mount(DiskDriver disk, string mountPoint)
        {
            // read drive info from disk, load root node and connect to mountPoint
            // for the first mounted drive, expect mountPoint to be named "/", FSConstants.ROOT_DIR_NAME, as the root
  
            try
            {
                // Step 1: Read infor from the disk to understand it's directory structure
                // Read DRIVE_INFO from FRIVE_INFO_SECTOR
                DRIVE_INFO di = DRIVE_INFO.CreateFromBytes(disk.ReadSector(DRIVE_INFO_SECTOR));
                DIR_NODE dn = DIR_NODE.CreateFromBytes(disk.ReadSector(di.RootNodeAt));
                DATA_SECTOR ds = DATA_SECTOR.CreateFromBytes(disk.ReadSector(dn.FirstDataAt));


                // Step 2: Join th new disk into the virtual file system structure, at the mount point.
                // Create a VistualDrive for the disk
                VirtualDrive vd = new VirtualDrive(disk, DRIVE_INFO_SECTOR, di);

                
                if (rootNode == null)
                {
                    // Create a VirtualNode to represent the root dictionaru, at the mount point.
                    // Set the VFS's root node, if this is the first disk to be mounted.
                    rootNode = new VirtualNode(vd, di.RootNodeAt, dn, null);
                    
                }
                else
                {
                    // TODO: Extra Credit:  Handle 2nd dick mounted to exsisting VFS
                    // Create a virtual node for this new disk's root
                    // "join" the new node to the exsisting node structure at the mount point
                }

                // Add a new VirtualDrive to drives dictionary, using te mountPoint as the key
                drives.Add(mountPoint, vd);

            }
            catch (Exception ex)
            {
                throw new Exception("Failed to mount disk");
            }
            
        }

        public void Unmount(string mountPoint)
        {
            // look up the drive and remove it's mountPoint
            if (!drives.ContainsKey(mountPoint))
                throw new Exception("Cannot unmount drive that has not been mounted!");

            // Remove from the drive list
            VirtualDrive vd = drives[mountPoint];
            drives.Remove(mountPoint);

            // Check if this drive was the first mounted drive...
            if (mountPoint == FSConstants.ROOT_DIR_NAME)
            {
                // If mounted first drive, blank out the root node
                rootNode = null;
            } else
            {
                // else it;s a2nd, 3rd, etc...
                // TODO: extra credit
            }
        }

        public VirtualNode RootNode => rootNode;
    }

    public class VirtualDrive
    {
        private int bytesPerDataSector;
        private DiskDriver disk;
        private int driveInfoSector;
        private DRIVE_INFO sector;      // caching entire sector for now

        public VirtualDrive(DiskDriver disk, int driveInfoSector, DRIVE_INFO sector)
        {
            this.disk = disk;
            this.driveInfoSector = driveInfoSector;
            this.bytesPerDataSector = DATA_SECTOR.MaxDataLength(disk.BytesPerSector);
            this.sector = sector;
        }

        public int[] GetNextFreeSectors(int count)
        {
            // find count available free sectors on the disk and return their addresses
            if (count <= 0)
                throw new Exception("Hey! No Negative!");
            
            int[] result = new int[count];

            // Itterate over all sectors, starting at the beginning, until we find count FREE_SECTORS
            int found = 0;
            for (int lba = 0; found < count && lba < disk.SectorCount; lba++)
            {
                //byte[] bytes = disk.ReadSector(lba);
                if (SECTOR.GetTypeFromBytes(disk.ReadSector(lba)) == SECTOR.SectorType.FREE_SECTOR)
                {
                    result[found++] = lba;
                }
            }

            // If we didn't find enough free sectors, just return null
            if (found < count)
                return null;

            return result;
        }

        public DiskDriver Disk => disk;
        public int BytesPerDataSector => bytesPerDataSector;
    }

    public class VirtualNode
    {
        private VirtualDrive drive;
        private int nodeSector;
        private NODE sector;                                // caching entire sector for now
        private VirtualNode parent;
        private Dictionary<string, VirtualNode> children;   // child name --> child node
        private List<VirtualBlock> blocks;                  // cache of file blocks

        public VirtualNode(VirtualDrive drive, int nodeSector, NODE sector, VirtualNode parent)
        {
            this.drive = drive;
            this.nodeSector = nodeSector;
            this.sector = sector;
            this.parent = parent;
            this.children = null;                           // initially empty cache
            this.blocks = null;                             // initially empty cache
        }

        public VirtualDrive Drive => drive;
        public string Name => sector.Name;
        public VirtualNode Parent => parent;
        public bool IsDirectory { get { return sector.Type == SECTOR.SectorType.DIR_NODE; } }
        public bool IsFile { get { return sector.Type == SECTOR.SectorType.FILE_NODE; } }
        public int ChildCount => (sector as DIR_NODE).EntryCount;
        public int FileLength => (sector as FILE_NODE).FileSize;

        public void Rename(string name)
        {
            // rename this node, update parent as needed, save new name on disk

            // Cannot rename the root node
            if (parent == null)
                throw new Exception("Cannot rename the root node!");
           
            // Validate name, e.g. no /, not empty, whitespace, not null, not . or not ..
            // Name muct be 12 characters or less
            if(string.IsNullOrWhiteSpace(name) 
                || name == "." || name == ".."
                || name.Contains('/')
                || name.Length > FSConstants.MAX_FILENAME)
            {
                throw new Exception("Invalid name!");
            }

            // Make our parent's cache of children are loaded and remove the reference to the old name.
            parent.LoadChildren();
            parent.children.Remove(Name);

            // Set the nodes name to the new name and commit it to disk
            sector.Name = name;
            drive.Disk.WriteSector(nodeSector, sector.RawBytes);

            // Add this node back to the parent's cache with the new name
            parent.children.Add(name, this);
            
        }

        public void Move(VirtualNode destination)
        {
            // remove this node from it's current parent and attach it to the new one
            // update the directory information for both parents on disk

            // Don't try and move the root!
            if (parent == null)
                throw new Exception("Can't Move the root!");

            // Validate destination
            if (destination == null || !destination.IsDirectory)
                throw new Exception("Invalid destination for Move!");

            // Remove node from current parent
            parent.LoadChildren();
            parent.children.Remove(Name);
            parent.CommitChildren();

            // Set parent to new parent
            parent = destination;

            // Add node to new parent
            parent.LoadChildren();
            parent.children.Add(Name, this);
            parent.CommitChildren();            
        }

        public void Delete()
        {
           /* make sectors free!
             - wipe data for this node from the disk
             - wipe this node from parent directory from the disk
             - remove this node from it's parent node 
           */

            // Don't try to nuke the root!
                                  if (parent == null)
                throw new Exception("Can't delete the root!");

            // Recurse into directory contents and delete children
            if (IsDirectory)
            {
                LoadChildren();
                // Loop throught a copy of the children cahce and delete each
                foreach (VirtualNode child in children.Values.ToArray())
                {
                    child.Delete();
                }
                // NOTE: do not need to call CommitChildren();
            }

            // Replace data sector(s) with free sector(s)
            FREE_SECTOR free = new FREE_SECTOR(drive.Disk.BytesPerSector);
            int dataSectorAt = sector.FirstDataAt;
            while (dataSectorAt != 0)
            {
                // Read the data sector om and save nextSectorAt
                DATA_SECTOR dataSector = DATA_SECTOR.CreateFromBytes(drive.Disk.ReadSector(dataSectorAt));
                int nextDataSectorAt = dataSector.NextSectorAt;

                // Replace it
                drive.Disk.WriteSector(dataSectorAt, free.RawBytes);

                // Next one
                dataSectorAt = nextDataSectorAt;
            }

            // Replace node sector with free sector
            drive.Disk.WriteSector(nodeSector, free.RawBytes);

            // Remove ourself from our parent and commit paren'ts list of children
            parent.LoadChildren();
            parent.children.Remove(Name);
            parent.CommitChildren();

        }

        private void LoadChildren()
        {
            // Ensure that the children cache is correctly put in memory (i.e. reflects what's on the disk)
            // Assume if the cache exsists, it is correct at the moment
            // So, we need to call CommitChildren()
            if (children == null)
            {
                // Create the cache itself
                children = new Dictionary<string, VirtualNode>();

                // Read the list of children for this directory from disk
                // Instantiate a VirtualNode for each [child?] and add them to the children cache.
                // Read the data sector for this directory
                DATA_SECTOR dataSector = DATA_SECTOR.CreateFromBytes(drive.Disk.ReadSector(sector.FirstDataAt));
                // Extract the list of children from the sata sector
                byte[] rawList = dataSector.DataBytes;

                // Foreach child in the list...
                for (int i = 0; i < ChildCount; i++)
                {
                    // Getthe child's sector address 
                    int childSectorAt = BitConverter.ToInt32(rawList, i*4);

                    // Read its sector from disk
                    // Check if it's a file or directory
                    byte[] childNodeBytes = drive.Disk.ReadSector(childSectorAt);
                    NODE childSector;
                    if (SECTOR.GetTypeFromBytes(childNodeBytes) == SECTOR.SectorType.DIR_NODE)
                    {
                        childSector = DIR_NODE.CreateFromBytes(drive.Disk.ReadSector(childSectorAt));
                    } 
                    else if(SECTOR.GetTypeFromBytes(childNodeBytes) == SECTOR.SectorType.FILE_NODE)
                    {
                        childSector = FILE_NODE.CreateFromBytes(drive.Disk.ReadSector(childSectorAt));
                    }
                    else
                    {
                        throw new Exception("Unexpected sector type whe reading directory's children!");
                    }

                    // Construct a VirtualNode
                    VirtualNode childNode = new VirtualNode(drive, childSectorAt, childSector, this);

                    // Add the VirtualNode to the children cache
                    children.Add(childNode.Name, childNode);
                }
            }
        }

        private void CommitChildren()
        {
            if (children != null)
            {
                // Write changes to the in-memory cache to disk
                // so that what is in the cache and on the disk is the same
                // Speciffically, write the list of children back to disk dor this directory
                // in the directory's data sector!

                // Create a list of children node sectors, in bytes
                byte[] rawList = new byte[drive.BytesPerDataSector];
                int i = 0;
                foreach (VirtualNode child in children.Values)
                {
                    int sectorAt = child.nodeSector;
                    BitConverter.GetBytes(sectorAt).CopyTo(rawList, i);
                    i += 4;         // 4 byte integers are being copied to the array
                }

                // write the bytes to the directory's DATA_SECTOR
                // by replacing it's data bytes with the new list
                int dataSectorAt = sector.FirstDataAt;
                DATA_SECTOR dataSector = DATA_SECTOR.CreateFromBytes(drive.Disk.ReadSector(dataSectorAt));
                dataSector.DataBytes = rawList;
                drive.Disk.WriteSector(dataSectorAt, dataSector.RawBytes);

                // Update the number of children in the dir's node on disk
                (sector as DIR_NODE).EntryCount = children.Count;
                drive.Disk.WriteSector(nodeSector, sector.RawBytes);
            }

        }

        public VirtualNode CreateDirectoryNode(string name)
        {
            return CreateNode(name, SECTOR.SectorType.DIR_NODE);
        }

        public VirtualNode CreateFileNode(string name)
        {
            // Create a new file, both on disk and in memory
            return CreateNode(name, SECTOR.SectorType.FILE_NODE);
        }

        private VirtualNode CreateNode(string name, SECTOR.SectorType type)
        {
            // Create a new file, both on disk and in memory
            // type may be DIR_NODE or FILE_NODE

            if (type != SECTOR.SectorType.DIR_NODE && type != SECTOR.SectorType.FILE_NODE)
                throw new Exception("Illegal type, CreateNode only take DIR_NODE and FILE_NODE types!");

            // Get 2 free sectors
            // First Sector: NODE, containing metadata 
            // Second Sector: DATA_SECTOR, containing the new node's data
            int[] freeSectors = drive.GetNextFreeSectors(2);
            if (freeSectors == null || freeSectors.Length != 2)
                throw new Exception("Can't find 2 free sectors for a new " +
                    (type == SECTOR.SectorType.DIR_NODE ? "directory" : "file") + "!");
            int newNodeAt = freeSectors[0];
            int newDataSectorAt = freeSectors[1];

            // Create the node sector on disk, initially empty
            int bps = drive.Disk.BytesPerSector;
            NODE newNode = ( type == SECTOR.SectorType.DIR_NODE 
                ? new DIR_NODE(bps, newDataSectorAt, name, 0) as NODE 
                : new FILE_NODE(bps, newDataSectorAt, name, 0) as NODE);

            //Create the DATA_SECTOR sector on disk for the new directory
            // initially empty data sector for this new directory
            DATA_SECTOR dataSector = new DATA_SECTOR(bps, 0, null);

            // Write sectors to disk
            drive.Disk.WriteSector(newNodeAt, newNode.RawBytes);
            drive.Disk.WriteSector(newDataSectorAt, dataSector.RawBytes);

            // Create a new VirtualNode instance
            VirtualNode newVirtualNode = new VirtualNode(drive, newNodeAt, newNode, this);

            // Add this to the in-memory cache of this directory's children
            LoadChildren();
            children.Add(name, newVirtualNode);
            CommitChildren();

            // Return the new VirtualNode instace
            return newVirtualNode;
        }

        public IEnumerable<VirtualNode> GetChildren()
        {
            // Make sure the cache is valid
            LoadChildren();

            // Return the children!
            return children.Values;
        }

        public VirtualNode GetChild(string name)
        {
            // Make sure the cache is filled first
            LoadChildren();

            // Make sure the child is actually there
            if (!children.ContainsKey(name))
                throw new Exception("Node does not contain child!");

            // Return the child
            return children[name];
        }

        private void LoadBlocks()
        {
            // Read each block if not alread in cache
            if (blocks == null)
            {
                // The cache is currently empty, so create and fill it!
                blocks = new List<VirtualBlock>();

                // Read the data sectors off the disk for this file and...
                int dataSectorAt = sector.FirstDataAt;
                while (dataSectorAt != 0)
                {
                    // ...create VirtualBlocks for them and add to the cache
                    DATA_SECTOR dataSector = DATA_SECTOR.CreateFromBytes(drive.Disk.ReadSector(dataSectorAt));
                    VirtualBlock vb = new VirtualBlock(drive, dataSectorAt, dataSector);
                    blocks.Add(vb);

                    // Go on to the next data sector
                    dataSectorAt = dataSector.NextSectorAt;
                }
                
            }
        }

        private void CommitBlocks()
        {
            // Commit all blocks in the cache
            if (blocks != null)
            {
                // Commit each block to disk
                foreach (VirtualBlock vb in blocks)
                {
                    vb.CommitBlock();
                }
            }
        }

        public byte[] Read(int index, int length)
        {
            
            if (!IsFile)
                throw new Exception("Must be a file to read bytes!");

            // Make sure the cache is filled
            LoadBlocks();

            // Copy the data from the blocks
            byte[] result = VirtualBlock.ReadBlockData(drive, blocks, index, length);

            // Return the data
            return result;
        }

        public void Write(int index, byte[] data)
        {
           

            if (!IsFile)
                throw new Exception("Must be a file to write bytes!");

            // Make sure the cache is filled!
            LoadBlocks();

            // Extend the blocks if necessarry
            int initialFileLength = FileLength;
            int finalFileLength = Math.Max(index + data.Length, initialFileLength);
            VirtualBlock.ExtendBlocks(drive, blocks, initialFileLength, finalFileLength);

            // Write the data to the blocks
            VirtualBlock.WriteBlockData(drive, blocks, index, data);
            

            //Write the blocks 
            CommitBlocks();

            // Update file length in file node sector
            // Make sure to incease the file length as needed
            if (finalFileLength > initialFileLength)
            {
                (sector as FILE_NODE).FileSize = finalFileLength;
                drive.Disk.WriteSector(nodeSector, sector.RawBytes);
            }
           
            
        }
    }

    public class VirtualBlock
    {
        private VirtualDrive drive;
        private DATA_SECTOR sector;
        private int sectorAddress;
        private bool dirty;

        public VirtualBlock(VirtualDrive drive, int sectorAddress, DATA_SECTOR sector, bool dirty = false)
        {
            this.drive = drive;
            this.sector = sector;
            this.sectorAddress = sectorAddress;
            this.dirty = dirty;
        }

        public int SectorAddress => sectorAddress;
        public DATA_SECTOR Sector => sector;
        public bool Dirty => dirty;

        public byte[] Data
        {
            get { return (byte[])sector.DataBytes.Clone(); }
            set
            {
                sector.DataBytes = value;
                dirty = true;
            }
        }

        public void CommitBlock()
        {
            // Commit only if dirty
            if (dirty)
            {
                // write the data to disk
                drive.Disk.WriteSector(sectorAddress, sector.RawBytes);

                // It's nice and clean now
                dirty = false;
            }
        }

        public static byte[] ReadBlockData(VirtualDrive drive, List<VirtualBlock> blocks, int startIndex, int length)
        {
            int bdps = drive.BytesPerDataSector;
            int totalBytesToCopy = length;

            // Create the result byte[] to return at the end
            byte[] result = new byte[length];

            // Part 1: Find the first block to be read and read the first chunk to the end of it
            int blockIndex = startIndex / bdps;                     // Index of the first block to be read
            VirtualBlock block = blocks[blockIndex];


            // Copy the first chunk into the first block
            int fromStart = startIndex % bdps;                                  // Where to start in the first block
            int bytesToCopy = Math.Min(totalBytesToCopy, bdps - fromStart);    // How many bytes to copy into the first block
            int toStart = 0;
            CopyBytes(bytesToCopy, block.Data, fromStart, result, toStart);
            
            // Move on to the next chunk of data
            toStart += bytesToCopy;

            // Part 2: Loop hoguh the middle blokcj and overwrite them completly with full chunks of data
            while (toStart < (totalBytesToCopy - bdps))
            {
                // Next block
                block = blocks[++blockIndex];

                // Copy next chunk
                bytesToCopy = bdps;     // Full block
                fromStart = 0;            // Overwrite starting at beginning of block
                CopyBytes(bytesToCopy, block.Data, fromStart, result, toStart);

                // Move on to the next chunk of data
                toStart += bytesToCopy;
            }

            // Part 3: Read the first part of the last block with the final chunk
            if (toStart < totalBytesToCopy)
            {
                // Next block
                block = blocks[++blockIndex];

                // Copy last chunk
                bytesToCopy = totalBytesToCopy - toStart;
                fromStart = 0;                                            // Overwrite starting at beginning of block
                CopyBytes(bytesToCopy, block.Data, fromStart, result, toStart);
            }
           
            return result;
        }

        public static void WriteBlockData(VirtualDrive drive, List<VirtualBlock> blocks, int startIndex, byte[] data)
        {
            int bdps = drive.BytesPerDataSector;
            int totalBytesToCopy = data.Length;
            
            // Part 1: Find the first block to be written and write the first chunk to the end of it
            int blockIndex = (startIndex / bdps);                     // Index of the first block to be written
            VirtualBlock block = blocks[blockIndex];


            // Copy the first chunk into the first block
            int toStart = startIndex % bdps;                        // Where to start in the first block
            int bytesToCopy = Math.Min(totalBytesToCopy, bdps - toStart);    // How many bytes to copy into the first block
            int fromStart = 0;
            byte[] db = block.sector.DataBytes;
            CopyBytes(bytesToCopy, data, fromStart, db, toStart);
            block.sector.DataBytes = db;
            block.dirty = true;

            // Move on to the next chunk of data
            fromStart += bytesToCopy;

            // Part 2: Loop hoguh the middle blokcj and overwrite them completly with full chunks of data
            while (fromStart < (totalBytesToCopy - bdps))
            {
                // Next block
                block = blocks[++blockIndex];

                // Copy next chunk
                bytesToCopy = bdps;     // Full block
                toStart = 0;            // Overwrite starting at beginning of block
                db = block.sector.DataBytes;
                CopyBytes(bytesToCopy, data, fromStart, db, toStart);
                block.sector.DataBytes = db;
                block.dirty = true;

                // Move on to the next chunk of data
                fromStart += bytesToCopy;
            }

            // Part 3: Write the first part of the last block with the final chunk
            if (fromStart < totalBytesToCopy)
            {
                // Next block
                block = blocks[++blockIndex];

                // Copy last chunk
                bytesToCopy = totalBytesToCopy - fromStart;
                toStart = 0;                                            // Overwrite starting at beginning of block
                db = block.sector.DataBytes;
                CopyBytes(bytesToCopy, data, fromStart, db, toStart);
                block.sector.DataBytes = db;
                block.dirty = true;
            }
        }

        public static void ExtendBlocks(VirtualDrive drive, List<VirtualBlock> blocks, int initialFileLength, int finalFileLength)
        {
            // If the file length has grown...
            if (finalFileLength > initialFileLength)
            {
                // ...and if we need ore blocks...
                int finalBlockCount = BlocksNeeded(drive, finalFileLength);
                if (finalBlockCount > blocks.Count)
                {
                    // how many new blocks to do what we need to do?
                    int neededNewBlocks = finalBlockCount - blocks.Count;

                    // Get needed number of free sectors
                    int[] freeSectors = drive.GetNextFreeSectors(neededNewBlocks);

                    // Allocate and connect them up
                    VirtualBlock previousBlock = blocks.Last();
                    for (int i = 0; i < neededNewBlocks; i++)
                    {

                        // write out the previousBlock to connect it to this new block
                        previousBlock.sector.NextSectorAt = freeSectors[i];
                        previousBlock.dirty = true;


                        // instantiate new DATA_SECTOR
                        DATA_SECTOR newDataSector = new DATA_SECTOR(drive.Disk.BytesPerSector, 0, null);

                        // instantiate new VirtualBlock
                        VirtualBlock newBlock = new VirtualBlock(drive, freeSectors[i], newDataSector, true);

                        // Add new VirtualBlocks to the list of blocks
                        blocks.Add(newBlock);

                        // Update prev block
                        previousBlock = newBlock;
                    }


                }
            }
        }

        private static int BlocksNeeded(VirtualDrive drive, int numBytes)
        {
            return Math.Max(1, (int)Math.Ceiling((double)numBytes / drive.BytesPerDataSector));
        }

        private static void CopyBytes(int copyCount, byte[] from, int fromStart, byte[] to, int toStart)
        {
            for (int i = 0; i < copyCount; i++)
            {
                to[toStart + i] = from[fromStart + i];
            }
        }
    }
}
