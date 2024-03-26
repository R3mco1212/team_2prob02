using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryEncryptionTool
{
    public static class AesEncryptionTool
    {

        public static Aes CreateAes()
        {
            return Aes.Create();
        }
        public static string AesToBase64(byte[] data)
        {
            return Convert.ToBase64String(data);
        }
        public static byte[] Base64ToByte(string base64)
        {
            byte[] aesKey = Convert.FromBase64String(base64);
            return aesKey;
        }

        public static byte[] EncrypteByte(byte[] imagedata, string key64, string iv64)
        {
            byte[] aesKey = Base64ToByte(key64);
            byte[] aesIv = Base64ToByte(iv64);
            using(Aes aes = Aes.Create())
            {
                aes.Key = aesKey;
                aes.IV = aesIv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using(MemoryStream msEncrypt = new MemoryStream())
                {
                    using(CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(imagedata, 0, imagedata.Length);
                        csEncrypt.FlushFinalBlock();
                        return msEncrypt.ToArray();
                    }
                }
            }
        }
        public static byte[] DecryptData(string image64, string key64, string iv64)
        {
            byte[] imageData = Base64ToByte(image64);
            byte[] aesKey = Base64ToByte(key64);
            byte[] aesIv = Base64ToByte(iv64);

            using(Aes aes = Aes.Create())
            {
                aes.Key = aesKey;
                aes.IV = aesIv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key,aes.IV);

                using(MemoryStream msDecrypt = new MemoryStream(imageData))
                {
                    using(CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (MemoryStream msPlain = new MemoryStream())
                        {
                            csDecrypt.CopyTo(msPlain);
                            return msPlain.ToArray();
                        }
                    }
                }
            }
        }
    }
}
