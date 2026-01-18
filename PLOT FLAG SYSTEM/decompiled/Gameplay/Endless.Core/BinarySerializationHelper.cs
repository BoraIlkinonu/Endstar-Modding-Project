using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ELM.Endless.EndlessFramework.Serialization;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Core;

public static class BinarySerializationHelper
{
	public static object ByteArrayToObject(byte[] byteArray)
	{
		if (byteArray == null || byteArray.Length == 0)
		{
			return null;
		}
		MemoryStream memoryStream = new MemoryStream();
		memoryStream.Write(byteArray, 0, byteArray.Length);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		object deserializedObject = null;
		Deserialize(memoryStream, out deserializedObject);
		return deserializedObject;
	}

	public static byte[] GetBytes(object dataObject)
	{
		MemoryStream memoryStream = new MemoryStream();
		if (Serialize(memoryStream, dataObject))
		{
			return memoryStream.ToArray();
		}
		return null;
	}

	private static bool Serialize(Stream stream, object obj)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter(new EndlessSurrogateSelector(), new StreamingContext(StreamingContextStates.All));
		binaryFormatter.Binder = new EndlessSerializationBinder();
		try
		{
			binaryFormatter.Serialize(stream, obj);
		}
		catch (SerializationException exception)
		{
			Debug.LogException(exception);
			return false;
		}
		finally
		{
			stream.Close();
		}
		return true;
	}

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
