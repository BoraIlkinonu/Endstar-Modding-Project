using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200018C RID: 396
	public class UITransformIdentifierListCellController : UIBaseListCellController<UITransformIdentifier>
	{
		// Token: 0x060005CE RID: 1486 RVA: 0x0001E021 File Offset: 0x0001C221
		protected override void Start()
		{
			base.Start();
			this.selectButton.onClick.AddListener(new UnityAction(this.Select));
		}

		// Token: 0x060005CF RID: 1487 RVA: 0x0001E046 File Offset: 0x0001C246
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			throw new NotImplementedException();
		}

		// Token: 0x04000512 RID: 1298
		[Header("UITransformIdentifierListCellController")]
		[SerializeField]
		private UIButton selectButton;
	}
}
