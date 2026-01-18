using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x0200015F RID: 351
	public class UIRolesListModel : UIBaseLocalFilterableListModel<Roles>
	{
		// Token: 0x17000084 RID: 132
		// (get) Token: 0x0600053F RID: 1343 RVA: 0x0001C82C File Offset: 0x0001AA2C
		// (set) Token: 0x06000540 RID: 1344 RVA: 0x0001C834 File Offset: 0x0001AA34
		public Roles LocalClientRole { get; private set; } = Roles.None;

		// Token: 0x17000085 RID: 133
		// (get) Token: 0x06000541 RID: 1345 RVA: 0x0001C83D File Offset: 0x0001AA3D
		protected override Comparison<Roles> DefaultSort
		{
			get
			{
				return (Roles x, Roles y) => x.Level().CompareTo(y.Level());
			}
		}

		// Token: 0x06000542 RID: 1346 RVA: 0x0001C860 File Offset: 0x0001AA60
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			List<Roles> list = new List<Roles>();
			foreach (object obj in Enum.GetValues(typeof(Roles)))
			{
				Roles roles = (Roles)obj;
				if (roles != Roles.None)
				{
					list.Add(roles);
				}
			}
			this.Set(list, true);
		}

		// Token: 0x06000543 RID: 1347 RVA: 0x0001C8EC File Offset: 0x0001AAEC
		public void SetLocalClientRole(Roles localClientRole)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetLocalClientRole", new object[] { localClientRole });
			}
			this.LocalClientRole = localClientRole;
			Action<UIBaseListModel<Roles>> modelChangedAction = UIBaseListModel<Roles>.ModelChangedAction;
			if (modelChangedAction != null)
			{
				modelChangedAction(this);
			}
			base.ModelChangedUnityEvent.Invoke();
		}
	}
}
