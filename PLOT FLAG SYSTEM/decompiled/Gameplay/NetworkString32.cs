using Unity.Collections;
using Unity.Netcode;

public struct NetworkString32 : INetworkSerializeByMemcpy
{
	private ForceNetworkSerializeByMemcpy<FixedString32Bytes> _info;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref _info, default(FastBufferWriter.ForStructs));
	}

	public override string ToString()
	{
		return _info.Value.ToString();
	}

	public static implicit operator string(NetworkString32 s)
	{
		return s.ToString();
	}

	public static implicit operator NetworkString32(string s)
	{
		return new NetworkString32
		{
			_info = new FixedString32Bytes(s)
		};
	}
}
