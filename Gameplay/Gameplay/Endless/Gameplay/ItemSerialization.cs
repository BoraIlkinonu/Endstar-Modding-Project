using System;
using Unity.Netcode;

namespace Endless.Gameplay
{
	// Token: 0x020002B2 RID: 690
	public static class ItemSerialization
	{
		// Token: 0x06000F6B RID: 3947 RVA: 0x000504B0 File Offset: 0x0004E6B0
		public static void WriteValueSafe(this FastBufferWriter writer, in Item item)
		{
			NetworkObjectReference networkObjectReference;
			NetworkObjectReference networkObjectReference2;
			if (!(item == null))
			{
				networkObjectReference = new NetworkObjectReference(item.NetworkObject);
			}
			else
			{
				networkObjectReference2 = default(NetworkObjectReference);
				networkObjectReference = networkObjectReference2;
			}
			networkObjectReference2 = networkObjectReference;
			writer.WriteValueSafe<NetworkObjectReference>(in networkObjectReference2, default(FastBufferWriter.ForNetworkSerializable));
		}

		// Token: 0x06000F6C RID: 3948 RVA: 0x000504F4 File Offset: 0x0004E6F4
		public static void ReadValueSafe(this FastBufferReader reader, out Item item)
		{
			NetworkObjectReference networkObjectReference;
			reader.ReadValueSafe<NetworkObjectReference>(out networkObjectReference, default(FastBufferWriter.ForNetworkSerializable));
			NetworkObject networkObject;
			item = (networkObjectReference.TryGet(out networkObject, null) ? networkObject.GetComponentInChildren<Item>() : null);
		}
	}
}
