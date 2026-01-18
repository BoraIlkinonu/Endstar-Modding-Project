using System;
using System.Linq;
using Endless.Shared.Debugging;

namespace Endless.Shared.UI
{
	// Token: 0x0200017B RID: 379
	public abstract class UIBaseListRowView<T> : UIBaseListItemView<T>
	{
		// Token: 0x170001A2 RID: 418
		// (get) Token: 0x06000963 RID: 2403 RVA: 0x0002859F File Offset: 0x0002679F
		// (set) Token: 0x06000964 RID: 2404 RVA: 0x000285A7 File Offset: 0x000267A7
		public UIBaseListItemView<T>[] Cells { get; private set; } = Array.Empty<UIBaseListItemView<T>>();

		// Token: 0x170001A3 RID: 419
		// (get) Token: 0x06000965 RID: 2405 RVA: 0x000285B0 File Offset: 0x000267B0
		public UIBaseListItemView<T>[] ActiveCells
		{
			get
			{
				UIBaseListItemView<T>[] array = new UIBaseListItemView<T>[this.activeCellCount];
				for (int i = 0; i < this.activeCellCount; i++)
				{
					array[i] = this.Cells[i];
				}
				return array;
			}
		}

		// Token: 0x170001A4 RID: 420
		// (get) Token: 0x06000966 RID: 2406 RVA: 0x000285E6 File Offset: 0x000267E6
		public override bool IsRow { get; } = true;

		// Token: 0x170001A5 RID: 421
		// (get) Token: 0x06000967 RID: 2407 RVA: 0x000285EE File Offset: 0x000267EE
		// (set) Token: 0x06000968 RID: 2408 RVA: 0x000285F6 File Offset: 0x000267F6
		public int DataIndexStart { get; private set; }

		// Token: 0x170001A6 RID: 422
		// (get) Token: 0x06000969 RID: 2409 RVA: 0x000285FF File Offset: 0x000267FF
		// (set) Token: 0x0600096A RID: 2410 RVA: 0x00028607 File Offset: 0x00026807
		public int DataIndexEnd { get; private set; }

		// Token: 0x0600096B RID: 2411 RVA: 0x00028610 File Offset: 0x00026810
		public override void Validate()
		{
			base.Validate();
			DebugUtility.DebugHasNullItem<UIBaseListItemView<T>>(this.Cells, "Cells", this);
		}

		// Token: 0x0600096C RID: 2412 RVA: 0x0002862C File Offset: 0x0002682C
		public override void View(UIBaseListView<T> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.DataIndexStart = dataIndex * this.Cells.Length;
			this.DataIndexEnd = this.DataIndexStart + this.Cells.Length - 1;
			UIBaseListModel<T> model = listView.Model;
			int num = this.DataIndexStart;
			this.activeCellCount = 0;
			foreach (UIBaseListItemView<T> uibaseListItemView in this.Cells)
			{
				bool flag = num < model.Count;
				uibaseListItemView.gameObject.SetActive(flag);
				if (flag)
				{
					uibaseListItemView.View(listView, num);
					this.activeCellCount++;
				}
				num++;
			}
			this.DataIndexEnd = this.DataIndexStart + this.activeCellCount;
		}

		// Token: 0x0600096D RID: 2413 RVA: 0x000286E4 File Offset: 0x000268E4
		public override void OnDespawn()
		{
			base.OnDespawn();
			foreach (UIBaseListItemView<T> uibaseListItemView in this.Cells)
			{
				uibaseListItemView.OnDespawn();
				uibaseListItemView.gameObject.SetActive(false);
			}
		}

		// Token: 0x0600096E RID: 2414 RVA: 0x00028720 File Offset: 0x00026920
		public UIBaseListItemView<T> GetCellViewAtDataIndex(int dataIndex)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetCellViewAtDataIndex", "dataIndex", dataIndex), this);
			}
			return this.Cells.FirstOrDefault((UIBaseListItemView<T> cellView) => cellView.DataIndex == dataIndex);
		}

		// Token: 0x0600096F RID: 2415 RVA: 0x00028780 File Offset: 0x00026980
		public bool ContainsDataIndex(int dataIndex)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ContainsDataIndex", "dataIndex", dataIndex), this);
			}
			return dataIndex >= this.DataIndexStart && dataIndex <= this.DataIndexEnd;
		}

		// Token: 0x06000970 RID: 2416 RVA: 0x000287CC File Offset: 0x000269CC
		public void ResetAllCellPositions()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("ResetAllCellPositions", this);
			}
			UIBaseListItemView<T>[] cells = this.Cells;
			for (int i = 0; i < cells.Length; i++)
			{
				(cells[i] as UIBaseListCellView<T>).Container.SetAnchor(AnchorPresets.StretchAll, 0f, 0f, 0f, 0f);
			}
		}

		// Token: 0x040005F0 RID: 1520
		protected float SetDataTweensDelay = 0.125f;

		// Token: 0x040005F1 RID: 1521
		private int activeCellCount;
	}
}
