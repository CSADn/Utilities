using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _1337xSearchTool
{
    public class Torrent
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string UrlName { get; set; }
        public int Seeders { get; set; }
        public int Leechers { get; set; }
        public long Size { get; set; }
        public string Uploader { get; set; }
        public int InPage { get; set; }

        public Torrent() { }
    }
}
