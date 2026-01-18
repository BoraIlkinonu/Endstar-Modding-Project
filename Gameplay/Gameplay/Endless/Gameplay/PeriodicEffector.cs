using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000330 RID: 816
	public class PeriodicEffector : EndlessBehaviour, IGameEndSubscriber, IAwakeSubscriber, IComponentBase, IScriptInjector
	{
		// Token: 0x170003F9 RID: 1017
		// (get) Token: 0x06001371 RID: 4977 RVA: 0x0005E8C6 File Offset: 0x0005CAC6
		// (set) Token: 0x06001372 RID: 4978 RVA: 0x0005E8CE File Offset: 0x0005CACE
		private bool IsActive
		{
			get
			{
				return this.isActive;
			}
			set
			{
				this.isActive = value;
			}
		}

		// Token: 0x170003FA RID: 1018
		// (get) Token: 0x06001373 RID: 4979 RVA: 0x0005E8D7 File Offset: 0x0005CAD7
		// (set) Token: 0x06001374 RID: 4980 RVA: 0x0005E8DF File Offset: 0x0005CADF
		internal float InitialInterval
		{
			get
			{
				return this.syncedInterval;
			}
			set
			{
				this.syncedInterval = Mathf.Clamp(value, this.minimumInterval, this.maximumInterval);
			}
		}

		// Token: 0x170003FB RID: 1019
		// (get) Token: 0x06001375 RID: 4981 RVA: 0x0005E8F9 File Offset: 0x0005CAF9
		// (set) Token: 0x06001376 RID: 4982 RVA: 0x0005E901 File Offset: 0x0005CB01
		internal float IntervalScalar
		{
			get
			{
				return this.syncedScalar;
			}
			set
			{
				this.syncedScalar = Mathf.Clamp(value, 0.1f, float.MaxValue);
			}
		}

		// Token: 0x06001377 RID: 4983 RVA: 0x0005E919 File Offset: 0x0005CB19
		internal void AddContext(Context target)
		{
			if (!this.activeContexts.Add(target))
			{
				return;
			}
			if (!this.IsActive)
			{
				return;
			}
			this.StartEffectRoutine(target);
		}

		// Token: 0x06001378 RID: 4984 RVA: 0x0005E93A File Offset: 0x0005CB3A
		private void StartEffectRoutine(Context target)
		{
			this.effectRoutinesByContext.Add(target, base.StartCoroutine(this.EffectRoutine(target, this.InitialInterval)));
		}

		// Token: 0x06001379 RID: 4985 RVA: 0x0005E95B File Offset: 0x0005CB5B
		private IEnumerator EffectRoutine(Context target, float interval)
		{
			yield return new WaitForSeconds(interval);
			this.AffectTarget(target);
			this.effectRoutinesByContext[target] = base.StartCoroutine(this.EffectRoutine(target, Mathf.Clamp(interval * this.IntervalScalar, this.minimumInterval, this.maximumInterval)));
			yield break;
		}

		// Token: 0x0600137A RID: 4986 RVA: 0x0005E978 File Offset: 0x0005CB78
		private void AffectTarget(Context target)
		{
			object[] array;
			this.endlessScriptComponent.TryExecuteFunction("AffectContext", out array, new object[] { target });
		}

		// Token: 0x0600137B RID: 4987 RVA: 0x0005E9A2 File Offset: 0x0005CBA2
		internal void RemoveContext(Context target)
		{
			if (this.activeContexts.Remove(target))
			{
				this.StopEffectRoutine(target);
			}
		}

		// Token: 0x0600137C RID: 4988 RVA: 0x0005E9BC File Offset: 0x0005CBBC
		private void StopEffectRoutine(Context target)
		{
			Coroutine coroutine;
			if (!this.effectRoutinesByContext.TryGetValue(target, out coroutine))
			{
				return;
			}
			base.StopCoroutine(coroutine);
			this.effectRoutinesByContext.Remove(target);
		}

		// Token: 0x0600137D RID: 4989 RVA: 0x0005E9F0 File Offset: 0x0005CBF0
		public void DeactivateEffector(Context _)
		{
			this.IsActive = false;
			foreach (KeyValuePair<Context, Coroutine> keyValuePair in this.effectRoutinesByContext)
			{
				base.StopCoroutine(keyValuePair.Value);
			}
			this.effectRoutinesByContext.Clear();
		}

		// Token: 0x0600137E RID: 4990 RVA: 0x0005EA5C File Offset: 0x0005CC5C
		public void ActivateEffector(Context _)
		{
			if (this.IsActive)
			{
				return;
			}
			foreach (Context context in this.activeContexts)
			{
				this.StartEffectRoutine(context);
			}
		}

		// Token: 0x0600137F RID: 4991 RVA: 0x0005EAB8 File Offset: 0x0005CCB8
		public void EndlessGameEnd()
		{
			this.DeactivateEffector(null);
		}

		// Token: 0x06001380 RID: 4992 RVA: 0x0005EAC1 File Offset: 0x0005CCC1
		public void EndlessAwake()
		{
			if (NetworkManager.Singleton.IsServer)
			{
				this.InitialInterval = this.initialInterval;
				this.IntervalScalar = this.intervalScalar;
				this.IsActive = this.startActive;
			}
		}

		// Token: 0x170003FC RID: 1020
		// (get) Token: 0x06001381 RID: 4993 RVA: 0x0005EAF3 File Offset: 0x0005CCF3
		// (set) Token: 0x06001382 RID: 4994 RVA: 0x0005EAFB File Offset: 0x0005CCFB
		public WorldObject WorldObject { get; private set; }

		// Token: 0x06001383 RID: 4995 RVA: 0x0005EB04 File Offset: 0x0005CD04
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x170003FD RID: 1021
		// (get) Token: 0x06001384 RID: 4996 RVA: 0x0005EB0D File Offset: 0x0005CD0D
		// (set) Token: 0x06001385 RID: 4997 RVA: 0x0005EB15 File Offset: 0x0005CD15
		public Context Context { get; private set; }

		// Token: 0x06001386 RID: 4998 RVA: 0x0005EB1E File Offset: 0x0005CD1E
		public void InitializeContext()
		{
			this.Context = new Context(this.WorldObject);
		}

		// Token: 0x170003FE RID: 1022
		// (get) Token: 0x06001387 RID: 4999 RVA: 0x0005EB34 File Offset: 0x0005CD34
		public object LuaObject
		{
			get
			{
				object obj;
				if ((obj = this.luaObject) == null)
				{
					obj = (this.luaObject = new Effector(this));
				}
				return obj;
			}
		}

		// Token: 0x170003FF RID: 1023
		// (get) Token: 0x06001388 RID: 5000 RVA: 0x0005EB5A File Offset: 0x0005CD5A
		public Type LuaObjectType
		{
			get
			{
				return typeof(Effector);
			}
		}

		// Token: 0x06001389 RID: 5001 RVA: 0x0005EB66 File Offset: 0x0005CD66
		public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.endlessScriptComponent = endlessScriptComponent;
		}

		// Token: 0x04001053 RID: 4179
		[SerializeField]
		private float initialInterval;

		// Token: 0x04001054 RID: 4180
		[SerializeField]
		private float intervalScalar;

		// Token: 0x04001055 RID: 4181
		[SerializeField]
		private float minimumInterval;

		// Token: 0x04001056 RID: 4182
		[SerializeField]
		private float maximumInterval;

		// Token: 0x04001057 RID: 4183
		[SerializeField]
		private bool startActive;

		// Token: 0x04001058 RID: 4184
		private float syncedInterval;

		// Token: 0x04001059 RID: 4185
		private float syncedScalar;

		// Token: 0x0400105A RID: 4186
		private bool isActive;

		// Token: 0x0400105B RID: 4187
		private readonly HashSet<Context> activeContexts = new HashSet<Context>();

		// Token: 0x0400105C RID: 4188
		private readonly Dictionary<Context, Coroutine> effectRoutinesByContext = new Dictionary<Context, Coroutine>();

		// Token: 0x0400105F RID: 4191
		private EndlessScriptComponent endlessScriptComponent;

		// Token: 0x04001060 RID: 4192
		private object luaObject;
	}
}
