using System;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000C1 RID: 193
	public struct LateJoinAudioInfo : INetworkSerializable
	{
		// Token: 0x060003A6 RID: 934 RVA: 0x00013E80 File Offset: 0x00012080
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			if (serializer.IsWriter)
			{
				Compression.SerializeFloatToUShort<T>(serializer, this.Volume, 0f, 1f);
				Compression.SerializeFloatDecimal1<T>(serializer, this.FadeTime);
				Compression.SerializeFloatDecimal1<T>(serializer, this.Duration);
				Compression.SerializeFloatDecimal1<T>(serializer, this.TimeStamp);
			}
			else
			{
				this.Volume = Compression.DeserializeFloatFromUShort<T>(serializer, 0f, 1f);
				this.FadeTime = Compression.DeserializeFloatDecimal1<T>(serializer);
				this.Duration = Compression.DeserializeFloatDecimal1<T>(serializer);
				this.TimeStamp = Compression.DeserializeFloatDecimal1<T>(serializer);
			}
			serializer.SerializeValue<int>(ref this.ActiveFileId, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<bool>(ref this.Loop, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<AudioType>(ref this.AudioType, default(FastBufferWriter.ForEnums));
		}

		// Token: 0x0400034F RID: 847
		public int ActiveFileId;

		// Token: 0x04000350 RID: 848
		public bool Loop;

		// Token: 0x04000351 RID: 849
		public float Volume;

		// Token: 0x04000352 RID: 850
		public float TimeStamp;

		// Token: 0x04000353 RID: 851
		public float FadeTime;

		// Token: 0x04000354 RID: 852
		public float Duration;

		// Token: 0x04000355 RID: 853
		public AudioType AudioType;
	}
}
