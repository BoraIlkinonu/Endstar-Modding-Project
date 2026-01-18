using System;
using System.Linq;

namespace Endless.Gameplay;

public static class StatEnumExtensions
{
	internal static GameEndBlock.Stats[] GetFlags(this GameEndBlock.Stats modKey)
	{
		return (from GameEndBlock.Stats v in Enum.GetValues(typeof(GameEndBlock.Stats))
			where modKey.HasFlag(v)
			select v).ToArray();
	}
}
