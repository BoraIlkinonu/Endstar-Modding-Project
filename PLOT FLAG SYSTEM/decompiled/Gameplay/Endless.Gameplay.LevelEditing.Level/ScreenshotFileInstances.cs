using System;
using Endless.Shared.Debugging;
using Newtonsoft.Json;
using Unity.Netcode;

namespace Endless.Gameplay.LevelEditing.Level;

[Serializable]
public class ScreenshotFileInstances : INetworkSerializable
{
	[JsonProperty("thumbnail")]
	public int Thumbnail;

	[JsonProperty("mainImage")]
	public int MainImage;

	[JsonProperty("original")]
	public int OriginalRes;

	public int GetFileInstanceId(ScreenshotTypes screenshotType)
	{
		switch (screenshotType)
		{
		case ScreenshotTypes.Thumbnail:
			return Thumbnail;
		case ScreenshotTypes.MainImage:
			return MainImage;
		case ScreenshotTypes.OriginalRes:
			return OriginalRes;
		default:
			DebugUtility.LogNoEnumSupportError(this, "GetFileInstanceId", screenshotType, screenshotType);
			return Thumbnail;
		}
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref Thumbnail, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref MainImage, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref OriginalRes, default(FastBufferWriter.ForPrimitives));
	}

	public override string ToString()
	{
		return string.Format("{0}: {1}, {2}: {3}, {4}: {5}", "Thumbnail", Thumbnail, "MainImage", MainImage, "OriginalRes", OriginalRes);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is ScreenshotFileInstances screenshotFileInstances))
		{
			return false;
		}
		if (screenshotFileInstances.Thumbnail == Thumbnail && screenshotFileInstances.MainImage == MainImage)
		{
			return screenshotFileInstances.OriginalRes == OriginalRes;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Thumbnail, MainImage, OriginalRes);
	}

	public ScreenshotFileInstances Copy()
	{
		return new ScreenshotFileInstances
		{
			Thumbnail = Thumbnail,
			MainImage = MainImage,
			OriginalRes = OriginalRes
		};
	}
}
