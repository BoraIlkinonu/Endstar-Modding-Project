using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI;

public class UIInstanceReferencePresenter : UIBaseInstanceReferencePresenter<InstanceReference>
{
	protected override string IEnumerableWindowTitle => "Select an Instance";

	protected override void Start()
	{
		base.Start();
		if (object.Equals(base.View.Interface.StyleEnum, UIInstanceReferenceView.Styles.NoneOrContext))
		{
			(base.View.Interface as UIInstanceReferenceNoneOrContextView).NoneOrContextChanged += base.SetModelAndTriggerOnModelChanged;
		}
	}

	protected override InstanceReference ConstructNewModelWithInstanceId(SerializableGuid instanceId)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ConstructNewModelWithInstanceId", "instanceId", instanceId), this);
		}
		return ReferenceFactory.CreateInstanceReference(instanceId, useContext: false);
	}

	protected override void SetSelection(List<object> selection)
	{
		List<object> list = new List<object>();
		foreach (object item2 in selection)
		{
			PropEntry obj = item2 as PropEntry;
			InstanceReference item = ReferenceFactory.CreateInstanceReference(useContext: obj == UIPropEntryView.UseContext, instanceId: obj.InstanceId);
			list.Add(item);
		}
		base.SetSelection(list);
	}

	protected override InstanceReference CreateDefaultModel()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("CreateDefaultModel", this);
		}
		return ReferenceFactory.CreateInstanceReference(SerializableGuid.Empty, useContext: false);
	}
}
