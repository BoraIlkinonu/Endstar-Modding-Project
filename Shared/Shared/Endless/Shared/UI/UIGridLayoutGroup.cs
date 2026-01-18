using System;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200015E RID: 350
	public class UIGridLayoutGroup : UIBaseLayoutGroup
	{
		// Token: 0x0600086D RID: 2157 RVA: 0x00023AA4 File Offset: 0x00021CA4
		public override float GetSpacingSize(Dimension dimension)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "GetSpacingSize", "dimension", dimension), this);
			}
			if (dimension != Dimension.Width)
			{
				return this.Spacing.y;
			}
			return this.Spacing.x;
		}

		// Token: 0x0600086E RID: 2158 RVA: 0x00023AF3 File Offset: 0x00021CF3
		public override void CalculateLayout()
		{
			if (this.verboseLogging)
			{
				Debug.Log("CalculateLayout", this);
			}
			this.ValidateAndCalculate();
		}

		// Token: 0x0600086F RID: 2159 RVA: 0x00023B0E File Offset: 0x00021D0E
		public override void ApplyLayout()
		{
			if (this.verboseLogging)
			{
				Debug.Log("ApplyLayout", this);
			}
			this.PositionChildren();
		}

		// Token: 0x06000870 RID: 2160 RVA: 0x00023B2C File Offset: 0x00021D2C
		private void ValidateAndCalculate()
		{
			if (this.verboseLogging)
			{
				Debug.Log("ValidateAndCalculate", this);
			}
			this.ConstraintCount = Mathf.Max(1, this.ConstraintCount);
			this.CellAspectRatio = Mathf.Clamp(this.CellAspectRatio, 0.1f, float.MaxValue);
			if (this.SizeMode == UIGridLayoutGroup.CellSizeMode.FixedSize)
			{
				this.calculatedCellSize = this.CellSize;
			}
			else
			{
				this.CalculateAspectRatioCellSize();
			}
			this.CalculateLayoutInputSize();
		}

		// Token: 0x06000871 RID: 2161 RVA: 0x00023B9C File Offset: 0x00021D9C
		private void CalculateAspectRatioCellSize()
		{
			if (this.verboseLogging)
			{
				Debug.Log("CalculateAspectRatioCellSize", this);
			}
			if (this.childLayoutItems.Count == 0)
			{
				this.calculatedCellSize = Vector2.zero;
				return;
			}
			int num;
			int num2;
			this.GetGridDimensions(out num, out num2);
			if (this.StartAxis == UIGridLayoutGroup.Axis.Horizontal)
			{
				float num3 = Mathf.Max(0f, base.RectTransform.rect.width - (float)this.Padding.horizontal - this.Spacing.x * (float)(num - 1)) / (float)num;
				float num4 = num3 / this.CellAspectRatio;
				this.calculatedCellSize = new Vector2(num3, num4);
				return;
			}
			float num5 = Mathf.Max(0f, base.RectTransform.rect.height - (float)this.Padding.vertical - this.Spacing.y * (float)(num2 - 1)) / (float)num2;
			float num6 = num5 * this.CellAspectRatio;
			this.calculatedCellSize = new Vector2(num6, num5);
		}

		// Token: 0x06000872 RID: 2162 RVA: 0x00023C9C File Offset: 0x00021E9C
		private void GetGridDimensions(out int columns, out int rows)
		{
			if (this.verboseLogging)
			{
				Debug.Log("GetGridDimensions", this);
			}
			int count = this.childLayoutItems.Count;
			if (this.ConstraintMode == UIGridLayoutGroup.Constraint.FixedColumnCount)
			{
				columns = this.ConstraintCount;
				rows = ((count > 0) ? Mathf.CeilToInt((float)count / (float)this.ConstraintCount) : 1);
				return;
			}
			if (this.ConstraintMode == UIGridLayoutGroup.Constraint.FixedRowCount)
			{
				rows = this.ConstraintCount;
				columns = ((count > 0) ? Mathf.CeilToInt((float)count / (float)this.ConstraintCount) : 1);
				return;
			}
			if (this.SizeMode == UIGridLayoutGroup.CellSizeMode.FixedSize)
			{
				float width = base.RectTransform.rect.width;
				float height = base.RectTransform.rect.height;
				columns = Mathf.Max(1, Mathf.FloorToInt((width - (float)this.Padding.horizontal + this.Spacing.x + 0.001f) / (this.CellSize.x + this.Spacing.x)));
				rows = ((count > 0) ? Mathf.CeilToInt((float)count / (float)columns) : 1);
				return;
			}
			columns = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt((float)count)));
			rows = ((count > 0) ? Mathf.CeilToInt((float)count / (float)columns) : 1);
		}

		// Token: 0x06000873 RID: 2163 RVA: 0x00023DD4 File Offset: 0x00021FD4
		private void CalculateLayoutInputSize()
		{
			if (this.verboseLogging)
			{
				Debug.Log("CalculateLayoutInputSize", this);
			}
			if (this.childLayoutItems.Count == 0)
			{
				base.SetLayoutInputForAxis((float)this.Padding.horizontal, (float)this.Padding.horizontal, 0f, UIBaseLayoutGroup.Axes.Horizontal);
				base.SetLayoutInputForAxis((float)this.Padding.vertical, (float)this.Padding.vertical, 0f, UIBaseLayoutGroup.Axes.Vertical);
				return;
			}
			int num;
			int num2;
			this.GetGridDimensions(out num, out num2);
			float num3 = (float)this.Padding.horizontal + this.calculatedCellSize.x * (float)num + this.Spacing.x * (float)Mathf.Max(0, num - 1);
			float num4 = (float)this.Padding.vertical + this.calculatedCellSize.y * (float)num2 + this.Spacing.y * (float)Mathf.Max(0, num2 - 1);
			base.SetLayoutInputForAxis(num3, num3, 0f, UIBaseLayoutGroup.Axes.Horizontal);
			base.SetLayoutInputForAxis(num4, num4, 0f, UIBaseLayoutGroup.Axes.Vertical);
		}

		// Token: 0x06000874 RID: 2164 RVA: 0x00023ED8 File Offset: 0x000220D8
		private void PositionChildren()
		{
			if (this.verboseLogging)
			{
				Debug.Log("PositionChildren", this);
			}
			if (this.childLayoutItems.Count == 0)
			{
				return;
			}
			int num;
			int num2;
			this.GetGridDimensions(out num, out num2);
			int num3 = Mathf.Min(num, this.childLayoutItems.Count);
			int num4 = Mathf.Min(num2, Mathf.CeilToInt((float)this.childLayoutItems.Count / (float)Mathf.Max(1, num)));
			Vector2 vector = new Vector2((float)num3 * this.calculatedCellSize.x + (float)(num3 - 1) * this.Spacing.x, (float)num4 * this.calculatedCellSize.y + (float)(num4 - 1) * this.Spacing.y);
			float startOffset = base.GetStartOffset(UIBaseLayoutGroup.Axes.Horizontal, vector.x);
			float startOffset2 = base.GetStartOffset(UIBaseLayoutGroup.Axes.Vertical, vector.y);
			int num5 = (int)(this.StartCorner % UIGridLayoutGroup.Corner.LowerLeft);
			int num6 = (int)(this.StartCorner / UIGridLayoutGroup.Corner.LowerLeft);
			for (int i = 0; i < this.childLayoutItems.Count; i++)
			{
				int num7;
				int num8;
				if (this.StartAxis == UIGridLayoutGroup.Axis.Horizontal)
				{
					num7 = i % num;
					num8 = i / num;
				}
				else
				{
					num7 = i / num2;
					num8 = i % num2;
				}
				if (num5 == 1)
				{
					num7 = num3 - 1 - num7;
				}
				if (num6 == 1)
				{
					num8 = num4 - 1 - num8;
				}
				float num9 = startOffset + (float)num7 * (this.calculatedCellSize.x + this.Spacing.x);
				float num10 = startOffset2 + (float)num8 * (this.calculatedCellSize.y + this.Spacing.y);
				UIBaseLayoutGroup.SetChildAlongAxisWithScale(this.childLayoutItems[i], UIBaseLayoutGroup.Axes.Horizontal, num9, new float?(this.calculatedCellSize.x), 1f);
				UIBaseLayoutGroup.SetChildAlongAxisWithScale(this.childLayoutItems[i], UIBaseLayoutGroup.Axes.Vertical, num10, new float?(this.calculatedCellSize.y), 1f);
			}
		}

		// Token: 0x0400053C RID: 1340
		[Header("Grid Settings")]
		[SerializeField]
		public UIGridLayoutGroup.Corner StartCorner;

		// Token: 0x0400053D RID: 1341
		[SerializeField]
		public UIGridLayoutGroup.Axis StartAxis;

		// Token: 0x0400053E RID: 1342
		[SerializeField]
		public UIGridLayoutGroup.Constraint ConstraintMode;

		// Token: 0x0400053F RID: 1343
		[SerializeField]
		public int ConstraintCount = 2;

		// Token: 0x04000540 RID: 1344
		[Header("Cell Settings")]
		[SerializeField]
		public UIGridLayoutGroup.CellSizeMode SizeMode;

		// Token: 0x04000541 RID: 1345
		[SerializeField]
		public Vector2 CellSize = new Vector2(100f, 100f);

		// Token: 0x04000542 RID: 1346
		[SerializeField]
		public float CellAspectRatio = 1f;

		// Token: 0x04000543 RID: 1347
		[SerializeField]
		public Vector2 Spacing = Vector2.zero;

		// Token: 0x04000544 RID: 1348
		private Vector2 calculatedCellSize;

		// Token: 0x0200015F RID: 351
		public enum Corner
		{
			// Token: 0x04000546 RID: 1350
			UpperLeft,
			// Token: 0x04000547 RID: 1351
			UpperRight,
			// Token: 0x04000548 RID: 1352
			LowerLeft,
			// Token: 0x04000549 RID: 1353
			LowerRight
		}

		// Token: 0x02000160 RID: 352
		public enum Axis
		{
			// Token: 0x0400054B RID: 1355
			Horizontal,
			// Token: 0x0400054C RID: 1356
			Vertical
		}

		// Token: 0x02000161 RID: 353
		public enum Constraint
		{
			// Token: 0x0400054E RID: 1358
			Flexible,
			// Token: 0x0400054F RID: 1359
			FixedColumnCount,
			// Token: 0x04000550 RID: 1360
			FixedRowCount
		}

		// Token: 0x02000162 RID: 354
		public enum CellSizeMode
		{
			// Token: 0x04000552 RID: 1362
			AspectRatio,
			// Token: 0x04000553 RID: 1363
			FixedSize
		}
	}
}
