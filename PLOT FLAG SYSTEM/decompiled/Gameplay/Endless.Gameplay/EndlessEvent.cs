using Endless.Gameplay.Scripting;
using UnityEngine.Events;

namespace Endless.Gameplay;

public class EndlessEvent : UnityEvent<Context>
{
	public new void Invoke(Context context)
	{
		Context.StaticLastContext = context;
		base.Invoke(context);
	}
}
public class EndlessEvent<T1> : UnityEvent<Context, T1>
{
	public new void Invoke(Context context, T1 t1)
	{
		Context.StaticLastContext = context;
		base.Invoke(context, t1);
	}
}
public class EndlessEvent<T1, T2> : UnityEvent<Context, T1, T2>
{
	public new void Invoke(Context context, T1 t1, T2 t2)
	{
		Context.StaticLastContext = context;
		base.Invoke(context, t1, t2);
	}
}
public class EndlessEvent<T1, T2, T3> : UnityEvent<Context, T1, T2, T3>
{
	public new void Invoke(Context context, T1 t1, T2 t2, T3 t3)
	{
		Context.StaticLastContext = context;
		base.Invoke(context, t1, t2, t3);
	}
}
