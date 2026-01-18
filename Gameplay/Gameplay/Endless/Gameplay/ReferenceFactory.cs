using System;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000290 RID: 656
	public static class ReferenceFactory
	{
		// Token: 0x06000E88 RID: 3720 RVA: 0x0004DBAD File Offset: 0x0004BDAD
		public static AudioReference CreateAudioReference(SerializableGuid assetId)
		{
			AudioReference audioReference = new AudioReference();
			InspectorReferenceUtility.SetId(audioReference, assetId);
			return audioReference;
		}

		// Token: 0x06000E89 RID: 3721 RVA: 0x0004DBBB File Offset: 0x0004BDBB
		public static CharacterVisualsReference CreateCharacterVisualsReference(SerializableGuid assetId)
		{
			CharacterVisualsReference characterVisualsReference = new CharacterVisualsReference();
			InspectorReferenceUtility.SetId(characterVisualsReference, assetId);
			return characterVisualsReference;
		}

		// Token: 0x06000E8A RID: 3722 RVA: 0x0004DBC9 File Offset: 0x0004BDC9
		public static AssetLibraryReferenceClass CreateAssetLibraryReferenceClass(SerializableGuid assetId)
		{
			AssetLibraryReferenceClass assetLibraryReferenceClass = new AssetLibraryReferenceClass();
			InspectorReferenceUtility.SetId(assetLibraryReferenceClass, assetId);
			return assetLibraryReferenceClass;
		}

		// Token: 0x06000E8B RID: 3723 RVA: 0x0004DBD8 File Offset: 0x0004BDD8
		public static CellReference CreateCellReference(Vector3Int? cell = null, float? rotation = null)
		{
			CellReference cellReference = new CellReference();
			Vector3Int? vector3Int = cell;
			cellReference.SetCell((vector3Int != null) ? new Vector3?(vector3Int.GetValueOrDefault()) : null, rotation);
			return cellReference;
		}

		// Token: 0x06000E8C RID: 3724 RVA: 0x0004DC18 File Offset: 0x0004BE18
		public static InstanceReference CreateInstanceReference(SerializableGuid instanceId, bool useContext)
		{
			return new InstanceReference(instanceId, useContext);
		}

		// Token: 0x06000E8D RID: 3725 RVA: 0x0004DC21 File Offset: 0x0004BE21
		public static NpcInstanceReference CreateNpcInstanceReference(SerializableGuid instanceId, bool useContext)
		{
			return new NpcInstanceReference(instanceId, useContext);
		}

		// Token: 0x06000E8E RID: 3726 RVA: 0x0004DC2A File Offset: 0x0004BE2A
		public static InventoryLibraryReference CreateInventoryLibraryReference(SerializableGuid assetId)
		{
			InventoryLibraryReference inventoryLibraryReference = new InventoryLibraryReference();
			InspectorReferenceUtility.SetId(inventoryLibraryReference, assetId);
			return inventoryLibraryReference;
		}

		// Token: 0x06000E8F RID: 3727 RVA: 0x0004DC38 File Offset: 0x0004BE38
		public static KeyLibraryReference CreateKeyLibraryReference(SerializableGuid assetId)
		{
			KeyLibraryReference keyLibraryReference = new KeyLibraryReference();
			InspectorReferenceUtility.SetId(keyLibraryReference, assetId);
			return keyLibraryReference;
		}

		// Token: 0x06000E90 RID: 3728 RVA: 0x0004DC46 File Offset: 0x0004BE46
		public static TradeInfo.InventoryAndQuantityReference CreateInventoryAndQuantityReference(SerializableGuid assetId)
		{
			TradeInfo.InventoryAndQuantityReference inventoryAndQuantityReference = new TradeInfo.InventoryAndQuantityReference();
			inventoryAndQuantityReference.Quantity = 1;
			InspectorReferenceUtility.SetId(inventoryAndQuantityReference, assetId);
			return inventoryAndQuantityReference;
		}

		// Token: 0x06000E91 RID: 3729 RVA: 0x0004DC5B File Offset: 0x0004BE5B
		public static PlayerReference CreatePlayerReference(bool useContext = true, int playerNumber = 0)
		{
			return new PlayerReference
			{
				useContext = useContext,
				playerNumber = playerNumber
			};
		}

		// Token: 0x06000E92 RID: 3730 RVA: 0x0004DC70 File Offset: 0x0004BE70
		public static PhysicsObjectLibraryReference CreatePhysicsObjectLibraryReference(SerializableGuid assetId)
		{
			PhysicsObjectLibraryReference physicsObjectLibraryReference = new PhysicsObjectLibraryReference();
			InspectorReferenceUtility.SetId(physicsObjectLibraryReference, assetId);
			return physicsObjectLibraryReference;
		}

		// Token: 0x06000E93 RID: 3731 RVA: 0x0004DC7E File Offset: 0x0004BE7E
		public static ResourceLibraryReference CreateResourceLibraryReference(SerializableGuid assetId)
		{
			ResourceLibraryReference resourceLibraryReference = new ResourceLibraryReference();
			InspectorReferenceUtility.SetId(resourceLibraryReference, assetId);
			return resourceLibraryReference;
		}

		// Token: 0x06000E94 RID: 3732 RVA: 0x0004DC8C File Offset: 0x0004BE8C
		public static PropLibraryReference CreatePropLibraryReference(SerializableGuid assetId)
		{
			PropLibraryReference propLibraryReference = new PropLibraryReference();
			InspectorReferenceUtility.SetId(propLibraryReference, assetId);
			return propLibraryReference;
		}
	}
}
