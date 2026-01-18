using Unity.Collections;
using Unity.Netcode;

public struct NetworkString64 : INetworkSerializeByMemcpy
{
	private ForceNetworkSerializeByMemcpy<FixedString64Bytes> _info;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref _info, default(FastBufferWriter.ForStructs));
	}

	public override string ToString()
	{
		return _info.Value.ToString();
	}

	public static implicit operator string(NetworkString64 s)
	{
		return s.ToString();
	}

	public static implicit operator NetworkString64(string s)
	{
		return new NetworkString64
		{
			_info = new FixedString64Bytes(s)
		};
	}
}
