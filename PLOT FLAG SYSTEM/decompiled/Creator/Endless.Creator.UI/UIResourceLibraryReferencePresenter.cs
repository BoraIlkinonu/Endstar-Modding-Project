using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UIResourceLibraryReferencePresenter : UIInspectorPropReferencePresenter<ResourceLibraryReference>
{
	protected override string IEnumerableWindowTitle => "Select a Resource";

	protected override SelectionType SelectionType => SelectionType.Select0To1;

	protected override ResourceLibraryReference CreateDefaultModel()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "CreateDefaultModel");
		}
		return ReferenceFactory.CreateResourceLibraryReference(SerializableGuid.Empty);
	}

	protected override void SetSelection(List<object> selection)
	{
		List<object> list = new List<object>();
		foreach (object item2 in selection)
		{
			ResourceLibraryReference item = ReferenceFactory.CreateResourceLibraryReference((item2 as PropLibrary.RuntimePropInfo).PropData.AssetID);
			list.Add(item);
		}
		base.SetSelection(list);
	}
}
