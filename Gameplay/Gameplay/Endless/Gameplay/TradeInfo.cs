using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000FA RID: 250
	[Serializable]
	public class TradeInfo
	{
		// Token: 0x0600058A RID: 1418 RVA: 0x0001BE30 File Offset: 0x0001A030
		public string GetTextMeshString()
		{
			if (this.Cost1 == null)
			{
				this.Cost1 = new TradeInfo.InventoryAndQuantityReference();
			}
			if (this.Cost2 == null)
			{
				this.Cost2 = new TradeInfo.InventoryAndQuantityReference();
			}
			if (this.Reward1 == null)
			{
				this.Reward1 = new TradeInfo.InventoryAndQuantityReference();
			}
			if (this.Reward2 == null)
			{
				this.Reward2 = new TradeInfo.InventoryAndQuantityReference();
			}
			if (!this.IsTradeValid())
			{
				return "Invalid trade!";
			}
			this.ReorderTradeArguments();
			StringBuilder stringBuilder = new StringBuilder(200);
			this.AppendIdImage(stringBuilder, this.Cost1.Id, this.Cost1.Quantity);
			if (this.Cost2.IsReferenceSet())
			{
				stringBuilder.Append('+');
				this.AppendIdImage(stringBuilder, this.Cost2.Id, this.Cost2.Quantity);
			}
			stringBuilder.Append("<sprite name=\"trade\">");
			if (this.Reward1.IsReferenceSet())
			{
				this.AppendIdImage(stringBuilder, this.Reward1.Id, this.Reward1.Quantity);
			}
			if (this.Reward2.IsReferenceSet())
			{
				stringBuilder.Append('+');
				this.AppendIdImage(stringBuilder, this.Reward2.Id, this.Reward2.Quantity);
			}
			return stringBuilder.ToString();
		}

		// Token: 0x0600058B RID: 1419 RVA: 0x0001BF7B File Offset: 0x0001A17B
		private void AppendIdImage(StringBuilder builder, string id, int stackableQuantity)
		{
			builder.Append("<color=#0000><link=\"cosmetic:");
			builder.Append(id);
			if (stackableQuantity > 1)
			{
				builder.Append('#');
				builder.Append(stackableQuantity);
			}
			builder.Append(">||0|</link></color>");
		}

		// Token: 0x0600058C RID: 1420 RVA: 0x0001BFB4 File Offset: 0x0001A1B4
		internal void ReorderTradeArguments()
		{
			if (this.Cost1.IsReferenceEmpty() && this.Cost2.IsReferenceSet())
			{
				this.Cost1 = this.Cost2;
				this.Cost2 = new TradeInfo.InventoryAndQuantityReference();
			}
			if (this.Reward1.IsReferenceEmpty() && this.Reward2.IsReferenceSet())
			{
				this.Reward1 = this.Reward2;
				this.Reward2 = new TradeInfo.InventoryAndQuantityReference();
			}
		}

		// Token: 0x0600058D RID: 1421 RVA: 0x0001C023 File Offset: 0x0001A223
		public bool IsTradeValid()
		{
			return !this.Cost1.IsReferenceEmpty() || !this.Cost2.IsReferenceEmpty();
		}

		// Token: 0x0600058E RID: 1422 RVA: 0x0001C044 File Offset: 0x0001A244
		public bool CanPlayerMakeTrade(Context playerContext, int itemGrantBehavior)
		{
			if (!Enum.IsDefined(typeof(ItemGrantBehavior), itemGrantBehavior))
			{
				throw new ArgumentException(string.Format("Invalid {0} value: {1}", "ItemGrantBehavior", itemGrantBehavior), "itemGrantBehavior");
			}
			if (!this.CanPlayerAfford(playerContext))
			{
				return false;
			}
			if (itemGrantBehavior == 0)
			{
				PlayerLuaComponent playerLuaComponent;
				playerContext.WorldObject.TryGetUserComponent<PlayerLuaComponent>(out playerLuaComponent);
				Inventory inventory = playerLuaComponent.References.Inventory;
				int slotDeltaFromTrade = inventory.GetSlotDeltaFromTrade(this);
				return inventory.GetEmptySlotCount() >= slotDeltaFromTrade;
			}
			return true;
		}

		// Token: 0x0600058F RID: 1423 RVA: 0x0001C0C8 File Offset: 0x0001A2C8
		public bool CanPlayerAfford(Context playerContext)
		{
			if (!this.IsTradeValid())
			{
				throw new Exception("Trade is not valid!");
			}
			this.ReorderTradeArguments();
			PlayerLuaComponent playerLuaComponent;
			playerContext.WorldObject.TryGetUserComponent<PlayerLuaComponent>(out playerLuaComponent);
			return this.CanPlayerAfford(playerLuaComponent.Player);
		}

		// Token: 0x06000590 RID: 1424 RVA: 0x0001C108 File Offset: 0x0001A308
		internal bool CanPlayerAfford(Player player)
		{
			return player.PlayerHasItem(this.Cost1, this.Cost1.Quantity) && (this.Cost2.IsReferenceEmpty() || player.PlayerHasItem(this.Cost2, this.Cost2.Quantity));
		}

		// Token: 0x06000591 RID: 1425 RVA: 0x0001C159 File Offset: 0x0001A359
		public bool AttemptTrade(Context playerContext, Context tradeSource)
		{
			return this.AttemptTrade(playerContext, tradeSource, 1);
		}

		// Token: 0x06000592 RID: 1426 RVA: 0x0001C164 File Offset: 0x0001A364
		public bool AttemptTrade(Context playerContext, Context tradeSource, int itemGrantBehavior)
		{
			if (!Enum.IsDefined(typeof(ItemGrantBehavior), itemGrantBehavior))
			{
				throw new ArgumentException(string.Format("Invalid {0} value: {1}", "ItemGrantBehavior", itemGrantBehavior), "itemGrantBehavior");
			}
			if (!this.IsTradeValid())
			{
				throw new Exception("Trade is not valid!");
			}
			this.ReorderTradeArguments();
			PlayerLuaComponent playerLuaComponent;
			playerContext.WorldObject.TryGetUserComponent<PlayerLuaComponent>(out playerLuaComponent);
			Player player = playerLuaComponent.Player;
			if (player == null)
			{
				throw new ArgumentException("Only players can make trades. Ensure that the context being passed in is a player!");
			}
			if (!this.CanPlayerMakeTrade(playerContext, itemGrantBehavior))
			{
				return false;
			}
			if (this.Cost1.IsReferenceSet())
			{
				player.ConsumeItem(playerContext, this.Cost1, this.Cost1.Quantity);
			}
			if (this.Cost2.IsReferenceSet())
			{
				player.ConsumeItem(playerContext, this.Cost2, this.Cost2.Quantity);
			}
			Transform transform = tradeSource.WorldObject.transform;
			Transform transform2 = playerContext.WorldObject.transform;
			global::UnityEngine.Vector3 position = transform.position;
			global::UnityEngine.Vector3 vector = transform2.position - position;
			vector.y = 0f;
			float num;
			if (vector.sqrMagnitude < 0.0001f)
			{
				num = transform.eulerAngles.y;
			}
			else
			{
				num = Quaternion.LookRotation(vector, global::UnityEngine.Vector3.up).eulerAngles.y;
			}
			List<TradeInfo.InventoryAndQuantityReference> list = new List<TradeInfo.InventoryAndQuantityReference>();
			list.Add(this.Reward1);
			list.Add(this.Reward2);
			Inventory inventory = playerLuaComponent.References.Inventory;
			foreach (TradeInfo.InventoryAndQuantityReference inventoryAndQuantityReference in list)
			{
				if (inventoryAndQuantityReference.IsReferenceSet())
				{
					int num2 = inventoryAndQuantityReference.Quantity;
					InventoryLibraryReference.InventoryReferenceType inventoryReferenceType = inventoryAndQuantityReference.GetInventoryReferenceType();
					if (itemGrantBehavior != 2)
					{
						switch (inventoryReferenceType)
						{
						case InventoryLibraryReference.InventoryReferenceType.Resource:
							NetworkBehaviourSingleton<ResourceManager>.Instance.ResourceCollected(inventoryAndQuantityReference, inventoryAndQuantityReference.Quantity, player.OwnerClientId);
							num2 = 0;
							goto IL_01F4;
						case InventoryLibraryReference.InventoryReferenceType.Item:
							num2 = inventory.AttemptGiveAsPossible(inventoryAndQuantityReference, false, inventoryAndQuantityReference.Quantity);
							goto IL_01F4;
						}
						throw new InvalidDataException("Invalid item reference in trade rewards!");
					}
					IL_01F4:
					if (num2 > 0)
					{
						inventoryAndQuantityReference.SpawnItem(playerContext, position, num, num2, true);
					}
				}
			}
			return true;
		}

		// Token: 0x04000427 RID: 1063
		public TradeInfo.InventoryAndQuantityReference Cost1;

		// Token: 0x04000428 RID: 1064
		public TradeInfo.InventoryAndQuantityReference Cost2;

		// Token: 0x04000429 RID: 1065
		public TradeInfo.InventoryAndQuantityReference Reward1;

		// Token: 0x0400042A RID: 1066
		public TradeInfo.InventoryAndQuantityReference Reward2;

		// Token: 0x020000FB RID: 251
		[Serializable]
		public class InventoryAndQuantityReference : InventoryLibraryReference
		{
			// Token: 0x06000594 RID: 1428 RVA: 0x0001BCF3 File Offset: 0x00019EF3
			public InventoryAndQuantityReference()
			{
			}

			// Token: 0x06000595 RID: 1429 RVA: 0x0001C3B4 File Offset: 0x0001A5B4
			public InventoryAndQuantityReference(TradeInfo.InventoryAndQuantityReference other)
			{
				this.Id = other.Id;
				this.CosmeticId = other.Id;
				this.Quantity = other.Quantity;
			}

			// Token: 0x0400042B RID: 1067
			public int Quantity;
		}
	}
}
