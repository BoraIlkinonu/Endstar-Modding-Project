using System;
using System.Collections.Generic;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x02000565 RID: 1381
	public class BlockTokenCollection
	{
		// Token: 0x17000656 RID: 1622
		// (get) Token: 0x0600212B RID: 8491 RVA: 0x000951B4 File Offset: 0x000933B4
		public bool IsPoolEmpty
		{
			get
			{
				return this.issuedTokens.Count == 0;
			}
		}

		// Token: 0x0600212D RID: 8493 RVA: 0x000951D8 File Offset: 0x000933D8
		public BlockToken RequestBlockToken()
		{
			BlockToken blockToken = new BlockToken(new Action<BlockToken>(this.ReleaseBlockToken));
			this.issuedTokens.Add(blockToken);
			return blockToken;
		}

		// Token: 0x0600212E RID: 8494 RVA: 0x00095204 File Offset: 0x00093404
		private void ReleaseBlockToken(BlockToken blockToken)
		{
			this.issuedTokens.Remove(blockToken);
		}

		// Token: 0x04001A61 RID: 6753
		private readonly List<BlockToken> issuedTokens = new List<BlockToken>();
	}
}
