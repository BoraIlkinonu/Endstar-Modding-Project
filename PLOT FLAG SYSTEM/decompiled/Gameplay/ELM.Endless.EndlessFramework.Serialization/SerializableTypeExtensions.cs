using System.Runtime.Serialization;

namespace ELM.Endless.EndlessFramework.Serialization;

internal static class SerializableTypeExtensions
{
	public static T GetValue<T>(this SerializationInfo info, string name) where T : struct
	{
		return (T)info.GetValue(name, typeof(T));
	}
}
