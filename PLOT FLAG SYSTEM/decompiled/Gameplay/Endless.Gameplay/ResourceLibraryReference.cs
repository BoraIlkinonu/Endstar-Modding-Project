using System;
using Endless.Shared.DataTypes;

namespace Endless.Gameplay;

[Serializable]
public class ResourceLibraryReference : PropLibraryReference
{
	internal override ReferenceFilter Filter => ReferenceFilter.NonStatic | ReferenceFilter.Resource;

	public ResourceLibraryReference(SerializableGuid assetId)
	{
		Id = assetId;
	}

	public ResourceLibraryReference()
	{
		Id = SerializableGuid.Empty;
		CosmeticId = SerializableGuid.Empty;
	}
}
