using System;
using Unity.Collections;
using Unity.Netcode;

// Token: 0x02000007 RID: 7
public struct NetworkString4096 : INetworkSerializeByMemcpy
{
	// Token: 0x06000020 RID: 32 RVA: 0x0000285C File Offset: 0x00000A5C
	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue<ForceNetworkSerializeByMemcpy<FixedString4096Bytes>>(ref this._info, default(FastBufferWriter.ForStructs));
	}

	// Token: 0x06000021 RID: 33 RVA: 0x0000287F File Offset: 0x00000A7F
	public override string ToString()
	{
		return this._info.Value.ToString();
	}

	// Token: 0x06000022 RID: 34 RVA: 0x00002897 File Offset: 0x00000A97
	public static implicit operator string(NetworkString4096 s)
	{
		return s.ToString();
	}

	// Token: 0x06000023 RID: 35 RVA: 0x000028A8 File Offset: 0x00000AA8
	public static implicit operator NetworkString4096(string s)
	{
		return new NetworkString4096
		{
			_info = new FixedString4096Bytes(s)
		};
	}

	// Token: 0x04000012 RID: 18
	private ForceNetworkSerializeByMemcpy<FixedString4096Bytes> _info;
}
