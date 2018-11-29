using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace BeySoft
{
    public class PckClass
    {
        // Private Variables
        #region Private Variables

        private string _pkxName;
        private FileHeaderV22 _header22;
        private FileHeaderV23 _header23;
        private FileHeaderV23S _header23s;

        #endregion

        // Public Properties
        #region Public Properties

        public string PckName { get; set; }        // The full path name of the PCK.
        public string PkxName                      // The full path name of the PKX.
        {
            get { return _pkxName; }
            set
            {
                _pkxName = value;
                _pkxName = Path.ChangeExtension(PckName, ".pkx");
            }
        }

        public uint EntryCount { get; set; }       // Number of File Table Entries
        public int Version { get; set; }           // The version number of the PCK.
        public ulong FileSize { get; set; }        // The file size of the PCK.
        public long CompressedSize { get; set; }   // The compressed size of the PCK in bytes.
        public long DecompressedSize { get; set; } // The decompressed size of the PCK in bytes.

        public TableEntry[] FileTable;             // The array of file table entries.

        public bool IsPck { get; set; }            // True is the file is a PCK without PKX.
                                                   // When IsPck is true, IsPkx is false.
        public bool IsPkx { get; set; }            // True is the file is a PKX. When PKX
                                                   // is true, IsPck is false.
        public bool IsSpanned { get; set; }        // True is a file in the archive spans
                                                   // across the PCK and PKX files.
        public bool IsVersion23 { get; set; }      // True is the PCK is version 0x20003, else false.

        public bool IsSwordsman { get; set; }      // True if the PCK is Swodsman, else false.

        public string GameName { get; set; }       // Game name the PCK originates from.

        #endregion  // Public Properties
        
        // Constants
        #region Constants

        private const int TwoGb = 0x7FFFFF00; // Actually, 0x80000000 - 0x100
        private const uint MaxId = 1000; // Highest AlgorithmID to search for, minus 1.
        
        #endregion

        // Public Methods
        #region Public Methods

        /// <summary>
        /// Constructor for the PckClass.
        /// </summary>
        /// <param name="pckName">The full path of the PCK file.</param>
        public PckClass(string pckName)
        {
            PckName = pckName;
            PkxName = Path.ChangeExtension(pckName, ".pkx");

            _header22  = new FileHeaderV22(0x20002);
            _header23  = new FileHeaderV23(0x20003);
            _header23s = new FileHeaderV23S(0);;
        }

        /// <summary>
        /// Open the PCK or PCK/PKX archive.
        /// </summary>
        /// <param name="fs">FileStream</param>
        /// <param name="br">Binaryreader</param>
        /// <param name="algoId">AlgorithmID for the PCK archive</param>
        /// <returns>Returns -1 on error, else 0.</returns>
        public int OpenArchive(FileStream fs, BinaryReader br, uint algoId)
        {
            if (GetFileInfo(fs, br) == -1)
                return -1;

            if (IsPkx) return OpenPckEx(fs, br, algoId) == -1 ? -1 : 0;
            if (IsPck) return OpenPck(  fs, br, algoId) == -1 ? -1 : 0;

            return 0;
        }

        #endregion // Public Methods

        // Private Methods
        #region Private Methods

        // General Methods
        #region General Methods

        /// <summary>
        /// Get the Version and number of file table entries.
        /// </summary>
        /// <param name="fs">FileStream</param>
        /// <param name="br">BinaryReader</param>
        /// <returns>Returns -1 on error.</returns>
        private int GetFileInfo(FileStream fs, BinaryReader br)
        {
            IsPkx = File.Exists(PkxName);

            if (IsPkx)
            {
                using (FileStream f = new FileStream(PkxName, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader b = new BinaryReader(f))
                    {
                        f.Seek(-8, SeekOrigin.End);
                        EntryCount = b.ReadUInt32();

                        Version = b.ReadInt32();
                        IsVersion23 = Version == 0x20003;

                        if (VerifyVersion() == -1)
                            return -1;

                        // check for swordsman
                        if (CheckForSwordsman(f, b))
                        {
                            f.Seek(-12, SeekOrigin.End);
                            _header23s.GuardByte1 = b.ReadUInt32();
                        }
                        else
                        {
                            if (IsVersion23)
                            {
                                f.Seek(-16, SeekOrigin.End);
                                _header23.GuardByte1 = b.ReadUInt64();
                            }
                            else
                            {
                                f.Seek(-12, SeekOrigin.End);
                                _header22.GuardByte1 = b.ReadUInt32();
                            }
                        }
                    }
                }

                fs.Seek(4, SeekOrigin.Begin);
                FileSize = IsVersion23 ? br.ReadUInt64() : br.ReadUInt32();

                return 0;
            }

            fs.Seek(-8, SeekOrigin.End);
            EntryCount = br.ReadUInt32();

            // get version
            Version = br.ReadInt32();
            IsVersion23 = Version == 0x20003;

            if (VerifyVersion() == -1)
                return -1;

            // check for swordsman
            if (CheckForSwordsman(fs, br))
            {
                fs.Seek(-12, SeekOrigin.End);
                _header23s.GuardByte1 = br.ReadUInt32();
            }
            else
            {
                if (IsVersion23)
                {
                    fs.Seek(-16, SeekOrigin.End);
                    _header23.GuardByte1 = br.ReadUInt64();
                }
                else
                {
                    fs.Seek(-12, SeekOrigin.End);
                    _header23.GuardByte1 = br.ReadUInt32();
                }
            }

            fs.Seek(4, SeekOrigin.Begin);
            FileSize = IsVersion23 ? br.ReadUInt64() : br.ReadUInt32();

            return 0;
        }

        /// <summary>
        /// Check if PCK/PKX is Swordsman
        /// </summary>
        /// <returns>True if Swordsman, else false.</returns>
        private bool CheckForSwordsman(FileStream stream, BinaryReader reader)
        {
            stream.Seek(-Marshal.SizeOf(typeof(FileHeaderV23S)), SeekOrigin.End);
            _header23s.GuardByte0 = reader.ReadUInt32();

            ulong tmp = reader.ReadUInt64();
            _header23s.TableOffset = tmp;

            uint sign = (uint) (tmp >> 0x20);

            if (IsPkx)
                if (sign == 0x49AB7F1C) { IsSwordsman = true; return true; } // PCK/PKX
            if (    sign == 0x49AB7F1D) { IsSwordsman = true; return true; } // PCK

            return false;
        }

        /// <summary>
        /// Verify the file opened has a valid version.
        /// </summary>
        /// <returns>Returns -1 on error.</returns>
        private int VerifyVersion()
        {
            return Version == _header22.Version ||
                   Version == _header23.Version
                ? 0
                : -1;
        }

        // Returns the incorrect offset on some versions. Using another PCK
        // tool and rebuilding it fixes the problem. Will have to research
        // this and account for it. I suppose the best way to do this would
        // be to rebuild the tables... no clue at this point.
        private uint GetSpannedTableEntry(PckClass pck)
        {
            uint offset = 0;
            uint i = 0;

            while (offset < TwoGb)
            {
                offset = pck.FileTable[i].DataOffset;
                i++;
            }

            return i - 2;
        }

        private void FlagCheck(int index, uint spannedIndex)
        {
            IsPck = false;
            IsPkx = false;
            IsSpanned = false;

            if (index < spannedIndex)
                IsPck = true;
            else if (index > spannedIndex)
                IsPkx = true;
            else if (index == spannedIndex)
                IsSpanned = true;
        }

        /// <summary>
        /// Convert the ID into the name of the game that uses the specified algorithm ID.
        /// </summary>
        /// <param name="id">The AlgorithmID of the Wanmei game</param>
        /// <returns>The name of the Wanmei game corresponding to the AlgorithmID</returns>
        private string IdToGameName(uint id)
        {
            string s;

            switch (id)
            {
                case 0:
                    s = "Jade Dynasty/Perfect World";
                    break;
                case 111:
                    s = "Hot Dance Party";
                    break;
                case 121:
                    s = "Ether Saga Odyssey";
                    break;
                case 131:
                    s = "Forsaken World";
                    break;
                case 161:
                    s = IsSwordsman ? "Swordsman Online" : "Saint Seiya";
                    break;
                default:
                    s = "Unknown Game PCK archive!";
                    break;
            }

            return s;
        }

        private uint AutoDetect()
        {
            uint i = 0;
            
            for (i = 0; i < MaxId; i++)
            {
                AlgorithmId id = new AlgorithmId(i);

                if (IsVersion23)
                {
                    if (id.PckGuardByte0 != _header23.GuardByte0 ||
                        id.PckGuardByte1 != _header23.GuardByte1)
                        continue;
                    break;
                }

                if (id.PckGuardByte0 != _header22.GuardByte0 ||
                    id.PckGuardByte1 != _header22.GuardByte1)
                    continue;
                break;
            }

            
            GameName = IdToGameName(i);

            return i;
        }

        #endregion // General Methods

        // Open Methods
        #region Open Methods

        /// <summary>
        /// Open PCK less than 0x7FFFFF00 in size.
        /// </summary>
        /// <param name="fs">FileStream</param>
        /// <param name="br">BinaryReader</param>
        /// <param name="algoId">AlgorithmId</param>
        /// <returns>Returns -1 on error.</returns>
        private int OpenPck(FileStream fs, BinaryReader br, uint algoId)
        {
            if (IsSwordsman)
            {
                IsPck = true;
                IsPkx = false;
            }
            else
            {
                switch (Version)
                {
                    case 0x20002:
                        IsPck = true;
                        IsPkx = false;
                        fs.Seek(-12, SeekOrigin.End);
                        _header22.GuardByte1 = br.ReadUInt32();
                        fs.Seek(-(Marshal.SizeOf(typeof(FileHeaderV22)) + 8), SeekOrigin.End);
                        _header22.GuardByte0 = br.ReadUInt32();
                        _header22.Version = br.ReadInt32();
                        break;
                    case 0x20003:
                        IsPck = true;
                        IsPkx = false;
                        fs.Seek(-16, SeekOrigin.End);
                        _header23.GuardByte1 = (uint)(br.ReadUInt64() >> 0x20);
                        fs.Seek(-(Marshal.SizeOf(typeof(FileHeaderV23)) + 8), SeekOrigin.End);
                        _header23.GuardByte0 = br.ReadUInt32();
                        _header23.Version = br.ReadInt32();
                        break;
                    default:
                        IsPck = false;
                        IsPkx = false;
                        return -1;
                }
            }

            AlgorithmId id = IsSwordsman
                ? new AlgorithmId(161)
                : new AlgorithmId(AutoDetect());

            if (IsSwordsman)
                GameName = IdToGameName(161);

            ulong offset;
            if (IsSwordsman)
            {
                ulong tmp = _header23s.TableOffset;
                offset = tmp ^ 0x49AB7F1D33C3EDDB;
            }
            else
                offset = br.ReadUInt32() ^ id.PckMaskDword;

            if (IsVersion23)
                _header23.TableOffset = (uint) offset;
            else
                _header22.TableOffset = (uint) offset;

            fs.Seek((long) offset, SeekOrigin.Begin);
            FileTable = new TableEntry[EntryCount];

            for (int i = 0; i < EntryCount; i++)
            {
                uint size = br.ReadUInt32() ^ id.PckMaskDword;
                size = br.ReadUInt32() ^ id.PckMaskDword ^ id.PckCheckMask;

                byte[] buffer = br.ReadBytes((int)size);
                FileTable[i] = new TableEntry();

                int flags = size < 0x114 ? 1 : 0;

                FileTable[i] = FileTable[i].ReadTableEntry(buffer, (int)offset, Version, flags != 0);
                FileTable[i].FilePath = FileTable[i].FilePath.Replace("/", "\\");
            }

            return 0;
        }

        /// <summary>
        /// Opens the PKX extension and reads the file table entries.
        /// </summary>
        /// <param name="fs">FileStream</param>
        /// <param name="br">BinaryReader</param>
        /// <param name="algoId">AlgorithmId</param>
        private void OpenPkx(FileStream fs, BinaryReader br, uint algoId)
        {
            // *****NOTE TO SELF******
            // This CAN be cleaned up!
            using (FileStream f = new FileStream(PkxName, FileMode.Open))
            {
                using (BinaryReader b = new BinaryReader(f))
                {
                    if (IsVersion23)
                    {
                        f.Seek(-16, SeekOrigin.End);
                        _header23.GuardByte1 = b.ReadUInt32();
                        f.Seek(-(Marshal.SizeOf(typeof(FileHeaderV23)) + 8), SeekOrigin.End);
                        _header23.GuardByte0 = b.ReadUInt32();
                        _header23.Version = b.ReadInt32();
                    }
                    else
                    {
                        f.Seek(-12, SeekOrigin.End);
                        _header22.GuardByte1 = b.ReadUInt32();
                        f.Seek(-(Marshal.SizeOf(typeof(FileHeaderV22)) + 8), SeekOrigin.End);
                        _header22.GuardByte0 = b.ReadUInt32();
                        _header22.Version = b.ReadInt32();
                    }

                    AlgorithmId id = new AlgorithmId(AutoDetect());
                    uint offset = b.ReadUInt32() ^ id.PckMaskDword;

                    if (offset <= TwoGb)
                    {
                        if (IsVersion23)
                        {
                            fs.Seek(-16, SeekOrigin.End);
                            _header23.GuardByte1 = br.ReadUInt64() >> 0x20;
                            fs.Seek(-(Marshal.SizeOf(typeof(FileHeaderV23)) + 8), SeekOrigin.End);
                            _header23.GuardByte0 = br.ReadUInt32();
                            _header23.Version = br.ReadInt32();
                        }
                        else
                        {
                            fs.Seek(-12, SeekOrigin.End);
                            _header22.GuardByte1 = br.ReadUInt32();
                            fs.Seek(-(Marshal.SizeOf(typeof(FileHeaderV22)) + 8), SeekOrigin.End);
                            _header22.GuardByte0 = br.ReadUInt32();
                            _header22.Version = br.ReadInt32();
                        }

                        id = new AlgorithmId(AutoDetect());
                        uint code = br.ReadUInt32() ^ id.PckMaskDword;
                        fs.Seek(code, SeekOrigin.Begin);
                    }
                    else
                    {
                        long pos = Convert.ToInt64(offset) - TwoGb;
                        f.Seek(pos, SeekOrigin.Begin);
                        offset -= TwoGb;
                    }

                    FileTable = new TableEntry[EntryCount];

                    for (int i = 0; i < EntryCount; i++)
                    {
                        uint size = b.ReadUInt32() ^ id.PckMaskDword;
                        size = b.ReadUInt32() ^ id.PckMaskDword ^ id.PckCheckMask;

                        byte[] buffer = b.ReadBytes((int) size);
                        FileTable[i] = new TableEntry();

                        int flags = size < 0x114 ? 1 : 0;

                        FileTable[i] = FileTable[i].ReadTableEntry(buffer, (int) offset, Version, flags != 0);
                        FileTable[i].FilePath = FileTable[i].FilePath.Replace("/", "\\");
                    }

                    // Sort array by data offset.
                    FileTable = FileTable.OrderBy(x => x.DataOffset).ToArray();
                }
            }
        }

        /// <summary>
        /// Opens PCK/PKX extended PCK where PCK & PKX
        /// are both less than 0x7FFFFF00 in size.
        /// </summary>
        /// <param name="fs">FileStream</param>
        /// <param name="br">BinaryReader</param>
        /// <param name="algoId">AlgorithmId</param>
        /// <returns>>Returns -1 on error.</returns>
        private int OpenPckEx(FileStream fs, BinaryReader br, uint algoId)
        {
            if (Version == 0x20002 || Version == 0x20003)
            {
                IsPck = false;
                IsPkx = true;
                OpenPkx(fs, br, algoId);
                return 0;
            }

            IsPck = false;
            IsPkx = false;
            return -1;
        }

        public void SortTable(PckClass pck)
        {
            uint count = pck.EntryCount;
            uint[] entry = new uint[count];

            for (int i = 0; i < count; i++)
                entry[i] = pck.FileTable[i].DataOffset;


        }

        #endregion // Open Methods

        // Extract Methods
        #region Extract Methods

        /// <summary>
        /// Extract the PCK or PCK/PKX archive to folder.
        /// </summary>
        /// <param name="folder">Location of folder to extract files to.</param>
        /// <param name="fs">FileStream</param>
        /// <param name="br">BinaryReader</param>
        /// <param name="algoId">AlgorithmId</param>
        /// <returns>Returns -1 on error.</returns>
        public int ExtractArchive(string folder, FileStream fs, BinaryReader br, uint algoId)
        {
            return ExtractPck(folder, fs, br) == -1 ? -1 : 0;
        }

        /// <summary>
        /// Determines version and whether it is a PCK or
        /// PCK/PKX extended archive and extracts to folder.
        /// </summary>
        /// <param name="folder">Location of folder to extract files to.</param>
        /// <param name="fs">FileStream</param>
        /// <param name="br">BinaryReader</param>
        /// <returns>Returns -1 on error.</returns>
        private int ExtractPck(string folder, FileStream fs, BinaryReader br)
        {
            if (IsSwordsman)
            {
                if (IsPck) { UnPck(folder, fs, br); return 0; }

                return 0;
            }

            if (Version == 0x20002 || Version == 0x20003)
            {
                if (IsPck) { UnPck(folder, fs, br); return 0; }

                if (IsPkx && Version == 0x20003)
                {
                    UnPkx(folder, fs, br);
                    return 0;
                }

                if (IsPkx && Version == 0x20002)
                {
                    UnPkx(folder, fs, br);
                    return 0;
                }
            }

            IsPck = false;
            IsPkx = false;
            return -1;
        }

        /// <summary>
        /// Extract PCK archive to folder path.
        /// </summary>
        /// <param name="folder">Location of folder to extract files to.</param>
        /// <param name="fs">FileStream</param>
        /// <param name="br">BinaryReader</param>
        private void UnPck(string folder, FileStream fs, BinaryReader br)
        {
            if (IsSwordsman)
                throw new NotImplementedException();

            uint count = EntryCount;

            for (int i = 0; i < count; i++)
            {
                string fPath = FileTable[i].FilePath;
                string sPath = folder + "\\" + fPath;

                if (fPath.Contains("\\"))
                {
                    fPath = fPath.Substring(0, fPath.LastIndexOf('\\'));

                    if (!Directory.Exists(folder + "\\" + fPath))
                        Directory.CreateDirectory(folder + "\\" + fPath);
                }

                using (FileStream fStream = new FileStream(sPath, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter bWriter = new BinaryWriter(fStream))
                    {
                        uint offset = FileTable[i].DataOffset;

                        fs.Seek(offset, SeekOrigin.Begin);

                        byte[] buf = br.ReadBytes((int)FileTable[i].CompressedSize);

                        if (FileTable[i].CompressedSize <
                            FileTable[i].DecompressedSize)
                        {
                            byte[] buffer = FileTable[i].Decompress(buf, (int)FileTable[i].CompressedSize,
                                (int)FileTable[i].DecompressedSize);
                            bWriter.Write(buffer);
                        }
                        else
                        {
                            bWriter.Write(buf);
                        }
                    }
                }
            }
        }

        private void DoPck(uint offset, int i, FileStream fs, BinaryReader br, BinaryWriter bw, byte[] buffer)
        {
            offset = FileTable[i].DataOffset;

            fs.Seek(offset, SeekOrigin.Begin);

            byte[] buf = br.ReadBytes((int)FileTable[i].CompressedSize);

            if (FileTable[i].CompressedSize <
                FileTable[i].DecompressedSize)
            {
                buffer = FileTable[i].Decompress(buf, (int)FileTable[i].CompressedSize,
                    (int)FileTable[i].DecompressedSize);
                bw.Write(buffer);
            }
            else
            {
                bw.Write(buf);
            }
        }

        private void DoPkx(long offset, int i, BinaryWriter bw, byte[] buffer)
        {
            using (FileStream fileStream = new FileStream(PkxName, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader binaryReader = new BinaryReader(fileStream))
                {
                    offset = FileTable[i].DataOffset - TwoGb;
                    fileStream.Seek(offset, SeekOrigin.Begin);

                    byte[] size = binaryReader.ReadBytes((int)FileTable[i].CompressedSize);

                    if (FileTable[i].CompressedSize <
                        FileTable[i].DecompressedSize)
                    {
                        buffer = FileTable[i].Decompress(size, (int)FileTable[i].CompressedSize,
                            (int)FileTable[i].DecompressedSize);
                        bw.Write(buffer);
                    }
                    else
                    {
                        bw.Write(size);
                    }
                }
            }
        }

        private void LoadBuffers(byte[] buf1, byte[] buf2, int i, int pckDataSize, int pkxDataSize)
        {
            using (FileStream stream1 = new FileStream(PckName, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader1 = new BinaryReader(stream1))
                {
                    stream1.Seek(FileTable[i].DataOffset, SeekOrigin.Begin);
                    buf1 = reader1.ReadBytes(pckDataSize);
                }
            }

            using (FileStream stream1 = new FileStream(PckName, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader1 = new BinaryReader(stream1))
                {
                    stream1.Seek(FileTable[i].DataOffset, SeekOrigin.Begin);
                    buf1 = reader1.ReadBytes(pckDataSize);
                }
            }

            using (FileStream stream2 = new FileStream(PkxName, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader2 = new BinaryReader(stream2))
                {
                    stream2.Seek(0, SeekOrigin.Begin);
                    buf2 = reader2.ReadBytes(pkxDataSize);
                }
            }
        }

        /// <summary>
        /// Extracts all the files from PCK/PKX archive into selected folder.
        /// </summary>
        /// <param name="folder">Location of folder to extract files to.</param>
        /// <param name="fs">FileStram</param>
        /// <param name="br">BinaryReader</param>
        private void UnPkx(string folder, FileStream fs, BinaryReader br)
        {
            uint count = EntryCount;
            uint spannedIndex = GetSpannedTableEntry(this);

            for (int i = 0; i < count; i++)
            {
                string fPath = FileTable[i].FilePath;
                string sPath = folder + "\\" + fPath;

                if (fPath.Contains("\\"))
                {
                    fPath = fPath.Substring(0, fPath.LastIndexOf('\\'));

                    if (!Directory.Exists(folder + "\\" + fPath))
                        Directory.CreateDirectory(folder + "\\" + fPath);
                }

                if (PkxName == null)
                    continue;

                FlagCheck(i, spannedIndex);

                using (FileStream fStream = new FileStream(sPath, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter bWriter = new BinaryWriter(fStream))
                    {
                        uint offset = 0;
                        byte[] buffer = { };

                        if (IsPck) DoPck(offset, i, fs, br, bWriter, buffer);
                        if (IsPkx) DoPkx(offset, i, bWriter, buffer);

                        if (!IsSpanned)
                            continue;

                        // Deal with file that spans across the pck/pkx archive.
                        int compressedSize = (int)FileTable[i].CompressedSize; // Total data in the spanned file.
                        int pckDataSize = (int)(TwoGb - FileTable[i].DataOffset); // Amount of data in PCK
                        int pkxDataSize = compressedSize - pckDataSize; // Amount of data in PKX

                        byte[] buf1 = new byte[pckDataSize]; // buffer for PCK part
                        byte[] buf2 = new byte[pkxDataSize]; // buffer for PKX part
                        
                        LoadBuffers(buf1, buf2, i, pckDataSize, pkxDataSize);

                        byte[] bytes = new byte[buf1.Length + buf2.Length];
                        buf1.CopyTo(bytes, 0);
                        buf2.CopyTo(bytes, buf1.Length);

                        if (FileTable[i].CompressedSize <
                            FileTable[i].DecompressedSize)
                        {
                            buffer = FileTable[i].Decompress(buffer, (int)FileTable[i].CompressedSize,
                                (int)FileTable[i].DecompressedSize);
                            bWriter.Write(buffer);
                        }
                        else
                        {
                            bWriter.Write(buffer);
                        }
                    }
                }
            }
        }

        #endregion // Extract Methods

        // Compress Methods
        #region Compress Methods



        #endregion
        
        #endregion // Private Methods
    }
}
