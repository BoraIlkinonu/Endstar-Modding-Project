using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Endless.Shared
{
	// Token: 0x0200004E RID: 78
	public static class EndlessEncryption
	{
		// Token: 0x06000295 RID: 661 RVA: 0x0000D444 File Offset: 0x0000B644
		public static string Encrypt(string text)
		{
			Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(EndlessEncryption.Key, EndlessEncryption.salt);
			Aes aes = Aes.Create();
			aes.Key = rfc2898DeriveBytes.GetBytes(aes.KeySize / 8);
			ICryptoTransform cryptoTransform = aes.CreateEncryptor(aes.Key, aes.IV);
			string text2;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				memoryStream.Write(BitConverter.GetBytes(aes.IV.Length), 0, 4);
				memoryStream.Write(aes.IV, 0, aes.IV.Length);
				using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
				{
					using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
					{
						streamWriter.Write(text);
					}
				}
				text2 = Convert.ToBase64String(memoryStream.ToArray());
			}
			return text2;
		}

		// Token: 0x06000296 RID: 662 RVA: 0x0000D538 File Offset: 0x0000B738
		public static string Decrypt(string text)
		{
			byte[] array = Convert.FromBase64String(text);
			Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(EndlessEncryption.Key, EndlessEncryption.salt);
			string text2;
			using (MemoryStream memoryStream = new MemoryStream(array))
			{
				Aes aes = Aes.Create();
				aes.Key = rfc2898DeriveBytes.GetBytes(aes.KeySize / 8);
				aes.IV = EndlessEncryption.ReadByteArray(memoryStream);
				ICryptoTransform cryptoTransform = aes.CreateDecryptor(aes.Key, aes.IV);
				using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read))
				{
					using (StreamReader streamReader = new StreamReader(cryptoStream))
					{
						text2 = streamReader.ReadToEnd();
					}
				}
			}
			return text2;
		}

		// Token: 0x06000297 RID: 663 RVA: 0x0000D604 File Offset: 0x0000B804
		private static byte[] ReadByteArray(Stream s)
		{
			byte[] array = new byte[4];
			if (s.Read(array, 0, array.Length) != array.Length)
			{
				throw new SystemException("Stream did not contain properly formatted byte array");
			}
			byte[] array2 = new byte[BitConverter.ToInt32(array, 0)];
			if (s.Read(array2, 0, array2.Length) != array2.Length)
			{
				throw new SystemException("Did not read byte array properly");
			}
			return array2;
		}

		// Token: 0x04000150 RID: 336
		private static string Key = "dwqqe2231ffe32";

		// Token: 0x04000151 RID: 337
		private static byte[] salt = Encoding.ASCII.GetBytes("tq4vdeji340tcvu2");
	}
}
