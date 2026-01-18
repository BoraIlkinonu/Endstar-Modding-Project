using System;
using System.Collections.Generic;

namespace Endless.Gameplay.Scripting;

public class LuaInterfaceEvent
{
	private class LuaEventSubscriber
	{
		public EndlessScriptComponent ScriptComponent;

		public string FunctionName;

		protected bool Equals(LuaEventSubscriber other)
		{
			if (object.Equals(ScriptComponent, other.ScriptComponent))
			{
				return FunctionName == other.FunctionName;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (this == obj)
			{
				return true;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((LuaEventSubscriber)obj);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(ScriptComponent, FunctionName);
		}

		public LuaEventSubscriber(EndlessScriptComponent scriptComponent, string functionName)
		{
			ScriptComponent = scriptComponent;
			FunctionName = functionName;
		}
	}

	private List<LuaEventSubscriber> subscribers = new List<LuaEventSubscriber>();

	private static event Action OnClearSubscribers;

	internal LuaInterfaceEvent()
	{
		OnClearSubscribers += Clear;
	}

	~LuaInterfaceEvent()
	{
		OnClearSubscribers -= Clear;
	}

	internal static void ClearAllSubscribers()
	{
		LuaInterfaceEvent.OnClearSubscribers?.Invoke();
	}

	internal void Subscribe(EndlessScriptComponent scriptComponent, string functionName)
	{
		subscribers.Add(new LuaEventSubscriber(scriptComponent, functionName));
	}

	internal void Unsubscribe(EndlessScriptComponent scriptComponent, string functionName)
	{
		subscribers.Remove(new LuaEventSubscriber(scriptComponent, functionName));
	}

	internal void InvokeEvent(params object[] args)
	{
		foreach (LuaEventSubscriber item in new List<LuaEventSubscriber>(subscribers))
		{
			item.ScriptComponent.FireToBoundReceiver(item.FunctionName, args);
		}
	}

	private void Clear()
	{
		subscribers.Clear();
	}
}
