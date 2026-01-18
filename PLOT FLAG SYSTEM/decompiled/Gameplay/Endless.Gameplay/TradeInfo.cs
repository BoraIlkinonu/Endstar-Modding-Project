using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay;

[Serializable]
public class TradeInfo
{
	[Serializable]
	public class InventoryAndQuantityReference : InventoryLibraryReference
	{
		public int Quantity;

		public InventoryAndQuantityReference()
		{
		}

		public InventoryAndQuantityReference(InventoryAndQuantityReference other)
		{
			Id = other.Id;
			CosmeticId = other.Id;
			Quantity = other.Quantity;
		}
	}

	public InventoryAndQuantityReference Cost1;

	public InventoryAndQuantityReference Cost2;

	public InventoryAndQuantityReference Reward1;

	public InventoryAndQuantityReference Reward2;

	public string GetTextMeshString()
	{
		if ((object)Cost1 == null)
		{
			Cost1 = new InventoryAndQuantityReference();
		}
		if ((object)Cost2 == null)
		{
			Cost2 = new InventoryAndQuantityReference();
		}
		if ((object)Reward1 == null)
		{
			Reward1 = new InventoryAndQuantityReference();
		}
		if ((object)Reward2 == null)
		{
			Reward2 = new InventoryAndQuantityReference();
		}
		if (!IsTradeValid())
		{
			return "Invalid trade!";
		}
		ReorderTradeArguments();
		StringBuilder stringBuilder = new StringBuilder(200);
		AppendIdImage(stringBuilder, Cost1.Id, Cost1.Quantity);
		if (Cost2.IsReferenceSet())
		{
			stringBuilder.Append('+');
			AppendIdImage(stringBuilder, Cost2.Id, Cost2.Quantity);
		}
		stringBuilder.Append("<sprite name=\"trade\">");
		if (Reward1.IsReferenceSet())
		{
			AppendIdImage(stringBuilder, Reward1.Id, Reward1.Quantity);
		}
		if (Reward2.IsReferenceSet())
		{
			stringBuilder.Append('+');
			AppendIdImage(stringBuilder, Reward2.Id, Reward2.Quantity);
		}
		return stringBuilder.ToString();
	}

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

	internal void ReorderTradeArguments()
	{
		if (Cost1.IsReferenceEmpty() && Cost2.IsReferenceSet())
		{
			Cost1 = Cost2;
			Cost2 = new InventoryAndQuantityReference();
		}
		if (Reward1.IsReferenceEmpty() && Reward2.IsReferenceSet())
		{
			Reward1 = Reward2;
			Reward2 = new InventoryAndQuantityReference();
		}
	}

	public bool IsTradeValid()
	{
		if (Cost1.IsReferenceEmpty())
		{
			return !Cost2.IsReferenceEmpty();
		}
		return true;
	}

	public bool CanPlayerMakeTrade(Context playerContext, int itemGrantBehavior)
	{
		if (!Enum.IsDefined(typeof(ItemGrantBehavior), itemGrantBehavior))
		{
			throw new ArgumentException(string.Format("Invalid {0} value: {1}", "ItemGrantBehavior", itemGrantBehavior), "itemGrantBehavior");
		}
		if (!CanPlayerAfford(playerContext))
		{
			return false;
		}
		if (itemGrantBehavior == 0)
		{
			playerContext.WorldObject.TryGetUserComponent<PlayerLuaComponent>(out var component);
			Inventory inventory = component.References.Inventory;
			int slotDeltaFromTrade = inventory.GetSlotDeltaFromTrade(this);
			return inventory.GetEmptySlotCount() >= slotDeltaFromTrade;
		}
		return true;
	}

	public bool CanPlayerAfford(Context playerContext)
	{
		if (!IsTradeValid())
		{
			throw new Exception("Trade is not valid!");
		}
		ReorderTradeArguments();
		playerContext.WorldObject.TryGetUserComponent<PlayerLuaComponent>(out var component);
		return CanPlayerAfford(component.Player);
	}

	internal bool CanPlayerAfford(Player player)
	{
		if (!player.PlayerHasItem(Cost1, Cost1.Quantity))
		{
			return false;
		}
		if (!Cost2.IsReferenceEmpty() && !player.PlayerHasItem(Cost2, Cost2.Quantity))
		{
			return false;
		}
		return true;
	}

	public bool AttemptTrade(Context playerContext, Context tradeSource)
	{
		return AttemptTrade(playerContext, tradeSource, 1);
	}

	public bool AttemptTrade(Context playerContext, Context tradeSource, int itemGrantBehavior)
	{
		if (!Enum.IsDefined(typeof(ItemGrantBehavior), itemGrantBehavior))
		{
			throw new ArgumentException(string.Format("Invalid {0} value: {1}", "ItemGrantBehavior", itemGrantBehavior), "itemGrantBehavior");
		}
		if (!IsTradeValid())
		{
			throw new Exception("Trade is not valid!");
		}
		ReorderTradeArguments();
		playerContext.WorldObject.TryGetUserComponent<PlayerLuaComponent>(out var component);
		Player player = component.Player;
		if (player != null)
		{
			if (!CanPlayerMakeTrade(playerContext, itemGrantBehavior))
			{
				return false;
			}
			if (Cost1.IsReferenceSet())
			{
				player.ConsumeItem(playerContext, Cost1, Cost1.Quantity);
			}
			if (Cost2.IsReferenceSet())
			{
				player.ConsumeItem(playerContext, Cost2, Cost2.Quantity);
			}
			Transform transform = tradeSource.WorldObject.transform;
			Transform transform2 = playerContext.WorldObject.transform;
			UnityEngine.Vector3 position = transform.position;
			UnityEngine.Vector3 forward = transform2.position - position;
			forward.y = 0f;
			float rotation = ((!(forward.sqrMagnitude < 0.0001f)) ? Quaternion.LookRotation(forward, UnityEngine.Vector3.up).eulerAngles.y : transform.eulerAngles.y);
			List<InventoryAndQuantityReference> obj = new List<InventoryAndQuantityReference> { Reward1, Reward2 };
			Inventory inventory = component.References.Inventory;
			foreach (InventoryAndQuantityReference item in obj)
			{
				if (!item.IsReferenceSet())
				{
					continue;
				}
				int num = item.Quantity;
				InventoryLibraryReference.InventoryReferenceType inventoryReferenceType = item.GetInventoryReferenceType();
				if (itemGrantBehavior != 2)
				{
					switch (inventoryReferenceType)
					{
					case InventoryLibraryReference.InventoryReferenceType.Resource:
						NetworkBehaviourSingleton<ResourceManager>.Instance.ResourceCollected(item, item.Quantity, player.OwnerClientId);
						num = 0;
						break;
					case InventoryLibraryReference.InventoryReferenceType.Item:
						num = inventory.AttemptGiveAsPossible(item, lockItem: false, item.Quantity);
						break;
					default:
						throw new InvalidDataException("Invalid item reference in trade rewards!");
					}
				}
				if (num > 0)
				{
					item.SpawnItem(playerContext, position, rotation, num, launch: true);
				}
			}
			return true;
		}
		throw new ArgumentException("Only players can make trades. Ensure that the context being passed in is a player!");
	}
}
