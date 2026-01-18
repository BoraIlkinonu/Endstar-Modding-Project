using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Validation
{
	// Token: 0x02000003 RID: 3
	public class ValidationResult
	{
		// Token: 0x06000003 RID: 3 RVA: 0x000020BB File Offset: 0x000002BB
		public static List<ValidationResult> Pass(global::UnityEngine.Object source = null)
		{
			return new List<ValidationResult>
			{
				new ValidationResult("Pass", true, source)
			};
		}

		// Token: 0x06000004 RID: 4 RVA: 0x000020D4 File Offset: 0x000002D4
		public static List<ValidationResult> Fail(string message, global::UnityEngine.Object source = null)
		{
			return new List<ValidationResult>
			{
				new ValidationResult(message, false, source)
			};
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000020E9 File Offset: 0x000002E9
		private ValidationResult(string message, bool success, global::UnityEngine.Object source)
		{
			this.Message = message;
			this.Success = success;
			this.Source = source;
		}

		// Token: 0x04000001 RID: 1
		public readonly string Message;

		// Token: 0x04000002 RID: 2
		public readonly bool Success;

		// Token: 0x04000003 RID: 3
		public readonly global::UnityEngine.Object Source;
	}
}
