using System;
using Endless.Props.Scripting;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200011E RID: 286
	public class UIInspectorScriptValueListCellController : UIBaseListCellController<InspectorScriptValue>
	{
		// Token: 0x06000483 RID: 1155 RVA: 0x0001A63A File Offset: 0x0001883A
		protected override void Start()
		{
			base.Start();
			this.removeButton.onClick.AddListener(new UnityAction(this.Remove));
		}

		// Token: 0x06000484 RID: 1156 RVA: 0x000190B2 File Offset: 0x000172B2
		protected override void OnAddButton()
		{
			throw new NotImplementedException();
		}

		// Token: 0x0400044C RID: 1100
		[Header("UIInspectorScriptValueListCellController")]
		[SerializeField]
		private UIButton removeButton;
	}
}
