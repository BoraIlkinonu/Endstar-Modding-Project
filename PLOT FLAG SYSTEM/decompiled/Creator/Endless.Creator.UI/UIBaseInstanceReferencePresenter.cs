using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Creator.UI;

public abstract class UIBaseInstanceReferencePresenter<TModel> : UIInspectorPropReferencePresenter<TModel> where TModel : InstanceReference
{
	[Header("UIBaseInstanceReferencePresenter")]
	[SerializeField]
	private InterfaceReference<IInstanceReferenceViewable> iInstanceContextReferenceViewable;

	protected override IEnumerable<object> SelectionOptions
	{
		get
		{
			List<PropEntry> referenceFilteredPropEntries = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetReferenceFilteredPropEntries(base.ReferenceFilter);
			referenceFilteredPropEntries.Insert(0, UIPropEntryView.UseContext);
			return referenceFilteredPropEntries;
		}
	}

	protected override void Start()
	{
		base.Start();
		iInstanceContextReferenceViewable.Interface.OnInstanceEyeDropped += SetModelToEyeDropperInstanceId;
	}

	public override void SetModel(TModel model, bool triggerOnModelChanged)
	{
		model = ProcessModelIfMissing(model);
		base.SetModel(model, triggerOnModelChanged);
	}

	protected abstract TModel ConstructNewModelWithInstanceId(SerializableGuid instanceId);

	protected override void UpdateOriginalSelection(TModel model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "UpdateOriginalSelection", "model", model), this);
		}
		if (!model.IsReferenceEmpty() || base.Model.useContext)
		{
			if (base.Model.useContext)
			{
				originalSelection.Add(UIPropEntryView.UseContext);
				return;
			}
			SerializableGuid id = InspectorReferenceUtility.GetId(base.Model);
			originalSelection.Add(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.GetPropEntry(id));
		}
	}

	private TModel ProcessModelIfMissing(TModel model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ProcessModelIfMissing", "model", model), this);
		}
		SerializableGuid instanceDefinitionId = InspectorReferenceUtility.GetInstanceDefinitionId(model);
		if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(instanceDefinitionId, out var _))
		{
			InspectorReferenceUtility.SetId(model, SerializableGuid.Empty);
		}
		return model;
	}

	private void SetModelToEyeDropperInstanceId(SerializableGuid instanceId)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetModelToEyeDropperInstanceId", "instanceId", instanceId), this);
		}
		TModel model = ConstructNewModelWithInstanceId(instanceId);
		SetModel(model, triggerOnModelChanged: true);
	}
}
