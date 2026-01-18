using System;
using Endless.Gameplay.Scripting;
using Newtonsoft.Json;

namespace Endless.Gameplay
{
	// Token: 0x020000F9 RID: 249
	[Serializable]
	public class PlayerReference
	{
		// Token: 0x06000585 RID: 1413 RVA: 0x0001BDA3 File Offset: 0x00019FA3
		internal bool GetUseContext()
		{
			return this.useContext;
		}

		// Token: 0x06000586 RID: 1414 RVA: 0x0001BDAB File Offset: 0x00019FAB
		internal int GetPlayerNumber()
		{
			return this.playerNumber;
		}

		// Token: 0x06000587 RID: 1415 RVA: 0x0001BDB3 File Offset: 0x00019FB3
		public Context GetPlayerContext()
		{
			if (!this.useContext)
			{
				return Game.Instance.GetPlayerBySlot(this.playerNumber);
			}
			if (!Context.StaticLastContext.IsPlayer())
			{
				return null;
			}
			return Context.StaticLastContext;
		}

		// Token: 0x06000588 RID: 1416 RVA: 0x0001BDE1 File Offset: 0x00019FE1
		public override string ToString()
		{
			return string.Format("{0}: {1}, {2}: {3}", new object[] { "useContext", this.useContext, "playerNumber", this.playerNumber });
		}

		// Token: 0x04000425 RID: 1061
		[JsonProperty]
		internal bool useContext = true;

		// Token: 0x04000426 RID: 1062
		[JsonProperty]
		internal int playerNumber;
	}
}
