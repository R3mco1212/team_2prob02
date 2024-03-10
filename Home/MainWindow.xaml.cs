using System;
using System.Collections.Generic;
using System.IO;
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

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            string text = "hey i am test text";
            using (Aes myAes = Aes.Create())
            {
                // key convert to base 64
                string keyBase64 = Convert.ToBase64String(myAes.Key);
                // iv convert to base 64
                byte[] encrypted = EncryptStringToByte(text, myAes.Key, myAes.IV);                
                string decryptedText = DecryptStringFromBytes_Aes(encrypted,myAes.Key,myAes.IV);
                MessageBox.Show(decryptedText);
            }
        }
        static byte[] EncryptStringToByte(string plainText, byte[] key, byte[] iv)
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
                        using(StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return encrypted;
        }
        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            string plainText = null;

            using(Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using(MemoryStream msDecrypt =  new MemoryStream(cipherText))
                {
                    using(CryptoStream csDecrypt =  new CryptoStream(msDecrypt,decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plainText = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plainText;
        }
    }
}
