using System;
using Endless.Shared;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003CD RID: 973
	public class UINpcClassCustomizationDataListCellView : UIBaseListCellView<NpcClassCustomizationData>
	{
		// Token: 0x060018A4 RID: 6308 RVA: 0x0007241C File Offset: 0x0007061C
		public override void View(UIBaseListView<NpcClassCustomizationData> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			if (this.IsAddButton)
			{
				return;
			}
			this.presentable = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnObjectModelWithDefaultStyle(base.Model, this.container, null);
			this.removeButton.gameObject.SetActive(base.ListModel.UserCanRemove);
		}

		// Token: 0x060018A5 RID: 6309 RVA: 0x00072472 File Offset: 0x00070672
		public override void OnDespawn()
		{
			base.OnDespawn();
			if (this.presentable != null)
			{
				this.presentable.ReturnToPool();
				this.presentable = null;
			}
		}

		// Token: 0x040013C3 RID: 5059
		[Header("UINpcClassCustomizationDataListCellView")]
		[SerializeField]
		private RectTransform container;

		// Token: 0x040013C4 RID: 5060
		[SerializeField]
		private UIButton removeButton;

		// Token: 0x040013C5 RID: 5061
		private IUIPresentable presentable;
	}
}
