using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace KfxLogger
{
    class MyHash
    {
        public delegate string CalculateHash(string filename);

        public static string Md5(object o)
        {
            var tmpSource = ASCIIEncoding.ASCII.GetBytes(""+o);
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(tmpSource);
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }
        public static string GetMd5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    var sb = new StringBuilder(hash.Length * 2);

                    foreach (byte b in hash)
                    {
                        // can be "x2" if you want lowercase
                        sb.Append(b.ToString("x2"));
                    }

                    return sb.ToString();
                }
            }
        }
        public static string GetSha1(string filename)
        {
            using (var stream = File.OpenRead(filename))
            {
                using (SHA1Managed sha1 = new SHA1Managed())
                {
                    var hash = sha1.ComputeHash(stream);
                    var sb = new StringBuilder(hash.Length * 2);

                    foreach (byte b in hash)
                    {
                        // can be "x2" if you want lowercase
                        sb.Append(b.ToString("x2"));
                    }

                    return sb.ToString();
                }
            }
        }

        public static string GetSha256(string filename)
        {
            using (var stream = File.OpenRead(filename))
            {
                using (var sha = new SHA256Managed())
                {
                    var hash = sha.ComputeHash(stream);
                    var sb = new StringBuilder(hash.Length * 2);

                    foreach (byte b in hash)
                    {
                        // can be "x2" if you want lowercase
                        sb.Append(b.ToString("x2"));
                    }

                    return sb.ToString();
                }
            }
        }
    }
}