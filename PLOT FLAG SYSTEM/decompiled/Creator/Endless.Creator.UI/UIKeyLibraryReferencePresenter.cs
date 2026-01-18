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

public class UIKeyLibraryReferencePresenter : UIBaseInventoryLibraryReferencePresenter<KeyLibraryReference>
{
	protected override string IEnumerableWindowTitle => "Select a Key";

	protected override SelectionType SelectionType => SelectionType.Select0To1;

	protected override void Start()
	{
		base.Start();
		(base.View.Interface as UIKeyLibraryReferenceView).OnLockedChanged += SetLocked;
	}

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
						KeyLibraryReference item = ReferenceFactory.CreateKeyLibraryReference(propEntry.AssetId);
						list.Add(item);
					}
					else
					{
						DebugUtility.LogException(new InvalidCastException("selection's element type must be of type RuntimePropInfo or PropEntry"), this);
					}
				}
				else
				{
					KeyLibraryReference item2 = ReferenceFactory.CreateKeyLibraryReference(runtimePropInfo.PropData.AssetID);
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

	protected override KeyLibraryReference CreateDefaultModel()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "CreateDefaultModel");
		}
		return ReferenceFactory.CreateKeyLibraryReference(SerializableGuid.Empty);
	}

	private void SetLocked(bool locked)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetLocked", locked);
		}
		SerializableGuid newAssetId = (locked ? ((SerializableGuid)"0e5eb3a8-f10b-4ee1-b0db-02400a34f63e") : SerializableGuid.Empty);
		InspectorReferenceUtility.SetId(base.Model, newAssetId);
		SetModelAndTriggerOnModelChanged(base.Model);
	}
}
