using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AmongUs_proxy
{
    static class BinaryHelper
    {
        /// <summary>Compare 2 bytes array.</summary>
        /// <remarks>
        /// Copyright (c) 2008-2013 Hafthor Stefansson.
        /// Distributed under the MIT/X11 software license.
        /// http://www.opensource.org/licenses/mit-license.php
        /// </remarks>
        public static unsafe bool UnsafeCompare(this byte[] a1, byte[] a2)
        {
            if (a1 == a2) return true;
            if (a1 == null || a2 == null || a1.Length != a2.Length)
                return false;
            fixed (byte* p1 = a1, p2 = a2)
            {
                byte* x1 = p1, x2 = p2;
                int l = a1.Length;
                for (int i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
                    if (*((long*)x1) != *((long*)x2)) return false;
                if ((l & 4) != 0) { if (*((int*)x1) != *((int*)x2)) return false; x1 += 4; x2 += 4; }
                if ((l & 2) != 0) { if (*((short*)x1) != *((short*)x2)) return false; x1 += 2; x2 += 2; }
                if ((l & 1) != 0) if (*((byte*)x1) != *((byte*)x2)) return false;
                return true;
            }
        }

        public static void WriteBytes(this byte[] buffer, ushort value, int offset)
        {
            using (var mem = new MemoryStream(buffer))
            using (var bw = new BinaryWriter(mem))
            {
                mem.Position = offset;
                bw.Write(value);
                bw.Flush();
            }
        }

        public static void WriteBytes(this byte[] buffer, short value, int offset)
        {
            using (var mem = new MemoryStream(buffer))
            using (var bw = new BinaryWriter(mem))
            {
                mem.Position = offset;
                bw.Write(value);
                bw.Flush();
            }
        }

        public static void WriteBytes(this byte[] buffer, int value, int offset)
        {
            using (var mem = new MemoryStream(buffer))
            using (var bw = new BinaryWriter(mem))
            {
                mem.Position = offset;
                bw.Write(value);
                bw.Flush();
            }
        }

        public static void WriteBytes(this byte[] buffer, uint value, int offset)
        {
            using (var mem = new MemoryStream(buffer))
            using (var bw = new BinaryWriter(mem))
            {
                mem.Position = offset;
                bw.Write(value);
                bw.Flush();
            }
        }

        public static void WriteBytes(this byte[] buffer, long value, int offset)
        {
            using (var mem = new MemoryStream(buffer))
            using (var bw = new BinaryWriter(mem))
            {
                mem.Position = offset;
                bw.Write(value);
                bw.Flush();
            }
        }

        public static void WriteBytes(this byte[] buffer, ulong value, int offset)
        {
            using (var mem = new MemoryStream(buffer))
            using (var bw = new BinaryWriter(mem))
            {
                mem.Position = offset;
                bw.Write(value);
                bw.Flush();
            }
        }

        public static void WriteBytes(this byte[] buffer, string value, Encoding encoding, int offset)
            => WriteBytes(buffer, value, 0, value.Length, encoding, offset);

        public static void WriteBytes(this byte[] buffer, string value, int start, int str_len, Encoding encoding, int offset)
        {
            encoding.GetBytes(value, start, str_len, buffer, offset);
        }

        public static ushort ReadUInt16(this byte[] buffer, int offset)
        {
            using (var mem = new MemoryStream(buffer))
            using (var br = new BinaryReader(mem))
            {
                mem.Position = offset;
                return br.ReadUInt16();
            }
        }

        public static short ReadInt16(this byte[] buffer, int offset)
        {
            using (var mem = new MemoryStream(buffer))
            using (var br = new BinaryReader(mem))
            {
                mem.Position = offset;
                return br.ReadInt16();
            }
        }

        public static uint ReadUInt32(this byte[] buffer, int offset)
        {
            using (var mem = new MemoryStream(buffer))
            using (var br = new BinaryReader(mem))
            {
                mem.Position = offset;
                return br.ReadUInt32();
            }
        }

        public static int ReadInt32(this byte[] buffer, int offset)
        {
            using (var mem = new MemoryStream(buffer))
            using (var br = new BinaryReader(mem))
            {
                mem.Position = offset;
                return br.ReadInt32();
            }
        }

        public static ulong ReadULong(this byte[] buffer, int offset)
        {
            using (var mem = new MemoryStream(buffer))
            using (var br = new BinaryReader(mem))
            {
                mem.Position = offset;
                return br.ReadUInt64();
            }
        }

        public static long ReadLong(this byte[] buffer, int offset)
        {
            using (var mem = new MemoryStream(buffer))
            using (var br = new BinaryReader(mem))
            {
                mem.Position = offset;
                return br.ReadInt64();
            }
        }

        public static string ReadString(this byte[] buffer, Encoding encoding, int offset, int count)
        {
            return encoding.GetString(buffer, offset, count);
        }
    }
}
