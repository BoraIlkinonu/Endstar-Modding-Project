using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200013D RID: 317
	public class UILevelStateTemplateListCellController : UIBaseListCellController<LevelStateTemplateSourceBase>
	{
		// Token: 0x060004EF RID: 1263 RVA: 0x0001BBE7 File Offset: 0x00019DE7
		protected override void Start()
		{
			base.Start();
			this.selectButton.onClick.AddListener(new UnityAction(this.Select));
		}

		// Token: 0x060004F0 RID: 1264 RVA: 0x0001BC0C File Offset: 0x00019E0C
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			throw new NotImplementedException();
		}

		// Token: 0x0400048D RID: 1165
		[Header("UILevelStateTemplateListCellController")]
		[SerializeField]
		private UIButton selectButton;
	}
}
