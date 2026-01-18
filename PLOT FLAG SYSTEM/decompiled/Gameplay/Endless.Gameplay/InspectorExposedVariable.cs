using System;
using System.Reflection;
using Endless.Props.Scripting;

namespace Endless.Gameplay;

[Serializable]
public class InspectorExposedVariable
{
	public string MemberName;

	public MemberTypes MemberType;

	public string DataType;

	public string DefaultValue;

	public string Description;

	public ClampValue[] ClampValues;

	protected bool Equals(InspectorExposedVariable other)
	{
		if (MemberName == other.MemberName)
		{
			return MemberType == other.MemberType;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((InspectorExposedVariable)obj);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(MemberName, (int)MemberType);
	}

	public InspectorExposedVariable(MemberInfo memberInfo, string defaultValue)
	{
		MemberName = memberInfo.Name;
		MemberType = memberInfo.MemberType;
		DataType = GetUnderlyingType(memberInfo).AssemblyQualifiedName;
		DefaultValue = defaultValue;
	}

	public static Type GetUnderlyingType(MemberInfo member)
	{
		return member.MemberType switch
		{
			MemberTypes.Event => ((EventInfo)member).EventHandlerType, 
			MemberTypes.Field => ((FieldInfo)member).FieldType, 
			MemberTypes.Method => ((MethodInfo)member).ReturnType, 
			MemberTypes.Property => ((PropertyInfo)member).PropertyType, 
			_ => throw new ArgumentException("Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"), 
		};
	}

	public static implicit operator InspectorExposedVariable(MemberInfo memberInfo)
	{
		return new InspectorExposedVariable(memberInfo, null);
	}

	public static bool operator ==(InspectorExposedVariable a, InspectorExposedVariable b)
	{
		if ((object)a == b)
		{
			return true;
		}
		if ((object)a == null)
		{
			return false;
		}
		if ((object)b == null)
		{
			return false;
		}
		return a.Equals(b);
	}

	public static bool operator !=(InspectorExposedVariable a, InspectorExposedVariable b)
	{
		return !(a == b);
	}
}
