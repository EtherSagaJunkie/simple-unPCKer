using System;
using System.Runtime.InteropServices;
using System.Text;

namespace BeySoft
{
    // Version 0x20002 header struct
    [StructLayout(LayoutKind.Explicit)]
    public struct FileHeaderV22
    {
        [FieldOffset(0)]
        public uint GuardByte0;

        [FieldOffset(4)]
        public int Version;

        [FieldOffset(8)]
        public uint TableOffset;

        [FieldOffset(12)]
        public uint Flags;

        [FieldOffset(16)]
        public byte[] Description;

        [FieldOffset(268)]
        public uint GuardByte1;

        public FileHeaderV22(int version)
        {
            GuardByte0 = 0;
            Version = version;
            TableOffset = 0;
            Flags = 0;
            Description = new byte[252];
            GuardByte1 = 0;
            Copyright = "Angelica File Package, Perfect World Co. Ltd. 2002~2008. All Rights Reserved.";
        }
        
        public uint GetFlags()
        {
            return Flags;
        }

        public int GetHeaderSize()
        {
            return Marshal.SizeOf(typeof(FileHeaderV22));
        }

        public string Copyright
        {
            get
            {
                return Encoding.GetEncoding("GBK").GetString(Description);
            }
            set
            {
                Encoding encoding = Encoding.GetEncoding("GBK");
                byte[] buffer = new byte[128];
                byte[] bytes = encoding.GetBytes(value);

                if (buffer.Length > bytes.Length)
                {
                    Array.Copy(bytes, buffer, bytes.Length);
                }
                else
                {
                    Array src = bytes;
                    Array dst = buffer;

                    Array.Copy(src, dst, dst.Length);
                }

                Description = buffer;
            }
        }
    }

    // Version 0x20003 header struct
    [StructLayout(LayoutKind.Explicit)]
    public struct FileHeaderV23
    {
        [FieldOffset(0)]
        public uint GuardByte0;

        [FieldOffset(4)]
        public int Version;

        [FieldOffset(8)]
        public uint TableOffset;

        [FieldOffset(12)]
        public ulong Flags;

        [FieldOffset(20)]
        public byte[] Description;

        [FieldOffset(272)]
        public ulong GuardByte1;

        public FileHeaderV23(int version)
        {
            GuardByte0 = 0;
            Version = version;
            TableOffset = 0;
            Flags = 0;
            Description = new byte[252];
            GuardByte1 = 0;
            Copyright = "Angelica File Package, Perfect World Co. Ltd. 2002~2008. All Rights Reserved.";
        }

        public ulong GetFlags()
        {
            return Flags;
        }

        public int GetHeaderSize()
        {
            return Marshal.SizeOf(typeof(FileHeaderV23));
        }

        public string Copyright
        {
            get
            {
                return Encoding.GetEncoding("GBK").GetString(Description);
            }
            set
            {
                Encoding encoding = Encoding.GetEncoding("GBK");
                byte[] buffer = new byte[128];
                byte[] bytes = encoding.GetBytes(value);

                if (buffer.Length > bytes.Length)
                {
                    Array.Copy(bytes, buffer, bytes.Length);
                }
                else
                {
                    Array src = bytes;
                    Array dst = buffer;

                    Array.Copy(src, dst, dst.Length);
                }

                Description = buffer;
            }
        }
    }

    // Swordsman header struct
    [StructLayout(LayoutKind.Explicit)]
    public struct FileHeaderV23S
    {
        [FieldOffset(0)]
        public uint GuardByte0;

        [FieldOffset(4)]
        public ulong TableOffset;

        [FieldOffset(12)]
        public uint Flags;

        [FieldOffset(20)]
        public byte[] Description;

        [FieldOffset(272)]
        public uint GuardByte1;

        public FileHeaderV23S(uint flags)
        {
            GuardByte0 = 0;
            TableOffset = 0;
            Flags = flags;
            Description = new byte[252];
            GuardByte1 = 0;
            Copyright = "Angelica File Package, Perfect World Co. Ltd. 2002~2008. All Rights Reserved.";
        }

        public uint GetFlags()
        {
            return Flags;
        }

        public int GetHeaderSize()
        {
            return Marshal.SizeOf(typeof(FileHeaderV22));
        }

        public string Copyright
        {
            get
            {
                return Encoding.GetEncoding("GBK").GetString(Description);
            }
            set
            {
                Encoding encoding = Encoding.GetEncoding("GBK");
                byte[] buffer = new byte[128];
                byte[] bytes = encoding.GetBytes(value);

                if (buffer.Length > bytes.Length)
                {
                    Array.Copy(bytes, buffer, bytes.Length);
                }
                else
                {
                    Array src = bytes;
                    Array dst = buffer;

                    Array.Copy(src, dst, dst.Length);
                }

                Description = buffer;
            }
        }
    }
}
