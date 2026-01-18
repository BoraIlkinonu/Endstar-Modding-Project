using System;
using Endless.Gameplay.Scripting;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x020002A0 RID: 672
	public class EndlessEvent<T1, T2> : UnityEvent<Context, T1, T2>
	{
		// Token: 0x06000ED8 RID: 3800 RVA: 0x0004EABC File Offset: 0x0004CCBC
		public new void Invoke(Context context, T1 t1, T2 t2)
		{
			Context.StaticLastContext = context;
			base.Invoke(context, t1, t2);
		}
	}
}
