using System;
using Unity.Netcode;

namespace Endless.Gameplay.LevelEditing
{
	// Token: 0x02000501 RID: 1281
	public struct StringArraySerialized : INetworkSerializable
	{
		// Token: 0x06001F23 RID: 7971 RVA: 0x00089E66 File Offset: 0x00088066
		public static implicit operator string[](StringArraySerialized obj)
		{
			return obj.value;
		}

		// Token: 0x06001F24 RID: 7972 RVA: 0x00089E70 File Offset: 0x00088070
		public static implicit operator StringArraySerialized(string[] objValue)
		{
			return new StringArraySerialized
			{
				value = objValue
			};
		}

		// Token: 0x06001F25 RID: 7973 RVA: 0x00089E90 File Offset: 0x00088090
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			int num = (serializer.IsWriter ? this.value.Length : 0);
			serializer.SerializeValue<int>(ref num, default(FastBufferWriter.ForPrimitives));
			if (serializer.IsReader)
			{
				this.value = new string[num];
			}
			for (int i = 0; i < num; i++)
			{
				serializer.SerializeValue(ref this.value[i], false);
			}
		}

		// Token: 0x04001873 RID: 6259
		public string[] value;
	}
}
