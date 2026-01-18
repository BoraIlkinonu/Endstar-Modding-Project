using System;
using System.Collections.Generic;
using System.Linq;

namespace Endless.Shared.UI
{
	// Token: 0x02000281 RID: 641
	public class UIIEnumerableWindowModel
	{
		// Token: 0x06001003 RID: 4099 RVA: 0x0004481C File Offset: 0x00042A1C
		public UIIEnumerableWindowModel(int windowCanvasOverrideSorting, string title, UIBaseIEnumerableView.ArrangementStyle arrangementStyle, SelectionType? selectionType, IEnumerable<object> items, Dictionary<Type, Enum> typeStyleOverrideDictionary, List<object> originalSelection, Action<List<object>> onSelectionChanged, Action<List<object>> onSelectionConfirmed)
		{
			this.WindowCanvasOverrideSorting = windowCanvasOverrideSorting;
			this.Title = title;
			this.Style = arrangementStyle;
			this.SelectionType = selectionType;
			this.Items = items;
			this.TypeStyleOverrideDictionary = new Dictionary<Type, Enum>(typeStyleOverrideDictionary);
			this.OriginalSelection = new List<object>(originalSelection);
			this.OnSelectionChanged = onSelectionChanged;
			this.OnSelectionConfirmed = onSelectionConfirmed;
		}

		// Token: 0x17000308 RID: 776
		// (get) Token: 0x06001004 RID: 4100 RVA: 0x0004487E File Offset: 0x00042A7E
		// (set) Token: 0x06001005 RID: 4101 RVA: 0x00044886 File Offset: 0x00042A86
		public int WindowCanvasOverrideSorting { get; private set; }

		// Token: 0x17000309 RID: 777
		// (get) Token: 0x06001006 RID: 4102 RVA: 0x0004488F File Offset: 0x00042A8F
		// (set) Token: 0x06001007 RID: 4103 RVA: 0x00044897 File Offset: 0x00042A97
		public string Title { get; private set; }

		// Token: 0x1700030A RID: 778
		// (get) Token: 0x06001008 RID: 4104 RVA: 0x000448A0 File Offset: 0x00042AA0
		// (set) Token: 0x06001009 RID: 4105 RVA: 0x000448A8 File Offset: 0x00042AA8
		public UIBaseIEnumerableView.ArrangementStyle Style { get; private set; }

		// Token: 0x1700030B RID: 779
		// (get) Token: 0x0600100A RID: 4106 RVA: 0x000448B1 File Offset: 0x00042AB1
		// (set) Token: 0x0600100B RID: 4107 RVA: 0x000448B9 File Offset: 0x00042AB9
		public SelectionType? SelectionType { get; private set; }

		// Token: 0x1700030C RID: 780
		// (get) Token: 0x0600100C RID: 4108 RVA: 0x000448C2 File Offset: 0x00042AC2
		// (set) Token: 0x0600100D RID: 4109 RVA: 0x000448CA File Offset: 0x00042ACA
		public IEnumerable<object> Items { get; private set; }

		// Token: 0x1700030D RID: 781
		// (get) Token: 0x0600100E RID: 4110 RVA: 0x000448D3 File Offset: 0x00042AD3
		public Dictionary<Type, Enum> TypeStyleOverrideDictionary { get; }

		// Token: 0x1700030E RID: 782
		// (get) Token: 0x0600100F RID: 4111 RVA: 0x000448DB File Offset: 0x00042ADB
		// (set) Token: 0x06001010 RID: 4112 RVA: 0x000448E3 File Offset: 0x00042AE3
		public List<object> OriginalSelection { get; private set; }

		// Token: 0x1700030F RID: 783
		// (get) Token: 0x06001011 RID: 4113 RVA: 0x000448EC File Offset: 0x00042AEC
		// (set) Token: 0x06001012 RID: 4114 RVA: 0x000448F4 File Offset: 0x00042AF4
		public Action<List<object>> OnSelectionChanged { get; private set; }

		// Token: 0x17000310 RID: 784
		// (get) Token: 0x06001013 RID: 4115 RVA: 0x000448FD File Offset: 0x00042AFD
		// (set) Token: 0x06001014 RID: 4116 RVA: 0x00044905 File Offset: 0x00042B05
		public Action<List<object>> OnSelectionConfirmed { get; private set; }

		// Token: 0x06001015 RID: 4117 RVA: 0x00044910 File Offset: 0x00042B10
		public override string ToString()
		{
			return string.Format("{0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}, {8}: {9}, {10}: {11}, {12}: {13}", new object[]
			{
				"WindowCanvasOverrideSorting",
				this.WindowCanvasOverrideSorting,
				"Title",
				this.Title,
				"Style",
				this.Style,
				"SelectionType",
				this.SelectionType,
				"Items",
				this.Items.Count<object>(),
				"TypeStyleOverrideDictionary",
				this.TypeStyleOverrideDictionary.Count,
				"OriginalSelection",
				this.OriginalSelection.Count<object>()
			});
		}
	}
}
