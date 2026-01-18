using System;
using Endless.Assets;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020000A2 RID: 162
	public abstract class UIAssetReadAndWriteView<T> : UIAssetView<T>, IRoleInteractable where T : Asset
	{
		// Token: 0x06000291 RID: 657 RVA: 0x000118FC File Offset: 0x0000FAFC
		public override void View(T model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "View", "model", model), this);
			}
			this.nameInputField.text = model.Name;
			this.descriptionInputField.text = model.Description;
		}

		// Token: 0x06000292 RID: 658 RVA: 0x0001195D File Offset: 0x0000FB5D
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Clear", this);
			}
			this.nameInputField.text = string.Empty;
			this.descriptionInputField.text = string.Empty;
		}

		// Token: 0x06000293 RID: 659 RVA: 0x00011994 File Offset: 0x0000FB94
		public virtual void SetLocalUserCanInteract(bool localUserCanInteract)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetLocalUserCanInteract", "localUserCanInteract", localUserCanInteract), this);
			}
			this.nameInputField.interactable = localUserCanInteract;
			this.descriptionInputField.interactable = localUserCanInteract;
		}

		// Token: 0x040002CA RID: 714
		[SerializeField]
		private UIInputField descriptionInputField;

		// Token: 0x040002CB RID: 715
		[SerializeField]
		private UIInputField nameInputField;
	}
}
