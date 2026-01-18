using System;
using System.Reflection;
using Endless.Gameplay.Scripting;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x0200058D RID: 1421
	public static class WireBindings
	{
		// Token: 0x1700067B RID: 1659
		// (get) Token: 0x0600224E RID: 8782 RVA: 0x0009DF2D File Offset: 0x0009C12D
		public static string PROCESS_NAME
		{
			get
			{
				return "ProcessEvent";
			}
		}

		// Token: 0x1700067C RID: 1660
		// (get) Token: 0x0600224F RID: 8783 RVA: 0x0009DF34 File Offset: 0x0009C134
		public static string BIND_LUA_NAME
		{
			get
			{
				return "BindLuaReceiver";
			}
		}

		// Token: 0x1700067D RID: 1661
		// (get) Token: 0x06002250 RID: 8784 RVA: 0x0009DF3B File Offset: 0x0009C13B
		public static string MAKE_ENDLESS_EVENT_NAME
		{
			get
			{
				return "MakeEndlessEvent";
			}
		}

		// Token: 0x06002251 RID: 8785 RVA: 0x0009DF44 File Offset: 0x0009C144
		private static void ProcessEvent(object eventMember, MethodInfo targetMethod, Component targetComponent, StoredParameter[] parameters)
		{
			Debug.Log("WireBindings.ProcessEvent -1");
			EndlessEvent endlessEvent = eventMember as EndlessEvent;
			if (parameters.Length == 0)
			{
				endlessEvent.AddListener(delegate(Context t0)
				{
					targetMethod.Invoke(targetComponent, new object[] { t0 });
				});
				return;
			}
			endlessEvent.AddListener(delegate(Context t0)
			{
				targetMethod.Invoke(targetComponent, WireBindings.GetObjects(t0, parameters));
			});
		}

		// Token: 0x06002252 RID: 8786 RVA: 0x0009DFAC File Offset: 0x0009C1AC
		private static void ProcessEvent<ContextBase>(object eventMember, MethodInfo targetMethod, Component targetComponent, StoredParameter[] parameters)
		{
			Debug.Log("WireBindings.WireBindings.ProcessEvent 0");
			EndlessEvent endlessEvent = eventMember as EndlessEvent;
			if (parameters.Length != 0)
			{
				endlessEvent.AddListener(delegate(Context t0)
				{
					targetMethod.Invoke(targetComponent, WireBindings.GetObjects(t0, parameters));
				});
				return;
			}
			if (targetMethod.GetParameters().Length == 1)
			{
				endlessEvent.AddListener(delegate(Context t0)
				{
					targetMethod.Invoke(targetComponent, new object[] { t0 });
				});
				return;
			}
			endlessEvent.AddListener(delegate(Context t0)
			{
				targetMethod.Invoke(targetComponent, new object[] { t0 });
			});
		}

		// Token: 0x06002253 RID: 8787 RVA: 0x0009E038 File Offset: 0x0009C238
		private static void ProcessEvent<ContextBase, T1>(object eventMember, MethodInfo targetMethod, Component targetComponent, StoredParameter[] parameters)
		{
			Debug.Log("WireBindings.ProcessEvent 1");
			EndlessEvent<T1> endlessEvent = eventMember as EndlessEvent<T1>;
			if (parameters.Length != 0)
			{
				endlessEvent.AddListener(delegate(Context t0, T1 t1)
				{
					targetMethod.Invoke(targetComponent, WireBindings.GetObjects(t0, parameters));
				});
				return;
			}
			if (targetMethod.GetParameters().Length == 1)
			{
				endlessEvent.AddListener(delegate(Context t0, T1 t1)
				{
					targetMethod.Invoke(targetComponent, new object[] { t0 });
				});
				return;
			}
			endlessEvent.AddListener(delegate(Context t0, T1 t1)
			{
				targetMethod.Invoke(targetComponent, new object[] { t0, t1 });
			});
		}

		// Token: 0x06002254 RID: 8788 RVA: 0x0009E0C4 File Offset: 0x0009C2C4
		private static void ProcessEvent<ContextBase, T1, T2>(object eventMember, MethodInfo targetMethod, Component targetComponent, StoredParameter[] parameters)
		{
			Debug.Log("WireBindings.ProcessEvent 2");
			EndlessEvent<T1, T2> endlessEvent = eventMember as EndlessEvent<T1, T2>;
			if (parameters.Length != 0)
			{
				endlessEvent.AddListener(delegate(Context t0, T1 t1, T2 t2)
				{
					targetMethod.Invoke(targetComponent, WireBindings.GetObjects(t0, parameters));
				});
				return;
			}
			if (targetMethod.GetParameters().Length == 1)
			{
				endlessEvent.AddListener(delegate(Context t0, T1 t1, T2 t2)
				{
					targetMethod.Invoke(targetComponent, new object[] { t0 });
				});
				return;
			}
			endlessEvent.AddListener(delegate(Context t0, T1 t1, T2 t2)
			{
				targetMethod.Invoke(targetComponent, new object[] { t0, t1, t2 });
			});
		}

		// Token: 0x06002255 RID: 8789 RVA: 0x0009E150 File Offset: 0x0009C350
		private static void ProcessEvent<ContextBase, T1, T2, T3>(object eventMember, MethodInfo targetMethod, Component targetComponent, StoredParameter[] parameters)
		{
			Debug.Log("WireBindings.ProcessEvent 3");
			EndlessEvent<T1, T2, T3> endlessEvent = eventMember as EndlessEvent<T1, T2, T3>;
			if (parameters.Length != 0)
			{
				endlessEvent.AddListener(delegate(Context t0, T1 t1, T2 t2, T3 t3)
				{
					targetMethod.Invoke(targetComponent, WireBindings.GetObjects(t0, parameters));
				});
				return;
			}
			if (targetMethod.GetParameters().Length == 1)
			{
				endlessEvent.AddListener(delegate(Context t0, T1 t1, T2 t2, T3 t3)
				{
					targetMethod.Invoke(targetComponent, null);
				});
				return;
			}
			endlessEvent.AddListener(delegate(Context t0, T1 t1, T2 t2, T3 t3)
			{
				targetMethod.Invoke(targetComponent, new object[] { t0, t1, t2, t3 });
			});
		}

		// Token: 0x06002256 RID: 8790 RVA: 0x0009E1DC File Offset: 0x0009C3DC
		private static object[] GetObjects(Context context, StoredParameter[] parameters)
		{
			if (parameters.Length == 0)
			{
				return new Context[] { context };
			}
			object[] array = new object[parameters.Length + 1];
			array[0] = context;
			for (int i = 0; i < parameters.Length; i++)
			{
				array[i + 1] = JsonConvert.DeserializeObject(parameters[i].JsonData, parameters[i].GetReferencedType());
			}
			return array;
		}

		// Token: 0x06002257 RID: 8791 RVA: 0x0009E234 File Offset: 0x0009C434
		private static void BindLuaReceiver(object eventMember, string receiverName, EndlessScriptComponent scriptComponent, int recieverArgumentCount, StoredParameter[] parameters)
		{
			EndlessEvent endlessEvent = eventMember as EndlessEvent;
			if (parameters.Length == 0)
			{
				endlessEvent.AddListener(delegate(Context t0)
				{
					scriptComponent.ExecuteReceiver(receiverName, new object[] { t0 });
				});
				return;
			}
			endlessEvent.AddListener(delegate(Context t0)
			{
				scriptComponent.ExecuteReceiver(receiverName, WireBindings.GetObjects(t0, parameters));
			});
		}

		// Token: 0x06002258 RID: 8792 RVA: 0x0009E294 File Offset: 0x0009C494
		private static void BindLuaReceiver<ContextBase>(object eventMember, string receiverName, EndlessScriptComponent scriptComponent, int recieverArgumentCount, StoredParameter[] parameters)
		{
			EndlessEvent endlessEvent = eventMember as EndlessEvent;
			if (parameters.Length == 0)
			{
				endlessEvent.AddListener(delegate(Context t0)
				{
					scriptComponent.ExecuteReceiver(receiverName, new object[] { t0 });
				});
				return;
			}
			endlessEvent.AddListener(delegate(Context t0)
			{
				scriptComponent.ExecuteReceiver(receiverName, WireBindings.GetObjects(t0, parameters));
			});
		}

		// Token: 0x06002259 RID: 8793 RVA: 0x0009E2F4 File Offset: 0x0009C4F4
		private static void BindLuaReceiver<ContextBase, T1>(object eventMember, string receiverName, EndlessScriptComponent scriptComponent, int recieverArgumentCount, StoredParameter[] parameters)
		{
			Debug.Log("WireBindings.BindLuaReceiver 1");
			EndlessEvent<T1> endlessEvent = eventMember as EndlessEvent<T1>;
			if (parameters.Length != 0)
			{
				endlessEvent.AddListener(delegate(Context t0, T1 t1)
				{
					scriptComponent.ExecuteReceiver(receiverName, WireBindings.GetObjects(t0, parameters));
				});
				return;
			}
			if (recieverArgumentCount == 0)
			{
				endlessEvent.AddListener(delegate(Context t0, T1 t1)
				{
					scriptComponent.ExecuteReceiver(receiverName, new object[] { t0 });
				});
				return;
			}
			endlessEvent.AddListener(delegate(Context t0, T1 t1)
			{
				scriptComponent.ExecuteReceiver(receiverName, new object[] { t0, t1 });
			});
		}

		// Token: 0x0600225A RID: 8794 RVA: 0x0009E374 File Offset: 0x0009C574
		private static void BindLuaReceiver<ContextBase, T1, T2>(object eventMember, string receiverName, EndlessScriptComponent scriptComponent, int recieverArgumentCount, StoredParameter[] parameters)
		{
			Debug.Log("WireBindings.BindLuaReceiver 2");
			EndlessEvent<T1, T2> endlessEvent = eventMember as EndlessEvent<T1, T2>;
			if (parameters.Length != 0)
			{
				endlessEvent.AddListener(delegate(Context t0, T1 t1, T2 t2)
				{
					scriptComponent.ExecuteReceiver(receiverName, WireBindings.GetObjects(t0, parameters));
				});
				return;
			}
			if (recieverArgumentCount == 0)
			{
				endlessEvent.AddListener(delegate(Context t0, T1 t1, T2 t2)
				{
					scriptComponent.ExecuteReceiver(receiverName, new object[] { t0 });
				});
				return;
			}
			endlessEvent.AddListener(delegate(Context t0, T1 t1, T2 t2)
			{
				scriptComponent.ExecuteReceiver(receiverName, new object[] { t0, t1, t2 });
			});
		}

		// Token: 0x0600225B RID: 8795 RVA: 0x0009E3F4 File Offset: 0x0009C5F4
		private static void BindLuaReceiver<ContextBase, T1, T2, T3>(object eventMember, string receiverName, EndlessScriptComponent scriptComponent, int recieverArgumentCount, StoredParameter[] parameters)
		{
			Debug.Log("WireBindings.BindLuaReceiver 3");
			EndlessEvent<T1, T2, T3> endlessEvent = eventMember as EndlessEvent<T1, T2, T3>;
			if (parameters.Length != 0)
			{
				endlessEvent.AddListener(delegate(Context t0, T1 t1, T2 t2, T3 t3)
				{
					scriptComponent.ExecuteReceiver(receiverName, WireBindings.GetObjects(t0, parameters));
				});
				return;
			}
			if (recieverArgumentCount == 0)
			{
				endlessEvent.AddListener(delegate(Context t0, T1 t1, T2 t2, T3 t3)
				{
					scriptComponent.ExecuteReceiver(receiverName, new object[] { t0 });
				});
				return;
			}
			endlessEvent.AddListener(delegate(Context t0, T1 t1, T2 t2, T3 t3)
			{
				scriptComponent.ExecuteReceiver(receiverName, new object[] { t0, t1, t2, t3 });
			});
		}

		// Token: 0x0600225C RID: 8796 RVA: 0x0009E472 File Offset: 0x0009C672
		private static object MakeEndlessEvent()
		{
			Debug.Log("WireBindings.MakeEndlessEvent 0");
			return new EndlessEvent();
		}

		// Token: 0x0600225D RID: 8797 RVA: 0x0009E483 File Offset: 0x0009C683
		private static object MakeEndlessEvent<T1>()
		{
			Debug.Log("WireBindings.MakeEndlessEvent 1");
			return new EndlessEvent<T1>();
		}

		// Token: 0x0600225E RID: 8798 RVA: 0x0009E494 File Offset: 0x0009C694
		private static object MakeEndlessEvent<T1, T2>()
		{
			Debug.Log("WireBindings.MakeEndlessEvent 2");
			return new EndlessEvent<T1, T2>();
		}

		// Token: 0x0600225F RID: 8799 RVA: 0x0009E4A5 File Offset: 0x0009C6A5
		private static object MakeEndlessEvent<T1, T2, T3>()
		{
			Debug.Log("WireBindings.MakeEndlessEvent 3");
			return new EndlessEvent<T1, T2, T3>();
		}
	}
}
