using System;
using System.Collections.Generic;

namespace Endless.Validation
{
	// Token: 0x02000004 RID: 4
	public abstract class Validator
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000006 RID: 6 RVA: 0x00002106 File Offset: 0x00000306
		public bool HasAutoFixSolution
		{
			get
			{
				return this.autoFixFunction != null;
			}
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002111 File Offset: 0x00000311
		public void ApplyAutoFix()
		{
			Action action = this.autoFixFunction;
			if (action == null)
			{
				return;
			}
			action();
		}

		// Token: 0x06000008 RID: 8
		public abstract List<ValidationResult> PassesValidation();

		// Token: 0x04000004 RID: 4
		protected Action autoFixFunction;
	}
}
