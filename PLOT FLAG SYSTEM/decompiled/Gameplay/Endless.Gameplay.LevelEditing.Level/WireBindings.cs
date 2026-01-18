using System.Reflection;
using Endless.Gameplay.Scripting;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level;

public static class WireBindings
{
	public static string PROCESS_NAME => "ProcessEvent";

	public static string BIND_LUA_NAME => "BindLuaReceiver";

	public static string MAKE_ENDLESS_EVENT_NAME => "MakeEndlessEvent";

	private static void ProcessEvent(object eventMember, MethodInfo targetMethod, Component targetComponent, StoredParameter[] parameters)
	{
		Debug.Log("WireBindings.ProcessEvent -1");
		EndlessEvent endlessEvent = eventMember as EndlessEvent;
		if (parameters.Length == 0)
		{
			endlessEvent.AddListener(delegate(Context t0)
			{
				targetMethod.Invoke(targetComponent, new object[1] { t0 });
			});
		}
		else
		{
			endlessEvent.AddListener(delegate(Context t0)
			{
				targetMethod.Invoke(targetComponent, GetObjects(t0, parameters));
			});
		}
	}

	private static void ProcessEvent<ContextBase>(object eventMember, MethodInfo targetMethod, Component targetComponent, StoredParameter[] parameters)
	{
		Debug.Log("WireBindings.WireBindings.ProcessEvent 0");
		EndlessEvent endlessEvent = eventMember as EndlessEvent;
		if (parameters.Length == 0)
		{
			if (targetMethod.GetParameters().Length == 1)
			{
				endlessEvent.AddListener(delegate(Context t0)
				{
					targetMethod.Invoke(targetComponent, new object[1] { t0 });
				});
			}
			else
			{
				endlessEvent.AddListener(delegate(Context t0)
				{
					targetMethod.Invoke(targetComponent, new object[1] { t0 });
				});
			}
		}
		else
		{
			endlessEvent.AddListener(delegate(Context t0)
			{
				targetMethod.Invoke(targetComponent, GetObjects(t0, parameters));
			});
		}
	}

	private static void ProcessEvent<ContextBase, T1>(object eventMember, MethodInfo targetMethod, Component targetComponent, StoredParameter[] parameters)
	{
		Debug.Log("WireBindings.ProcessEvent 1");
		EndlessEvent<T1> endlessEvent = eventMember as EndlessEvent<T1>;
		if (parameters.Length == 0)
		{
			if (targetMethod.GetParameters().Length == 1)
			{
				endlessEvent.AddListener(delegate(Context t0, T1 t1)
				{
					targetMethod.Invoke(targetComponent, new object[1] { t0 });
				});
				return;
			}
			endlessEvent.AddListener(delegate(Context t0, T1 t1)
			{
				targetMethod.Invoke(targetComponent, new object[2] { t0, t1 });
			});
		}
		else
		{
			endlessEvent.AddListener(delegate(Context t0, T1 t1)
			{
				targetMethod.Invoke(targetComponent, GetObjects(t0, parameters));
			});
		}
	}

	private static void ProcessEvent<ContextBase, T1, T2>(object eventMember, MethodInfo targetMethod, Component targetComponent, StoredParameter[] parameters)
	{
		Debug.Log("WireBindings.ProcessEvent 2");
		EndlessEvent<T1, T2> endlessEvent = eventMember as EndlessEvent<T1, T2>;
		if (parameters.Length == 0)
		{
			if (targetMethod.GetParameters().Length == 1)
			{
				endlessEvent.AddListener(delegate(Context t0, T1 t1, T2 t2)
				{
					targetMethod.Invoke(targetComponent, new object[1] { t0 });
				});
				return;
			}
			endlessEvent.AddListener(delegate(Context t0, T1 t1, T2 t2)
			{
				targetMethod.Invoke(targetComponent, new object[3] { t0, t1, t2 });
			});
		}
		else
		{
			endlessEvent.AddListener(delegate(Context t0, T1 t1, T2 t2)
			{
				targetMethod.Invoke(targetComponent, GetObjects(t0, parameters));
			});
		}
	}

	private static void ProcessEvent<ContextBase, T1, T2, T3>(object eventMember, MethodInfo targetMethod, Component targetComponent, StoredParameter[] parameters)
	{
		Debug.Log("WireBindings.ProcessEvent 3");
		EndlessEvent<T1, T2, T3> endlessEvent = eventMember as EndlessEvent<T1, T2, T3>;
		if (parameters.Length == 0)
		{
			if (targetMethod.GetParameters().Length == 1)
			{
				endlessEvent.AddListener(delegate
				{
					targetMethod.Invoke(targetComponent, null);
				});
				return;
			}
			endlessEvent.AddListener(delegate(Context t0, T1 t1, T2 t2, T3 t3)
			{
				targetMethod.Invoke(targetComponent, new object[4] { t0, t1, t2, t3 });
			});
		}
		else
		{
			endlessEvent.AddListener(delegate(Context t0, T1 t1, T2 t2, T3 t3)
			{
				targetMethod.Invoke(targetComponent, GetObjects(t0, parameters));
			});
		}
	}

	private static object[] GetObjects(Context context, StoredParameter[] parameters)
	{
		if (parameters.Length == 0)
		{
			return new Context[1] { context };
		}
		object[] array = new object[parameters.Length + 1];
		array[0] = context;
		for (int i = 0; i < parameters.Length; i++)
		{
			array[i + 1] = JsonConvert.DeserializeObject(parameters[i].JsonData, parameters[i].GetReferencedType());
		}
		return array;
	}

	private static void BindLuaReceiver(object eventMember, string receiverName, EndlessScriptComponent scriptComponent, int recieverArgumentCount, StoredParameter[] parameters)
	{
		EndlessEvent endlessEvent = eventMember as EndlessEvent;
		if (parameters.Length == 0)
		{
			endlessEvent.AddListener(delegate(Context t0)
			{
				scriptComponent.ExecuteReceiver(receiverName, new object[1] { t0 });
			});
		}
		else
		{
			endlessEvent.AddListener(delegate(Context t0)
			{
				scriptComponent.ExecuteReceiver(receiverName, GetObjects(t0, parameters));
			});
		}
	}

	private static void BindLuaReceiver<ContextBase>(object eventMember, string receiverName, EndlessScriptComponent scriptComponent, int recieverArgumentCount, StoredParameter[] parameters)
	{
		EndlessEvent endlessEvent = eventMember as EndlessEvent;
		if (parameters.Length == 0)
		{
			endlessEvent.AddListener(delegate(Context t0)
			{
				scriptComponent.ExecuteReceiver(receiverName, new object[1] { t0 });
			});
		}
		else
		{
			endlessEvent.AddListener(delegate(Context t0)
			{
				scriptComponent.ExecuteReceiver(receiverName, GetObjects(t0, parameters));
			});
		}
	}

	private static void BindLuaReceiver<ContextBase, T1>(object eventMember, string receiverName, EndlessScriptComponent scriptComponent, int recieverArgumentCount, StoredParameter[] parameters)
	{
		Debug.Log("WireBindings.BindLuaReceiver 1");
		EndlessEvent<T1> endlessEvent = eventMember as EndlessEvent<T1>;
		if (parameters.Length == 0)
		{
			if (recieverArgumentCount == 0)
			{
				endlessEvent.AddListener(delegate(Context t0, T1 t1)
				{
					scriptComponent.ExecuteReceiver(receiverName, new object[1] { t0 });
				});
				return;
			}
			endlessEvent.AddListener(delegate(Context t0, T1 t1)
			{
				scriptComponent.ExecuteReceiver(receiverName, new object[2] { t0, t1 });
			});
		}
		else
		{
			endlessEvent.AddListener(delegate(Context t0, T1 t1)
			{
				scriptComponent.ExecuteReceiver(receiverName, GetObjects(t0, parameters));
			});
		}
	}

	private static void BindLuaReceiver<ContextBase, T1, T2>(object eventMember, string receiverName, EndlessScriptComponent scriptComponent, int recieverArgumentCount, StoredParameter[] parameters)
	{
		Debug.Log("WireBindings.BindLuaReceiver 2");
		EndlessEvent<T1, T2> endlessEvent = eventMember as EndlessEvent<T1, T2>;
		if (parameters.Length == 0)
		{
			if (recieverArgumentCount == 0)
			{
				endlessEvent.AddListener(delegate(Context t0, T1 t1, T2 t2)
				{
					scriptComponent.ExecuteReceiver(receiverName, new object[1] { t0 });
				});
				return;
			}
			endlessEvent.AddListener(delegate(Context t0, T1 t1, T2 t2)
			{
				scriptComponent.ExecuteReceiver(receiverName, new object[3] { t0, t1, t2 });
			});
		}
		else
		{
			endlessEvent.AddListener(delegate(Context t0, T1 t1, T2 t2)
			{
				scriptComponent.ExecuteReceiver(receiverName, GetObjects(t0, parameters));
			});
		}
	}

	private static void BindLuaReceiver<ContextBase, T1, T2, T3>(object eventMember, string receiverName, EndlessScriptComponent scriptComponent, int recieverArgumentCount, StoredParameter[] parameters)
	{
		Debug.Log("WireBindings.BindLuaReceiver 3");
		EndlessEvent<T1, T2, T3> endlessEvent = eventMember as EndlessEvent<T1, T2, T3>;
		if (parameters.Length == 0)
		{
			if (recieverArgumentCount == 0)
			{
				endlessEvent.AddListener(delegate(Context t0, T1 t1, T2 t2, T3 t3)
				{
					scriptComponent.ExecuteReceiver(receiverName, new object[1] { t0 });
				});
				return;
			}
			endlessEvent.AddListener(delegate(Context t0, T1 t1, T2 t2, T3 t3)
			{
				scriptComponent.ExecuteReceiver(receiverName, new object[4] { t0, t1, t2, t3 });
			});
		}
		else
		{
			endlessEvent.AddListener(delegate(Context t0, T1 t1, T2 t2, T3 t3)
			{
				scriptComponent.ExecuteReceiver(receiverName, GetObjects(t0, parameters));
			});
		}
	}

	private static object MakeEndlessEvent()
	{
		Debug.Log("WireBindings.MakeEndlessEvent 0");
		return new EndlessEvent();
	}

	private static object MakeEndlessEvent<T1>()
	{
		Debug.Log("WireBindings.MakeEndlessEvent 1");
		return new EndlessEvent<T1>();
	}

	private static object MakeEndlessEvent<T1, T2>()
	{
		Debug.Log("WireBindings.MakeEndlessEvent 2");
		return new EndlessEvent<T1, T2>();
	}

	private static object MakeEndlessEvent<T1, T2, T3>()
	{
		Debug.Log("WireBindings.MakeEndlessEvent 3");
		return new EndlessEvent<T1, T2, T3>();
	}
}
