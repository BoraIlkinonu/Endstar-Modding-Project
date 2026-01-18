using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Props.ReferenceComponents;
using Endless.Validation;

namespace Endless.Props
{
	// Token: 0x02000005 RID: 5
	public class ColliderInfoValidator : ReferenceBaseValidator
	{
		// Token: 0x0600000F RID: 15 RVA: 0x00002354 File Offset: 0x00000554
		public ColliderInfoValidator(ReferenceBase source, ColliderInfo info, ColliderInfo.ColliderType desiredType, string failureMessage)
			: base(source, null)
		{
			this.collection = new ColliderInfo[] { info };
			this.desiredType = desiredType;
			this.failureMessage = failureMessage;
			this.autoFixFunction = new Action(this.AutoFix);
		}

		// Token: 0x06000010 RID: 16 RVA: 0x0000238F File Offset: 0x0000058F
		public ColliderInfoValidator(ReferenceBase source, IEnumerable<ColliderInfo> collection, ColliderInfo.ColliderType desiredType, string failureMessage)
			: base(source, null)
		{
			this.collection = collection;
			this.desiredType = desiredType;
			this.failureMessage = failureMessage;
			this.autoFixFunction = new Action(this.AutoFix);
		}

		// Token: 0x06000011 RID: 17 RVA: 0x000023C4 File Offset: 0x000005C4
		public void AutoFix()
		{
			foreach (ColliderInfo colliderInfo in this.collection)
			{
				colliderInfo.Type = this.desiredType;
			}
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002414 File Offset: 0x00000614
		public override List<ValidationResult> PassesValidation()
		{
			if (!this.collection.All((ColliderInfo info) => info != null && info.Type == this.desiredType))
			{
				return ValidationResult.Fail(this.failureMessage, this.Source);
			}
			return ValidationResult.Pass(null);
		}

		// Token: 0x04000004 RID: 4
		private readonly IEnumerable<ColliderInfo> collection;

		// Token: 0x04000005 RID: 5
		private readonly ColliderInfo.ColliderType desiredType;

		// Token: 0x04000006 RID: 6
		private readonly string failureMessage;
	}
}
