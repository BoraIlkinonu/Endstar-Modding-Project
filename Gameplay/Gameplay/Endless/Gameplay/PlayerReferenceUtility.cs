using System;

namespace Endless.Gameplay
{
	// Token: 0x020000D6 RID: 214
	public static class PlayerReferenceUtility
	{
		// Token: 0x0600043A RID: 1082 RVA: 0x00016B6B File Offset: 0x00014D6B
		public static void SetUseContext(PlayerReference reference, bool useContext)
		{
			reference.useContext = useContext;
		}

		// Token: 0x0600043B RID: 1083 RVA: 0x00016B74 File Offset: 0x00014D74
		public static bool GetUseContext(PlayerReference reference)
		{
			return reference.GetUseContext();
		}

		// Token: 0x0600043C RID: 1084 RVA: 0x00016B7C File Offset: 0x00014D7C
		public static void SetPlayerNumber(PlayerReference reference, int playerNumber)
		{
			reference.playerNumber = playerNumber;
		}

		// Token: 0x0600043D RID: 1085 RVA: 0x00016B85 File Offset: 0x00014D85
		public static int GetPlayerNumber(PlayerReference reference)
		{
			return reference.GetPlayerNumber();
		}
	}
}
