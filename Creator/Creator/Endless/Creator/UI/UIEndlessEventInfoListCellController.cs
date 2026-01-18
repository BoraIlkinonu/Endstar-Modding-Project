using System;
using Endless.Props.Scripting;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020000EF RID: 239
	public class UIEndlessEventInfoListCellController : UIBaseListCellController<EndlessEventInfo>
	{
		// Token: 0x060003F3 RID: 1011 RVA: 0x0001908D File Offset: 0x0001728D
		protected override void Start()
		{
			base.Start();
			this.removeButton.onClick.AddListener(new UnityAction(this.Remove));
		}

		// Token: 0x060003F4 RID: 1012 RVA: 0x000190B2 File Offset: 0x000172B2
		protected override void OnAddButton()
		{
			throw new NotImplementedException();
		}

		// Token: 0x04000409 RID: 1033
		[Header("UIEndlessEventInfoListCellController")]
		[SerializeField]
		private UIButton removeButton;
	}
}
