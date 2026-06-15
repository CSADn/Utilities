using DITool.Enums;
using DITool.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DITool.Models
{
    public class MF1ICS20Block
    {
        private int _id;
        private byte[] _data;

        public MF1ICS20Blocks Id => (MF1ICS20Blocks) _id;
        public byte[] Data => _data;


        public MF1ICS20Block(int id, byte[] data)
        {
            _id = id;
            _data = data;
        }


        public string KeyA => _data.Subset(0, 6);
        public string Access => _data.Subset(6, 4);
        public string KeyB => _data.Subset(10, 6);


        public byte[] SetKeys(byte[] keyA, byte[] keyB)
        {
            _data.Overwrite(0, keyA);
            _data.Overwrite(10, keyB);

            return _data;
        }
    }
}
