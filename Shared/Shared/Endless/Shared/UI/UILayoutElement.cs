using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000155 RID: 341
	public class UILayoutElement : UIGameObject, ILayoutElement, ILayoutIgnorer
	{
		// Token: 0x17000165 RID: 357
		// (get) Token: 0x0600083E RID: 2110 RVA: 0x000226A4 File Offset: 0x000208A4
		// (set) Token: 0x0600083F RID: 2111 RVA: 0x000226AC File Offset: 0x000208AC
		public bool IgnoreLayout { get; private set; }

		// Token: 0x17000166 RID: 358
		// (get) Token: 0x06000840 RID: 2112 RVA: 0x000226B5 File Offset: 0x000208B5
		public float minWidth
		{
			get
			{
				return this.MinWidthLayoutDimension.Value;
			}
		}

		// Token: 0x17000167 RID: 359
		// (get) Token: 0x06000841 RID: 2113 RVA: 0x000226C2 File Offset: 0x000208C2
		public float preferredWidth
		{
			get
			{
				return this.PreferredWidthLayoutDimension.Value;
			}
		}

		// Token: 0x17000168 RID: 360
		// (get) Token: 0x06000842 RID: 2114 RVA: 0x000226CF File Offset: 0x000208CF
		public float flexibleWidth
		{
			get
			{
				return this.FlexibleWidthLayoutDimension.Value;
			}
		}

		// Token: 0x17000169 RID: 361
		// (get) Token: 0x06000843 RID: 2115 RVA: 0x000226DC File Offset: 0x000208DC
		public float minHeight
		{
			get
			{
				return this.MinHeightLayoutDimension.Value;
			}
		}

		// Token: 0x1700016A RID: 362
		// (get) Token: 0x06000844 RID: 2116 RVA: 0x000226E9 File Offset: 0x000208E9
		public float preferredHeight
		{
			get
			{
				return this.PreferredHeightLayoutDimension.Value;
			}
		}

		// Token: 0x1700016B RID: 363
		// (get) Token: 0x06000845 RID: 2117 RVA: 0x000226F6 File Offset: 0x000208F6
		public float flexibleHeight
		{
			get
			{
				return this.FlexibleHeightLayoutDimension.Value;
			}
		}

		// Token: 0x1700016C RID: 364
		// (get) Token: 0x06000846 RID: 2118 RVA: 0x00022703 File Offset: 0x00020903
		public int layoutPriority
		{
			get
			{
				return this.LayoutPriority;
			}
		}

		// Token: 0x1700016D RID: 365
		// (get) Token: 0x06000847 RID: 2119 RVA: 0x0002270B File Offset: 0x0002090B
		// (set) Token: 0x06000848 RID: 2120 RVA: 0x00022713 File Offset: 0x00020913
		public bool ignoreLayout
		{
			get
			{
				return this.IgnoreLayout;
			}
			set
			{
				if (this.IgnoreLayout == value)
				{
					return;
				}
				this.IgnoreLayout = value;
				this.MarkForUnityLayoutRebuild();
			}
		}

		// Token: 0x06000849 RID: 2121 RVA: 0x000050BB File Offset: 0x000032BB
		public void CalculateLayoutInputHorizontal()
		{
		}

		// Token: 0x0600084A RID: 2122 RVA: 0x000050BB File Offset: 0x000032BB
		public void CalculateLayoutInputVertical()
		{
		}

		// Token: 0x0600084B RID: 2123 RVA: 0x0002272C File Offset: 0x0002092C
		private void MarkForUnityLayoutRebuild()
		{
			if (!base.isActiveAndEnabled)
			{
				return;
			}
			if (this.useUnityLayoutSystem)
			{
				LayoutRebuilder.MarkLayoutForRebuild(base.transform as RectTransform);
			}
		}

		// Token: 0x04000500 RID: 1280
		[Header("Min")]
		public UILayoutDimension MinWidthLayoutDimension = new UILayoutDimension();

		// Token: 0x04000501 RID: 1281
		public UILayoutDimension MinHeightLayoutDimension = new UILayoutDimension();

		// Token: 0x04000502 RID: 1282
		[Header("Preferred")]
		public UILayoutDimension PreferredWidthLayoutDimension = new UILayoutDimension();

		// Token: 0x04000503 RID: 1283
		public UILayoutDimension PreferredHeightLayoutDimension = new UILayoutDimension();

		// Token: 0x04000504 RID: 1284
		[Header("Flexible")]
		[FormerlySerializedAs("FlexibleWidth")]
		public UILayoutDimension FlexibleWidthLayoutDimension = new UILayoutDimension();

		// Token: 0x04000505 RID: 1285
		[FormerlySerializedAs("FlexibleHeight")]
		public UILayoutDimension FlexibleHeightLayoutDimension = new UILayoutDimension();

		// Token: 0x04000506 RID: 1286
		[Header("Layout")]
		public int LayoutPriority = 1;

		// Token: 0x04000507 RID: 1287
		[SerializeField]
		private bool useUnityLayoutSystem;
	}
}
