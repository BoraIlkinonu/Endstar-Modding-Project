using System;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x02000074 RID: 116
	public abstract class NetworkBehaviourSingleton<T> : NetworkBehaviour where T : NetworkBehaviour
	{
		// Token: 0x17000094 RID: 148
		// (get) Token: 0x0600038A RID: 906 RVA: 0x0001040A File Offset: 0x0000E60A
		public static T Instance
		{
			get
			{
				return NetworkBehaviourSingleton<T>._instance;
			}
		}

		// Token: 0x0600038B RID: 907 RVA: 0x00010414 File Offset: 0x0000E614
		protected virtual void Awake()
		{
			if (NetworkBehaviourSingleton<T>._instance != null && NetworkBehaviourSingleton<T>._instance != this)
			{
				Debug.LogException(new Exception(base.GetType().Name + " is being a duplicate on " + base.gameObject.name + ", because a singelton reference was already set!"), base.gameObject);
				return;
			}
			NetworkBehaviourSingleton<T>._instance = this as T;
		}

		// Token: 0x0600038C RID: 908 RVA: 0x0001048B File Offset: 0x0000E68B
		public void SetSingletonInstance()
		{
			NetworkBehaviourSingleton<T>._instance = this as T;
		}

		// Token: 0x0600038D RID: 909 RVA: 0x0001049D File Offset: 0x0000E69D
		public override void OnDestroy()
		{
			base.OnDestroy();
			if (NetworkBehaviourSingleton<T>._instance == this)
			{
				NetworkBehaviourSingleton<T>._instance = default(T);
			}
		}

		// Token: 0x0600038F RID: 911 RVA: 0x000104CC File Offset: 0x0000E6CC
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000390 RID: 912 RVA: 0x000104E2 File Offset: 0x0000E6E2
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06000391 RID: 913 RVA: 0x000104EC File Offset: 0x0000E6EC
		protected internal override string __getTypeName()
		{
			return "NetworkBehaviourSingleton`1";
		}

		// Token: 0x040001B5 RID: 437
		private static T _instance;
	}
}
