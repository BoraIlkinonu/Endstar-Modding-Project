using System;
using System.Reflection;

namespace HackAnythingAnywhere.Core;

public static class ReflectionExtension
{
	public static object GetValue(this MemberInfo member, object obj)
	{
		return member.MemberType switch
		{
			MemberTypes.Field => ((FieldInfo)member).GetValue(obj), 
			MemberTypes.Property => ((PropertyInfo)member).GetValue(obj), 
			_ => throw new ArgumentException("MemberInfo must be of type Field or Property", "member"), 
		};
	}

	public static void SetValue(this MemberInfo member, object obj, object value)
	{
		switch (member.MemberType)
		{
		case MemberTypes.Field:
			((FieldInfo)member).SetValue(obj, value);
			break;
		case MemberTypes.Property:
			((PropertyInfo)member).SetValue(obj, value);
			break;
		default:
			throw new ArgumentException("MemberInfo must be of type FieldInfo or PropertyInfo", "member");
		}
	}
}
