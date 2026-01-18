using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Endless
{
	// Token: 0x02000007 RID: 7
	public static class IOUtils
	{
		// Token: 0x06000015 RID: 21 RVA: 0x00003BB4 File Offset: 0x00001DB4
		public static void SaveObjectToBinaryFile(object data, string filePath)
		{
			bool flag = data == null;
			if (flag)
			{
				Logger.Log(null, "Cannot serialize data, provided null as data", true);
			}
			else
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				try
				{
					using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate))
					{
						binaryFormatter.Serialize(fileStream, data);
					}
				}
				catch (SerializationException ex)
				{
					Logger.Log(null, "Failed to serialize. Reason: " + ex.Message, true);
				}
				catch (Exception ex2)
				{
					Logger.Log(null, "Error while trying to serialize. Reason: " + ex2.Message, true);
				}
			}
		}

		// Token: 0x06000016 RID: 22 RVA: 0x00003C68 File Offset: 0x00001E68
		public static T LoadObjectFromBinaryFile<T>(string filePath)
		{
			T t = default(T);
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			try
			{
				using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate))
				{
					bool flag = fileStream.Length == 0L;
					if (flag)
					{
						return default(T);
					}
					t = (T)((object)binaryFormatter.Deserialize(fileStream));
				}
			}
			catch (SerializationException ex)
			{
				Logger.Log(null, "Failed to deserialize. Reason: " + ex.Message, true);
			}
			return t;
		}
	}
}
