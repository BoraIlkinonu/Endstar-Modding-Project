using System;
using System.Collections.Generic;
using UnityEngine;

namespace HackAnythingAnywhere.Core
{
	// Token: 0x0200002E RID: 46
	[Serializable]
	public class HackableMemberReference
	{
		// Token: 0x17000029 RID: 41
		// (get) Token: 0x060000B3 RID: 179 RVA: 0x00004272 File Offset: 0x00002472
		public Component Component
		{
			get
			{
				return this.component;
			}
		}

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x060000B4 RID: 180 RVA: 0x0000427A File Offset: 0x0000247A
		public string MemberName
		{
			get
			{
				return this.memberName;
			}
		}

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x060000B5 RID: 181 RVA: 0x00004282 File Offset: 0x00002482
		public string AssemblyQualifiedTypeName
		{
			get
			{
				return this.assemblyQualifiedTypeName;
			}
		}

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x060000B6 RID: 182 RVA: 0x0000428A File Offset: 0x0000248A
		public HackableMemberType MemberType
		{
			get
			{
				return this.memberType;
			}
		}

		// Token: 0x060000B7 RID: 183 RVA: 0x00004294 File Offset: 0x00002494
		public override bool Equals(object obj)
		{
			HackableMemberReference hackableMemberReference = obj as HackableMemberReference;
			return hackableMemberReference != null && EqualityComparer<Component>.Default.Equals(this.component, hackableMemberReference.component) && this.memberName == hackableMemberReference.memberName && this.assemblyQualifiedTypeName == hackableMemberReference.assemblyQualifiedTypeName && this.memberType == hackableMemberReference.memberType;
		}

		// Token: 0x060000B8 RID: 184 RVA: 0x00004300 File Offset: 0x00002500
		public override int GetHashCode()
		{
			return (((-826833188 * -1521134295 + EqualityComparer<Component>.Default.GetHashCode(this.component)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.memberName)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.assemblyQualifiedTypeName)) * -1521134295 + this.memberType.GetHashCode();
		}

		// Token: 0x060000B9 RID: 185 RVA: 0x0000436F File Offset: 0x0000256F
		public static bool operator ==(HackableMemberReference reference, HackableMember hackableMember)
		{
			return reference.MemberType == hackableMember.MemberType && reference.MemberName == hackableMember.MemberName && reference.AssemblyQualifiedTypeName == hackableMember.AssemblyQualifiedTypeName;
		}

		// Token: 0x060000BA RID: 186 RVA: 0x000043A5 File Offset: 0x000025A5
		public static bool operator !=(HackableMemberReference reference, HackableMember hackableMember)
		{
			return reference.MemberType != hackableMember.MemberType || reference.MemberName != hackableMember.MemberName || reference.AssemblyQualifiedTypeName != hackableMember.AssemblyQualifiedTypeName;
		}

		// Token: 0x04000079 RID: 121
		[SerializeField]
		private Component component;

		// Token: 0x0400007A RID: 122
		[SerializeField]
		private string memberName = "";

		// Token: 0x0400007B RID: 123
		[SerializeField]
		private string assemblyQualifiedTypeName;

		// Token: 0x0400007C RID: 124
		[SerializeField]
		private HackableMemberType memberType;
	}
}
