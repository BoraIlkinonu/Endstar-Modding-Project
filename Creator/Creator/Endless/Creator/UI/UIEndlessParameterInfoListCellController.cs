using System;
using Endless.Props.Scripting;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020000F5 RID: 245
	public class UIEndlessParameterInfoListCellController : UIBaseListCellController<EndlessParameterInfo>
	{
		// Token: 0x06000400 RID: 1024 RVA: 0x000191A1 File Offset: 0x000173A1
		protected override void Start()
		{
			base.Start();
			this.removeButton.onClick.AddListener(new UnityAction(this.Remove));
		}

		// Token: 0x06000401 RID: 1025 RVA: 0x000191C6 File Offset: 0x000173C6
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			throw new NotImplementedException();
		}

		// Token: 0x04000414 RID: 1044
		[Header("UIEndlessParameterInfoListCellController")]
		[SerializeField]
		private UIButton removeButton;
	}
}
