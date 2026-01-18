using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using Endless.Shared;

namespace Endless.Gameplay.LuaInterfaces;

public class Player
{
	internal PlayerLuaComponent player;

	internal ulong OwnerClientId => player.OwnerClientId;

	internal Player(PlayerLuaComponent player)
	{
		this.player = player;
	}

	public void SetInventorySize(Context instigator, int count)
	{
		player.references.Inventory.SetTotalInventorySlots(count);
	}

	public int GetInventorySize()
	{
		return player.references.Inventory.GetTotalInventorySlots();
	}

	public bool PlayerHasItem(InventoryLibraryReference reference)
	{
		return PlayerHasItem(reference, 1);
	}

	public bool PlayerHasItem(InventoryLibraryReference reference, int quantity)
	{
		if (quantity < 1)
		{
			return true;
		}
		if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(reference.Id, out var metadata) && MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(metadata.PropData.BaseTypeId, out var componentDefinition))
		{
			if (componentDefinition.ComponentBase is Endless.Gameplay.ResourcePickup)
			{
				ulong ownerClientId = player.OwnerClientId;
				return NetworkBehaviourSingleton<ResourceManager>.Instance.HasResourceCheck(reference, ownerClientId, quantity);
			}
			return player.references.Inventory.HasItem(reference, quantity);
		}
		return false;
	}

	public bool ConsumeItem(Context instigator, InventoryLibraryReference itemToConsume)
	{
		return ConsumeItem(instigator, itemToConsume, 1);
	}

	public bool ConsumeItem(Context instigator, InventoryLibraryReference itemToConsume, int quantity)
	{
		if (quantity < 1)
		{
			return true;
		}
		if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(itemToConsume.Id, out var metadata) && MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(metadata.PropData.BaseTypeId, out var componentDefinition))
		{
			if (componentDefinition.ComponentBase is Endless.Gameplay.ResourcePickup)
			{
				ulong ownerClientId = player.OwnerClientId;
				return NetworkBehaviourSingleton<ResourceManager>.Instance.AttemptSpendResource(itemToConsume, quantity, ownerClientId);
			}
			return player.references.Inventory.ConsumeItem(itemToConsume, quantity);
		}
		return false;
	}

	public void ClearAllItems(Context instigator)
	{
		ClearAllItems(instigator, includeLockedItems: true);
	}

	public void ClearAllItems(Context instigator, bool includeLockedItems)
	{
		player.references.Inventory.ClearAllItems(includeLockedItems);
	}

	public bool AttemptGiveItem(Context instigator, InventoryLibraryReference itemToGrant)
	{
		return AttemptGiveItem(instigator, itemToGrant, lockItem: false, 1);
	}

	public bool AttemptGiveItem(Context instigator, InventoryLibraryReference itemToGrant, int quantity)
	{
		return AttemptGiveItem(instigator, itemToGrant, lockItem: false, quantity);
	}

	public bool AttemptGiveItem(Context instigator, InventoryLibraryReference itemToGrant, bool lockItem)
	{
		return AttemptGiveItem(instigator, itemToGrant, lockItem, 1);
	}

	public bool AttemptGiveItem(Context instigator, InventoryLibraryReference itemToGrant, bool lockItem, int stackableQuantity)
	{
		if (stackableQuantity < 1)
		{
			return false;
		}
		if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(itemToGrant.Id, out var metadata) && MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(metadata.PropData.BaseTypeId, out var componentDefinition))
		{
			if (componentDefinition.ComponentBase is Endless.Gameplay.ResourcePickup)
			{
				NetworkBehaviourSingleton<ResourceManager>.Instance.ResourceCollected(itemToGrant, stackableQuantity, OwnerClientId);
				return true;
			}
			return player.references.Inventory.AttemptGiveItem(itemToGrant, lockItem, stackableQuantity);
		}
		return false;
	}

	public int GetPlayerSlot()
	{
		return player.references.UserSlot;
	}

	public string GetPlayerName()
	{
		return player.GetUserName();
	}

	public void SetInputSettings(Context source, int value)
	{
		player.references.PlayerNetworkController.SetGameplayInputSettingsFlags((InputSettings)value);
	}
}
