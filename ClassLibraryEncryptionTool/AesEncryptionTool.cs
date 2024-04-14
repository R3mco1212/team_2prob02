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

        public static string GenerateHash(byte[] data)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashData = sha256.ComputeHash(data);
                return BitConverter.ToString(hashData).Replace("-", "").ToLowerInvariant();
            }
        }

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
            try
            {
                return Convert.FromBase64String(base64);

            }
            catch(FormatException ex)
            {
                throw new CryptographicException("Invalid base64 string.", ex);
            }
        }

        public static byte[] EncrypteByte(byte[] imagedata, string key64, string iv64)
        {
            try
            {
                byte[] aesKey = Base64ToByte(key64);
                byte[] aesIv = Base64ToByte(iv64);
                using (Aes aes = Aes.Create())
                {
                    aes.Key = aesKey;
                    aes.IV = aesIv;
                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(imagedata, 0, imagedata.Length);
                            csEncrypt.FlushFinalBlock();
                            return msEncrypt.ToArray();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw new CryptographicException("Encryption failed.", ex);
            }

        }
        public static byte[] DecryptData(string image64, string key64, string iv64)
        {
            try
            {
                byte[] imageData = Base64ToByte(image64);
                byte[] aesKey = Base64ToByte(key64);
                byte[] aesIv = Base64ToByte(iv64);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = aesKey;
                    aes.IV = aesIv;
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(imageData))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
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
            catch(Exception ex)
            {
                throw new CryptographicException("Decryption failed.", ex);
            }
        }
    }
}
