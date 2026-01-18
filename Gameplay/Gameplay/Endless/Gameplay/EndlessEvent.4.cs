using System;
using Endless.Gameplay.Scripting;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x020002A1 RID: 673
	public class EndlessEvent<T1, T2, T3> : UnityEvent<Context, T1, T2, T3>
	{
		// Token: 0x06000EDA RID: 3802 RVA: 0x0004EAD5 File Offset: 0x0004CCD5
		public new void Invoke(Context context, T1 t1, T2 t2, T3 t3)
		{
			Context.StaticLastContext = context;
			base.Invoke(context, t1, t2, t3);
		}
	}
}
