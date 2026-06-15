using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.Mail
{
    public class Attachment
    {
        public string Name { get; private set; }

        public string Path { get; private set; }

        public byte[] BinaryContent { get; private set; }

        public MemoryStream Stream { get; private set; }


        public Attachment(string name, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException();

            var fullPath = Utilities.ResolvePath(path);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException(path);

            Path = fullPath;

            var delimitedName = name
                .Trim()
                .Replace(" ", ".");

            var cacheKey = string.Concat("Helpers.Mail.Attachment.", delimitedName);

            var binaryContent = cacheKey.FromCache(
                () => File.ReadAllBytes(fullPath),
                false
            );

            ConstructFromBinary(name, binaryContent);
        }

        public Attachment(string name, byte[] binaryContent)
        {
            ConstructFromBinary(name, binaryContent);
        }


        private void ConstructFromBinary(string name, byte[] binaryContent)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException();

            if (binaryContent.Length == 0)
                throw new ArgumentNullException();

            Name = name;
            BinaryContent = binaryContent;
            Stream = new MemoryStream(BinaryContent);
        }
    }
}
