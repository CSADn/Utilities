using DITool.Enums;
using DITool.Extensions;
using DITool.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DITool.Models
{
    public class MF1ICS20
    {
        private byte[] _data;
        private List<MF1ICS20Sector> _sectors;

        private byte[] _keyA;
        private byte[] _keyB;


        public byte[] Data => _data;
        public string UID => _data.Subset(0, 7);
        public string KeyA => HexConverter.ToString(_keyA);
        public string KeyB => HexConverter.ToString(_keyB);
        
        
        public MF1ICS20(byte[] data)
        {
            _data = data;

            LoadSectors();
        }


        public MF1ICS20Sector GetSector(MF1ICS20Sectors sector)
            => _sectors[(int)sector];


        public void ComputeKeys()
        {
            var pattern = new Regex("^04[0-9a-f]{12}$", RegexOptions.IgnoreCase);

            if (!pattern.IsMatch(UID))
                throw new ArgumentException("UID inválido");

            var magicNumbers = new List<BigInteger>
            {
                3, 5, 7, 23,
                BigInteger.Parse("9985861487287759675192201655940647"),
                BigInteger.Parse("38844225342798321268237511320137937")
            };

            var pre = magicNumbers[0] * magicNumbers[1] * magicNumbers[3] * magicNumbers[5];
            var post = magicNumbers[0] * magicNumbers[2] * magicNumbers[4];
            var concat = $"{pre.ToString("x")}{UID}{post.ToString("x")}";
            var inputBuffer = HexConverter.ToBytes(concat);

            var sha1 = System.Security.Cryptography.SHA1.Create();
            var outputBuffer = sha1.ComputeHash(inputBuffer);

            // Bytes: 3, 2, 1, 0, 7, 6
            var key = new List<byte>
            {
                outputBuffer[3],
                outputBuffer[2],
                outputBuffer[1],
                outputBuffer[0],
                outputBuffer[7],
                outputBuffer[6]
            };

            _keyA = key.ToArray();
            _keyB = key.ToArray();

            SetKeys(_keyA, _keyB);
        }

        public byte[] SetKeys(byte[] keyA, byte[] keyB)
        {
            for (var i = 0; i < _sectors.Count; i++)
            {
                var sectorData = _sectors[i].SetKeys(keyA, keyB);
                _data.Overwrite(i * 64, sectorData);
            }

            return _data;
        }


        public void ToMFD(string fullPath) => ToBIN(fullPath, ".mfd");

        public void ToDMP(string fullPath) => ToBIN(fullPath, ".dmp");

        public void ToBIN(string fullPath, string extension = ".bin")
        {
            if (!fullPath.ToLower().EndsWith(extension))
                fullPath += extension;

            File.WriteAllBytes(fullPath, _data);
        }

        public void ToMCT(string fullPath)
        {
            if (!fullPath.ToLower().EndsWith(".mct"))
                fullPath += ".mct";

            var sectorNumber = 0;
            var blockNumber = 0;
            var output = new StringBuilder();

            using (var ms = new MemoryStream(_data))
            {
                var block = new byte[16];
                
                while (ms.Read(block, 0 * blockNumber, 16) > 0)
                {
                    if (blockNumber % 4 == 0)
                        output.AppendLine($"+Sector: {sectorNumber++}");

                    var blockHex = HexConverter.ToString(block).ToUpper();
                    output.AppendLine(blockHex);

                    blockNumber++;
                }
            }

            File.WriteAllText(fullPath, output.ToString());
        }


        public void GenerateKeysFile(string fullPath)
        {
            if (!fullPath.ToLower().EndsWith(".mfd"))
                fullPath += ".mfd";

            using (var fw = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                var uid = _data.Take(7).ToArray();

                for (int i = 0; i < 5; i++)
                {
                    var buffer = new byte[64];

                    if (i == 0)
                        Array.Copy(uid, buffer, 7);

                    Array.Copy(_keyA, 0, buffer, 48, 6);
                    Array.Copy(_keyB, 0, buffer, 58, 6);

                    fw.Write(buffer, 0, buffer.Length);
                }

                fw.Flush();
                fw.Close();
            }
        }


        private void LoadSectors()
        {
            _sectors = new List<MF1ICS20Sector>();

            for (int i = 0; i < 5; i++)
                _sectors.Add(new MF1ICS20Sector(i, _data.Skip(i * 64).Take(64).ToArray()));
        }
    }
}
