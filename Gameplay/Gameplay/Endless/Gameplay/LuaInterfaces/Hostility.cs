using System;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000453 RID: 1107
	public class Hostility
	{
		// Token: 0x06001BAB RID: 7083 RVA: 0x0007C8A7 File Offset: 0x0007AAA7
		internal Hostility(HostilityComponent hostility)
		{
			this.hostilityComponent = hostility;
		}

		// Token: 0x06001BAC RID: 7084 RVA: 0x0007C8B6 File Offset: 0x0007AAB6
		public void SetHostilityLossRate(Context instigator, float value)
		{
			this.hostilityComponent.HostilityLossRate = value;
		}

		// Token: 0x06001BAD RID: 7085 RVA: 0x0007C8C4 File Offset: 0x0007AAC4
		public void SetHostilityDamageAddend(Context instigator, float value)
		{
			this.hostilityComponent.HostilityDamageAddend = value;
		}

		// Token: 0x040015B6 RID: 5558
		private readonly HostilityComponent hostilityComponent;
	}
}
