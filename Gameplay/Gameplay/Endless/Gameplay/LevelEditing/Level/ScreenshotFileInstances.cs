using System;
using Endless.Shared.Debugging;
using Newtonsoft.Json;
using Unity.Netcode;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x02000560 RID: 1376
	[Serializable]
	public class ScreenshotFileInstances : INetworkSerializable
	{
		// Token: 0x0600211B RID: 8475 RVA: 0x00094F8C File Offset: 0x0009318C
		public int GetFileInstanceId(ScreenshotTypes screenshotType)
		{
			switch (screenshotType)
			{
			case ScreenshotTypes.Thumbnail:
				return this.Thumbnail;
			case ScreenshotTypes.MainImage:
				return this.MainImage;
			case ScreenshotTypes.OriginalRes:
				return this.OriginalRes;
			default:
				DebugUtility.LogNoEnumSupportError<ScreenshotTypes>(this, "GetFileInstanceId", screenshotType, new object[] { screenshotType });
				return this.Thumbnail;
			}
		}

		// Token: 0x0600211C RID: 8476 RVA: 0x00094FE4 File Offset: 0x000931E4
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue<int>(ref this.Thumbnail, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<int>(ref this.MainImage, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<int>(ref this.OriginalRes, default(FastBufferWriter.ForPrimitives));
		}

		// Token: 0x0600211D RID: 8477 RVA: 0x00095034 File Offset: 0x00093234
		public override string ToString()
		{
			return string.Format("{0}: {1}, {2}: {3}, {4}: {5}", new object[] { "Thumbnail", this.Thumbnail, "MainImage", this.MainImage, "OriginalRes", this.OriginalRes });
		}

		// Token: 0x0600211E RID: 8478 RVA: 0x00095094 File Offset: 0x00093294
		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			ScreenshotFileInstances screenshotFileInstances = obj as ScreenshotFileInstances;
			return screenshotFileInstances != null && (screenshotFileInstances.Thumbnail == this.Thumbnail && screenshotFileInstances.MainImage == this.MainImage) && screenshotFileInstances.OriginalRes == this.OriginalRes;
		}

		// Token: 0x0600211F RID: 8479 RVA: 0x000950DE File Offset: 0x000932DE
		public override int GetHashCode()
		{
			return HashCode.Combine<int, int, int>(this.Thumbnail, this.MainImage, this.OriginalRes);
		}

		// Token: 0x06002120 RID: 8480 RVA: 0x000950F7 File Offset: 0x000932F7
		public ScreenshotFileInstances Copy()
		{
			return new ScreenshotFileInstances
			{
				Thumbnail = this.Thumbnail,
				MainImage = this.MainImage,
				OriginalRes = this.OriginalRes
			};
		}

		// Token: 0x04001A56 RID: 6742
		[JsonProperty("thumbnail")]
		public int Thumbnail;

		// Token: 0x04001A57 RID: 6743
		[JsonProperty("mainImage")]
		public int MainImage;

		// Token: 0x04001A58 RID: 6744
		[JsonProperty("original")]
		public int OriginalRes;
	}
}
