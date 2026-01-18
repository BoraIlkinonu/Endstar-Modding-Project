using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay;

public class WireTesting : EndlessBehaviour, IStartSubscriber, IBaseType, IComponentBase
{
	public EndlessEvent BasicEvent = new EndlessEvent();

	public EndlessEvent<int> IntEvent = new EndlessEvent<int>();

	public EndlessEvent<int, int> Int2Event = new EndlessEvent<int, int>();

	private Context context;

	[field: SerializeField]
	[field: HideInInspector]
	public WorldObject WorldObject { get; private set; }

	public Context Context => context ?? (context = new Context(WorldObject));

	public void EndlessStart()
	{
		BasicEvent.Invoke(Context);
		IntEvent.Invoke(Context, 5);
		Int2Event.Invoke(Context, 6, 7);
	}

	public void ReceiverBasic(Context context)
	{
		Debug.Log("ReceiverBasic called.");
	}

	public void ReceiverInt1(Context context, int value)
	{
		Debug.Log(string.Format("{0} received a value of {1}", "ReceiverInt1", value));
	}

	public void ReceiverInt2(Context context, int value1, int value2)
	{
		Debug.Log(string.Format("{0} received a value of {1} and {2}", "ReceiverInt2", value1, value2));
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}
}
