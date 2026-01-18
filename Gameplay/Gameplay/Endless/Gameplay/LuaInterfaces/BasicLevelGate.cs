using System;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000442 RID: 1090
	public class BasicLevelGate
	{
		// Token: 0x06001B50 RID: 6992 RVA: 0x0007C247 File Offset: 0x0007A447
		internal BasicLevelGate(BasicLevelGate levelGate)
		{
			this.levelGate = levelGate;
		}

		// Token: 0x06001B51 RID: 6993 RVA: 0x0007C256 File Offset: 0x0007A456
		public void PlayerReady(Context playerContext)
		{
			this.levelGate.PlayerReady(playerContext);
		}

		// Token: 0x06001B52 RID: 6994 RVA: 0x0007C264 File Offset: 0x0007A464
		public void PlayerUnready(Context playerContext)
		{
			this.levelGate.PlayerUnready(playerContext);
		}

		// Token: 0x06001B53 RID: 6995 RVA: 0x0007C272 File Offset: 0x0007A472
		public void TriggerGate(Context context)
		{
			this.levelGate.StartCountdown(context);
		}

		// Token: 0x06001B54 RID: 6996 RVA: 0x0007C280 File Offset: 0x0007A480
		public void ToggleReadyParticles(Context context, bool ready)
		{
			this.levelGate.ToggleReadyParticles(ready);
		}

		// Token: 0x06001B55 RID: 6997 RVA: 0x0007C28E File Offset: 0x0007A48E
		public bool GetIsValidDestination()
		{
			return this.levelGate.IsValidDestination();
		}

		// Token: 0x040015A8 RID: 5544
		private BasicLevelGate levelGate;
	}
}
