using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;

namespace Endless.Creator.UI
{
	// Token: 0x02000194 RID: 404
	public class UIUserRoleListModel : UIBaseListModel<UserRole>
	{
		// Token: 0x17000099 RID: 153
		// (get) Token: 0x060005E5 RID: 1509 RVA: 0x0001E64A File Offset: 0x0001C84A
		// (set) Token: 0x060005E6 RID: 1510 RVA: 0x0001E652 File Offset: 0x0001C852
		public UIUserRolesModel UserRolesModel { get; private set; }

		// Token: 0x1700009A RID: 154
		// (get) Token: 0x060005E7 RID: 1511 RVA: 0x0001E65B File Offset: 0x0001C85B
		// (set) Token: 0x060005E8 RID: 1512 RVA: 0x0001E663 File Offset: 0x0001C863
		public int OwnerCount { get; private set; }

		// Token: 0x060005E9 RID: 1513 RVA: 0x0001E66C File Offset: 0x0001C86C
		public override void Set(List<UserRole> list, bool triggerEvents)
		{
			this.OwnerCount = list.Count((UserRole item) => item.Role == Roles.Owner);
			list.Sort((UserRole x, UserRole y) => y.Role.Level().CompareTo(x.Role.Level()));
			int localClientUserId = EndlessServices.Instance.CloudService.ActiveUserId;
			int num = list.FindIndex((UserRole item) => item.UserId == localClientUserId);
			if (num > -1)
			{
				UserRole userRole = list[num];
				list.RemoveAt(num);
				list.Insert(0, userRole);
			}
			base.Set(list, triggerEvents);
		}

		// Token: 0x060005EA RID: 1514 RVA: 0x0001E71C File Offset: 0x0001C91C
		public int IndexOf(int userId)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "IndexOf", new object[] { userId });
			}
			for (int i = 0; i < this.Count; i++)
			{
				if (this[i].UserId == userId)
				{
					return i;
				}
			}
			return -1;
		}
	}
}
