using System;
using System.Collections.Generic;
using System.Linq;

namespace Endless.Validation
{
	// Token: 0x02000006 RID: 6
	public class SequenceValidator : Validator
	{
		// Token: 0x0600000C RID: 12 RVA: 0x0000217F File Offset: 0x0000037F
		public SequenceValidator(List<Validator> subValidators)
		{
			this.children = subValidators ?? new List<Validator>();
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002198 File Offset: 0x00000398
		public override List<ValidationResult> PassesValidation()
		{
			List<ValidationResult> list = new List<ValidationResult>();
			for (int i = 0; i < this.children.Count; i++)
			{
				List<ValidationResult> list2 = this.children[i].PassesValidation();
				list.AddRange(list2);
				if (list2.Any((ValidationResult entry) => !entry.Success))
				{
					break;
				}
			}
			return list;
		}

		// Token: 0x04000006 RID: 6
		private List<Validator> children;
	}
}
