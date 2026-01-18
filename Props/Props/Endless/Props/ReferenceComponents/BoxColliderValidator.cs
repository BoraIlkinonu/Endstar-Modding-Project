using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Validation;
using UnityEngine;

namespace Endless.Props.ReferenceComponents
{
	// Token: 0x02000029 RID: 41
	public class BoxColliderValidator : ReferenceBaseValidator
	{
		// Token: 0x060000A8 RID: 168 RVA: 0x00002EC1 File Offset: 0x000010C1
		public BoxColliderValidator(ReferenceBase source, BoxCollider info, Vector3 size, string failureMessage)
			: base(source, null)
		{
			this.collection = new BoxCollider[] { info };
			this.size = size;
			this.failureMessage = failureMessage;
			this.autoFixFunction = new Action(this.AutoFix);
		}

		// Token: 0x060000A9 RID: 169 RVA: 0x00002EFC File Offset: 0x000010FC
		public BoxColliderValidator(ReferenceBase source, IEnumerable<BoxCollider> collection, Vector3 size, string failureMessage)
			: base(source, null)
		{
			this.collection = collection;
			this.size = size;
			this.failureMessage = failureMessage;
			this.autoFixFunction = new Action(this.AutoFix);
		}

		// Token: 0x060000AA RID: 170 RVA: 0x00002F30 File Offset: 0x00001130
		public void AutoFix()
		{
			foreach (BoxCollider boxCollider in this.collection)
			{
				boxCollider.size = this.size;
			}
		}

		// Token: 0x060000AB RID: 171 RVA: 0x00002F80 File Offset: 0x00001180
		public override List<ValidationResult> PassesValidation()
		{
			if (!this.collection.All((BoxCollider info) => info != null && info.size == this.size))
			{
				return ValidationResult.Fail(this.failureMessage, this.Source);
			}
			return ValidationResult.Pass(null);
		}

		// Token: 0x04000067 RID: 103
		private readonly IEnumerable<BoxCollider> collection;

		// Token: 0x04000068 RID: 104
		private readonly Vector3 size;

		// Token: 0x04000069 RID: 105
		private readonly string failureMessage;
	}
}
