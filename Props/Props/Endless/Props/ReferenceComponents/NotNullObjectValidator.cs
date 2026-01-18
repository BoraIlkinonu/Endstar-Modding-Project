using System;
using System.Collections.Generic;
using Endless.Validation;
using UnityEngine;

namespace Endless.Props.ReferenceComponents
{
	// Token: 0x02000025 RID: 37
	public class NotNullObjectValidator : ReferenceBaseValidator
	{
		// Token: 0x0600009D RID: 157 RVA: 0x00002D3D File Offset: 0x00000F3D
		public NotNullObjectValidator(ReferenceBase source, global::UnityEngine.Object targetObject, string failureMessage)
			: base(source, null)
		{
			this.targetObject = targetObject;
			this.failureMessage = failureMessage;
		}

		// Token: 0x0600009E RID: 158 RVA: 0x00002D55 File Offset: 0x00000F55
		public override List<ValidationResult> PassesValidation()
		{
			if (!(this.targetObject != null))
			{
				return ValidationResult.Fail(this.failureMessage, this.Source);
			}
			return ValidationResult.Pass(null);
		}

		// Token: 0x04000061 RID: 97
		private global::UnityEngine.Object targetObject;

		// Token: 0x04000062 RID: 98
		private string failureMessage;
	}
}
