using System;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UIInventorySpawnOptionsListModel : UIBaseLocalFilterableListModel<InventorySpawnOption>
{
	private UIInventorySpawnOptionsPresenter inventorySpawnOptionsPresenterToApplyTo;

	private readonly Dictionary<SerializableGuid, int> inventorySpawnOptionIndexLookUp = new Dictionary<SerializableGuid, int>();

	public Dictionary<SerializableGuid, InventoryUsableDefinition> InventoryDefinitionLookUp { get; } = new Dictionary<SerializableGuid, InventoryUsableDefinition>();

	protected override Comparison<InventorySpawnOption> DefaultSort => (InventorySpawnOption x, InventorySpawnOption y) => string.Compare(TryGetInventoryUsableDefinitionName(x.AssetId), TryGetInventoryUsableDefinitionName(y.AssetId), StringComparison.Ordinal);

	public void Initialize(UIInventorySpawnOptionsPresenter inventorySpawnOptionsPresenter)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize", inventorySpawnOptionsPresenter);
		}
		inventorySpawnOptionsPresenterToApplyTo = inventorySpawnOptionsPresenter;
		Clear(triggerEvents: false);
		inventorySpawnOptionIndexLookUp.Clear();
		InventoryDefinitionLookUp.Clear();
		List<InventorySpawnOption> list = new List<InventorySpawnOption>();
		for (int i = 0; i < RuntimeDatabase.UsableDefinitions.Length; i++)
		{
			if (RuntimeDatabase.UsableDefinitions[i] is InventoryUsableDefinition inventoryUsableDefinition)
			{
				InventoryDefinitionLookUp.Add(inventoryUsableDefinition.Guid, inventoryUsableDefinition);
				InventorySpawnOption item = new InventorySpawnOption
				{
					AssetId = inventoryUsableDefinition.Guid
				};
				list.Add(item);
				if (inventorySpawnOptionIndexLookUp.ContainsKey(inventoryUsableDefinition.Guid))
				{
					string warning = string.Format("Somehow there is more than 1 {0} with a {1} ", inventoryUsableDefinition.GetType(), "Guid") + string.Format("of {0} in {1}. That should not be possible! Unless something was added more than once?", inventoryUsableDefinition.Guid, "UsableDefinitions");
					DebugUtility.LogWarning(this, "Initialize", warning, inventorySpawnOptionsPresenterToApplyTo);
				}
				else
				{
					inventorySpawnOptionIndexLookUp.Add(inventoryUsableDefinition.Guid, i);
				}
			}
		}
		foreach (InventorySpawnOption inventorySpawnOption in inventorySpawnOptionsPresenterToApplyTo.Model.inventorySpawnOptions)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log($"saved option: {InventoryDefinitionLookUp[inventorySpawnOption.AssetId].DisplayName} with a quant of {inventorySpawnOption.Quantity}", this);
			}
			if (inventorySpawnOptionIndexLookUp.ContainsKey(inventorySpawnOption.AssetId))
			{
				int index = inventorySpawnOptionIndexLookUp[inventorySpawnOption.AssetId];
				list[index] = inventorySpawnOption;
			}
			else
			{
				DebugUtility.LogWarning(this, "Initialize", "Somehow there is no asset with an AssetId of " + string.Format("{0} in {1}. ", inventorySpawnOption.AssetId, "UsableDefinitions") + "That should not be possible! Unless something got removed?", inventorySpawnOptionsPresenterToApplyTo);
			}
		}
		Set(list, triggerEvents: true);
	}

	public void ApplyChanges()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ApplyChanges");
		}
		InventorySpawnOptions inventorySpawnOptions = new InventorySpawnOptions();
		foreach (InventorySpawnOption item in List)
		{
			if (item.Quantity > 0)
			{
				inventorySpawnOptions.inventorySpawnOptions.Add(item);
			}
		}
		inventorySpawnOptionsPresenterToApplyTo.SetModel(inventorySpawnOptions, triggerOnModelChanged: true);
	}

	private string TryGetInventoryUsableDefinitionName(SerializableGuid id)
	{
		if (!InventoryDefinitionLookUp.TryGetValue(id, out var value))
		{
			return "Error";
		}
		return value.DisplayName;
	}
}
