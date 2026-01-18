using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000280 RID: 640
	public class UIIEnumerableWindowController : UIWindowController
	{
		// Token: 0x17000305 RID: 773
		// (get) Token: 0x06000FFD RID: 4093 RVA: 0x00044717 File Offset: 0x00042917
		private UIIEnumerableWindowView SelectionWindowView
		{
			get
			{
				return this.BaseWindowView as UIIEnumerableWindowView;
			}
		}

		// Token: 0x17000306 RID: 774
		// (get) Token: 0x06000FFE RID: 4094 RVA: 0x00044724 File Offset: 0x00042924
		private UIIEnumerableWindowModel Model
		{
			get
			{
				return this.SelectionWindowView.Model;
			}
		}

		// Token: 0x17000307 RID: 775
		// (get) Token: 0x06000FFF RID: 4095 RVA: 0x00044731 File Offset: 0x00042931
		private UIIEnumerablePresenter iEnumerablePresenter
		{
			get
			{
				return this.SelectionWindowView.IEnumerablePresenter;
			}
		}

		// Token: 0x06001000 RID: 4096 RVA: 0x0004473E File Offset: 0x0004293E
		protected override void Start()
		{
			base.Start();
			this.confirmButton.onClick.AddListener(new UnityAction(this.Confirm));
		}

		// Token: 0x06001001 RID: 4097 RVA: 0x00044764 File Offset: 0x00042964
		private void Confirm()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Confirm", Array.Empty<object>());
			}
			List<object> list = new List<object>(this.iEnumerablePresenter.SelectedItemsList);
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "selected", list.Count), this);
				for (int i = 0; i < list.Count; i++)
				{
					DebugUtility.Log(string.Format("{0}[{1}]: {2}", "selected", i, list[i]), this);
				}
			}
			Action<List<object>> onSelectionConfirmed = this.Model.OnSelectionConfirmed;
			if (onSelectionConfirmed != null)
			{
				onSelectionConfirmed(list);
			}
			this.Close();
		}

		// Token: 0x04000A32 RID: 2610
		[Header("UIIEnumerableWindowController")]
		[SerializeField]
		private UIButton confirmButton;
	}
}
