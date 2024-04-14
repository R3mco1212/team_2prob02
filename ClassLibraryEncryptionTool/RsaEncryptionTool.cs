using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ClassLibraryEncryptionTool
{
	public class RsaEncryptionTool
	{
		private RSAParameters _privateKey;
		private RSAParameters _publicKey;

		public RsaEncryptionTool()
		{
			using (var csp = new RSACryptoServiceProvider(2048))
			{
				_privateKey = csp.ExportParameters(true);
				_publicKey = csp.ExportParameters(false);
			}
		}

		public bool SaveKeys(string folderPath, string keyName)
		{

			string publicKeyPath = System.IO.Path.Combine(folderPath, "publickey - " + keyName + ".xml");
			string privateKeyPath = System.IO.Path.Combine(folderPath, "privatekey - " + keyName + ".xml");

			if (File.Exists(publicKeyPath) || File.Exists(privateKeyPath))
			{
				return false;
			}

			using (var csp = new RSACryptoServiceProvider())
			{
				csp.ImportParameters(_publicKey);
				string publicKeyXml = csp.ToXmlString(false);
				System.IO.File.WriteAllText(publicKeyPath, publicKeyXml);
			}

			using (var csp = new RSACryptoServiceProvider())
			{
				csp.ImportParameters(_privateKey);
				string privateKeyXml = csp.ToXmlString(true);
				System.IO.File.WriteAllText(privateKeyPath, privateKeyXml);
			}
			return true;
		}

		public string Encrypt(string plainText, string publicKey)
		{
			using (var csp = new RSACryptoServiceProvider())
			{
				csp.FromXmlString(publicKey);
				var dataBytes = Convert.FromBase64String(plainText);
				var cipherData = csp.Encrypt(dataBytes, true);

				return Convert.ToBase64String(cipherData);
			}
		}

		public string Decrypt(string cipherText, string privateKey)
		{
			using (var csp = new RSACryptoServiceProvider())
			{
				try
				{
					csp.FromXmlString(privateKey);
					var dataBytes = Convert.FromBase64String(cipherText);
					var plainData = csp.Decrypt(dataBytes, true);
					return Convert.ToBase64String(plainData);
				}
				catch (CryptographicException)
				{
					return null;
				}
			}
		}

		public static string GenerateHashRSA(byte[] data)
		{
			using (var sha256 = SHA256.Create())
			{
				byte[] hashData = sha256.ComputeHash(data);
				return BitConverter.ToString(hashData).Replace("-", "").ToLowerInvariant();
			}
		}
	}
}
