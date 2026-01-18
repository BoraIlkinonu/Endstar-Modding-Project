using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UIRolesListModel : UIBaseLocalFilterableListModel<Roles>
{
	public Roles LocalClientRole { get; private set; } = Roles.None;

	protected override Comparison<Roles> DefaultSort => (Roles x, Roles y) => x.Level().CompareTo(y.Level());

	private void OnEnable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnable");
		}
		List<Roles> list = new List<Roles>();
		foreach (Roles value in Enum.GetValues(typeof(Roles)))
		{
			if (value != Roles.None)
			{
				list.Add(value);
			}
		}
		Set(list, triggerEvents: true);
	}

	public void SetLocalClientRole(Roles localClientRole)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetLocalClientRole", localClientRole);
		}
		LocalClientRole = localClientRole;
		UIBaseListModel<Roles>.ModelChangedAction?.Invoke(this);
		base.ModelChangedUnityEvent.Invoke();
	}
}
