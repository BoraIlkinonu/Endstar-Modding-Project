using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UIInventoryLibraryReferencePresenter : UIBasePropLibraryReferencePresenter<InventoryLibraryReference>
{
	protected override string IEnumerableWindowTitle => "Select an Inventory Item";

	protected override SelectionType SelectionType => SelectionType.Select0To1;

	protected override void SetSelection(List<object> selection)
	{
		if (selection.Any())
		{
			List<object> list = new List<object>();
			foreach (object item3 in selection)
			{
				if (!(item3 is PropLibrary.RuntimePropInfo runtimePropInfo))
				{
					if (item3 is PropEntry propEntry)
					{
						InventoryLibraryReference item = ReferenceFactory.CreateInventoryLibraryReference(propEntry.AssetId);
						list.Add(item);
					}
					else
					{
						DebugUtility.LogException(new InvalidCastException("selection's element type must be of type RuntimePropInfo or PropEntry"), this);
					}
				}
				else
				{
					InventoryLibraryReference item2 = ReferenceFactory.CreateInventoryLibraryReference(runtimePropInfo.PropData.AssetID);
					list.Add(item2);
				}
			}
			base.SetSelection(list);
		}
		else
		{
			base.SetSelection(selection);
		}
	}

	protected override InventoryLibraryReference CreateDefaultModel()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("CreateDefaultModel", this);
		}
		return ReferenceFactory.CreateInventoryLibraryReference(SerializableGuid.Empty);
	}
}
