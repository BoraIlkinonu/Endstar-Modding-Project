using System;
using System.Reflection;
using Endless.Props.Scripting;

namespace Endless.Gameplay
{
	// Token: 0x020002A8 RID: 680
	[Serializable]
	public class InspectorExposedVariable
	{
		// Token: 0x06000EF3 RID: 3827 RVA: 0x0004ECDA File Offset: 0x0004CEDA
		protected bool Equals(InspectorExposedVariable other)
		{
			return this.MemberName == other.MemberName && this.MemberType == other.MemberType;
		}

		// Token: 0x06000EF4 RID: 3828 RVA: 0x0004ECFF File Offset: 0x0004CEFF
		public override bool Equals(object obj)
		{
			return obj != null && (this == obj || (!(obj.GetType() != base.GetType()) && this.Equals((InspectorExposedVariable)obj)));
		}

		// Token: 0x06000EF5 RID: 3829 RVA: 0x0004ED2D File Offset: 0x0004CF2D
		public override int GetHashCode()
		{
			return HashCode.Combine<string, int>(this.MemberName, (int)this.MemberType);
		}

		// Token: 0x06000EF6 RID: 3830 RVA: 0x0004ED40 File Offset: 0x0004CF40
		public InspectorExposedVariable(MemberInfo memberInfo, string defaultValue)
		{
			this.MemberName = memberInfo.Name;
			this.MemberType = memberInfo.MemberType;
			this.DataType = InspectorExposedVariable.GetUnderlyingType(memberInfo).AssemblyQualifiedName;
			this.DefaultValue = defaultValue;
		}

		// Token: 0x06000EF7 RID: 3831 RVA: 0x0004ED78 File Offset: 0x0004CF78
		public static Type GetUnderlyingType(MemberInfo member)
		{
			MemberTypes memberType = member.MemberType;
			if (memberType <= MemberTypes.Field)
			{
				if (memberType == MemberTypes.Event)
				{
					return ((EventInfo)member).EventHandlerType;
				}
				if (memberType == MemberTypes.Field)
				{
					return ((FieldInfo)member).FieldType;
				}
			}
			else
			{
				if (memberType == MemberTypes.Method)
				{
					return ((MethodInfo)member).ReturnType;
				}
				if (memberType == MemberTypes.Property)
				{
					return ((PropertyInfo)member).PropertyType;
				}
			}
			throw new ArgumentException("Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo");
		}

		// Token: 0x06000EF8 RID: 3832 RVA: 0x0004EDDF File Offset: 0x0004CFDF
		public static implicit operator InspectorExposedVariable(MemberInfo memberInfo)
		{
			return new InspectorExposedVariable(memberInfo, null);
		}

		// Token: 0x06000EF9 RID: 3833 RVA: 0x0004EDE8 File Offset: 0x0004CFE8
		public static bool operator ==(InspectorExposedVariable a, InspectorExposedVariable b)
		{
			return a == b || (a != null && b != null && a.Equals(b));
		}

		// Token: 0x06000EFA RID: 3834 RVA: 0x0004EE01 File Offset: 0x0004D001
		public static bool operator !=(InspectorExposedVariable a, InspectorExposedVariable b)
		{
			return !(a == b);
		}

		// Token: 0x04000D50 RID: 3408
		public string MemberName;

		// Token: 0x04000D51 RID: 3409
		public MemberTypes MemberType;

		// Token: 0x04000D52 RID: 3410
		public string DataType;

		// Token: 0x04000D53 RID: 3411
		public string DefaultValue;

		// Token: 0x04000D54 RID: 3412
		public string Description;

		// Token: 0x04000D55 RID: 3413
		public ClampValue[] ClampValues;
	}
}
