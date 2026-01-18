using System;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000125 RID: 293
	public class UIInventorySpawnOptionsListModel : UIBaseLocalFilterableListModel<InventorySpawnOption>
	{
		// Token: 0x17000071 RID: 113
		// (get) Token: 0x0600049A RID: 1178 RVA: 0x0001AA1D File Offset: 0x00018C1D
		public Dictionary<SerializableGuid, InventoryUsableDefinition> InventoryDefinitionLookUp { get; } = new Dictionary<SerializableGuid, InventoryUsableDefinition>();

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x0600049B RID: 1179 RVA: 0x0001AA25 File Offset: 0x00018C25
		protected override Comparison<InventorySpawnOption> DefaultSort
		{
			get
			{
				return (InventorySpawnOption x, InventorySpawnOption y) => string.Compare(this.TryGetInventoryUsableDefinitionName(x.AssetId), this.TryGetInventoryUsableDefinitionName(y.AssetId), StringComparison.Ordinal);
			}
		}

		// Token: 0x0600049C RID: 1180 RVA: 0x0001AA34 File Offset: 0x00018C34
		public void Initialize(UIInventorySpawnOptionsPresenter inventorySpawnOptionsPresenter)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", new object[] { inventorySpawnOptionsPresenter });
			}
			this.inventorySpawnOptionsPresenterToApplyTo = inventorySpawnOptionsPresenter;
			this.Clear(false);
			this.inventorySpawnOptionIndexLookUp.Clear();
			this.InventoryDefinitionLookUp.Clear();
			List<InventorySpawnOption> list = new List<InventorySpawnOption>();
			for (int i = 0; i < RuntimeDatabase.UsableDefinitions.Length; i++)
			{
				InventoryUsableDefinition inventoryUsableDefinition = RuntimeDatabase.UsableDefinitions[i] as InventoryUsableDefinition;
				if (inventoryUsableDefinition != null)
				{
					this.InventoryDefinitionLookUp.Add(inventoryUsableDefinition.Guid, inventoryUsableDefinition);
					InventorySpawnOption inventorySpawnOption = new InventorySpawnOption
					{
						AssetId = inventoryUsableDefinition.Guid
					};
					list.Add(inventorySpawnOption);
					if (this.inventorySpawnOptionIndexLookUp.ContainsKey(inventoryUsableDefinition.Guid))
					{
						string text = string.Format("Somehow there is more than 1 {0} with a {1} ", inventoryUsableDefinition.GetType(), "Guid") + string.Format("of {0} in {1}. That should not be possible! Unless something was added more than once?", inventoryUsableDefinition.Guid, "UsableDefinitions");
						DebugUtility.LogWarning(this, "Initialize", text, new object[] { this.inventorySpawnOptionsPresenterToApplyTo });
					}
					else
					{
						this.inventorySpawnOptionIndexLookUp.Add(inventoryUsableDefinition.Guid, i);
					}
				}
			}
			foreach (InventorySpawnOption inventorySpawnOption2 in this.inventorySpawnOptionsPresenterToApplyTo.Model.inventorySpawnOptions)
			{
				if (base.VerboseLogging)
				{
					DebugUtility.Log(string.Format("saved option: {0} with a quant of {1}", this.InventoryDefinitionLookUp[inventorySpawnOption2.AssetId].DisplayName, inventorySpawnOption2.Quantity), this);
				}
				if (this.inventorySpawnOptionIndexLookUp.ContainsKey(inventorySpawnOption2.AssetId))
				{
					int num = this.inventorySpawnOptionIndexLookUp[inventorySpawnOption2.AssetId];
					list[num] = inventorySpawnOption2;
				}
				else
				{
					DebugUtility.LogWarning(this, "Initialize", "Somehow there is no asset with an AssetId of " + string.Format("{0} in {1}. ", inventorySpawnOption2.AssetId, "UsableDefinitions") + "That should not be possible! Unless something got removed?", new object[] { this.inventorySpawnOptionsPresenterToApplyTo });
				}
			}
			this.Set(list, true);
		}

		// Token: 0x0600049D RID: 1181 RVA: 0x0001AC64 File Offset: 0x00018E64
		public void ApplyChanges()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyChanges", Array.Empty<object>());
			}
			InventorySpawnOptions inventorySpawnOptions = new InventorySpawnOptions();
			foreach (InventorySpawnOption inventorySpawnOption in this.List)
			{
				if (inventorySpawnOption.Quantity > 0)
				{
					inventorySpawnOptions.inventorySpawnOptions.Add(inventorySpawnOption);
				}
			}
			this.inventorySpawnOptionsPresenterToApplyTo.SetModel(inventorySpawnOptions, true);
		}

		// Token: 0x0600049E RID: 1182 RVA: 0x0001ACF0 File Offset: 0x00018EF0
		private string TryGetInventoryUsableDefinitionName(SerializableGuid id)
		{
			InventoryUsableDefinition inventoryUsableDefinition;
			if (!this.InventoryDefinitionLookUp.TryGetValue(id, out inventoryUsableDefinition))
			{
				return "Error";
			}
			return inventoryUsableDefinition.DisplayName;
		}

		// Token: 0x0400045E RID: 1118
		private UIInventorySpawnOptionsPresenter inventorySpawnOptionsPresenterToApplyTo;

		// Token: 0x0400045F RID: 1119
		private readonly Dictionary<SerializableGuid, int> inventorySpawnOptionIndexLookUp = new Dictionary<SerializableGuid, int>();
	}
}
