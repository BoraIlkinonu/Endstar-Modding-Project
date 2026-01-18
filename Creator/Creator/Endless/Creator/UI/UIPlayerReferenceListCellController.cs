using System;
using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000144 RID: 324
	public class UIPlayerReferenceListCellController : UIBaseListCellController<PlayerReference>
	{
		// Token: 0x06000502 RID: 1282 RVA: 0x0001BF9C File Offset: 0x0001A19C
		protected override void Start()
		{
			base.Start();
			this.removeButton.onClick.AddListener(new UnityAction(this.Remove));
		}

		// Token: 0x06000503 RID: 1283 RVA: 0x0001BFC4 File Offset: 0x0001A1C4
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			PlayerReference playerReference = new PlayerReference();
			base.ListModel.Add(playerReference, true);
		}

		// Token: 0x0400049B RID: 1179
		[Header("UIPlayerReferenceListCellController")]
		[SerializeField]
		private UIButton removeButton;
	}
}
