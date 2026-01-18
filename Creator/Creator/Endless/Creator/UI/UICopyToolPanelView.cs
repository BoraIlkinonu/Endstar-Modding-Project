using System;
using System.Collections.Generic;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200029D RID: 669
	public class UICopyToolPanelView : UIDockableToolPanelView<CopyTool>
	{
		// Token: 0x17000169 RID: 361
		// (get) Token: 0x06000B22 RID: 2850 RVA: 0x000343F6 File Offset: 0x000325F6
		protected override float ListSize
		{
			get
			{
				return this.clientCopyHistoryEntryListView.CompleteHeight;
			}
		}

		// Token: 0x06000B23 RID: 2851 RVA: 0x00034404 File Offset: 0x00032604
		protected override void Start()
		{
			base.Start();
			this.selectMoreDisplayAndHideHandler.SetToHideEnd(true);
			this.Tool.CopyHistoryEntryInserted.AddListener(new UnityAction<ClientCopyHistoryEntry>(this.ViewClientCopyHistoryList));
			this.Tool.CopyHistoryTrimmed.AddListener(new UnityAction<int>(this.ViewClientCopyHistoryList));
			this.Tool.OnSelectedCopyIndexSet.AddListener(new UnityAction<int>(this.ViewClientCopyHistoryList));
			this.clientCopyHistoryEntryListModel.ItemRemovedUnityEvent.AddListener(new UnityAction<int, ClientCopyHistoryEntry>(this.OnItemRemoved));
			this.clientCopyHistoryEntryListModel.ModelChangedUnityEvent.AddListener(new UnityAction(this.HandleSelectMoreVisibility));
		}

		// Token: 0x06000B24 RID: 2852 RVA: 0x000344AF File Offset: 0x000326AF
		public override void Display()
		{
			base.Display();
			this.ViewClientCopyHistoryList();
		}

		// Token: 0x06000B25 RID: 2853 RVA: 0x000344C0 File Offset: 0x000326C0
		protected override float GetMaxPanelHeight()
		{
			float num = base.GetMaxPanelHeight();
			if (this.clientCopyHistoryEntryListModel.Count > 0)
			{
				num += this.selectMoreDisplayAndHideHandler.RectTransform.rect.height;
			}
			return num;
		}

		// Token: 0x06000B26 RID: 2854 RVA: 0x000344FE File Offset: 0x000326FE
		private void ViewClientCopyHistoryList(ClientCopyHistoryEntry clientCopyHistoryEntry)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ViewClientCopyHistoryList", "clientCopyHistoryEntry", clientCopyHistoryEntry), this);
			}
			this.ViewClientCopyHistoryList();
		}

		// Token: 0x06000B27 RID: 2855 RVA: 0x00034529 File Offset: 0x00032729
		private void ViewClientCopyHistoryList(int index)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ViewClientCopyHistoryList", "index", index), this);
			}
			this.ViewClientCopyHistoryList();
		}

		// Token: 0x06000B28 RID: 2856 RVA: 0x0003455C File Offset: 0x0003275C
		private void ViewClientCopyHistoryList()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("ViewClientCopyHistoryList", this);
			}
			List<ClientCopyHistoryEntry> list = new List<ClientCopyHistoryEntry>(this.Tool.ClientCopyHistoryEntries);
			this.clientCopyHistoryEntryListModel.Clear(false);
			this.clientCopyHistoryEntryListModel.Set(list, false);
			this.clientCopyHistoryEntryListModel.ModelChangedUnityEvent.Invoke();
			base.TweenToMaxPanelHeight();
		}

		// Token: 0x06000B29 RID: 2857 RVA: 0x000345BC File Offset: 0x000327BC
		private void OnItemRemoved(int index, ClientCopyHistoryEntry clientCopyHistoryEntry)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnItemRemoved", new object[] { index, clientCopyHistoryEntry.Label });
			}
			this.ViewClientCopyHistoryList();
		}

		// Token: 0x06000B2A RID: 2858 RVA: 0x000345EF File Offset: 0x000327EF
		private void HandleSelectMoreVisibility()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleSelectMoreVisibility", Array.Empty<object>());
			}
			if (this.clientCopyHistoryEntryListModel.Count > 0)
			{
				this.selectMoreDisplayAndHideHandler.Display();
				return;
			}
			this.selectMoreDisplayAndHideHandler.Hide();
		}

		// Token: 0x0400096A RID: 2410
		[Header("UICopyToolPanelView")]
		[SerializeField]
		private UIDisplayAndHideHandler selectMoreDisplayAndHideHandler;

		// Token: 0x0400096B RID: 2411
		[SerializeField]
		private UIClientCopyHistoryEntryListModel clientCopyHistoryEntryListModel;

		// Token: 0x0400096C RID: 2412
		[SerializeField]
		private UIClientCopyHistoryEntryListView clientCopyHistoryEntryListView;
	}
}
