using System;
using Endless.Shared.DataTypes;
using Unity.Netcode;

namespace Endless.Gameplay
{
	// Token: 0x0200006C RID: 108
	[Serializable]
	public class AssetLibraryReferenceClass : InspectorReference, INetworkSerializable, IEquatable<AssetLibraryReferenceClass>
	{
		// Token: 0x060001AE RID: 430 RVA: 0x0000A6BC File Offset: 0x000088BC
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue<SerializableGuid>(ref this.Id, default(FastBufferWriter.ForNetworkSerializable));
		}

		// Token: 0x060001AF RID: 431 RVA: 0x0000A6DF File Offset: 0x000088DF
		public override int GetHashCode()
		{
			return HashCode.Combine<SerializableGuid>(this.Id);
		}

		// Token: 0x060001B0 RID: 432 RVA: 0x0000A6EC File Offset: 0x000088EC
		public static bool operator ==(AssetLibraryReferenceClass a, AssetLibraryReferenceClass b)
		{
			return a == b || (a != null && b != null && a.Equals(b));
		}

		// Token: 0x060001B1 RID: 433 RVA: 0x0000A705 File Offset: 0x00008905
		public static bool operator !=(AssetLibraryReferenceClass a, AssetLibraryReferenceClass b)
		{
			return !(a == b);
		}

		// Token: 0x060001B2 RID: 434 RVA: 0x0000A714 File Offset: 0x00008914
		public override bool Equals(object obj)
		{
			AssetLibraryReferenceClass assetLibraryReferenceClass = obj as AssetLibraryReferenceClass;
			return assetLibraryReferenceClass != null && this.Id == assetLibraryReferenceClass.Id;
		}

		// Token: 0x060001B3 RID: 435 RVA: 0x0000A73E File Offset: 0x0000893E
		public bool Equals(AssetLibraryReferenceClass other)
		{
			return this.Id == other.Id;
		}
	}
}
