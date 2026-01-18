using System;
using Endless.Validation;

namespace Endless.Props.ReferenceComponents
{
	// Token: 0x02000024 RID: 36
	public abstract class ReferenceBaseValidator : Validator
	{
		// Token: 0x0600009C RID: 156 RVA: 0x00002D27 File Offset: 0x00000F27
		public ReferenceBaseValidator(ReferenceBase source, Action autoFixFunction = null)
		{
			this.Source = source;
			this.autoFixFunction = autoFixFunction;
		}

		// Token: 0x04000060 RID: 96
		protected ReferenceBase Source;
	}
}
