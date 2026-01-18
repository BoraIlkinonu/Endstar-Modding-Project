using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI;

public class UINpcInstanceReferencePresenter : UIBaseInstanceReferencePresenter<NpcInstanceReference>
{
	protected override string IEnumerableWindowTitle => "Select an NPC";

	protected override void Start()
	{
		base.Start();
		if (object.Equals(base.View.Interface.StyleEnum, UINpcInstanceReferenceView.Styles.NoneOrContext))
		{
			(base.View.Interface as UINpcInstanceReferenceNoneOrContextView).NoneOrContextChanged += base.SetModelAndTriggerOnModelChanged;
		}
	}

	protected override void SetSelection(List<object> selection)
	{
		if (selection.Any())
		{
			List<object> list = new List<object>();
			foreach (object item2 in selection)
			{
				if (!(item2 is PropEntry propEntry))
				{
					DebugUtility.LogException(new InvalidCastException("selection's element type must be of type PropEntry"), this);
					continue;
				}
				bool useContext = propEntry.InstanceId == UIPropEntryView.UseContext.InstanceId;
				NpcInstanceReference item = ReferenceFactory.CreateNpcInstanceReference(propEntry.InstanceId, useContext);
				list.Add(item);
			}
			base.SetSelection(list);
		}
		else
		{
			base.SetSelection(selection);
		}
	}

	protected override NpcInstanceReference ConstructNewModelWithInstanceId(SerializableGuid instanceId)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ConstructNewModelWithInstanceId", "instanceId", instanceId), this);
		}
		return ReferenceFactory.CreateNpcInstanceReference(instanceId, useContext: false);
	}

	protected override NpcInstanceReference CreateDefaultModel()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("CreateDefaultModel", this);
		}
		return ReferenceFactory.CreateNpcInstanceReference(SerializableGuid.Empty, useContext: false);
	}
}
