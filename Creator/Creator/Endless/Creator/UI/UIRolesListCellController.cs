using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200015D RID: 349
	public class UIRolesListCellController : UIBaseListCellController<Roles>
	{
		// Token: 0x06000539 RID: 1337 RVA: 0x0001C712 File Offset: 0x0001A912
		protected override void Start()
		{
			base.Start();
			this.selectButton.onClick.AddListener(new UnityAction(this.Select));
		}

		// Token: 0x0600053A RID: 1338 RVA: 0x0001C737 File Offset: 0x0001A937
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			throw new NotImplementedException();
		}

		// Token: 0x0600053B RID: 1339 RVA: 0x0001C756 File Offset: 0x0001A956
		protected override void Select()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Select", Array.Empty<object>());
			}
			Action<Roles> onSelected = UIRolesListCellController.OnSelected;
			if (onSelected == null)
			{
				return;
			}
			onSelected(base.Model);
		}

		// Token: 0x040004BB RID: 1211
		public static Action<Roles> OnSelected;

		// Token: 0x040004BC RID: 1212
		[Header("UIRolesListCellController")]
		[SerializeField]
		private UIButton selectButton;
	}
}
