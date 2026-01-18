using System;

namespace Endless.Shared.UI
{
	// Token: 0x02000156 RID: 342
	public class UILayoutElementValue
	{
		// Token: 0x0600084D RID: 2125 RVA: 0x000227AC File Offset: 0x000209AC
		public void ApplyTo(UILayoutElement layoutElement)
		{
			if (layoutElement == null)
			{
				return;
			}
			UILayoutElementValue.CopyDimensionValues(this.MinWidthLayoutDimension, layoutElement.MinWidthLayoutDimension);
			UILayoutElementValue.CopyDimensionValues(this.MinHeightLayoutDimension, layoutElement.MinHeightLayoutDimension);
			UILayoutElementValue.CopyDimensionValues(this.PreferredWidthLayoutDimension, layoutElement.PreferredWidthLayoutDimension);
			UILayoutElementValue.CopyDimensionValues(this.PreferredHeightLayoutDimension, layoutElement.PreferredHeightLayoutDimension);
			UILayoutElementValue.CopyDimensionValues(this.FlexibleWidthLayoutDimension, layoutElement.FlexibleWidthLayoutDimension);
			UILayoutElementValue.CopyDimensionValues(this.FlexibleHeightLayoutDimension, layoutElement.FlexibleHeightLayoutDimension);
			layoutElement.LayoutPriority = this.LayoutPriority;
			layoutElement.ignoreLayout = this.IgnoreLayout;
		}

		// Token: 0x0600084E RID: 2126 RVA: 0x00022844 File Offset: 0x00020A44
		public void CopyFrom(UILayoutElement layoutElement)
		{
			if (layoutElement == null)
			{
				return;
			}
			UILayoutElementValue.CopyDimensionValues(layoutElement.MinWidthLayoutDimension, this.MinWidthLayoutDimension);
			UILayoutElementValue.CopyDimensionValues(layoutElement.MinHeightLayoutDimension, this.MinHeightLayoutDimension);
			UILayoutElementValue.CopyDimensionValues(layoutElement.PreferredWidthLayoutDimension, this.PreferredWidthLayoutDimension);
			UILayoutElementValue.CopyDimensionValues(layoutElement.PreferredHeightLayoutDimension, this.PreferredHeightLayoutDimension);
			UILayoutElementValue.CopyDimensionValues(layoutElement.FlexibleWidthLayoutDimension, this.FlexibleWidthLayoutDimension);
			UILayoutElementValue.CopyDimensionValues(layoutElement.FlexibleHeightLayoutDimension, this.FlexibleHeightLayoutDimension);
			this.LayoutPriority = layoutElement.LayoutPriority;
			this.IgnoreLayout = layoutElement.ignoreLayout;
		}

		// Token: 0x0600084F RID: 2127 RVA: 0x000228DC File Offset: 0x00020ADC
		private static void CopyDimensionValues(UILayoutDimension source, UILayoutDimension target)
		{
			if (source == null || target == null)
			{
				return;
			}
			target.ExplicitValue = source.ExplicitValue;
			target.Reference = source.Reference;
			target.ReferenceOperator = source.ReferenceOperator;
			target.ReferenceModifier = source.ReferenceModifier;
			target.ReferenceType = source.ReferenceType;
			target.Max = source.Max;
		}

		// Token: 0x04000509 RID: 1289
		public UILayoutDimension MinWidthLayoutDimension = new UILayoutDimension();

		// Token: 0x0400050A RID: 1290
		public UILayoutDimension MinHeightLayoutDimension = new UILayoutDimension();

		// Token: 0x0400050B RID: 1291
		public UILayoutDimension PreferredWidthLayoutDimension = new UILayoutDimension();

		// Token: 0x0400050C RID: 1292
		public UILayoutDimension PreferredHeightLayoutDimension = new UILayoutDimension();

		// Token: 0x0400050D RID: 1293
		public UILayoutDimension FlexibleWidthLayoutDimension = new UILayoutDimension();

		// Token: 0x0400050E RID: 1294
		public UILayoutDimension FlexibleHeightLayoutDimension = new UILayoutDimension();

		// Token: 0x0400050F RID: 1295
		public int LayoutPriority = 1;

		// Token: 0x04000510 RID: 1296
		public bool IgnoreLayout;
	}
}
