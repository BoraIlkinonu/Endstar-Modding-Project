using System;
using Endless.Gameplay.Scripting;
using Endless.Gameplay.Stats;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x0200044C RID: 1100
	public class GameEndBlock
	{
		// Token: 0x06001B84 RID: 7044 RVA: 0x0007C5DF File Offset: 0x0007A7DF
		public GameEndBlock(GameEndBlock gameEndBlock)
		{
			this.gameEndBlock = gameEndBlock;
		}

		// Token: 0x06001B85 RID: 7045 RVA: 0x0007C5EE File Offset: 0x0007A7EE
		public void RecordBasicStat(Context instigator, BasicStat basicStat)
		{
			this.gameEndBlock.RecordBasicStat(instigator, basicStat);
		}

		// Token: 0x06001B86 RID: 7046 RVA: 0x0007C5FD File Offset: 0x0007A7FD
		public void RecordPerPlayerStat(Context instigator, PerPlayerStat perPlayerStat)
		{
			this.gameEndBlock.RecordPerPlayerStat(instigator, perPlayerStat);
		}

		// Token: 0x06001B87 RID: 7047 RVA: 0x0007C60C File Offset: 0x0007A80C
		public void RecordComparativeStat(Context instigator, ComparativeStat comparativeStat)
		{
			this.gameEndBlock.RecordComparativeStat(instigator, comparativeStat);
		}

		// Token: 0x06001B88 RID: 7048 RVA: 0x0007C61B File Offset: 0x0007A81B
		public void TriggerEndGame(Context instigator)
		{
			this.gameEndBlock.EndGame(instigator);
		}

		// Token: 0x040015B0 RID: 5552
		private readonly GameEndBlock gameEndBlock;
	}
}
