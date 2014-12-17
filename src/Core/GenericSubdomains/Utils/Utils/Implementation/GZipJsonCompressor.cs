using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.GenericSubdomains.Utils.Implementation
{
    public class GZipJsonCompressor : IStringCompressor
    {
        private readonly IJsonUtils jsonSerrializer;

        public GZipJsonCompressor(IJsonUtils jsonSerrializer)
        {
            this.jsonSerrializer = jsonSerrializer;
        }

        public Stream Compress(string data)
        {
            var output = new MemoryStream();

            using (var zip = new GZipStream(output, CompressionMode.Compress, true))
            {
                using (var writer = new StreamWriter(zip, Encoding.UTF8))
                {
                    writer.Write(data);
                }
            }

            output.Seek(0, SeekOrigin.Begin);

            return output;
        }

        public T Decompress<T>(Stream stream) where T : class
        {
            using (var zip = new GZipStream(stream, CompressionMode.Decompress))
            {
                using (var reader = new StreamReader(zip, Encoding.UTF8))
                {
                    return jsonSerrializer.Deserialize<T>(reader.ReadToEnd());
                }
            }
        }

        public string CompressObject(object s)
        {
            var bytes = Encoding.Unicode.GetBytes(jsonSerrializer.Serialize(s));
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }
                return Convert.ToBase64String(mso.ToArray());
            }
        }

        public T DecompressString<T>(string s) where T:class 
        {
            var bytes = Convert.FromBase64String(s);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }
                var arrayOfData = mso.ToArray();
                var stringData = Encoding.Unicode.GetString(arrayOfData, 0, arrayOfData.Length);
                return this.jsonSerrializer.Deserialize<T>(stringData);
            }
        }

        public string CompressString(string s)
        {
            var bytes = Encoding.Unicode.GetBytes(s);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }
                return Convert.ToBase64String(mso.ToArray());
            }
        }

        public string DecompressString(string s)
        {
            var bytes = Convert.FromBase64String(s);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }
                var arrayOfData = mso.ToArray();
                return Encoding.Unicode.GetString(arrayOfData, 0, arrayOfData.Length);
            }
        }
    }
}