using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000183 RID: 387
	public abstract class UIBaseFilterableListController<T> : UIBaseListController<T>
	{
		// Token: 0x170001A9 RID: 425
		// (get) Token: 0x0600098F RID: 2447 RVA: 0x00028E80 File Offset: 0x00027080
		protected string StringFilter
		{
			get
			{
				return this.stringFilterInputField.text;
			}
		}

		// Token: 0x170001AA RID: 426
		// (get) Token: 0x06000990 RID: 2448 RVA: 0x00028E8D File Offset: 0x0002708D
		protected bool CaseSensitive
		{
			get
			{
				return this.caseSensitive;
			}
		}

		// Token: 0x06000991 RID: 2449 RVA: 0x00028E98 File Offset: 0x00027098
		protected override void Start()
		{
			base.Start();
			this.stringFilterInputField.onValueChanged.AddListener(new UnityAction<string>(this.GoToStart));
			this.stringFilterInputField.onValueChanged.AddListener(new UnityAction<string>(this.SetStringFilter));
		}

		// Token: 0x06000992 RID: 2450 RVA: 0x00028EE4 File Offset: 0x000270E4
		private void OnDisable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnDisable", this);
			}
			if (!this.clearFilterOnDisable)
			{
				return;
			}
			this.stringFilterInputField.Clear(true);
		}

		// Token: 0x06000993 RID: 2451 RVA: 0x00028F0E File Offset: 0x0002710E
		public void SetCanFilterByString(bool newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetCanFilterByString", "newValue", newValue), this);
			}
			this.canFilterByString = newValue;
			this.ViewControls();
		}

		// Token: 0x06000994 RID: 2452 RVA: 0x00028F45 File Offset: 0x00027145
		protected override void ViewControls()
		{
			base.ViewControls();
			this.stringFilterInputField.transform.parent.gameObject.SetActive(this.canFilterByString);
		}

		// Token: 0x06000995 RID: 2453
		protected abstract void SetStringFilter(string toFilterBy);

		// Token: 0x06000996 RID: 2454 RVA: 0x00028F6D File Offset: 0x0002716D
		private void GoToStart(string toFilterBy)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("GoToStart ( toFilterBy: " + toFilterBy + " )", this);
			}
			base.View.GoToStart();
		}

		// Token: 0x04000603 RID: 1539
		[Header("UIBaseFilterableListController")]
		[SerializeField]
		private bool canFilterByString = true;

		// Token: 0x04000604 RID: 1540
		[SerializeField]
		private UIInputField stringFilterInputField;

		// Token: 0x04000605 RID: 1541
		[SerializeField]
		private bool caseSensitive;

		// Token: 0x04000606 RID: 1542
		[SerializeField]
		private bool clearFilterOnDisable = true;
	}
}
