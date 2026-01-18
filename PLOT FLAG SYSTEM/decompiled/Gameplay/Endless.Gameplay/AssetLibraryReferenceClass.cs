using System;
using Unity.Netcode;

namespace Endless.Gameplay;

[Serializable]
public class AssetLibraryReferenceClass : InspectorReference, INetworkSerializable, IEquatable<AssetLibraryReferenceClass>
{
	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref Id, default(FastBufferWriter.ForNetworkSerializable));
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Id);
	}

	public static bool operator ==(AssetLibraryReferenceClass a, AssetLibraryReferenceClass b)
	{
		if ((object)a == b)
		{
			return true;
		}
		if ((object)a == null)
		{
			return false;
		}
		if ((object)b == null)
		{
			return false;
		}
		return a.Equals(b);
	}

	public static bool operator !=(AssetLibraryReferenceClass a, AssetLibraryReferenceClass b)
	{
		return !(a == b);
	}

	public override bool Equals(object obj)
	{
		if (obj is AssetLibraryReferenceClass assetLibraryReferenceClass)
		{
			return Id == assetLibraryReferenceClass.Id;
		}
		return false;
	}

	public bool Equals(AssetLibraryReferenceClass other)
	{
		return Id == other.Id;
	}
}
