using Unity.Collections;
using Unity.Netcode;

public struct NetworkString4096 : INetworkSerializeByMemcpy
{
	private ForceNetworkSerializeByMemcpy<FixedString4096Bytes> _info;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref _info, default(FastBufferWriter.ForStructs));
	}

	public override string ToString()
	{
		return _info.Value.ToString();
	}

	public static implicit operator string(NetworkString4096 s)
	{
		return s.ToString();
	}

	public static implicit operator NetworkString4096(string s)
	{
		return new NetworkString4096
		{
			_info = new FixedString4096Bytes(s)
		};
	}
}
