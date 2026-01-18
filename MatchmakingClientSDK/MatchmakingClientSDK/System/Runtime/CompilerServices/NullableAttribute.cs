using System;
using Microsoft.CodeAnalysis;

namespace System.Runtime.CompilerServices
{
	// Token: 0x02000006 RID: 6
	[CompilerGenerated]
	[Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableAttribute : Attribute
	{
		// Token: 0x06000005 RID: 5 RVA: 0x00002070 File Offset: 0x00000270
		public NullableAttribute(byte A_1)
		{
			this.NullableFlags = new byte[] { A_1 };
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002088 File Offset: 0x00000288
		public NullableAttribute(byte[] A_1)
		{
			this.NullableFlags = A_1;
		}

		// Token: 0x04000001 RID: 1
		public readonly byte[] NullableFlags;
	}
}
