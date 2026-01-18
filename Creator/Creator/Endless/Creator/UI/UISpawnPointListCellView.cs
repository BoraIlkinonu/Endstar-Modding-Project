using System;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x0200017F RID: 383
	public class UISpawnPointListCellView : UIBaseListCellView<UISpawnPoint>
	{
		// Token: 0x060005A4 RID: 1444 RVA: 0x0001DA06 File Offset: 0x0001BC06
		protected override void Start()
		{
			base.Start();
			this.hideExtraEditButtonsTweens.SetToEnd();
		}

		// Token: 0x060005A5 RID: 1445 RVA: 0x0001DA19 File Offset: 0x0001BC19
		public override void OnDespawn()
		{
			base.OnDespawn();
			this.extraEditButtonsActive = false;
			this.hideExtraEditButtonsTweens.SetToEnd();
		}

		// Token: 0x060005A6 RID: 1446 RVA: 0x0001DA34 File Offset: 0x0001BC34
		public override void View(UIBaseListView<UISpawnPoint> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			UISpawnPointListModel uispawnPointListModel = (UISpawnPointListModel)base.ListModel;
			bool localClientCanSelect = uispawnPointListModel.LocalClientCanSelect;
			UISpawnPointListView uispawnPointListView = base.ListView as UISpawnPointListView;
			this.toggleSelectedButton.interactable = localClientCanSelect && uispawnPointListView.CanSelect;
			this.displayNameText.text = base.Model.DisplayName;
			GameObject[] array = this.setActiveIfLocalClientCanSelect;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(localClientCanSelect);
			}
			this.moveUpButton.interactable = dataIndex > 0;
			this.moveDownButton.interactable = dataIndex < base.ListModel.Count - 1;
			this.removeButton.gameObject.SetActive(uispawnPointListModel.UserCanRemove);
		}

		// Token: 0x060005A7 RID: 1447 RVA: 0x0001DAF8 File Offset: 0x0001BCF8
		public void ToggleExtraEditButtons()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ToggleExtraEditButtons", Array.Empty<object>());
			}
			if (this.extraEditButtonsActive)
			{
				this.hideExtraEditButtonsTweens.Tween();
			}
			else
			{
				this.displayExtraEditButtonsTweens.Tween();
			}
			this.extraEditButtonsActive = !this.extraEditButtonsActive;
		}

		// Token: 0x040004F9 RID: 1273
		[Header("UISpawnPointListCellView")]
		[SerializeField]
		private UIButton toggleSelectedButton;

		// Token: 0x040004FA RID: 1274
		[SerializeField]
		private TextMeshProUGUI displayNameText;

		// Token: 0x040004FB RID: 1275
		[SerializeField]
		private GameObject[] setActiveIfLocalClientCanSelect = Array.Empty<GameObject>();

		// Token: 0x040004FC RID: 1276
		[SerializeField]
		private UIButton moveUpButton;

		// Token: 0x040004FD RID: 1277
		[SerializeField]
		private UIButton moveDownButton;

		// Token: 0x040004FE RID: 1278
		[SerializeField]
		private UIButton removeButton;

		// Token: 0x040004FF RID: 1279
		[SerializeField]
		private TweenCollection displayExtraEditButtonsTweens;

		// Token: 0x04000500 RID: 1280
		[SerializeField]
		private TweenCollection hideExtraEditButtonsTweens;

		// Token: 0x04000501 RID: 1281
		private bool extraEditButtonsActive;
	}
}
