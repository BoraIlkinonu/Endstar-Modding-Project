using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000173 RID: 371
	public class UIUserScriptAutocompleteListCellController : UIBaseListCellController<UIUserScriptAutocompleteListModelItem>
	{
		// Token: 0x06000584 RID: 1412 RVA: 0x0001D671 File Offset: 0x0001B871
		protected override void Start()
		{
			base.Start();
			this.selectButton.onClick.AddListener(new UnityAction(this.Select));
		}

		// Token: 0x06000585 RID: 1413 RVA: 0x0001D696 File Offset: 0x0001B896
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			throw new NotImplementedException();
		}

		// Token: 0x06000586 RID: 1414 RVA: 0x0001D6B5 File Offset: 0x0001B8B5
		protected override void Select()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Select", Array.Empty<object>());
			}
			Action<string> onSelect = UIUserScriptAutocompleteListCellController.OnSelect;
			if (onSelect == null)
			{
				return;
			}
			onSelect(base.Model.Value);
		}

		// Token: 0x040004E5 RID: 1253
		public static Action<string> OnSelect;

		// Token: 0x040004E6 RID: 1254
		[Header("UIUserScriptAutocompleteListCellController")]
		[SerializeField]
		private UIButton selectButton;
	}
}
