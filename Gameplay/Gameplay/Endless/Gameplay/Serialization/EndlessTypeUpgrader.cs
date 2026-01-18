using System;
using Endless.Props.Scripting;

namespace Endless.Gameplay.Serialization
{
	// Token: 0x020004C6 RID: 1222
	public abstract class EndlessTypeUpgrader
	{
		// Token: 0x06001E61 RID: 7777
		public abstract void Upgrade(MemberChange memberChange, bool isLua);

		// Token: 0x06001E62 RID: 7778
		public abstract void Upgrade(InspectorScriptValue inspectorScriptValue);
	}
}
