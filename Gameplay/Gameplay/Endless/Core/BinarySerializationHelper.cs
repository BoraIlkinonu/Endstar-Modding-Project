using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ELM.Endless.EndlessFramework.Serialization;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Core
{
	// Token: 0x0200003F RID: 63
	public static class BinarySerializationHelper
	{
		// Token: 0x06000110 RID: 272 RVA: 0x000059C4 File Offset: 0x00003BC4
		public static object ByteArrayToObject(byte[] byteArray)
		{
			if (byteArray == null || byteArray.Length == 0)
			{
				return null;
			}
			MemoryStream memoryStream = new MemoryStream();
			memoryStream.Write(byteArray, 0, byteArray.Length);
			memoryStream.Seek(0L, SeekOrigin.Begin);
			object obj = null;
			BinarySerializationHelper.Deserialize(memoryStream, out obj);
			return obj;
		}

		// Token: 0x06000111 RID: 273 RVA: 0x00005A00 File Offset: 0x00003C00
		public static byte[] GetBytes(object dataObject)
		{
			MemoryStream memoryStream = new MemoryStream();
			if (BinarySerializationHelper.Serialize(memoryStream, dataObject))
			{
				return memoryStream.ToArray();
			}
			return null;
		}

		// Token: 0x06000112 RID: 274 RVA: 0x00005A24 File Offset: 0x00003C24
		private static bool Serialize(Stream stream, object obj)
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter(new EndlessSurrogateSelector(), new StreamingContext(StreamingContextStates.All));
			binaryFormatter.Binder = new EndlessSerializationBinder();
			try
			{
				binaryFormatter.Serialize(stream, obj);
			}
			catch (SerializationException ex)
			{
				Debug.LogException(ex);
				return false;
			}
			finally
			{
				stream.Close();
			}
			return true;
		}

		// Token: 0x06000113 RID: 275 RVA: 0x00005A8C File Offset: 0x00003C8C
		private static bool Deserialize(Stream stream, out object deserializedObject)
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter(new EndlessSurrogateSelector(), new StreamingContext(StreamingContextStates.All));
			binaryFormatter.Binder = new EndlessSerializationBinder();
			try
			{
				deserializedObject = binaryFormatter.Deserialize(stream);
			}
			catch (SerializationException ex)
			{
				Debug.LogError("Failed to deserialize.\n" + ex.ToString());
				deserializedObject = null;
			}
			stream.Close();
			return deserializedObject != null;
		}
	}
}
