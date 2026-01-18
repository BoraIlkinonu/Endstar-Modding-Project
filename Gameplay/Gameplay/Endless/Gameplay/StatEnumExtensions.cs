using System;
using System.Linq;

namespace Endless.Gameplay
{
	// Token: 0x0200031F RID: 799
	public static class StatEnumExtensions
	{
		// Token: 0x06001284 RID: 4740 RVA: 0x0005BA48 File Offset: 0x00059C48
		internal static GameEndBlock.Stats[] GetFlags(this GameEndBlock.Stats modKey)
		{
			return (from GameEndBlock.Stats v in Enum.GetValues(typeof(GameEndBlock.Stats))
				where modKey.HasFlag(v)
				select v).ToArray<GameEndBlock.Stats>();
		}
	}
}
