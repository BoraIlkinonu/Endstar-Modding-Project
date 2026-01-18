using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Validation;

namespace Endless.Props.ReferenceComponents
{
	// Token: 0x02000026 RID: 38
	public class CollectionLengthValidator : ReferenceBaseValidator
	{
		// Token: 0x0600009F RID: 159 RVA: 0x00002D7D File Offset: 0x00000F7D
		public CollectionLengthValidator(ReferenceBase source, IEnumerable<object> targetCollection, int minLength, string failureMessage)
			: base(source, null)
		{
			this.targetCollection = targetCollection;
			this.minLength = minLength;
			this.failureMessage = failureMessage;
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x00002DA0 File Offset: 0x00000FA0
		public override List<ValidationResult> PassesValidation()
		{
			if (!this.targetCollection.All((object entry) => entry != null) || this.targetCollection.Count<object>() < this.minLength)
			{
				return ValidationResult.Fail(this.failureMessage, this.Source);
			}
			return ValidationResult.Pass(null);
		}

		// Token: 0x04000063 RID: 99
		private IEnumerable<object> targetCollection;

		// Token: 0x04000064 RID: 100
		private int minLength;

		// Token: 0x04000065 RID: 101
		private string failureMessage;
	}
}
