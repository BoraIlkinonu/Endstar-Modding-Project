using System;

namespace Endless.Shared.SoVariables
{
	// Token: 0x020000D6 RID: 214
	[Serializable]
	public class VariableReference<T>
	{
		// Token: 0x170000D6 RID: 214
		// (get) Token: 0x06000575 RID: 1397 RVA: 0x00017F92 File Offset: 0x00016192
		public T Value
		{
			get
			{
				if (!this.UseConstant)
				{
					return this.Variable.Value;
				}
				return this.ConstantValue;
			}
		}

		// Token: 0x040002E2 RID: 738
		public bool UseConstant = true;

		// Token: 0x040002E3 RID: 739
		public T ConstantValue;

		// Token: 0x040002E4 RID: 740
		public SoVariable<T> Variable;
	}
}
