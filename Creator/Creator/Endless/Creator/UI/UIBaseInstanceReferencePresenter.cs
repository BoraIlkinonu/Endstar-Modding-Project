using System;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000221 RID: 545
	public abstract class UIBaseInstanceReferencePresenter<TModel> : UIInspectorPropReferencePresenter<TModel> where TModel : InstanceReference
	{
		// Token: 0x17000111 RID: 273
		// (get) Token: 0x060008C6 RID: 2246 RVA: 0x0002A548 File Offset: 0x00028748
		protected override IEnumerable<object> SelectionOptions
		{
			get
			{
				List<PropEntry> referenceFilteredPropEntries = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetReferenceFilteredPropEntries(base.ReferenceFilter);
				referenceFilteredPropEntries.Insert(0, UIPropEntryView.UseContext);
				return referenceFilteredPropEntries;
			}
		}

		// Token: 0x060008C7 RID: 2247 RVA: 0x0002A56B File Offset: 0x0002876B
		protected override void Start()
		{
			base.Start();
			this.iInstanceContextReferenceViewable.Interface.OnInstanceEyeDropped += this.SetModelToEyeDropperInstanceId;
		}

		// Token: 0x060008C8 RID: 2248 RVA: 0x0002A58F File Offset: 0x0002878F
		public override void SetModel(TModel model, bool triggerOnModelChanged)
		{
			model = this.ProcessModelIfMissing(model);
			base.SetModel(model, triggerOnModelChanged);
		}

		// Token: 0x060008C9 RID: 2249
		protected abstract TModel ConstructNewModelWithInstanceId(SerializableGuid instanceId);

		// Token: 0x060008CA RID: 2250 RVA: 0x0002A5A4 File Offset: 0x000287A4
		protected override void UpdateOriginalSelection(TModel model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "UpdateOriginalSelection", "model", model), this);
			}
			if (model.IsReferenceEmpty() && !base.Model.useContext)
			{
				return;
			}
			if (base.Model.useContext)
			{
				this.originalSelection.Add(UIPropEntryView.UseContext);
				return;
			}
			SerializableGuid id = InspectorReferenceUtility.GetId(base.Model);
			this.originalSelection.Add(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.GetPropEntry(id));
		}

		// Token: 0x060008CB RID: 2251 RVA: 0x0002A650 File Offset: 0x00028850
		private TModel ProcessModelIfMissing(TModel model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ProcessModelIfMissing", "model", model), this);
			}
			InstanceReference instanceReference = model;
			SerializableGuid instanceDefinitionId = InspectorReferenceUtility.GetInstanceDefinitionId(instanceReference);
			PropLibrary.RuntimePropInfo runtimePropInfo;
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(instanceDefinitionId, out runtimePropInfo))
			{
				InspectorReferenceUtility.SetId(instanceReference, SerializableGuid.Empty);
			}
			return model;
		}

		// Token: 0x060008CC RID: 2252 RVA: 0x0002A6B4 File Offset: 0x000288B4
		private void SetModelToEyeDropperInstanceId(SerializableGuid instanceId)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetModelToEyeDropperInstanceId", "instanceId", instanceId), this);
			}
			TModel tmodel = this.ConstructNewModelWithInstanceId(instanceId);
			this.SetModel(tmodel, true);
		}

		// Token: 0x0400077C RID: 1916
		[Header("UIBaseInstanceReferencePresenter")]
		[SerializeField]
		private InterfaceReference<IInstanceReferenceViewable> iInstanceContextReferenceViewable;
	}
}
