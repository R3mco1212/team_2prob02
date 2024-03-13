using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Home
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public static byte[] ImageToByteArray(BitmapImage image)
        {
            byte[] imageData;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                imageData = ms.ToArray();
            }
            return imageData;
        }
        public static BitmapImage ByteArrayToImage(byte[] imageData)
        {
            BitmapImage bitmap = new BitmapImage();
            using(MemoryStream ms = new MemoryStream(imageData))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
            }
            return bitmap;
        }
        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage image = new BitmapImage(new Uri("Assets/images.jpeg",UriKind.RelativeOrAbsolute));
            byte[] imagedata = ImageToByteArray(image);
            using (Aes myAes = Aes.Create())
            {
                // key convert to base 64
                string keyBase64 = Convert.ToBase64String(myAes.Key);
                // iv convert to base 64
                byte[] encryptedImage = EncryptBytes(imagedata, myAes.Key, myAes.IV);                
                byte[] decryptedImage = DecryptStringFromBytes(encryptedImage,myAes.Key,myAes.IV);
                BitmapImage decryptedBitmapImage = ByteArrayToImage(decryptedImage);
                imgDecrypted.Source = decryptedBitmapImage;
            }
        }
        static byte[] EncryptBytes(byte[] plainBytes, byte[] key, byte[] iv)
        {
            byte[] encrypted;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using(MemoryStream msEncrypt  = new MemoryStream())
                {
                    using(CryptoStream csEncrypt = new CryptoStream(msEncrypt,encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(plainBytes,0, plainBytes.Length);
                        csEncrypt.FlushFinalBlock();
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return encrypted;
        }
        static byte[] DecryptStringFromBytes(byte[] cipherBytes, byte[] Key, byte[] IV)
        {
            byte[] decrypted;
            using(Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using(MemoryStream msDecrypt =  new MemoryStream(cipherBytes))
                {
                    using(CryptoStream csDecrypt =  new CryptoStream(msDecrypt,decryptor, CryptoStreamMode.Read))
                    {
                        using(MemoryStream msPlain = new MemoryStream())
                        {
                            csDecrypt.CopyTo(msPlain);
                            decrypted = msPlain.ToArray();
                        }
                    }
                }
            }
            return decrypted;
        }
    }
}
