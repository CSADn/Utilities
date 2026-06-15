using System;

namespace Packs.Entities
{
    public class File
    {
        public string Filename { get; set; }
        public int Length { get; set; }
        public byte[] Content { get; set; }

        public File()
        {
            //
        }
    }
}
