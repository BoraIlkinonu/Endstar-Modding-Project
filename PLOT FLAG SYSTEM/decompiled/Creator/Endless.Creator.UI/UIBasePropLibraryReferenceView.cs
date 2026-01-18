using System;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI;

public abstract class UIBasePropLibraryReferenceView<TModel, TViewStyle> : UIInspectorPropReferenceView<TModel, TViewStyle> where TModel : PropLibraryReference where TViewStyle : Enum
{
	protected string GetPropLibraryReferenceName(PropLibraryReference model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetPropLibraryReferenceName", "model", model), this);
		}
		if (model.IsReferenceEmpty())
		{
			return model.GetReferenceName();
		}
		SerializableGuid id = InspectorReferenceUtility.GetId(model);
		if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(id, out var _))
		{
			return "Missing";
		}
		return model.GetReferenceName();
	}
}
