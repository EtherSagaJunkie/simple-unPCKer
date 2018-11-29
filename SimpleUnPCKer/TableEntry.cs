using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

namespace BeySoft
{
    public class TableEntry
    {
        //  Properties
        #region Properties

        public string FilePath { get; set; }
        public uint DataOffset { get; set; }
        public uint DecompressedSize { get; set; }
        public uint CompressedSize { get; set; }
        public uint AccessCnt { get; set; }

        #endregion // Properties

        #region Constants

        private const int MaxPath = 260;
        private const int ShiftCount = 0x20;

        #endregion

        //  Public Methods
        #region Methods
        public TableEntry ReadTableEntry(byte[] buffer, int size, int version, bool flag)
        {
            TableEntry entry = new TableEntry();

            buffer = version == 0x20002 && flag
                ? Decompress(buffer, size, Marshal.SizeOf(typeof(FileHeaderV22)) + 4)
                : Decompress(buffer, size, Marshal.SizeOf(typeof(FileHeaderV23)) + 4);

            using (MemoryStream ms = new MemoryStream(buffer))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    byte[] bytes = br.ReadBytes(MaxPath);
                    Encoding encoding = Encoding.GetEncoding("GBK");
                    string filePath = encoding.GetString(bytes).Replace("\0", "");

                    switch (version)
                    {
                        case 0x20003:
                            entry.FilePath = filePath;
                            entry.DataOffset = (uint)(br.ReadUInt64() >> ShiftCount);
                            entry.DecompressedSize = (uint)(br.ReadUInt64() >> ShiftCount);
                            entry.CompressedSize = (uint)br.ReadUInt64();
                            break;
                        default: // version 0x20002
                            entry.FilePath = filePath;
                            entry.DataOffset = br.ReadUInt32();
                            entry.DecompressedSize = br.ReadUInt32();
                            entry.CompressedSize = br.ReadUInt32();
                            break;
                    }
                }
            }

            return entry;
        }

        public byte[] Decompress(byte[] compressed, int sizeCompressed, int sizeDecompressed)
        {
            byte[] buffer = new byte[sizeDecompressed];

            using (MemoryStream ms = new MemoryStream(compressed))
            {
                ms.ReadByte(); ms.ReadByte(); // Throw away the first 2 bytes.

                using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Decompress, true))
                {
                    ds.Read(buffer, 0, sizeDecompressed);
                }
            }

            return buffer;
        }
        
        #endregion  // Public Methods
    }
}
