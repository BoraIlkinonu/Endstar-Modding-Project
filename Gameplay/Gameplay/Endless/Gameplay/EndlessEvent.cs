using System;
using Endless.Gameplay.Scripting;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x0200029E RID: 670
	public class EndlessEvent : UnityEvent<Context>
	{
		// Token: 0x06000ED4 RID: 3796 RVA: 0x0004EA8D File Offset: 0x0004CC8D
		public new void Invoke(Context context)
		{
			Context.StaticLastContext = context;
			base.Invoke(context);
		}
	}
}
