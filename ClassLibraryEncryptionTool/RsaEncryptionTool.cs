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

				var keyBytes = Convert.FromBase64String(plainText);
				var cypherData = csp.Encrypt(keyBytes, false);

				return Convert.ToBase64String(cypherData);
			}
		}



		public string Decrypt(string cipherText, string PrivateKey)
		{
			using (var csp = new RSACryptoServiceProvider())
			{
				try
				{
					csp.FromXmlString(PrivateKey);
					var dataBytes = Convert.FromBase64String(cipherText);
					var cipherData = csp.Decrypt(dataBytes, false);
					return Convert.ToBase64String(cipherData);
				}
				catch (CryptographicException)
				{
					return null;
				}
			}
		}


	}
}
