using System;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;

namespace Endless.Gameplay;

[Serializable]
public class PropLibraryReference : InspectorPropReference, INetworkSerializable, IEquatable<PropLibraryReference>
{
	internal SerializableGuid CosmeticId;

	internal override ReferenceFilter Filter => ReferenceFilter.None;

	internal virtual PropLibrary.RuntimePropInfo GetReference()
	{
		try
		{
			return MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(Id);
		}
		catch
		{
			return null;
		}
	}

	public string GetReferenceName()
	{
		PropLibrary.RuntimePropInfo reference = GetReference();
		if (reference == null)
		{
			return "None";
		}
		return reference.PropData.Name;
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref Id, default(FastBufferWriter.ForNetworkSerializable));
		serializer.SerializeValue(ref CosmeticId, default(FastBufferWriter.ForNetworkSerializable));
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Id, CosmeticId);
	}

	public override bool Equals(object obj)
	{
		if (obj is PropLibraryReference propLibraryReference && Id == propLibraryReference.Id)
		{
			return CosmeticId == propLibraryReference.CosmeticId;
		}
		return false;
	}

	public static bool operator ==(PropLibraryReference a, PropLibraryReference b)
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

	public static bool operator !=(PropLibraryReference a, PropLibraryReference b)
	{
		return !(a == b);
	}

	public bool Equals(PropLibraryReference other)
	{
		if (other != null && Id == other.Id)
		{
			return CosmeticId == other.CosmeticId;
		}
		return false;
	}

	public override string ToString()
	{
		return string.Format("{0}, {1}: {2}", base.ToString(), "CosmeticId", CosmeticId);
	}
}
