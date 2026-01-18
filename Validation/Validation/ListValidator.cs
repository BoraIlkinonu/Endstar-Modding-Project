using System;
using System.Collections.Generic;

namespace Endless.Validation
{
	// Token: 0x02000005 RID: 5
	public class ListValidator : Validator
	{
		// Token: 0x0600000A RID: 10 RVA: 0x0000212B File Offset: 0x0000032B
		public ListValidator(List<Validator> children)
		{
			this.children = children;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x0000213C File Offset: 0x0000033C
		public override List<ValidationResult> PassesValidation()
		{
			List<ValidationResult> list = new List<ValidationResult>();
			for (int i = 0; i < this.children.Count; i++)
			{
				List<ValidationResult> list2 = this.children[i].PassesValidation();
				list.AddRange(list2);
			}
			return list;
		}

		// Token: 0x04000005 RID: 5
		private List<Validator> children;
	}
}
