using System;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using Endless.Shared;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x0200045E RID: 1118
	public class Player
	{
		// Token: 0x1700058B RID: 1419
		// (get) Token: 0x06001BF5 RID: 7157 RVA: 0x0007D030 File Offset: 0x0007B230
		internal ulong OwnerClientId
		{
			get
			{
				return this.player.OwnerClientId;
			}
		}

		// Token: 0x06001BF6 RID: 7158 RVA: 0x0007D03D File Offset: 0x0007B23D
		internal Player(PlayerLuaComponent player)
		{
			this.player = player;
		}

		// Token: 0x06001BF7 RID: 7159 RVA: 0x0007D04C File Offset: 0x0007B24C
		public void SetInventorySize(Context instigator, int count)
		{
			this.player.references.Inventory.SetTotalInventorySlots(count);
		}

		// Token: 0x06001BF8 RID: 7160 RVA: 0x0007D064 File Offset: 0x0007B264
		public int GetInventorySize()
		{
			return this.player.references.Inventory.GetTotalInventorySlots();
		}

		// Token: 0x06001BF9 RID: 7161 RVA: 0x0007D07B File Offset: 0x0007B27B
		public bool PlayerHasItem(InventoryLibraryReference reference)
		{
			return this.PlayerHasItem(reference, 1);
		}

		// Token: 0x06001BFA RID: 7162 RVA: 0x0007D088 File Offset: 0x0007B288
		public bool PlayerHasItem(InventoryLibraryReference reference, int quantity)
		{
			if (quantity < 1)
			{
				return true;
			}
			PropLibrary.RuntimePropInfo runtimePropInfo;
			BaseTypeDefinition baseTypeDefinition;
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(reference.Id, out runtimePropInfo) || !MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(runtimePropInfo.PropData.BaseTypeId, out baseTypeDefinition))
			{
				return false;
			}
			if (baseTypeDefinition.ComponentBase is ResourcePickup)
			{
				ulong ownerClientId = this.player.OwnerClientId;
				return NetworkBehaviourSingleton<ResourceManager>.Instance.HasResourceCheck(reference, ownerClientId, quantity);
			}
			return this.player.references.Inventory.HasItem(reference, quantity);
		}

		// Token: 0x06001BFB RID: 7163 RVA: 0x0007D11C File Offset: 0x0007B31C
		public bool ConsumeItem(Context instigator, InventoryLibraryReference itemToConsume)
		{
			return this.ConsumeItem(instigator, itemToConsume, 1);
		}

		// Token: 0x06001BFC RID: 7164 RVA: 0x0007D128 File Offset: 0x0007B328
		public bool ConsumeItem(Context instigator, InventoryLibraryReference itemToConsume, int quantity)
		{
			if (quantity < 1)
			{
				return true;
			}
			PropLibrary.RuntimePropInfo runtimePropInfo;
			BaseTypeDefinition baseTypeDefinition;
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(itemToConsume.Id, out runtimePropInfo) || !MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(runtimePropInfo.PropData.BaseTypeId, out baseTypeDefinition))
			{
				return false;
			}
			if (baseTypeDefinition.ComponentBase is ResourcePickup)
			{
				ulong ownerClientId = this.player.OwnerClientId;
				return NetworkBehaviourSingleton<ResourceManager>.Instance.AttemptSpendResource(itemToConsume, quantity, ownerClientId);
			}
			return this.player.references.Inventory.ConsumeItem(itemToConsume, quantity);
		}

		// Token: 0x06001BFD RID: 7165 RVA: 0x0007D1BC File Offset: 0x0007B3BC
		public void ClearAllItems(Context instigator)
		{
			this.ClearAllItems(instigator, true);
		}

		// Token: 0x06001BFE RID: 7166 RVA: 0x0007D1C6 File Offset: 0x0007B3C6
		public void ClearAllItems(Context instigator, bool includeLockedItems)
		{
			this.player.references.Inventory.ClearAllItems(includeLockedItems);
		}

		// Token: 0x06001BFF RID: 7167 RVA: 0x0007D1DE File Offset: 0x0007B3DE
		public bool AttemptGiveItem(Context instigator, InventoryLibraryReference itemToGrant)
		{
			return this.AttemptGiveItem(instigator, itemToGrant, false, 1);
		}

		// Token: 0x06001C00 RID: 7168 RVA: 0x0007D1EA File Offset: 0x0007B3EA
		public bool AttemptGiveItem(Context instigator, InventoryLibraryReference itemToGrant, int quantity)
		{
			return this.AttemptGiveItem(instigator, itemToGrant, false, quantity);
		}

		// Token: 0x06001C01 RID: 7169 RVA: 0x0007D1F6 File Offset: 0x0007B3F6
		public bool AttemptGiveItem(Context instigator, InventoryLibraryReference itemToGrant, bool lockItem)
		{
			return this.AttemptGiveItem(instigator, itemToGrant, lockItem, 1);
		}

		// Token: 0x06001C02 RID: 7170 RVA: 0x0007D204 File Offset: 0x0007B404
		public bool AttemptGiveItem(Context instigator, InventoryLibraryReference itemToGrant, bool lockItem, int stackableQuantity)
		{
			if (stackableQuantity < 1)
			{
				return false;
			}
			PropLibrary.RuntimePropInfo runtimePropInfo;
			BaseTypeDefinition baseTypeDefinition;
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(itemToGrant.Id, out runtimePropInfo) || !MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(runtimePropInfo.PropData.BaseTypeId, out baseTypeDefinition))
			{
				return false;
			}
			if (baseTypeDefinition.ComponentBase is ResourcePickup)
			{
				NetworkBehaviourSingleton<ResourceManager>.Instance.ResourceCollected(itemToGrant, stackableQuantity, this.OwnerClientId);
				return true;
			}
			return this.player.references.Inventory.AttemptGiveItem(itemToGrant, lockItem, stackableQuantity);
		}

		// Token: 0x06001C03 RID: 7171 RVA: 0x0007D296 File Offset: 0x0007B496
		public int GetPlayerSlot()
		{
			return this.player.references.UserSlot;
		}

		// Token: 0x06001C04 RID: 7172 RVA: 0x0007D2A8 File Offset: 0x0007B4A8
		public string GetPlayerName()
		{
			return this.player.GetUserName();
		}

		// Token: 0x06001C05 RID: 7173 RVA: 0x0007D2B5 File Offset: 0x0007B4B5
		public void SetInputSettings(Context source, int value)
		{
			this.player.references.PlayerNetworkController.SetGameplayInputSettingsFlags((InputSettings)value);
		}

		// Token: 0x040015C0 RID: 5568
		internal PlayerLuaComponent player;
	}
}
