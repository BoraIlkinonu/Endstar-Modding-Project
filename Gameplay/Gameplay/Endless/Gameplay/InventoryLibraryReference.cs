using System;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000F5 RID: 245
	[Serializable]
	public class InventoryLibraryReference : PropLibraryReference
	{
		// Token: 0x170000E7 RID: 231
		// (get) Token: 0x0600056D RID: 1389 RVA: 0x0001B97C File Offset: 0x00019B7C
		internal override ReferenceFilter Filter
		{
			get
			{
				return ReferenceFilter.InventoryItem;
			}
		}

		// Token: 0x0600056E RID: 1390 RVA: 0x0001B97F File Offset: 0x00019B7F
		internal InventoryLibraryReference()
		{
		}

		// Token: 0x0600056F RID: 1391 RVA: 0x000142B4 File Offset: 0x000124B4
		internal InventoryLibraryReference(SerializableGuid assetId)
		{
			this.Id = assetId;
		}

		// Token: 0x06000570 RID: 1392 RVA: 0x0001B987 File Offset: 0x00019B87
		public void SpawnItem(Context instigator, CellReference destination)
		{
			this.SpawnItem(instigator, destination.GetCellPosition(), destination.GetRotation(), 1, true);
		}

		// Token: 0x06000571 RID: 1393 RVA: 0x0001B99E File Offset: 0x00019B9E
		public void SpawnItem(Context instigator, CellReference destination, int count)
		{
			this.SpawnItem(instigator, destination.GetCellPosition(), destination.GetRotation(), count, true);
		}

		// Token: 0x06000572 RID: 1394 RVA: 0x0001B9B5 File Offset: 0x00019BB5
		public void SpawnItem(Context instigator, CellReference destination, bool launch)
		{
			this.SpawnItem(instigator, destination.GetCellPosition(), destination.GetRotation(), 1, launch);
		}

		// Token: 0x06000573 RID: 1395 RVA: 0x0001B9CC File Offset: 0x00019BCC
		public void SpawnItem(Context instigator, CellReference destination, int count, bool launch)
		{
			this.SpawnItem(instigator, destination.GetCellPosition(), destination.GetRotation(), count, launch);
		}

		// Token: 0x06000574 RID: 1396 RVA: 0x0001B9E4 File Offset: 0x00019BE4
		public void SpawnItem(Context instigator, global::UnityEngine.Vector3 position, float rotation)
		{
			this.SpawnItem(instigator, position, rotation, 1, true);
		}

		// Token: 0x06000575 RID: 1397 RVA: 0x0001B9F1 File Offset: 0x00019BF1
		public void SpawnItem(Context instigator, global::UnityEngine.Vector3 position, float rotation, bool launch)
		{
			this.SpawnItem(instigator, position, rotation, 1, launch);
		}

		// Token: 0x06000576 RID: 1398 RVA: 0x0001B9FF File Offset: 0x00019BFF
		public void SpawnItem(Context instigator, global::UnityEngine.Vector3 position, float rotation, int count)
		{
			this.SpawnItem(instigator, position, rotation, count, true);
		}

		// Token: 0x06000577 RID: 1399 RVA: 0x0001BA10 File Offset: 0x00019C10
		public void SpawnItem(Context instigator, global::UnityEngine.Vector3 position, float rotation, int count, bool launch)
		{
			if (this.Id == SerializableGuid.Empty)
			{
				throw new NullReferenceException("Cannot spawn an item without an empty reference!");
			}
			if (count < 1)
			{
				throw new ArgumentException("Cannot spawn 0 or less of an item!");
			}
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(this.Id);
			BaseTypeDefinition baseTypeDefinition;
			if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(runtimePropInfo.PropData.BaseTypeId, out baseTypeDefinition))
			{
				if (baseTypeDefinition.ComponentBase is ResourcePickup)
				{
					ResourcePickup componentInChildren = global::UnityEngine.Object.Instantiate<GameObject>(runtimePropInfo.EndlessProp.gameObject, position, Quaternion.Euler(global::UnityEngine.Vector3.up * rotation)).GetComponentInChildren<ResourcePickup>();
					componentInChildren.Quantity = count;
					componentInChildren.ShouldSaveAndLoad = false;
					componentInChildren.InitializeNewItem(launch, position, rotation);
					componentInChildren.SetDynamicSpawnQuantity_ClientRpc(componentInChildren.Quantity);
					return;
				}
				Item item = baseTypeDefinition.ComponentBase as Item;
				if (item != null)
				{
					StackableItem stackableItem = item as StackableItem;
					if (stackableItem != null)
					{
						global::UnityEngine.Object.Instantiate<GameObject>(runtimePropInfo.EndlessProp.gameObject, position, Quaternion.Euler(global::UnityEngine.Vector3.up * rotation)).GetComponentInChildren<Item>().InitializeNewItem(launch, position, rotation);
						stackableItem.ForceStackCount(count);
						return;
					}
					count = Mathf.Min(count, 10);
					for (int i = 0; i < count; i++)
					{
						float num = ((count == 1) ? 0f : global::UnityEngine.Random.Range(-6f, 6f));
						Quaternion quaternion = Quaternion.Euler(0f, rotation + num, 0f);
						global::UnityEngine.Object.Instantiate<GameObject>(runtimePropInfo.EndlessProp.gameObject, position, quaternion).GetComponentInChildren<Item>().InitializeNewItem(launch, position, rotation + num);
					}
				}
			}
		}

		// Token: 0x06000578 RID: 1400 RVA: 0x0001BBA4 File Offset: 0x00019DA4
		public bool IsStackable()
		{
			if (base.IsReferenceEmpty())
			{
				return false;
			}
			PropLibrary.RuntimePropInfo runtimePropInfo;
			BaseTypeDefinition baseTypeDefinition;
			if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(this.Id, out runtimePropInfo) && MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(runtimePropInfo.PropData.BaseTypeId, out baseTypeDefinition))
			{
				if (baseTypeDefinition.ComponentBase is ResourcePickup)
				{
					return true;
				}
				Item item = baseTypeDefinition.ComponentBase as Item;
				if (item != null)
				{
					return item.IsStackable;
				}
			}
			return false;
		}

		// Token: 0x06000579 RID: 1401 RVA: 0x0001BC20 File Offset: 0x00019E20
		internal InventoryLibraryReference.InventoryReferenceType GetInventoryReferenceType()
		{
			if (base.IsReferenceEmpty())
			{
				return InventoryLibraryReference.InventoryReferenceType.Invalid;
			}
			PropLibrary.RuntimePropInfo runtimePropInfo;
			BaseTypeDefinition baseTypeDefinition;
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(this.Id, out runtimePropInfo) || !MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(runtimePropInfo.PropData.BaseTypeId, out baseTypeDefinition))
			{
				return InventoryLibraryReference.InventoryReferenceType.Invalid;
			}
			if (baseTypeDefinition.ComponentBase is ResourcePickup)
			{
				return InventoryLibraryReference.InventoryReferenceType.Resource;
			}
			if (baseTypeDefinition.ComponentBase is Item)
			{
				return InventoryLibraryReference.InventoryReferenceType.Item;
			}
			return InventoryLibraryReference.InventoryReferenceType.Invalid;
		}

		// Token: 0x0600057A RID: 1402 RVA: 0x0001BC94 File Offset: 0x00019E94
		internal bool Matches(InventoryLibraryReference other)
		{
			return this.Id == other.Id && this.CosmeticId == other.CosmeticId;
		}

		// Token: 0x0600057B RID: 1403 RVA: 0x0001BCBC File Offset: 0x00019EBC
		internal bool Matches(InventorySlot slot)
		{
			return this.Id == slot.AssetID;
		}

		// Token: 0x0600057C RID: 1404 RVA: 0x0001BCD0 File Offset: 0x00019ED0
		public static implicit operator ResourceLibraryReference(InventoryLibraryReference reference)
		{
			return new ResourceLibraryReference
			{
				Id = reference.Id,
				CosmeticId = reference.CosmeticId
			};
		}

		// Token: 0x020000F6 RID: 246
		internal enum InventoryReferenceType
		{
			// Token: 0x04000422 RID: 1058
			Resource,
			// Token: 0x04000423 RID: 1059
			Item,
			// Token: 0x04000424 RID: 1060
			Invalid
		}
	}
}
