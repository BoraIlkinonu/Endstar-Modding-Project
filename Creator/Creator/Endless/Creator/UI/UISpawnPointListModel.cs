using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000181 RID: 385
	public class UISpawnPointListModel : UIBaseListModel<UISpawnPoint>, IUIRoleSubscribable
	{
		// Token: 0x1700008F RID: 143
		// (get) Token: 0x060005AA RID: 1450 RVA: 0x0001DB6F File Offset: 0x0001BD6F
		// (set) Token: 0x060005AB RID: 1451 RVA: 0x0001DB77 File Offset: 0x0001BD77
		public bool LocalClientCanSelect { get; private set; } = true;

		// Token: 0x17000090 RID: 144
		// (get) Token: 0x060005AC RID: 1452 RVA: 0x0001DB80 File Offset: 0x0001BD80
		public List<SerializableGuid> SpawnPointIds
		{
			get
			{
				return this.List.Select((UISpawnPoint entry) => entry.Id).ToList<SerializableGuid>();
			}
		}

		// Token: 0x17000091 RID: 145
		// (get) Token: 0x060005AD RID: 1453 RVA: 0x0001DBB4 File Offset: 0x0001BDB4
		public List<SerializableGuid> SelectedSpawnPointIds
		{
			get
			{
				List<SerializableGuid> list = new List<SerializableGuid>();
				for (int i = 0; i < this.Count; i++)
				{
					if (base.IsSelected(i))
					{
						list.Add(this.List[i].Id);
					}
				}
				return list;
			}
		}

		// Token: 0x060005AE RID: 1454 RVA: 0x0001DBF9 File Offset: 0x0001BDF9
		public void OnLocalClientRoleChanged(Roles localClientRole)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnLocalClientRoleChanged", new object[] { localClientRole });
			}
			this.LocalClientCanSelect = localClientRole.IsGreaterThanOrEqualTo(Roles.Editor);
			base.TriggerModelChanged();
		}
	}
}
