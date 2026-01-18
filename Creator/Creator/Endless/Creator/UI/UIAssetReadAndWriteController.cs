using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200009A RID: 154
	public abstract class UIAssetReadAndWriteController : UIGameObject, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x17000024 RID: 36
		// (get) Token: 0x06000263 RID: 611 RVA: 0x000111DE File Offset: 0x0000F3DE
		// (set) Token: 0x06000264 RID: 612 RVA: 0x000111E6 File Offset: 0x0000F3E6
		private protected UIInputField NameInputField { protected get; private set; }

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x06000265 RID: 613 RVA: 0x000111EF File Offset: 0x0000F3EF
		// (set) Token: 0x06000266 RID: 614 RVA: 0x000111F7 File Offset: 0x0000F3F7
		private protected UIInputField DescriptionInputField { protected get; private set; }

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x06000267 RID: 615 RVA: 0x00011200 File Offset: 0x0000F400
		// (set) Token: 0x06000268 RID: 616 RVA: 0x00011208 File Offset: 0x0000F408
		protected bool VerboseLogging { get; set; }

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x06000269 RID: 617 RVA: 0x00011211 File Offset: 0x0000F411
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x0600026A RID: 618 RVA: 0x00011219 File Offset: 0x0000F419
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x0600026B RID: 619 RVA: 0x00011224 File Offset: 0x0000F424
		protected virtual void Start()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.NameInputField.onSubmit.AddListener(new UnityAction<string>(this.SetName));
			this.NameInputField.onDeselect.AddListener(new UnityAction<string>(this.SetName));
			this.DescriptionInputField.onSubmit.AddListener(new UnityAction<string>(this.SetDescription));
			this.DescriptionInputField.onDeselect.AddListener(new UnityAction<string>(this.SetDescription));
		}

		// Token: 0x0600026C RID: 620
		protected abstract void SetName(string newValue);

		// Token: 0x0600026D RID: 621
		protected abstract void SetDescription(string newValue);
	}
}
