using System;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x02000564 RID: 1380
	public class BlockToken
	{
		// Token: 0x06002129 RID: 8489 RVA: 0x00095197 File Offset: 0x00093397
		public BlockToken(Action<BlockToken> releaseAction)
		{
			this.releaseAction = releaseAction;
		}

		// Token: 0x0600212A RID: 8490 RVA: 0x000951A6 File Offset: 0x000933A6
		public void Release()
		{
			this.releaseAction(this);
		}

		// Token: 0x04001A60 RID: 6752
		private readonly Action<BlockToken> releaseAction;
	}
}
