using System;
using Endless.Shared.DataTypes;

namespace Endless.Gameplay;

[Serializable]
public class KeyLibraryReference : InventoryLibraryReference
{
	internal override ReferenceFilter Filter => ReferenceFilter.Key;

	internal KeyLibraryReference()
	{
	}

	internal KeyLibraryReference(SerializableGuid assetId)
		: base(assetId)
	{
	}
}
