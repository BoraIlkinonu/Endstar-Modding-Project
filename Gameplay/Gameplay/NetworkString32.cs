using System;
using Unity.Collections;
using Unity.Netcode;

// Token: 0x02000009 RID: 9
public struct NetworkString32 : INetworkSerializeByMemcpy
{
	// Token: 0x06000028 RID: 40 RVA: 0x00002944 File Offset: 0x00000B44
	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue<ForceNetworkSerializeByMemcpy<FixedString32Bytes>>(ref this._info, default(FastBufferWriter.ForStructs));
	}

	// Token: 0x06000029 RID: 41 RVA: 0x00002967 File Offset: 0x00000B67
	public override string ToString()
	{
		return this._info.Value.ToString();
	}

	// Token: 0x0600002A RID: 42 RVA: 0x0000297F File Offset: 0x00000B7F
	public static implicit operator string(NetworkString32 s)
	{
		return s.ToString();
	}

	// Token: 0x0600002B RID: 43 RVA: 0x00002990 File Offset: 0x00000B90
	public static implicit operator NetworkString32(string s)
	{
		return new NetworkString32
		{
			_info = new FixedString32Bytes(s)
		};
	}

	// Token: 0x04000014 RID: 20
	private ForceNetworkSerializeByMemcpy<FixedString32Bytes> _info;
}
