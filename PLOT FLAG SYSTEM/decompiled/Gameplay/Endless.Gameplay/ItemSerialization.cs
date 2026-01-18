using Unity.Netcode;

namespace Endless.Gameplay;

public static class ItemSerialization
{
	public static void WriteValueSafe(this FastBufferWriter writer, in Item item)
	{
		writer.WriteValueSafe<NetworkObjectReference>((item == null) ? default(NetworkObjectReference) : new NetworkObjectReference(item.NetworkObject), default(FastBufferWriter.ForNetworkSerializable));
	}

	public static void ReadValueSafe(this FastBufferReader reader, out Item item)
	{
		reader.ReadValueSafe(out NetworkObjectReference value, default(FastBufferWriter.ForNetworkSerializable));
		item = (value.TryGet(out var networkObject) ? networkObject.GetComponentInChildren<Item>() : null);
	}
}
