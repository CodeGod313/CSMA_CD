using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSMA_CD
{
    public class Package
    {
        private byte[] data;
        private string source;
        private string destination;
        private static UInt32[] crc_table = new UInt32[256];

        private static bool isGenerated = false;
        
        public byte[] Data
        {
            get => data;
            set => data = value;
        }
        
        public Package(string source, string destination, byte[] data)
        {
            this.source = source;
            this.destination = destination;
            this.data = data;
            generateCRCTable();
        }

        public byte[] getBytes()
        {
            byte[] buf = Encoding.ASCII.GetBytes(source)
                .Concat(Encoding.ASCII.GetBytes(destination))
                .Concat(data)
                .ToArray();
            
            uint fcs = countCheckSum(buf);
            buf = buf.Concat(BitConverter.GetBytes(fcs)).ToArray();
            doByteStuffing(ref buf);
            byte[] ans =     { 0x7E };
            return ans.Concat(buf).ToArray();
        }

        private void doByteStuffing(ref byte[] data)
        {
            List<byte> buffer = new List<byte>();
            buffer.Add(0xFF);
            int overheadID = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (buffer.Count - overheadID == 255)
                {
                    if (data[i] == 0x7E)
                    {
                        buffer.Add(1);
                        buffer.Add(0xFF);
                        overheadID = buffer.Count - 1;
                    }
                    else
                    {
                        overheadID = buffer.Count;
                        buffer.Add(0xFF);
                        buffer.Add(data[i]);
                    }
                }
                else
                {
                    if (data[i] == 0x7E)
                    {
                        buffer[overheadID] = (byte) (buffer.Count - overheadID);
                        overheadID = buffer.Count;
                        buffer.Add(0xFF);
                    }else buffer.Add(data[i]);
                }
            }

            buffer[overheadID] = (byte) (buffer.Count + 1 - overheadID);
            data = buffer.ToArray();
        }
        
        private static void generateCRCTable()
        {
            UInt32 crc;
            for (UInt32 i = 0; i < 256; i++) {
                crc = i;
                for (UInt32 j = 0; j < 8; j++)
                    crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320 : crc >> 1;
                crc_table[i] = crc;
            };
        }
        
        
        private static uint countCheckSum(byte[] data)
        {
            if(!isGenerated) generateCRCTable();
            UInt32 crc;
            crc = 0xFFFFFFFF;
            foreach (byte s in data) {
                crc = crc_table[(crc ^ s) & 0xFF] ^ (crc >> 8);
            }
            crc ^= 0xFFFFFFFF;
            return crc;
        }
        
        
        private static void reverseByteStuffing(ref byte[] data)
        {
            if (data.Length == 0) throw new ArgumentException("Package data isn't defined");
            List<int> indexesToDelete = new List<int>();
            indexesToDelete.Add(0);
            int i = data[0];
            while (i < data.Length)
            {
                if (data[i] == 0xFF)
                {
                    indexesToDelete.Add(i);
                    i += data[i];
                }
                else
                {
                    byte tmp = data[i];
                    data[i] = 0x7E;
                    i += tmp;
                }
            }

            data = data
                .Where((x, index) => !indexesToDelete
                .Exists(y => y == index))
                .ToArray();
        }

        public static string getStringFromPackage(byte[] package, ref string source, ref string destination)
        {
            package = package.Where((x,i) => i != 0).ToArray();
            reverseByteStuffing(ref package);
            
            uint receivedFcs = BitConverter.ToUInt32(
                package.Skip(package.Length - 4)
                            .Take(4)
                            .ToArray(), 0);
            
            uint fcs = countCheckSum(package.Take(package.Length - 4).ToArray());

            if (fcs != receivedFcs) return null;
            source = Encoding.UTF8.GetString(package.Take(4).ToArray());
            destination = Encoding.UTF8.GetString(package.Skip(4).Take(4).ToArray());
            return Encoding.UTF8.GetString(
                package
                    .Skip(8)
                    .Take(package.Length-12)
                    .ToArray());
        }
    }
}