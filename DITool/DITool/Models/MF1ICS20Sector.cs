using DITool.Enums;
using DITool.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace DITool.Models
{
    public class MF1ICS20Sector
    {
        private int _id;
        private byte[] _data;
        private List<MF1ICS20Block> _blocks;

        public byte[] Data => _data;


        public MF1ICS20Sector(int id, byte[] data)
        {
            _id = id;
            _data = data;

            LoadBlocks();
        }


        public MF1ICS20Block GetBlock(MF1ICS20Blocks block)
            => _blocks[(int)block];
        public MF1ICS20Block GetTrailing()
            => _blocks[3];
        public string GetAccess()
            => _blocks[3].Access;

        public byte[] SetKeys(byte[] keyA, byte[] keyB)
        {
            var blockData = _blocks[3].SetKeys(keyA, keyB);
            _data.Overwrite(3 * 16, blockData);

            return _data;
        }


        private void LoadBlocks()
        {
            _blocks= new List<MF1ICS20Block>();

            for (int i = 0; i < 4; i++)
                _blocks.Add(new MF1ICS20Block(i, _data.Skip(i * 16).Take(16).ToArray()));
        }
    }
}
