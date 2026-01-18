using System;
using System.Collections.Generic;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x02000498 RID: 1176
	public class LuaInterfaceEvent
	{
		// Token: 0x14000043 RID: 67
		// (add) Token: 0x06001CE1 RID: 7393 RVA: 0x0007E884 File Offset: 0x0007CA84
		// (remove) Token: 0x06001CE2 RID: 7394 RVA: 0x0007E8B8 File Offset: 0x0007CAB8
		private static event Action OnClearSubscribers;

		// Token: 0x06001CE3 RID: 7395 RVA: 0x0007E8EB File Offset: 0x0007CAEB
		internal LuaInterfaceEvent()
		{
			LuaInterfaceEvent.OnClearSubscribers += this.Clear;
		}

		// Token: 0x06001CE4 RID: 7396 RVA: 0x0007E910 File Offset: 0x0007CB10
		~LuaInterfaceEvent()
		{
			LuaInterfaceEvent.OnClearSubscribers -= this.Clear;
		}

		// Token: 0x06001CE5 RID: 7397 RVA: 0x0007E948 File Offset: 0x0007CB48
		internal static void ClearAllSubscribers()
		{
			Action onClearSubscribers = LuaInterfaceEvent.OnClearSubscribers;
			if (onClearSubscribers == null)
			{
				return;
			}
			onClearSubscribers();
		}

		// Token: 0x06001CE6 RID: 7398 RVA: 0x0007E959 File Offset: 0x0007CB59
		internal void Subscribe(EndlessScriptComponent scriptComponent, string functionName)
		{
			this.subscribers.Add(new LuaInterfaceEvent.LuaEventSubscriber(scriptComponent, functionName));
		}

		// Token: 0x06001CE7 RID: 7399 RVA: 0x0007E96D File Offset: 0x0007CB6D
		internal void Unsubscribe(EndlessScriptComponent scriptComponent, string functionName)
		{
			this.subscribers.Remove(new LuaInterfaceEvent.LuaEventSubscriber(scriptComponent, functionName));
		}

		// Token: 0x06001CE8 RID: 7400 RVA: 0x0007E984 File Offset: 0x0007CB84
		internal void InvokeEvent(params object[] args)
		{
			foreach (LuaInterfaceEvent.LuaEventSubscriber luaEventSubscriber in new List<LuaInterfaceEvent.LuaEventSubscriber>(this.subscribers))
			{
				luaEventSubscriber.ScriptComponent.FireToBoundReceiver(luaEventSubscriber.FunctionName, args);
			}
		}

		// Token: 0x06001CE9 RID: 7401 RVA: 0x0007E9E8 File Offset: 0x0007CBE8
		private void Clear()
		{
			this.subscribers.Clear();
		}

		// Token: 0x040016C0 RID: 5824
		private List<LuaInterfaceEvent.LuaEventSubscriber> subscribers = new List<LuaInterfaceEvent.LuaEventSubscriber>();

		// Token: 0x02000499 RID: 1177
		private class LuaEventSubscriber
		{
			// Token: 0x06001CEA RID: 7402 RVA: 0x0007E9F5 File Offset: 0x0007CBF5
			protected bool Equals(LuaInterfaceEvent.LuaEventSubscriber other)
			{
				return object.Equals(this.ScriptComponent, other.ScriptComponent) && this.FunctionName == other.FunctionName;
			}

			// Token: 0x06001CEB RID: 7403 RVA: 0x0007EA1D File Offset: 0x0007CC1D
			public override bool Equals(object obj)
			{
				return obj != null && (this == obj || (!(obj.GetType() != base.GetType()) && this.Equals((LuaInterfaceEvent.LuaEventSubscriber)obj)));
			}

			// Token: 0x06001CEC RID: 7404 RVA: 0x0007EA4B File Offset: 0x0007CC4B
			public override int GetHashCode()
			{
				return HashCode.Combine<EndlessScriptComponent, string>(this.ScriptComponent, this.FunctionName);
			}

			// Token: 0x06001CED RID: 7405 RVA: 0x0007EA5E File Offset: 0x0007CC5E
			public LuaEventSubscriber(EndlessScriptComponent scriptComponent, string functionName)
			{
				this.ScriptComponent = scriptComponent;
				this.FunctionName = functionName;
			}

			// Token: 0x040016C1 RID: 5825
			public EndlessScriptComponent ScriptComponent;

			// Token: 0x040016C2 RID: 5826
			public string FunctionName;
		}
	}
}
