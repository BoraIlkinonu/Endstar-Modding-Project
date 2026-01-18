using System;
using Endless.Gameplay.Scripting;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x0200029F RID: 671
	public class EndlessEvent<T1> : UnityEvent<Context, T1>
	{
		// Token: 0x06000ED6 RID: 3798 RVA: 0x0004EAA4 File Offset: 0x0004CCA4
		public new void Invoke(Context context, T1 t1)
		{
			Context.StaticLastContext = context;
			base.Invoke(context, t1);
		}
	}
}
