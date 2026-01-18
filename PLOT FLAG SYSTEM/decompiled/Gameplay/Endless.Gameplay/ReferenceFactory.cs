using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay;

public static class ReferenceFactory
{
	public static AudioReference CreateAudioReference(SerializableGuid assetId)
	{
		AudioReference audioReference = new AudioReference();
		InspectorReferenceUtility.SetId(audioReference, assetId);
		return audioReference;
	}

	public static CharacterVisualsReference CreateCharacterVisualsReference(SerializableGuid assetId)
	{
		CharacterVisualsReference characterVisualsReference = new CharacterVisualsReference();
		InspectorReferenceUtility.SetId(characterVisualsReference, assetId);
		return characterVisualsReference;
	}

	public static AssetLibraryReferenceClass CreateAssetLibraryReferenceClass(SerializableGuid assetId)
	{
		AssetLibraryReferenceClass assetLibraryReferenceClass = new AssetLibraryReferenceClass();
		InspectorReferenceUtility.SetId(assetLibraryReferenceClass, assetId);
		return assetLibraryReferenceClass;
	}

	public static CellReference CreateCellReference(Vector3Int? cell = null, float? rotation = null)
	{
		CellReference cellReference = new CellReference();
		cellReference.SetCell(cell, rotation);
		return cellReference;
	}

	public static InstanceReference CreateInstanceReference(SerializableGuid instanceId, bool useContext)
	{
		return new InstanceReference(instanceId, useContext);
	}

	public static NpcInstanceReference CreateNpcInstanceReference(SerializableGuid instanceId, bool useContext)
	{
		return new NpcInstanceReference(instanceId, useContext);
	}

	public static InventoryLibraryReference CreateInventoryLibraryReference(SerializableGuid assetId)
	{
		InventoryLibraryReference inventoryLibraryReference = new InventoryLibraryReference();
		InspectorReferenceUtility.SetId(inventoryLibraryReference, assetId);
		return inventoryLibraryReference;
	}

	public static KeyLibraryReference CreateKeyLibraryReference(SerializableGuid assetId)
	{
		KeyLibraryReference keyLibraryReference = new KeyLibraryReference();
		InspectorReferenceUtility.SetId(keyLibraryReference, assetId);
		return keyLibraryReference;
	}

	public static TradeInfo.InventoryAndQuantityReference CreateInventoryAndQuantityReference(SerializableGuid assetId)
	{
		TradeInfo.InventoryAndQuantityReference obj = new TradeInfo.InventoryAndQuantityReference
		{
			Quantity = 1
		};
		InspectorReferenceUtility.SetId(obj, assetId);
		return obj;
	}

	public static PlayerReference CreatePlayerReference(bool useContext = true, int playerNumber = 0)
	{
		return new PlayerReference
		{
			useContext = useContext,
			playerNumber = playerNumber
		};
	}

	public static PhysicsObjectLibraryReference CreatePhysicsObjectLibraryReference(SerializableGuid assetId)
	{
		PhysicsObjectLibraryReference physicsObjectLibraryReference = new PhysicsObjectLibraryReference();
		InspectorReferenceUtility.SetId(physicsObjectLibraryReference, assetId);
		return physicsObjectLibraryReference;
	}

	public static ResourceLibraryReference CreateResourceLibraryReference(SerializableGuid assetId)
	{
		ResourceLibraryReference resourceLibraryReference = new ResourceLibraryReference();
		InspectorReferenceUtility.SetId(resourceLibraryReference, assetId);
		return resourceLibraryReference;
	}

	public static PropLibraryReference CreatePropLibraryReference(SerializableGuid assetId)
	{
		PropLibraryReference propLibraryReference = new PropLibraryReference();
		InspectorReferenceUtility.SetId(propLibraryReference, assetId);
		return propLibraryReference;
	}
}
