using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x02000344 RID: 836
	public class TriggerVolume : MonoBehaviour, IBaseType, IComponentBase, IScriptInjector
	{
		// Token: 0x17000428 RID: 1064
		// (get) Token: 0x0600145C RID: 5212 RVA: 0x00061F31 File Offset: 0x00060131
		// (set) Token: 0x0600145D RID: 5213 RVA: 0x00061F39 File Offset: 0x00060139
		public int Forward
		{
			get
			{
				return this.forward;
			}
			set
			{
				value = Mathf.Max(value, 0);
				this.forward = value;
				this.RecalculateZAxis();
			}
		}

		// Token: 0x17000429 RID: 1065
		// (get) Token: 0x0600145E RID: 5214 RVA: 0x00061F51 File Offset: 0x00060151
		// (set) Token: 0x0600145F RID: 5215 RVA: 0x00061F59 File Offset: 0x00060159
		public int Backward
		{
			get
			{
				return this.backward;
			}
			set
			{
				value = Mathf.Max(value, 0);
				this.backward = value;
				this.RecalculateZAxis();
			}
		}

		// Token: 0x1700042A RID: 1066
		// (get) Token: 0x06001460 RID: 5216 RVA: 0x00061F71 File Offset: 0x00060171
		// (set) Token: 0x06001461 RID: 5217 RVA: 0x00061F79 File Offset: 0x00060179
		public int Left
		{
			get
			{
				return this.left;
			}
			set
			{
				value = Mathf.Max(value, 0);
				this.left = value;
				this.RecalculateXAxis();
			}
		}

		// Token: 0x1700042B RID: 1067
		// (get) Token: 0x06001462 RID: 5218 RVA: 0x00061F91 File Offset: 0x00060191
		// (set) Token: 0x06001463 RID: 5219 RVA: 0x00061F99 File Offset: 0x00060199
		public int Right
		{
			get
			{
				return this.right;
			}
			set
			{
				value = Mathf.Max(value, 0);
				this.right = value;
				this.RecalculateXAxis();
			}
		}

		// Token: 0x1700042C RID: 1068
		// (get) Token: 0x06001464 RID: 5220 RVA: 0x00061FB1 File Offset: 0x000601B1
		// (set) Token: 0x06001465 RID: 5221 RVA: 0x00061FB9 File Offset: 0x000601B9
		public int Up
		{
			get
			{
				return this.up;
			}
			set
			{
				value = Mathf.Max(value, 0);
				this.up = value;
				this.RecalculateYAxis();
			}
		}

		// Token: 0x1700042D RID: 1069
		// (get) Token: 0x06001466 RID: 5222 RVA: 0x00061FD1 File Offset: 0x000601D1
		// (set) Token: 0x06001467 RID: 5223 RVA: 0x00061FD9 File Offset: 0x000601D9
		public int Down
		{
			get
			{
				return this.down;
			}
			set
			{
				value = Mathf.Max(value, 0);
				this.down = value;
				this.RecalculateYAxis();
			}
		}

		// Token: 0x06001468 RID: 5224 RVA: 0x00061FF4 File Offset: 0x000601F4
		private void RecalculateYAxis()
		{
			int num = this.up + this.down + 1;
			float num2 = (float)(this.up - this.down) / 2f;
			this.scaleTransform.localScale = new global::UnityEngine.Vector3(this.scaleTransform.localScale.x, (float)num, this.scaleTransform.localScale.z);
			this.scaleTransform.localPosition = new global::UnityEngine.Vector3(this.scaleTransform.localPosition.x, num2, this.scaleTransform.localPosition.z);
		}

		// Token: 0x06001469 RID: 5225 RVA: 0x0006208C File Offset: 0x0006028C
		private void RecalculateXAxis()
		{
			int num = this.right + this.left + 1;
			float num2 = (float)(this.right - this.left) / 2f;
			this.scaleTransform.localScale = new global::UnityEngine.Vector3((float)num, this.scaleTransform.localScale.y, this.scaleTransform.localScale.z);
			this.scaleTransform.localPosition = new global::UnityEngine.Vector3(num2, this.scaleTransform.localPosition.y, this.scaleTransform.localPosition.z);
		}

		// Token: 0x0600146A RID: 5226 RVA: 0x00062124 File Offset: 0x00060324
		private void RecalculateZAxis()
		{
			int num = this.forward + this.backward + 1;
			float num2 = (float)(this.forward - this.backward) / 2f;
			this.scaleTransform.localScale = new global::UnityEngine.Vector3(this.scaleTransform.localScale.x, this.scaleTransform.localScale.y, (float)num);
			this.scaleTransform.localPosition = new global::UnityEngine.Vector3(this.scaleTransform.localPosition.x, this.scaleTransform.localPosition.y, num2);
		}

		// Token: 0x0600146B RID: 5227 RVA: 0x000621BC File Offset: 0x000603BC
		private void Awake()
		{
			this.worldTrigger.OnTriggerEnter.AddListener(new UnityAction<WorldCollidable, bool>(this.HandleTriggerEntered));
			this.worldTrigger.OnTriggerExit.AddListener(new UnityAction<WorldCollidable, bool>(this.HandleTriggerExited));
			this.endlessProp.OnInspectionStateChanged.AddListener(new UnityAction<bool>(this.HandleInspectionStateChanged));
		}

		// Token: 0x0600146C RID: 5228 RVA: 0x00062220 File Offset: 0x00060420
		private void HandleTriggerEntered(WorldCollidable worldCollider, bool rollbackFrame)
		{
			if (worldCollider == null || worldCollider.WorldObject == null)
			{
				return;
			}
			WorldObject worldObject = worldCollider.WorldObject;
			Debug.Log(string.Format("Entered {0}, {1}, {2}", worldCollider.gameObject.name, rollbackFrame, worldCollider.IsServer));
			if (!rollbackFrame && worldCollider.IsServer)
			{
				NpcEntity npcEntity;
				if (this.objectTypes.HasFlag(ContextTypes.NPC) && worldObject.TryGetUserComponent<NpcEntity>(out npcEntity))
				{
					this.AttemptTrigger(npcEntity.Context);
					return;
				}
				PlayerLuaComponent playerLuaComponent;
				if (this.objectTypes.HasFlag(ContextTypes.Player) && worldObject.TryGetUserComponent<PlayerLuaComponent>(out playerLuaComponent))
				{
					this.AttemptTrigger(playerLuaComponent.Context);
					return;
				}
				DraggablePhysicsCube draggablePhysicsCube;
				if (this.objectTypes.HasFlag(ContextTypes.PhysicsObject) && worldObject.TryGetUserComponent<DraggablePhysicsCube>(out draggablePhysicsCube))
				{
					this.AttemptTrigger(draggablePhysicsCube.Context);
					return;
				}
			}
		}

		// Token: 0x0600146D RID: 5229 RVA: 0x00062318 File Offset: 0x00060518
		private void HandleTriggerExited(WorldCollidable worldCollidable, bool rollbackFrame)
		{
			if (worldCollidable == null || worldCollidable.WorldObject == null || worldCollidable.WorldObject.Context == null)
			{
				return;
			}
			Context context = worldCollidable.WorldObject.Context;
			if (this.enteredContexts.Remove(context))
			{
				this.OnExited.Invoke(context);
			}
		}

		// Token: 0x0600146E RID: 5230 RVA: 0x00062370 File Offset: 0x00060570
		private void AttemptTrigger(Context context)
		{
			bool flag;
			if (this.endlessScriptComponent.TryExecuteFunction<bool>("OnAttemptEnter", out flag, new object[] { context }))
			{
				if (flag)
				{
					this.HandleContextEntered(context);
					return;
				}
			}
			else
			{
				this.HandleContextEntered(context);
			}
		}

		// Token: 0x0600146F RID: 5231 RVA: 0x000623AD File Offset: 0x000605AD
		private void HandleContextEntered(Context context)
		{
			this.enteredContexts.Add(context);
			this.OnEntered.Invoke(context);
		}

		// Token: 0x06001470 RID: 5232 RVA: 0x000623C7 File Offset: 0x000605C7
		public void ChangeAcceptedObjectTypes(ContextTypes newTypes)
		{
			this.objectTypes = newTypes;
		}

		// Token: 0x06001471 RID: 5233 RVA: 0x000623D0 File Offset: 0x000605D0
		private void HandleInspectionStateChanged(bool isInspected)
		{
			this.volumeVisuals.enabled = isInspected;
		}

		// Token: 0x1700042E RID: 1070
		// (get) Token: 0x06001472 RID: 5234 RVA: 0x000623DE File Offset: 0x000605DE
		// (set) Token: 0x06001473 RID: 5235 RVA: 0x000623E6 File Offset: 0x000605E6
		public WorldObject WorldObject { get; private set; }

		// Token: 0x1700042F RID: 1071
		// (get) Token: 0x06001474 RID: 5236 RVA: 0x000623F0 File Offset: 0x000605F0
		public Context Context
		{
			get
			{
				Context context;
				if ((context = this.context) == null)
				{
					context = (this.context = new Context(this.WorldObject));
				}
				return context;
			}
		}

		// Token: 0x17000430 RID: 1072
		// (get) Token: 0x06001475 RID: 5237 RVA: 0x0001BD04 File Offset: 0x00019F04
		public NavType NavValue
		{
			get
			{
				return NavType.Intangible;
			}
		}

		// Token: 0x06001476 RID: 5238 RVA: 0x0006241B File Offset: 0x0006061B
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x06001477 RID: 5239 RVA: 0x00062424 File Offset: 0x00060624
		public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
			this.endlessProp = endlessProp;
			this.worldTrigger.gameObject.AddComponent<WorldTriggerCollider>().Initialize(this.worldTrigger);
		}

		// Token: 0x17000431 RID: 1073
		// (get) Token: 0x06001478 RID: 5240 RVA: 0x00062448 File Offset: 0x00060648
		public object LuaObject
		{
			get
			{
				object obj;
				if ((obj = this.luaObject) == null)
				{
					obj = (this.luaObject = new TriggerVolume(this));
				}
				return obj;
			}
		}

		// Token: 0x17000432 RID: 1074
		// (get) Token: 0x06001479 RID: 5241 RVA: 0x0006246E File Offset: 0x0006066E
		public Type LuaObjectType
		{
			get
			{
				return typeof(TriggerVolume);
			}
		}

		// Token: 0x0600147A RID: 5242 RVA: 0x0006247A File Offset: 0x0006067A
		public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.endlessScriptComponent = endlessScriptComponent;
		}

		// Token: 0x040010ED RID: 4333
		[SerializeField]
		private WorldTrigger worldTrigger;

		// Token: 0x040010EE RID: 4334
		[SerializeField]
		private ContextTypes objectTypes = ContextTypes.Player | ContextTypes.NPC | ContextTypes.PhysicsObject;

		// Token: 0x040010EF RID: 4335
		public EndlessEvent OnEntered = new EndlessEvent();

		// Token: 0x040010F0 RID: 4336
		public EndlessEvent OnExited = new EndlessEvent();

		// Token: 0x040010F1 RID: 4337
		[SerializeField]
		private Transform scaleTransform;

		// Token: 0x040010F2 RID: 4338
		[SerializeField]
		private Renderer volumeVisuals;

		// Token: 0x040010F3 RID: 4339
		private List<Context> enteredContexts = new List<Context>();

		// Token: 0x040010F4 RID: 4340
		private int forward;

		// Token: 0x040010F5 RID: 4341
		private int backward;

		// Token: 0x040010F6 RID: 4342
		private int left;

		// Token: 0x040010F7 RID: 4343
		private int right;

		// Token: 0x040010F8 RID: 4344
		private int up;

		// Token: 0x040010F9 RID: 4345
		private int down;

		// Token: 0x040010FA RID: 4346
		[SerializeField]
		[HideInInspector]
		private EndlessProp endlessProp;

		// Token: 0x040010FB RID: 4347
		private Context context;

		// Token: 0x040010FD RID: 4349
		private EndlessScriptComponent endlessScriptComponent;

		// Token: 0x040010FE RID: 4350
		private object luaObject;
	}
}
