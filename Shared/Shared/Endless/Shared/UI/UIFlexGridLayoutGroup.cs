using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000158 RID: 344
	public class UIFlexGridLayoutGroup : UIBaseLayoutGroup
	{
		// Token: 0x06000854 RID: 2132 RVA: 0x000229AD File Offset: 0x00020BAD
		public override void CalculateLayout()
		{
			if (this.verboseLogging)
			{
				Debug.Log("CalculateLayout", this);
			}
			this.CalculateLines();
			this.CalculateLayoutSize();
		}

		// Token: 0x06000855 RID: 2133 RVA: 0x000229CE File Offset: 0x00020BCE
		public override void ApplyLayout()
		{
			if (this.verboseLogging)
			{
				Debug.Log("ApplyLayout", this);
			}
			this.PositionChildren();
		}

		// Token: 0x06000856 RID: 2134 RVA: 0x000229E9 File Offset: 0x00020BE9
		public override float GetSpacingSize(Dimension dimension)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "GetSpacingSize", "dimension", dimension), this);
			}
			return this.ItemSpacing;
		}

		// Token: 0x06000857 RID: 2135 RVA: 0x00022A1C File Offset: 0x00020C1C
		private void CalculateLines()
		{
			if (this.verboseLogging)
			{
				Debug.Log("CalculateLines", this);
			}
			this.ReturnLinesToPool();
			if (this.childLayoutItems.Count == 0)
			{
				return;
			}
			float num = ((this.Direction == UIFlexGridLayoutGroup.WrapDirection.Horizontal) ? (base.RectTransform.rect.width - (float)this.Padding.horizontal) : (base.RectTransform.rect.height - (float)this.Padding.vertical));
			UIFlexGridLayoutGroup.Line line = this.GetLineFromPool();
			this.lines.Add(line);
			float num2 = 0f;
			foreach (RectTransform rectTransform in this.childLayoutItems)
			{
				float childMainAxisSize = this.GetChildMainAxisSize(rectTransform);
				if (line.children.Count > 0 && num2 + this.ItemSpacing + childMainAxisSize > num)
				{
					this.FinalizeLine(line);
					line = this.GetLineFromPool();
					this.lines.Add(line);
					num2 = 0f;
				}
				line.children.Add(rectTransform);
				num2 += childMainAxisSize;
				if (line.children.Count > 1)
				{
					num2 += this.ItemSpacing;
				}
				this.UpdateLineCrossAxisSize(line, rectTransform);
			}
			if (line.children.Count > 0)
			{
				this.FinalizeLine(line);
			}
		}

		// Token: 0x06000858 RID: 2136 RVA: 0x00022B90 File Offset: 0x00020D90
		private UIFlexGridLayoutGroup.Line GetLineFromPool()
		{
			if (this.linePool.Count > 0)
			{
				int num = this.linePool.Count - 1;
				UIFlexGridLayoutGroup.Line line = this.linePool[num];
				this.linePool.RemoveAt(num);
				return line;
			}
			return new UIFlexGridLayoutGroup.Line();
		}

		// Token: 0x06000859 RID: 2137 RVA: 0x00022BD8 File Offset: 0x00020DD8
		private void ReturnLinesToPool()
		{
			foreach (UIFlexGridLayoutGroup.Line line in this.lines)
			{
				line.Reset();
				this.linePool.Add(line);
			}
			this.lines.Clear();
		}

		// Token: 0x0600085A RID: 2138 RVA: 0x00022C44 File Offset: 0x00020E44
		private void PositionChildren()
		{
			if (this.verboseLogging)
			{
				Debug.Log("PositionChildren", this);
			}
			if (this.lines.Count == 0)
			{
				return;
			}
			float num = this.CalculateTotalCrossSize();
			float crossAxisStart = this.GetCrossAxisStart(num);
			float[] array = this.CalculateLinePositions(num);
			for (int i = 0; i < this.lines.Count; i++)
			{
				UIFlexGridLayoutGroup.Line line = this.lines[i];
				float num2 = line.height;
				if (this.ContentAlign == UIFlexGridLayoutGroup.AlignContent.Stretch && this.lines.Count > 1)
				{
					float availableCrossAxisSize = this.GetAvailableCrossAxisSize();
					float num3 = this.LineSpacing * (float)(this.lines.Count - 1);
					float num4 = availableCrossAxisSize - num + num3;
					if (num4 > 0f)
					{
						num2 += num4 / (float)this.lines.Count;
					}
				}
				float mainAxisStart = this.GetMainAxisStart(line);
				float[] array2 = this.CalculateItemPositions(line);
				for (int j = 0; j < line.children.Count; j++)
				{
					RectTransform rectTransform = line.children[j];
					float childMainAxisSize = this.GetChildMainAxisSize(rectTransform);
					float num6;
					float num7;
					float num5 = this.GetChildCrossAxisSize(rectTransform, out num6, out num7);
					float num8 = this.CalculateAlignOffset(num5, num2);
					if (this.Align == UIFlexGridLayoutGroup.AlignItems.Stretch)
					{
						num5 = num2;
					}
					float num9 = mainAxisStart + array2[j];
					float num10 = crossAxisStart + array[i] + num8;
					this.SetChildPosition(rectTransform, num9, num10, childMainAxisSize, num5);
				}
			}
		}

		// Token: 0x0600085B RID: 2139 RVA: 0x00022DA8 File Offset: 0x00020FA8
		private void CalculateLayoutSize()
		{
			if (this.verboseLogging)
			{
				Debug.Log("CalculateLayoutSize", this);
			}
			if (this.childLayoutItems.Count == 0)
			{
				base.SetLayoutInputForAxis((float)this.Padding.horizontal, (float)this.Padding.horizontal, 0f, UIBaseLayoutGroup.Axes.Horizontal);
				base.SetLayoutInputForAxis((float)this.Padding.vertical, (float)this.Padding.vertical, 0f, UIBaseLayoutGroup.Axes.Vertical);
				return;
			}
			float num = this.CalculateTotalCrossSize();
			if (this.Direction == UIFlexGridLayoutGroup.WrapDirection.Horizontal)
			{
				float num2 = 0f;
				foreach (UIFlexGridLayoutGroup.Line line in this.lines)
				{
					num2 = Mathf.Max(num2, line.width);
				}
				float num3 = num2 + (float)this.Padding.horizontal;
				float num4 = num + (float)this.Padding.vertical;
				base.SetLayoutInputForAxis(num3, num3, 0f, UIBaseLayoutGroup.Axes.Horizontal);
				base.SetLayoutInputForAxis(num4, num4, 0f, UIBaseLayoutGroup.Axes.Vertical);
				return;
			}
			float num5 = 0f;
			foreach (UIFlexGridLayoutGroup.Line line2 in this.lines)
			{
				num5 = Mathf.Max(num5, line2.width);
			}
			float num6 = num + (float)this.Padding.horizontal;
			float num7 = num5 + (float)this.Padding.vertical;
			base.SetLayoutInputForAxis(num6, num6, 0f, UIBaseLayoutGroup.Axes.Horizontal);
			base.SetLayoutInputForAxis(num7, num7, 0f, UIBaseLayoutGroup.Axes.Vertical);
		}

		// Token: 0x0600085C RID: 2140 RVA: 0x00022F58 File Offset: 0x00021158
		private float GetChildMainAxisSize(RectTransform child)
		{
			if (this.verboseLogging)
			{
				Debug.Log("GetChildMainAxisSize ( child: " + child.DebugSafeName(true) + " )", this);
			}
			if (this.Direction == UIFlexGridLayoutGroup.WrapDirection.Horizontal)
			{
				if (this.ChildControlWidth)
				{
					return UILayoutUtility.GetPreferredSize(child, UIBaseLayoutGroup.Axes.Horizontal);
				}
				return child.sizeDelta.x;
			}
			else
			{
				if (this.ChildControlHeight)
				{
					return UILayoutUtility.GetPreferredSize(child, UIBaseLayoutGroup.Axes.Vertical);
				}
				return child.sizeDelta.y;
			}
		}

		// Token: 0x0600085D RID: 2141 RVA: 0x00022FC8 File Offset: 0x000211C8
		private float GetChildCrossAxisSize(RectTransform child, out float min, out float preferred)
		{
			if (this.verboseLogging)
			{
				Debug.Log("GetChildCrossAxisSize ( child: " + child.DebugSafeName(true) + " )", this);
			}
			if (this.Direction == UIFlexGridLayoutGroup.WrapDirection.Horizontal)
			{
				if (this.ChildControlHeight)
				{
					min = UILayoutUtility.GetMinSize(child, UIBaseLayoutGroup.Axes.Vertical);
					preferred = UILayoutUtility.GetPreferredSize(child, UIBaseLayoutGroup.Axes.Vertical);
					return preferred;
				}
				min = (preferred = child.sizeDelta.y);
				return child.sizeDelta.y;
			}
			else
			{
				if (this.ChildControlWidth)
				{
					min = UILayoutUtility.GetMinSize(child, UIBaseLayoutGroup.Axes.Horizontal);
					preferred = UILayoutUtility.GetPreferredSize(child, UIBaseLayoutGroup.Axes.Horizontal);
					return preferred;
				}
				min = (preferred = child.sizeDelta.x);
				return child.sizeDelta.x;
			}
		}

		// Token: 0x0600085E RID: 2142 RVA: 0x00023078 File Offset: 0x00021278
		private void UpdateLineCrossAxisSize(UIFlexGridLayoutGroup.Line line, RectTransform child)
		{
			if (this.verboseLogging)
			{
				Debug.Log("UpdateLineCrossAxisSize ( child: " + child.DebugSafeName(true) + " )", this);
			}
			float num;
			float num2;
			float childCrossAxisSize = this.GetChildCrossAxisSize(child, out num, out num2);
			line.minHeight = Mathf.Max(line.minHeight, num);
			line.preferredHeight = Mathf.Max(line.preferredHeight, num2);
			line.height = Mathf.Max(line.height, childCrossAxisSize);
		}

		// Token: 0x0600085F RID: 2143 RVA: 0x000230EC File Offset: 0x000212EC
		private void FinalizeLine(UIFlexGridLayoutGroup.Line line)
		{
			if (this.verboseLogging)
			{
				Debug.Log("FinalizeLine", this);
			}
			line.width = 0f;
			for (int i = 0; i < line.children.Count; i++)
			{
				line.width += this.GetChildMainAxisSize(line.children[i]);
				if (i > 0)
				{
					line.width += this.ItemSpacing;
				}
			}
		}

		// Token: 0x06000860 RID: 2144 RVA: 0x00023164 File Offset: 0x00021364
		private float CalculateTotalCrossSize()
		{
			if (this.verboseLogging)
			{
				Debug.Log("CalculateTotalCrossSize", this);
			}
			float num = 0f;
			for (int i = 0; i < this.lines.Count; i++)
			{
				num += this.lines[i].height;
				if (i > 0)
				{
					num += this.LineSpacing;
				}
			}
			return num;
		}

		// Token: 0x06000861 RID: 2145 RVA: 0x000231C4 File Offset: 0x000213C4
		private float GetAvailableCrossAxisSize()
		{
			if (this.verboseLogging)
			{
				Debug.Log("GetAvailableCrossAxisSize", this);
			}
			if (this.Direction != UIFlexGridLayoutGroup.WrapDirection.Horizontal)
			{
				return base.RectTransform.rect.width - (float)this.Padding.horizontal;
			}
			return base.RectTransform.rect.height - (float)this.Padding.vertical;
		}

		// Token: 0x06000862 RID: 2146 RVA: 0x00023230 File Offset: 0x00021430
		private float GetMainAxisStart(UIFlexGridLayoutGroup.Line line)
		{
			if (this.verboseLogging)
			{
				Debug.Log("GetMainAxisStart", this);
			}
			float num = (float)((this.Direction == UIFlexGridLayoutGroup.WrapDirection.Horizontal) ? this.Padding.left : this.Padding.top);
			float num2 = ((this.Direction == UIFlexGridLayoutGroup.WrapDirection.Horizontal) ? (base.RectTransform.rect.width - (float)this.Padding.horizontal) : (base.RectTransform.rect.height - (float)this.Padding.vertical)) - line.width;
			switch (this.Justify)
			{
			case UIFlexGridLayoutGroup.JustifyContent.End:
				return num + num2;
			case UIFlexGridLayoutGroup.JustifyContent.Center:
				return num + num2 * 0.5f;
			}
			return num;
		}

		// Token: 0x06000863 RID: 2147 RVA: 0x000232FC File Offset: 0x000214FC
		private float GetCrossAxisStart(float totalCrossSize)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "GetCrossAxisStart", "totalCrossSize", totalCrossSize), this);
			}
			float num = (float)((this.Direction == UIFlexGridLayoutGroup.WrapDirection.Horizontal) ? this.Padding.top : this.Padding.left);
			float num2 = this.GetAvailableCrossAxisSize() - totalCrossSize;
			UIFlexGridLayoutGroup.AlignContent contentAlign = this.ContentAlign;
			if (contentAlign == UIFlexGridLayoutGroup.AlignContent.End)
			{
				return num + num2;
			}
			if (contentAlign != UIFlexGridLayoutGroup.AlignContent.Center)
			{
				return num;
			}
			return num + num2 * 0.5f;
		}

		// Token: 0x06000864 RID: 2148 RVA: 0x0002337C File Offset: 0x0002157C
		private float[] CalculateLinePositions(float totalCrossSize)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "CalculateLinePositions", "totalCrossSize", totalCrossSize), this);
			}
			this.EnsureLinePositionsCacheSize(this.lines.Count);
			float num = this.GetAvailableCrossAxisSize() - totalCrossSize;
			if (this.lines.Count == 1 || num <= 0f)
			{
				float num2 = 0f;
				for (int i = 0; i < this.lines.Count; i++)
				{
					this.linePositionsCache[i] = num2;
					num2 += this.lines[i].height + this.LineSpacing;
				}
				return this.linePositionsCache;
			}
			switch (this.ContentAlign)
			{
			case UIFlexGridLayoutGroup.AlignContent.SpaceBetween:
			{
				float num3 = num / (float)(this.lines.Count - 1);
				float num4 = 0f;
				for (int j = 0; j < this.lines.Count; j++)
				{
					this.linePositionsCache[j] = num4;
					num4 += this.lines[j].height + num3;
				}
				break;
			}
			case UIFlexGridLayoutGroup.AlignContent.SpaceAround:
			{
				float num5 = num / (float)this.lines.Count;
				float num6 = num5 * 0.5f;
				for (int k = 0; k < this.lines.Count; k++)
				{
					this.linePositionsCache[k] = num6;
					num6 += this.lines[k].height + num5;
				}
				break;
			}
			case UIFlexGridLayoutGroup.AlignContent.SpaceEvenly:
			{
				float num7 = num / (float)(this.lines.Count + 1);
				float num8 = num7;
				for (int l = 0; l < this.lines.Count; l++)
				{
					this.linePositionsCache[l] = num8;
					num8 += this.lines[l].height + num7;
				}
				break;
			}
			default:
			{
				float num9 = 0f;
				for (int m = 0; m < this.lines.Count; m++)
				{
					this.linePositionsCache[m] = num9;
					num9 += this.lines[m].height + this.LineSpacing;
				}
				break;
			}
			}
			return this.linePositionsCache;
		}

		// Token: 0x06000865 RID: 2149 RVA: 0x000235AA File Offset: 0x000217AA
		private void EnsureLinePositionsCacheSize(int requiredSize)
		{
			if (this.linePositionsCache.Length < requiredSize)
			{
				this.linePositionsCache = new float[requiredSize * 2];
			}
		}

		// Token: 0x06000866 RID: 2150 RVA: 0x000235C8 File Offset: 0x000217C8
		private float[] CalculateItemPositions(UIFlexGridLayoutGroup.Line line)
		{
			if (this.verboseLogging)
			{
				Debug.Log("CalculateItemPositions", this);
			}
			this.EnsureItemPositionsCacheSize(line.children.Count);
			float num = ((this.Direction == UIFlexGridLayoutGroup.WrapDirection.Horizontal) ? (base.RectTransform.rect.width - (float)this.Padding.horizontal) : (base.RectTransform.rect.height - (float)this.Padding.vertical)) - line.width;
			if (line.children.Count == 1 || num <= 0f || this.Justify == UIFlexGridLayoutGroup.JustifyContent.Start || this.Justify == UIFlexGridLayoutGroup.JustifyContent.End || this.Justify == UIFlexGridLayoutGroup.JustifyContent.Center)
			{
				float num2 = 0f;
				for (int i = 0; i < line.children.Count; i++)
				{
					this.itemPositionsCache[i] = num2;
					num2 += this.GetChildMainAxisSize(line.children[i]) + this.ItemSpacing;
				}
				return this.itemPositionsCache;
			}
			switch (this.Justify)
			{
			case UIFlexGridLayoutGroup.JustifyContent.SpaceBetween:
			{
				float num3 = num / (float)(line.children.Count - 1);
				float num4 = 0f;
				for (int j = 0; j < line.children.Count; j++)
				{
					this.itemPositionsCache[j] = num4;
					num4 += this.GetChildMainAxisSize(line.children[j]) + num3;
				}
				break;
			}
			case UIFlexGridLayoutGroup.JustifyContent.SpaceAround:
			{
				float num5 = num / (float)line.children.Count;
				float num6 = num5 * 0.5f;
				for (int k = 0; k < line.children.Count; k++)
				{
					this.itemPositionsCache[k] = num6;
					num6 += this.GetChildMainAxisSize(line.children[k]) + num5;
				}
				break;
			}
			case UIFlexGridLayoutGroup.JustifyContent.SpaceEvenly:
			{
				float num7 = num / (float)(line.children.Count + 1);
				float num8 = num7;
				for (int l = 0; l < line.children.Count; l++)
				{
					this.itemPositionsCache[l] = num8;
					num8 += this.GetChildMainAxisSize(line.children[l]) + num7;
				}
				break;
			}
			}
			return this.itemPositionsCache;
		}

		// Token: 0x06000867 RID: 2151 RVA: 0x000237FB File Offset: 0x000219FB
		private void EnsureItemPositionsCacheSize(int requiredSize)
		{
			if (this.itemPositionsCache.Length < requiredSize)
			{
				this.itemPositionsCache = new float[requiredSize * 2];
			}
		}

		// Token: 0x06000868 RID: 2152 RVA: 0x00023818 File Offset: 0x00021A18
		private float CalculateAlignOffset(float childSize, float lineSize)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "CalculateAlignOffset", "childSize", childSize, "lineSize", lineSize }), this);
			}
			switch (this.Align)
			{
			case UIFlexGridLayoutGroup.AlignItems.End:
				return lineSize - childSize;
			case UIFlexGridLayoutGroup.AlignItems.Center:
				return (lineSize - childSize) * 0.5f;
			}
			return 0f;
		}

		// Token: 0x06000869 RID: 2153 RVA: 0x000238A0 File Offset: 0x00021AA0
		private void SetChildPosition(RectTransform child, float mainPos, float crossPos, float mainSize, float crossSize)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6}, {7}: {8}, {9}: {10} )", new object[]
				{
					"SetChildPosition",
					"child",
					child.DebugSafeName(true),
					"mainPos",
					mainPos,
					"crossPos",
					crossPos,
					"mainSize",
					mainSize,
					"crossSize",
					crossSize
				}), this);
			}
			if (this.Direction == UIFlexGridLayoutGroup.WrapDirection.Horizontal)
			{
				float num = (this.ChildControlWidth ? mainSize : child.sizeDelta.x);
				float num2 = ((this.ChildControlHeight || this.Align == UIFlexGridLayoutGroup.AlignItems.Stretch) ? crossSize : child.sizeDelta.y);
				UIBaseLayoutGroup.SetChildAlongAxisWithScale(child, UIBaseLayoutGroup.Axes.Horizontal, mainPos, new float?(num), 1f);
				UIBaseLayoutGroup.SetChildAlongAxisWithScale(child, UIBaseLayoutGroup.Axes.Vertical, crossPos, new float?(num2), 1f);
				return;
			}
			float num3 = ((this.ChildControlWidth || this.Align == UIFlexGridLayoutGroup.AlignItems.Stretch) ? crossSize : child.sizeDelta.x);
			float num4 = (this.ChildControlHeight ? mainSize : child.sizeDelta.y);
			UIBaseLayoutGroup.SetChildAlongAxisWithScale(child, UIBaseLayoutGroup.Axes.Horizontal, crossPos, new float?(num3), 1f);
			UIBaseLayoutGroup.SetChildAlongAxisWithScale(child, UIBaseLayoutGroup.Axes.Vertical, mainPos, new float?(num4), 1f);
		}

		// Token: 0x04000512 RID: 1298
		[Header("Flex Settings")]
		[SerializeField]
		public UIFlexGridLayoutGroup.WrapDirection Direction;

		// Token: 0x04000513 RID: 1299
		[SerializeField]
		public UIFlexGridLayoutGroup.JustifyContent Justify;

		// Token: 0x04000514 RID: 1300
		[SerializeField]
		public UIFlexGridLayoutGroup.AlignItems Align;

		// Token: 0x04000515 RID: 1301
		[SerializeField]
		public UIFlexGridLayoutGroup.AlignContent ContentAlign;

		// Token: 0x04000516 RID: 1302
		[Header("Spacing")]
		[SerializeField]
		public float ItemSpacing;

		// Token: 0x04000517 RID: 1303
		[SerializeField]
		public float LineSpacing;

		// Token: 0x04000518 RID: 1304
		[Header("Size Control")]
		[SerializeField]
		public bool ChildControlWidth = true;

		// Token: 0x04000519 RID: 1305
		[SerializeField]
		public bool ChildControlHeight = true;

		// Token: 0x0400051A RID: 1306
		[SerializeField]
		public bool ChildForceExpandWidth;

		// Token: 0x0400051B RID: 1307
		[SerializeField]
		public bool ChildForceExpandHeight;

		// Token: 0x0400051C RID: 1308
		private readonly List<UIFlexGridLayoutGroup.Line> lines = new List<UIFlexGridLayoutGroup.Line>();

		// Token: 0x0400051D RID: 1309
		private readonly List<UIFlexGridLayoutGroup.Line> linePool = new List<UIFlexGridLayoutGroup.Line>();

		// Token: 0x0400051E RID: 1310
		private float[] linePositionsCache = new float[4];

		// Token: 0x0400051F RID: 1311
		private float[] itemPositionsCache = new float[8];

		// Token: 0x02000159 RID: 345
		public enum JustifyContent
		{
			// Token: 0x04000521 RID: 1313
			Start,
			// Token: 0x04000522 RID: 1314
			End,
			// Token: 0x04000523 RID: 1315
			Center,
			// Token: 0x04000524 RID: 1316
			SpaceBetween,
			// Token: 0x04000525 RID: 1317
			SpaceAround,
			// Token: 0x04000526 RID: 1318
			SpaceEvenly
		}

		// Token: 0x0200015A RID: 346
		public enum AlignItems
		{
			// Token: 0x04000528 RID: 1320
			Start,
			// Token: 0x04000529 RID: 1321
			End,
			// Token: 0x0400052A RID: 1322
			Center,
			// Token: 0x0400052B RID: 1323
			Stretch
		}

		// Token: 0x0200015B RID: 347
		public enum AlignContent
		{
			// Token: 0x0400052D RID: 1325
			Start,
			// Token: 0x0400052E RID: 1326
			End,
			// Token: 0x0400052F RID: 1327
			Center,
			// Token: 0x04000530 RID: 1328
			SpaceBetween,
			// Token: 0x04000531 RID: 1329
			SpaceAround,
			// Token: 0x04000532 RID: 1330
			SpaceEvenly,
			// Token: 0x04000533 RID: 1331
			Stretch
		}

		// Token: 0x0200015C RID: 348
		public enum WrapDirection
		{
			// Token: 0x04000535 RID: 1333
			Horizontal,
			// Token: 0x04000536 RID: 1334
			Vertical
		}

		// Token: 0x0200015D RID: 349
		private class Line
		{
			// Token: 0x0600086B RID: 2155 RVA: 0x00023A57 File Offset: 0x00021C57
			public void Reset()
			{
				this.children.Clear();
				this.width = 0f;
				this.height = 0f;
				this.minHeight = 0f;
				this.preferredHeight = 0f;
			}

			// Token: 0x04000537 RID: 1335
			public readonly List<RectTransform> children = new List<RectTransform>();

			// Token: 0x04000538 RID: 1336
			public float width;

			// Token: 0x04000539 RID: 1337
			public float height;

			// Token: 0x0400053A RID: 1338
			public float minHeight;

			// Token: 0x0400053B RID: 1339
			public float preferredHeight;
		}
	}
}
