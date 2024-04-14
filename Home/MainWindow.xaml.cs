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
using System.Security.Cryptography.X509Certificates;
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
using System.Xml.Linq;

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

		#region AES encryptie

		private void BtnCreateKeyAndIv_Click(object sender, RoutedEventArgs e)
        {
            Aes aes =  AesEncryptionTool.CreateAes();
            string keyBase64 = AesEncryptionTool.AesToBase64(aes.Key);
            string ivBase64 = AesEncryptionTool.AesToBase64(aes.IV);
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
				Filter = "Selecteer een Aes Key (*.txt)|*.txt",
				InitialDirectory = Properties.Settings.Default.DefaultFolder,
			};
			if (ofd.ShowDialog() == true)
			{
				LblKey.Content = ofd.FileName;
			}
			else
			{
				MessageBox.Show("Geen key gekozen");
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

		private void BtnSelectIv_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog()
			{
				Filter = "Selecteer een Aes Iv (*.txt)|*.txt",
				InitialDirectory = Properties.Settings.Default.DefaultFolder,
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

			byte[] encryptedData = AesEncryptionTool.EncrypteByte(imageData, key64, iv64);
			SaveFileDialog sfd = new SaveFileDialog
			{
				Filter = "Encrypted image data (*.txt)|*.txt",
				InitialDirectory = Properties.Settings.Default.DefaultFolder,
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
			if (ofd.ShowDialog() == true)
			{
				image64Path = ofd.FileName;
			}
			string image64 = File.ReadAllText(image64Path);
			string keyPath = File.ReadAllText(LblKey.Content.ToString());
			string ivPath = File.ReadAllText(LblIv.Content.ToString());
			byte[] imageData = AesEncryptionTool.DecryptData(image64, keyPath, ivPath);

			BitmapImage image = ByteArrayToImage(imageData);
			ImgDecrypted.Source = image;


		}


		private void MnuDefault_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Properties.Settings.Default.DefaultFolder;
            ofd.FileName = "Select Folder";
            ofd.CheckFileExists = false;
            ofd.CheckPathExists = true;
            ofd.ValidateNames = false;
            ofd.FileName = "Folder Selection.";
            ofd.Filter = "Folders|*.";

            if (ofd.ShowDialog() == true)
            {
                Properties.Settings.Default.DefaultFolder = System.IO.Path.GetDirectoryName(ofd.FileName);
                Properties.Settings.Default.Save();
            }
        }

		#endregion

		#region RSA encryptie

		RsaEncryptionTool ret = new RsaEncryptionTool();

		private void BtnGenerateRSAKey_Click(object sender, RoutedEventArgs e)
		{
			string folderName = Properties.Settings.Default.DefaultFolder;

			string keyName = TbNaamKey.Text;

			if (!string.IsNullOrEmpty(keyName))
			{
				bool keySaved = ret.SaveKeys(folderName, keyName);
				if (keySaved == true)
				{
					MessageBox.Show("RSA sleutels gegenereerd en opgeslagen in de standaard map");
				}
				else
				{
					MessageBox.Show("er bestaan al sleutels met deze naam kies een andere naam aub");
				}
			}
			else
			{
				MessageBox.Show("gelieve eerst een naam te kiezen voor de sleutels");
			}
		}

		private void MnuEncryptieDecryptieFolder_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new System.Windows.Forms.FolderBrowserDialog();
			System.Windows.Forms.DialogResult result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				Properties.Settings.Default.DefaultResultFolder = dialog.SelectedPath;
				Properties.Settings.Default.Save();
			}
		}

		private void BtnLaadPublicKey_Click(object sender, RoutedEventArgs e)
		{
			LbSleutelList.ItemsSource = null;
			LbSleutelList.IsEnabled = true;
			BtnDecrypteer.IsEnabled = false;
			BtnKiesBestand.IsEnabled = true;
			BtnSelecteerSleutel.IsEnabled = true;
			TbNaamCipher.IsEnabled = true;
			TbNaamPlain.IsEnabled = false;
			LblBestand.Content = "";
			filePath = null;
			LbCipherText.Foreground = new SolidColorBrush(Colors.LimeGreen);
			LbPlainText.Foreground = new SolidColorBrush(Colors.Red);
			string[] files = Directory.GetFiles(Properties.Settings.Default.DefaultFolder, "publickey - *.xml");

			List<string> keyNames = new List<string>();

			foreach (string file in files)
			{
				keyNames.Add(System.IO.Path.GetFileNameWithoutExtension(file));
			}

			LbSleutelList.ItemsSource = keyNames;

		}

		private void BtnLaadPrivateKey_Click(object sender, RoutedEventArgs e)
		{
			LbSleutelList.ItemsSource = null;
			LbSleutelList.IsEnabled = true;
			BtnEncrypteer.IsEnabled = false;
			BtnDecrypteer.IsEnabled = false;
			BtnKiesBestand.IsEnabled = false;
			BtnSelecteerSleutel.IsEnabled = true;
			TbNaamCipher.IsEnabled = false;
			TbNaamPlain.IsEnabled = true;
			filePath = null;
			LblBestand.Content = "";
			LbPlainText.Foreground = new SolidColorBrush(Colors.LimeGreen);
			LbCipherText.Foreground = new SolidColorBrush(Colors.Red);
			string[] files = Directory.GetFiles(Properties.Settings.Default.DefaultFolder, "privatekey - *.xml");

			List<string> keyNames = new List<string>();

			foreach (string file in files)
			{
				keyNames.Add(System.IO.Path.GetFileNameWithoutExtension(file));
			}

			LbSleutelList.ItemsSource = keyNames;

		}

		private string filePath;

		private void CheckButtonEncrypt()
		{
			if (!string.IsNullOrEmpty(filePath) && !string.IsNullOrEmpty(selectedKeyFilePath))
			{
				BtnEncrypteer.IsEnabled = true;
			}
		}
		private void CheckButtonDecrypt()
		{
			if (!string.IsNullOrEmpty(selectedKeyFilePath) && !BtnKiesBestand.IsEnabled)
			{
				BtnDecrypteer.IsEnabled = true;
			}
		}

		private string selectedKeyFilePath;
		private void BtnSelecteerSleutel_Click(object sender, RoutedEventArgs e)
		{
			string selectedKey = LbSleutelList.SelectedItem.ToString();

			selectedKeyFilePath = System.IO.Path.Combine(Properties.Settings.Default.DefaultFolder, selectedKey + ".xml");
			CheckButtonEncrypt();
			CheckButtonDecrypt();
		}

		private void BtnKiesBestand_Click(object sender, RoutedEventArgs e)
		{
			var openFileDialog = new OpenFileDialog();
			openFileDialog.InitialDirectory = Properties.Settings.Default.DefaultFolder;
			if (openFileDialog.ShowDialog() == true)
			{
				filePath = openFileDialog.FileName;
				LblBestand.Content = filePath;
				CheckButtonEncrypt();
			}
		}

		private void BtnEncrypteer_Click(object sender, RoutedEventArgs e)
		{

			TbNaamPlain.IsEnabled = false;
			BtnKiesBestand.IsEnabled = false;
			string filePathText = LblBestand.Content.ToString();
			string publicKey = File.ReadAllText(selectedKeyFilePath);
			string plainText = File.ReadAllText(filePathText);
			string resultNameC = TbNaamCipher.Text;
			if (!string.IsNullOrEmpty(resultNameC))
			{
				string resultFilePath = System.IO.Path.Combine(Properties.Settings.Default.DefaultResultFolder, "Encrypted - " + resultNameC + ".txt");
				if (!File.Exists(resultFilePath))
				{
					if (!string.IsNullOrEmpty(plainText) || !string.IsNullOrEmpty(publicKey))
					{
						var output = ret.Encrypt(plainText, publicKey);
						File.WriteAllText(resultFilePath, output);
						MessageBox.Show($"encryptie process is voltooid het versleutelde bestand bevindt zich in {resultFilePath}");
						LbSleutelList.IsEnabled = false;
						LbSleutelList.ItemsSource = null;
						BtnSelecteerSleutel.IsEnabled = false;
						BtnKiesBestand.IsEnabled = false;
						BtnEncrypteer.IsEnabled = false;
						filePath = null;
						LblBestand.Content = "";
						TbNaamCipher.Text = null;
						LbCipherText.Foreground = new SolidColorBrush(Colors.Red);
					}
				}
				else
				{
					MessageBox.Show("er bestaat al een bestand met deze naam kies iets anders");
				}
			}
			else
			{
				MessageBox.Show("vergeet geen naam te geven voor het bestand wat de encryptie bevat");
				LbCipherText.Foreground = new SolidColorBrush(Colors.LimeGreen);
			}
		}

		private void BtnDecrypteer_Click(object sender, RoutedEventArgs e)
		{
			string privateKey = File.ReadAllText(selectedKeyFilePath);
			var ofd = new OpenFileDialog();
			ofd.InitialDirectory = Properties.Settings.Default.DefaultResultFolder;
			if (ofd.ShowDialog() == true)
			{
				string file = ofd.FileName;
				string cipherText = File.ReadAllText(file);
				string resultNameP = TbNaamPlain.Text;
				if (!string.IsNullOrEmpty(resultNameP))
				{
					string resultFilePath = System.IO.Path.Combine(Properties.Settings.Default.DefaultResultFolder, "Decrypted - " + resultNameP + ".txt");
					if (!File.Exists(resultFilePath))
					{
						if (!string.IsNullOrEmpty(cipherText) || !string.IsNullOrEmpty(privateKey))
						{
							var output = ret.Decrypt(cipherText, privateKey);
							if (output != null)
							{
								File.WriteAllText(resultFilePath, output);
								MessageBox.Show($"encryptie process is voltooid het versleutelde bestand bevindt zich in {resultFilePath}");
								LbPlainText.Foreground = new SolidColorBrush(Colors.Red);
								LbSleutelList.IsEnabled = false;
								LbSleutelList.ItemsSource = null;
								BtnDecrypteer.IsEnabled = false;
								TbNaamPlain.Text = null;
								BtnSelecteerSleutel.IsEnabled = false;
							}
							else
							{
								MessageBox.Show("de gekozen private key is onjuist kies een andere private key");
								LbSleutelList.IsEnabled = false;
							}
						}
					}
					else
					{
						MessageBox.Show("er bestaat al een bestand met deze naam kies iets anders");
					}
				}
				else
				{
					MessageBox.Show("vergeet geen naam te geven voor het gedecrypteerde bestand");
				}
			}

		}

		#endregion

	}
}
