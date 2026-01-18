using System;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI
{
	// Token: 0x0200022C RID: 556
	public abstract class UIBasePropLibraryReferenceView<TModel, TViewStyle> : UIInspectorPropReferenceView<TModel, TViewStyle> where TModel : PropLibraryReference where TViewStyle : Enum
	{
		// Token: 0x06000907 RID: 2311 RVA: 0x0002B0F8 File Offset: 0x000292F8
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
			PropLibrary.RuntimePropInfo runtimePropInfo;
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(id, out runtimePropInfo))
			{
				return "Missing";
			}
			return model.GetReferenceName();
		}
	}
}
