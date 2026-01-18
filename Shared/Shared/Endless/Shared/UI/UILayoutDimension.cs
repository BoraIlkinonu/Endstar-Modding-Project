using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000154 RID: 340
	[Serializable]
	public class UILayoutDimension
	{
		// Token: 0x17000164 RID: 356
		// (get) Token: 0x06000839 RID: 2105 RVA: 0x00022578 File Offset: 0x00020778
		public float Value
		{
			get
			{
				float num = this.CalculateBaseValue();
				return this.ClampToMax(num);
			}
		}

		// Token: 0x0600083A RID: 2106 RVA: 0x00022594 File Offset: 0x00020794
		private float CalculateBaseValue()
		{
			if (this.Reference == null)
			{
				return this.ExplicitValue;
			}
			float num = ((this.ReferenceType == LayoutReferenceType.Width) ? this.Reference.rect.width : this.Reference.rect.height);
			return this.ApplyOperator(num);
		}

		// Token: 0x0600083B RID: 2107 RVA: 0x000225F0 File Offset: 0x000207F0
		private float ApplyOperator(float value)
		{
			switch (this.ReferenceOperator)
			{
			case LayoutOperator.Add:
				return value + this.ReferenceModifier;
			case LayoutOperator.Subtract:
				return value - this.ReferenceModifier;
			case LayoutOperator.Multiply:
				return value * this.ReferenceModifier;
			case LayoutOperator.Divide:
				return value / this.ReferenceModifier;
			default:
				DebugUtility.LogException(new ArgumentOutOfRangeException(string.Format("Unsupported {0}: {1}", "LayoutOperator", this.ReferenceOperator)), null);
				return value;
			}
		}

		// Token: 0x0600083C RID: 2108 RVA: 0x00022666 File Offset: 0x00020866
		private float ClampToMax(float value)
		{
			if (this.Max >= 0f && value > this.Max)
			{
				return this.Max;
			}
			return value;
		}

		// Token: 0x040004FA RID: 1274
		public float ExplicitValue = -1f;

		// Token: 0x040004FB RID: 1275
		public RectTransform Reference;

		// Token: 0x040004FC RID: 1276
		public LayoutOperator ReferenceOperator;

		// Token: 0x040004FD RID: 1277
		public float ReferenceModifier;

		// Token: 0x040004FE RID: 1278
		public LayoutReferenceType ReferenceType;

		// Token: 0x040004FF RID: 1279
		public float Max = -1f;
	}
}
