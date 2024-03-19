using ClassLibraryEncryptionTool;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net.Mail;
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

        private void BtnCreateKeyAndIv_Click(object sender, RoutedEventArgs e)
        {
            Aes aes =  AesEncryption.CreateAes();
            string keyBase64 = AesEncryption.AesToBase64(aes.Key);
            string ivBase64 = AesEncryption.AesToBase64(aes.IV);
            SaveFileDialog sfd = new SaveFileDialog()
            {
                Filter = "Aes key (*.txt)|*.txt"
            };
            while (true)
            {
                if (sfd.ShowDialog() == true)
                {
                    File.WriteAllText(sfd.FileName, keyBase64);
                    MessageBox.Show("Key opgeslaan");
                    break;
                }
                else
                {
                    MessageBox.Show("Je moet het key opslaan");
                }
            }
            sfd = new SaveFileDialog()
            {
                Filter = "Aes IV (*.txt)|*.txt"
            };
            while (true)
            {
                if (sfd.ShowDialog() == true)
                {
                    File.WriteAllText(sfd.FileName, ivBase64);
                    MessageBox.Show("Iv opgeslaan");
                    break;
                }
                else
                {
                    MessageBox.Show("Je moet het iv opslaan");
                }
            }
        }

        private void BtnSelectKey_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "Selecteer een Aes Key (*.txt)|*.txt"
            };
            if(ofd.ShowDialog() == true)
            {
                LblKey.Content = ofd.FileName;
            }
            else
            {
                MessageBox.Show("Geen key gekozen");
            }
        }

        private void BtnSelectIv_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "Selecteer een Aes Iv (*.txt)|*.txt"
            };
            if (ofd.ShowDialog() == true)
            {
                LblIv.Content = ofd.FileName;
            }
            else
            {
                MessageBox.Show("Geen Iv gekozen");
            }
        }

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "Images (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png"
            };
            if(ofd.ShowDialog() == true)
            {
                LblImagePath.Content = ofd.FileName;
            }

        }
        public byte[] ImageToByteArray(BitmapImage image)
        {
            if (image == null || image.UriSource == null)
            {
                MessageBox.Show("Image is null / Geen source");
                return null;
            }
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
        public BitmapImage ByteArrayToImage(byte[] imageData)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using(MemoryStream ms = new MemoryStream(imageData))
            {
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }
        private void BtnEncrypt_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(LblImagePath.Content.ToString()))
            {
                MessageBox.Show("Image Bestaat niet");
                return;
            }
            if (!File.Exists(LblKey.Content.ToString()))
            {
                MessageBox.Show("Key bestaat niet");
                return;
            }
            if (!File.Exists(LblIv.Content.ToString()))
            {
                MessageBox.Show("Iv bestaat niet");
                return;
            }
            string imagePath = LblImagePath.Content.ToString();
            BitmapImage bitmapImage = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
            byte[] imageData = ImageToByteArray(bitmapImage);
            string key64 = File.ReadAllText(LblKey.Content.ToString());
            string iv64 = File.ReadAllText(LblIv.Content.ToString());

            byte[] encryptedData = AesEncryption.EncrypteByte(imageData, key64, iv64);
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Encrypted image data (*.txt)|*.txt"
            };

            while (true)
            {
                if (sfd.ShowDialog() == true)
                {
                    string encryptedData64 = Convert.ToBase64String(encryptedData);
                    File.WriteAllText(sfd.FileName, encryptedData64);
                    MessageBox.Show("nice");
                    break;
                }
            }
        }

        private void BtnDecrypt_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(LblKey.Content.ToString()))
            {
                MessageBox.Show("Key bestaat niet");
                return;
            }
            if (!File.Exists(LblIv.Content.ToString()))
            {
                MessageBox.Show("Iv bestaat niet");
                return;
            }
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "Selecteer de encrypted imageDate (*.txt)|*.txt"
            };
            string image64Path = "";
            if(ofd.ShowDialog() == true)
            {
                image64Path = ofd.FileName;
            }
            string image64 = File.ReadAllText(image64Path);
            string keyPath = File.ReadAllText(LblKey.Content.ToString());
            string ivPath = File.ReadAllText(LblIv.Content.ToString());
            byte[] imageData = AesEncryption.DecryptData(image64, keyPath, ivPath);

            BitmapImage image = ByteArrayToImage(imageData);
            ImgDecrypted.Source = image;


        }
    }
}
