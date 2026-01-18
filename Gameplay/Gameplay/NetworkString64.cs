using System;
using Unity.Collections;
using Unity.Netcode;

// Token: 0x02000008 RID: 8
public struct NetworkString64 : INetworkSerializeByMemcpy
{
	// Token: 0x06000024 RID: 36 RVA: 0x000028D0 File Offset: 0x00000AD0
	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue<ForceNetworkSerializeByMemcpy<FixedString64Bytes>>(ref this._info, default(FastBufferWriter.ForStructs));
	}

	// Token: 0x06000025 RID: 37 RVA: 0x000028F3 File Offset: 0x00000AF3
	public override string ToString()
	{
		return this._info.Value.ToString();
	}

	// Token: 0x06000026 RID: 38 RVA: 0x0000290B File Offset: 0x00000B0B
	public static implicit operator string(NetworkString64 s)
	{
		return s.ToString();
	}

	// Token: 0x06000027 RID: 39 RVA: 0x0000291C File Offset: 0x00000B1C
	public static implicit operator NetworkString64(string s)
	{
		return new NetworkString64
		{
			_info = new FixedString64Bytes(s)
		};
	}

	// Token: 0x04000013 RID: 19
	private ForceNetworkSerializeByMemcpy<FixedString64Bytes> _info;
}
