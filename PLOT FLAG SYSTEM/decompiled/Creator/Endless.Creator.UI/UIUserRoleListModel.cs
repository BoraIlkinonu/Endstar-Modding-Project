using System.Collections.Generic;
using System.Linq;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIUserRoleListModel : UIBaseListModel<UserRole>
{
	[field: Header("UIUserRoleListModel")]
	[field: SerializeField]
	public UIUserRolesModel UserRolesModel { get; private set; }

	public int OwnerCount { get; private set; }

	public override void Set(List<UserRole> list, bool triggerEvents)
	{
		OwnerCount = list.Count((UserRole userRole) => userRole.Role == Roles.Owner);
		list.Sort((UserRole x, UserRole y) => y.Role.Level().CompareTo(x.Role.Level()));
		int localClientUserId = EndlessServices.Instance.CloudService.ActiveUserId;
		int num = list.FindIndex((UserRole userRole) => userRole.UserId == localClientUserId);
		if (num > -1)
		{
			UserRole item = list[num];
			list.RemoveAt(num);
			list.Insert(0, item);
		}
		base.Set(list, triggerEvents);
	}

	public int IndexOf(int userId)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "IndexOf", userId);
		}
		for (int i = 0; i < Count; i++)
		{
			if (this[i].UserId == userId)
			{
				return i;
			}
		}
		return -1;
	}
}
