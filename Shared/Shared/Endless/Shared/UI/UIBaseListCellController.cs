using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000175 RID: 373
	public abstract class UIBaseListCellController<T> : UIGameObject
	{
		// Token: 0x17000185 RID: 389
		// (get) Token: 0x06000912 RID: 2322 RVA: 0x000273BD File Offset: 0x000255BD
		// (set) Token: 0x06000913 RID: 2323 RVA: 0x000273C5 File Offset: 0x000255C5
		protected bool VerboseLogging { get; set; }

		// Token: 0x17000186 RID: 390
		// (get) Token: 0x06000914 RID: 2324 RVA: 0x000273CE File Offset: 0x000255CE
		protected int DataIndex
		{
			get
			{
				return this.View.DataIndex;
			}
		}

		// Token: 0x17000187 RID: 391
		// (get) Token: 0x06000915 RID: 2325 RVA: 0x000273DB File Offset: 0x000255DB
		protected int ViewIndex
		{
			get
			{
				return this.View.ViewIndex;
			}
		}

		// Token: 0x17000188 RID: 392
		// (get) Token: 0x06000916 RID: 2326 RVA: 0x000273E8 File Offset: 0x000255E8
		protected UIBaseListView<T> ListView
		{
			get
			{
				return this.View.ListView;
			}
		}

		// Token: 0x17000189 RID: 393
		// (get) Token: 0x06000917 RID: 2327 RVA: 0x000273F5 File Offset: 0x000255F5
		protected UIBaseListModel<T> ListModel
		{
			get
			{
				return this.ListView.Model;
			}
		}

		// Token: 0x1700018A RID: 394
		// (get) Token: 0x06000918 RID: 2328 RVA: 0x00027402 File Offset: 0x00025602
		protected T Model
		{
			get
			{
				return this.ListModel[this.DataIndex];
			}
		}

		// Token: 0x06000919 RID: 2329 RVA: 0x00027415 File Offset: 0x00025615
		protected virtual void Start()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.addButton.onClick.AddListener(new UnityAction(this.OnAddButton));
		}

		// Token: 0x0600091A RID: 2330
		protected abstract void OnAddButton();

		// Token: 0x0600091B RID: 2331 RVA: 0x00027447 File Offset: 0x00025647
		protected virtual void Select()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Select", this);
			}
			this.ListModel.Select(this.DataIndex, true);
		}

		// Token: 0x0600091C RID: 2332 RVA: 0x0002746E File Offset: 0x0002566E
		protected virtual void ToggleSelected()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("ToggleSelected", this);
			}
			this.ListModel.ToggleSelected(this.DataIndex, true);
		}

		// Token: 0x0600091D RID: 2333 RVA: 0x00027498 File Offset: 0x00025698
		protected virtual void Remove()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Remove", this);
			}
			UIBaseLocalFilterableListModel<T> uibaseLocalFilterableListModel = this.ListModel as UIBaseLocalFilterableListModel<T>;
			if (uibaseLocalFilterableListModel != null)
			{
				int num = this.DataIndex;
				if (uibaseLocalFilterableListModel.AddButtonInserted)
				{
					num--;
				}
				uibaseLocalFilterableListModel.RemoveFilteredAt(num, true);
				return;
			}
			this.ListModel.RemoveAt(this.DataIndex, true);
		}

		// Token: 0x040005C0 RID: 1472
		[SerializeField]
		protected UIBaseListItemView<T> View;

		// Token: 0x040005C1 RID: 1473
		[SerializeField]
		private UIButton addButton;
	}
}
