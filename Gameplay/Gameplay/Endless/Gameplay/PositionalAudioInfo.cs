using System;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000C2 RID: 194
	public struct PositionalAudioInfo : INetworkSerializable
	{
		// Token: 0x060003A7 RID: 935 RVA: 0x00013F50 File Offset: 0x00012150
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue<int>(ref this.ActiveFileId, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<AudioType>(ref this.AudioType, default(FastBufferWriter.ForEnums));
			serializer.SerializeValue<bool>(ref this.Loop, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<float>(ref this.Duration, default(FastBufferWriter.ForPrimitives));
			if (!serializer.IsWriter)
			{
				this.Volume = Compression.DeserializeFloatFromUShort<T>(serializer, 0f, 1f);
				this.FadeTime = Compression.DeserializeFloatDecimal1<T>(serializer);
				this.TimeStamp = Compression.DeserializeFloatDecimal1<T>(serializer);
				this.Pitch = Compression.DeserializeFloatFromUShort<T>(serializer, -3f, 3f);
				this.MinDistance = Compression.DeserializeFloatDecimal1<T>(serializer);
				this.MaxDistance = Compression.DeserializeFloatDecimal1<T>(serializer);
				PositionalAudioInfo.SfxSource sfxSource = PositionalAudioInfo.SfxSource.NetworkId;
				serializer.SerializeValue<PositionalAudioInfo.SfxSource>(ref sfxSource, default(FastBufferWriter.ForEnums));
				switch (sfxSource)
				{
				case PositionalAudioInfo.SfxSource.InstanceId:
					serializer.SerializeValue<SerializableGuid>(ref this.InstanceId, default(FastBufferWriter.ForNetworkSerializable));
					serializer.SerializeValue<SerializableGuid>(ref this.TransformId, default(FastBufferWriter.ForNetworkSerializable));
					return;
				case PositionalAudioInfo.SfxSource.Position:
					this.Position = new Vector3(Compression.DeserializeFloatDecimal1<T>(serializer), Compression.DeserializeFloatDecimal1<T>(serializer), Compression.DeserializeFloatDecimal1<T>(serializer));
					return;
				}
				serializer.SerializeValue<ulong>(ref this.NetworkId, default(FastBufferWriter.ForPrimitives));
				serializer.SerializeValue<SerializableGuid>(ref this.TransformId, default(FastBufferWriter.ForNetworkSerializable));
				return;
			}
			Compression.SerializeFloatToUShort<T>(serializer, this.Volume, 0f, 1f);
			Compression.SerializeFloatDecimal1<T>(serializer, this.FadeTime);
			Compression.SerializeFloatDecimal1<T>(serializer, this.TimeStamp);
			Compression.SerializeFloatToUShort<T>(serializer, this.Pitch, -3f, 3f);
			Compression.SerializeFloatDecimal1<T>(serializer, this.MinDistance);
			Compression.SerializeFloatDecimal1<T>(serializer, this.MaxDistance);
			PositionalAudioInfo.SfxSource sfxSource2;
			if (this.NetworkId != 0UL)
			{
				sfxSource2 = PositionalAudioInfo.SfxSource.NetworkId;
				serializer.SerializeValue<PositionalAudioInfo.SfxSource>(ref sfxSource2, default(FastBufferWriter.ForEnums));
				serializer.SerializeValue<ulong>(ref this.NetworkId, default(FastBufferWriter.ForPrimitives));
				serializer.SerializeValue<SerializableGuid>(ref this.TransformId, default(FastBufferWriter.ForNetworkSerializable));
				return;
			}
			if (!this.InstanceId.IsEmpty)
			{
				sfxSource2 = PositionalAudioInfo.SfxSource.InstanceId;
				serializer.SerializeValue<PositionalAudioInfo.SfxSource>(ref sfxSource2, default(FastBufferWriter.ForEnums));
				serializer.SerializeValue<SerializableGuid>(ref this.InstanceId, default(FastBufferWriter.ForNetworkSerializable));
				serializer.SerializeValue<SerializableGuid>(ref this.TransformId, default(FastBufferWriter.ForNetworkSerializable));
				return;
			}
			sfxSource2 = PositionalAudioInfo.SfxSource.Position;
			serializer.SerializeValue<PositionalAudioInfo.SfxSource>(ref sfxSource2, default(FastBufferWriter.ForEnums));
			Compression.SerializeFloatDecimal1<T>(serializer, this.Position.x);
			Compression.SerializeFloatDecimal1<T>(serializer, this.Position.y);
			Compression.SerializeFloatDecimal1<T>(serializer, this.Position.z);
		}

		// Token: 0x04000356 RID: 854
		private const int MIN_PITCH = -3;

		// Token: 0x04000357 RID: 855
		private const int MAX_PITCH = 3;

		// Token: 0x04000358 RID: 856
		public int ActiveFileId;

		// Token: 0x04000359 RID: 857
		public AudioType AudioType;

		// Token: 0x0400035A RID: 858
		public bool Loop;

		// Token: 0x0400035B RID: 859
		public float Volume;

		// Token: 0x0400035C RID: 860
		public float TimeStamp;

		// Token: 0x0400035D RID: 861
		public float FadeTime;

		// Token: 0x0400035E RID: 862
		public float Duration;

		// Token: 0x0400035F RID: 863
		public float Pitch;

		// Token: 0x04000360 RID: 864
		public float MinDistance;

		// Token: 0x04000361 RID: 865
		public float MaxDistance;

		// Token: 0x04000362 RID: 866
		public Vector3 Position;

		// Token: 0x04000363 RID: 867
		public SerializableGuid InstanceId;

		// Token: 0x04000364 RID: 868
		public ulong NetworkId;

		// Token: 0x04000365 RID: 869
		public SerializableGuid TransformId;

		// Token: 0x020000C3 RID: 195
		private enum SfxSource
		{
			// Token: 0x04000367 RID: 871
			NetworkId,
			// Token: 0x04000368 RID: 872
			InstanceId,
			// Token: 0x04000369 RID: 873
			Position
		}
	}
}
