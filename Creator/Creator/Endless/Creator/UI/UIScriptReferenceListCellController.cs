using System;
using Endless.Props.Assets;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000179 RID: 377
	public class UIScriptReferenceListCellController : UIBaseListCellController<ScriptReference>
	{
		// Token: 0x06000592 RID: 1426 RVA: 0x0001D7E2 File Offset: 0x0001B9E2
		protected override void Start()
		{
			base.Start();
			this.removeButton.onClick.AddListener(new UnityAction(this.Remove));
		}

		// Token: 0x06000593 RID: 1427 RVA: 0x000190B2 File Offset: 0x000172B2
		protected override void OnAddButton()
		{
			throw new NotImplementedException();
		}

		// Token: 0x040004EE RID: 1262
		[Header("UIScriptReferenceListCellView")]
		[SerializeField]
		private UIButton removeButton;
	}
}
