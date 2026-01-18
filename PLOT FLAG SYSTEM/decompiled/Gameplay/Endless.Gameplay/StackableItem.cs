using System;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public abstract class StackableItem : Item
{
	private NetworkVariable<int> stackCount = new NetworkVariable<int>(5);

	public override bool IsStackable => true;

	public override int StackCount => stackCount.Value;

	public int StartingStackCount
	{
		get
		{
			return StackCount;
		}
		set
		{
			if (base.IsServer)
			{
				stackCount.Value = value;
			}
		}
	}

	public override void CopyToItem(Item item)
	{
		(item as StackableItem).stackCount.Value = stackCount.Value;
		base.CopyToItem(item);
	}

	public void ForceStackCount(int value)
	{
		if (base.IsServer)
		{
			if (value < 1)
			{
				stackCount.Value = 0;
				base.Carrier?.Inventory.StackCountChanged(inventoryUsableDefinition.Guid);
				Destroy();
			}
			else
			{
				stackCount.Value = value;
				base.Carrier?.Inventory.StackCountChanged(inventoryUsableDefinition.Guid);
			}
		}
	}

	public void ChangeStackCount(int delta)
	{
		if (base.IsServer)
		{
			int num = Mathf.Max(0, stackCount.Value + delta);
			if (num < 1)
			{
				stackCount.Value = 0;
				base.Carrier?.Inventory.StackCountChanged(inventoryUsableDefinition.Guid);
				Destroy();
			}
			else if (num != stackCount.Value)
			{
				stackCount.Value = num;
				base.Carrier?.Inventory.StackCountChanged(inventoryUsableDefinition.Guid);
			}
		}
	}

	protected override void LoadedCount(int count)
	{
		if (base.IsServer)
		{
			stackCount.Value = count;
		}
	}

	public override void OnNetworkSpawn()
	{
		NetworkVariable<int> networkVariable = stackCount;
		networkVariable.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<int>.OnValueChangedDelegate(HandleStackCountChanged));
		base.OnNetworkSpawn();
	}

	private void HandleStackCountChanged(int previousValue, int value)
	{
	}

	protected override void __initializeVariables()
	{
		if (stackCount == null)
		{
			throw new Exception("StackableItem.stackCount cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		stackCount.Initialize(this);
		__nameNetworkVariable(stackCount, "stackCount");
		NetworkVariableFields.Add(stackCount);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "StackableItem";
	}
}
