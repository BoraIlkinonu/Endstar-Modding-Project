using System.Collections.Generic;
using Endless.Assets;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI;

public abstract class UIInspectorPropReferencePresenter<TModel> : UIInspectorReferencePresenter<TModel> where TModel : InspectorPropReference
{
	protected ReferenceFilter ReferenceFilter => InspectorReferenceUtility.GetReferenceFilter(base.Model);

	protected override IEnumerable<object> SelectionOptions
	{
		get
		{
			List<object> list = new List<object>();
			foreach (AssetReference propReference in MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.GameLibrary.PropReferences)
			{
				if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(propReference, out var metadata) && !metadata.IsMissingObject && (ReferenceFilter == ReferenceFilter.None || metadata.EndlessProp.ReferenceFilter.HasFlag(ReferenceFilter)))
				{
					list.Add(metadata);
				}
			}
			return list;
		}
	}

	protected override void UpdateOriginalSelection(TModel model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "UpdateOriginalSelection", "model", model), this);
		}
		if (model.IsReferenceEmpty())
		{
			return;
		}
		SerializableGuid id = InspectorReferenceUtility.GetId(model);
		foreach (AssetReference propReference in MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.GameLibrary.PropReferences)
		{
			if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(propReference, out var metadata) && !metadata.IsMissingObject && (SerializableGuid)metadata.PropData.AssetID == id)
			{
				originalSelection.Add(metadata);
				break;
			}
		}
	}
}
