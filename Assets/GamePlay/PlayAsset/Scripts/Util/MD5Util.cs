using System.Security.Cryptography;
using System.Text;
using System.IO;
namespace Util
{
    public static class MD5Util
    {
        /// <summary>
        /// 字符串 MD5
        /// </summary>
        public static string GetMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = md5.ComputeHash(bytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                    sb.Append(b.ToString("x2")); // 小写16进制

                return sb.ToString();
            }
        }

        /// <summary>
        /// 文件 MD5
        /// </summary>
        public static string GetFileMD5(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(fs);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                    sb.Append(b.ToString("x2"));

                return sb.ToString();
            }
        }
    }
}