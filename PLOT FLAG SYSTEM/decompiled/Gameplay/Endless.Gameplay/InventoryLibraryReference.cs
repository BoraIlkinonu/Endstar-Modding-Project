using System;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay;

[Serializable]
public class InventoryLibraryReference : PropLibraryReference
{
	internal enum InventoryReferenceType
	{
		Resource,
		Item,
		Invalid
	}

	internal override ReferenceFilter Filter => ReferenceFilter.InventoryItem;

	internal InventoryLibraryReference()
	{
	}

	internal InventoryLibraryReference(SerializableGuid assetId)
	{
		Id = assetId;
	}

	public void SpawnItem(Context instigator, CellReference destination)
	{
		SpawnItem(instigator, destination.GetCellPosition(), destination.GetRotation(), 1, launch: true);
	}

	public void SpawnItem(Context instigator, CellReference destination, int count)
	{
		SpawnItem(instigator, destination.GetCellPosition(), destination.GetRotation(), count, launch: true);
	}

	public void SpawnItem(Context instigator, CellReference destination, bool launch)
	{
		SpawnItem(instigator, destination.GetCellPosition(), destination.GetRotation(), 1, launch);
	}

	public void SpawnItem(Context instigator, CellReference destination, int count, bool launch)
	{
		SpawnItem(instigator, destination.GetCellPosition(), destination.GetRotation(), count, launch);
	}

	public void SpawnItem(Context instigator, UnityEngine.Vector3 position, float rotation)
	{
		SpawnItem(instigator, position, rotation, 1, launch: true);
	}

	public void SpawnItem(Context instigator, UnityEngine.Vector3 position, float rotation, bool launch)
	{
		SpawnItem(instigator, position, rotation, 1, launch);
	}

	public void SpawnItem(Context instigator, UnityEngine.Vector3 position, float rotation, int count)
	{
		SpawnItem(instigator, position, rotation, count, launch: true);
	}

	public void SpawnItem(Context instigator, UnityEngine.Vector3 position, float rotation, int count, bool launch)
	{
		if (Id == SerializableGuid.Empty)
		{
			throw new NullReferenceException("Cannot spawn an item without an empty reference!");
		}
		if (count < 1)
		{
			throw new ArgumentException("Cannot spawn 0 or less of an item!");
		}
		PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(Id);
		if (!MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(runtimePropInfo.PropData.BaseTypeId, out var componentDefinition))
		{
			return;
		}
		if (componentDefinition.ComponentBase is ResourcePickup)
		{
			ResourcePickup componentInChildren = UnityEngine.Object.Instantiate(runtimePropInfo.EndlessProp.gameObject, position, Quaternion.Euler(UnityEngine.Vector3.up * rotation)).GetComponentInChildren<ResourcePickup>();
			componentInChildren.Quantity = count;
			componentInChildren.ShouldSaveAndLoad = false;
			componentInChildren.InitializeNewItem(launch, position, rotation);
			componentInChildren.SetDynamicSpawnQuantity_ClientRpc(componentInChildren.Quantity);
		}
		else
		{
			if (!(componentDefinition.ComponentBase is Item item))
			{
				return;
			}
			if (item is StackableItem stackableItem)
			{
				UnityEngine.Object.Instantiate(runtimePropInfo.EndlessProp.gameObject, position, Quaternion.Euler(UnityEngine.Vector3.up * rotation)).GetComponentInChildren<Item>().InitializeNewItem(launch, position, rotation);
				stackableItem.ForceStackCount(count);
				return;
			}
			count = Mathf.Min(count, 10);
			for (int i = 0; i < count; i++)
			{
				float num = ((count == 1) ? 0f : UnityEngine.Random.Range(-6f, 6f));
				Quaternion rotation2 = Quaternion.Euler(0f, rotation + num, 0f);
				UnityEngine.Object.Instantiate(runtimePropInfo.EndlessProp.gameObject, position, rotation2).GetComponentInChildren<Item>().InitializeNewItem(launch, position, rotation + num);
			}
		}
	}

	public bool IsStackable()
	{
		if (IsReferenceEmpty())
		{
			return false;
		}
		if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(Id, out var metadata) && MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(metadata.PropData.BaseTypeId, out var componentDefinition))
		{
			if (componentDefinition.ComponentBase is ResourcePickup)
			{
				return true;
			}
			if (componentDefinition.ComponentBase is Item item)
			{
				return item.IsStackable;
			}
		}
		return false;
	}

	internal InventoryReferenceType GetInventoryReferenceType()
	{
		if (IsReferenceEmpty())
		{
			return InventoryReferenceType.Invalid;
		}
		if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(Id, out var metadata) && MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(metadata.PropData.BaseTypeId, out var componentDefinition))
		{
			if (componentDefinition.ComponentBase is ResourcePickup)
			{
				return InventoryReferenceType.Resource;
			}
			if (componentDefinition.ComponentBase is Item)
			{
				return InventoryReferenceType.Item;
			}
			return InventoryReferenceType.Invalid;
		}
		return InventoryReferenceType.Invalid;
	}

	internal bool Matches(InventoryLibraryReference other)
	{
		if (Id == other.Id)
		{
			return CosmeticId == other.CosmeticId;
		}
		return false;
	}

	internal bool Matches(InventorySlot slot)
	{
		return Id == slot.AssetID;
	}

	public static implicit operator ResourceLibraryReference(InventoryLibraryReference reference)
	{
		return new ResourceLibraryReference
		{
			Id = reference.Id,
			CosmeticId = reference.CosmeticId
		};
	}
}
