using System;
using System.Collections.Generic;
using System.Globalization;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Newtonsoft.Json;
using NLua;
using UnityEngine;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x02000494 RID: 1172
	public class Context
	{
		// Token: 0x06001C99 RID: 7321 RVA: 0x0007DD90 File Offset: 0x0007BF90
		internal Context(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x17000598 RID: 1432
		// (get) Token: 0x06001C9A RID: 7322 RVA: 0x0007DDEC File Offset: 0x0007BFEC
		internal WorldObject WorldObject { get; }

		// Token: 0x17000599 RID: 1433
		// (get) Token: 0x06001C9B RID: 7323 RVA: 0x0007DDF4 File Offset: 0x0007BFF4
		// (set) Token: 0x06001C9C RID: 7324 RVA: 0x0007DDFC File Offset: 0x0007BFFC
		internal string InternalId { get; set; }

		// Token: 0x1700059A RID: 1434
		// (get) Token: 0x06001C9D RID: 7325 RVA: 0x0007DE05 File Offset: 0x0007C005
		public string UniqueId
		{
			get
			{
				if (!string.IsNullOrEmpty(this.InternalId))
				{
					return this.InternalId;
				}
				return MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(this.WorldObject.gameObject);
			}
		}

		// Token: 0x1700059B RID: 1435
		// (get) Token: 0x06001C9E RID: 7326 RVA: 0x0007DE3A File Offset: 0x0007C03A
		public Context LevelContext
		{
			get
			{
				return Context.StaticLevelContext;
			}
		}

		// Token: 0x1700059C RID: 1436
		// (get) Token: 0x06001C9F RID: 7327 RVA: 0x0004EBBD File Offset: 0x0004CDBD
		public Context GameContext
		{
			get
			{
				return Context.StaticGameContext;
			}
		}

		// Token: 0x1700059D RID: 1437
		// (get) Token: 0x06001CA0 RID: 7328 RVA: 0x0007DE41 File Offset: 0x0007C041
		public Context LastContext
		{
			get
			{
				return Context.StaticLastContext;
			}
		}

		// Token: 0x1700059E RID: 1438
		// (get) Token: 0x06001CA1 RID: 7329 RVA: 0x0007DE48 File Offset: 0x0007C048
		// (set) Token: 0x06001CA2 RID: 7330 RVA: 0x0007DE4F File Offset: 0x0007C04F
		internal static Context StaticLevelContext { get; set; }

		// Token: 0x1700059F RID: 1439
		// (get) Token: 0x06001CA3 RID: 7331 RVA: 0x0007DE57 File Offset: 0x0007C057
		// (set) Token: 0x06001CA4 RID: 7332 RVA: 0x0007DE5E File Offset: 0x0007C05E
		internal static Context StaticGameContext { get; set; }

		// Token: 0x170005A0 RID: 1440
		// (get) Token: 0x06001CA5 RID: 7333 RVA: 0x0007DE66 File Offset: 0x0007C066
		// (set) Token: 0x06001CA6 RID: 7334 RVA: 0x0007DE6D File Offset: 0x0007C06D
		internal static Context StaticLastContext { get; set; }

		// Token: 0x06001CA7 RID: 7335 RVA: 0x0007DE75 File Offset: 0x0007C075
		public bool IsNpc()
		{
			return this.WorldObject.BaseType is NpcEntity;
		}

		// Token: 0x06001CA8 RID: 7336 RVA: 0x0007DE8C File Offset: 0x0007C08C
		public bool IsPlayer()
		{
			PlayerLuaComponent playerLuaComponent;
			return this.WorldObject && this.WorldObject.TryGetUserComponent<PlayerLuaComponent>(out playerLuaComponent);
		}

		// Token: 0x06001CA9 RID: 7337 RVA: 0x0007DEB5 File Offset: 0x0007C0B5
		public Vector3 GetPosition()
		{
			return (this.WorldObject.BaseType as MonoBehaviour).gameObject.transform.position;
		}

		// Token: 0x06001CAA RID: 7338 RVA: 0x0007DED8 File Offset: 0x0007C0D8
		public Vector3 GetRotationVector()
		{
			return (this.WorldObject.BaseType as MonoBehaviour).gameObject.transform.rotation.eulerAngles;
		}

		// Token: 0x06001CAB RID: 7339 RVA: 0x0007DF0C File Offset: 0x0007C10C
		public float GetYAxisRotation()
		{
			return (this.WorldObject.BaseType as MonoBehaviour).gameObject.transform.rotation.eulerAngles.y;
		}

		// Token: 0x06001CAC RID: 7340 RVA: 0x0007DF48 File Offset: 0x0007C148
		public CellReference GetCellReference()
		{
			MonoBehaviour monoBehaviour = this.WorldObject.BaseType as MonoBehaviour;
			Vector3Int vector3Int = Stage.WorldSpacePointToGridCoordinate(monoBehaviour.gameObject.transform.position);
			return new CellReference
			{
				Cell = new Vector3?(vector3Int),
				Rotation = new float?(monoBehaviour.gameObject.transform.rotation.eulerAngles.y)
			};
		}

		// Token: 0x06001CAD RID: 7341 RVA: 0x0007DFBC File Offset: 0x0007C1BC
		public void SetTable(string name, LuaTable value)
		{
			string text = LuaTableJsonConverter.Serialize(value);
			this.SetValue(name, text);
			LuaInterfaceEvent onTableSet = this.OnTableSet;
			if (onTableSet == null)
			{
				return;
			}
			onTableSet.InvokeEvent(new object[] { this, name, value });
		}

		// Token: 0x06001CAE RID: 7342 RVA: 0x0007DFFC File Offset: 0x0007C1FC
		public LuaTable GetTable(string name)
		{
			if (this.HasMember(name))
			{
				string text = this.members[name];
				if (!string.IsNullOrEmpty(text))
				{
					try
					{
						return LuaTableJsonConverter.Deserialize(this.WorldObject.EndlessProp.ScriptComponent.Lua, text);
					}
					catch
					{
						return null;
					}
				}
			}
			return null;
		}

		// Token: 0x06001CAF RID: 7343 RVA: 0x0007E060 File Offset: 0x0007C260
		public void SetFloat(string name, float value)
		{
			this.SetValue(name, value.ToString("R", CultureInfo.InvariantCulture));
			LuaInterfaceEvent onFloatSet = this.OnFloatSet;
			if (onFloatSet == null)
			{
				return;
			}
			onFloatSet.InvokeEvent(new object[] { this, name, value });
		}

		// Token: 0x06001CB0 RID: 7344 RVA: 0x0007E0AC File Offset: 0x0007C2AC
		public float GetFloat(string name)
		{
			float num;
			if (this.HasMember(name) && float.TryParse(this.members[name], NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out num))
			{
				return num;
			}
			return 0f;
		}

		// Token: 0x06001CB1 RID: 7345 RVA: 0x0007E0E8 File Offset: 0x0007C2E8
		public void SetInt(string name, int value)
		{
			this.SetValue(name, value);
			LuaInterfaceEvent onIntSet = this.OnIntSet;
			if (onIntSet == null)
			{
				return;
			}
			onIntSet.InvokeEvent(new object[] { this, name, value });
		}

		// Token: 0x06001CB2 RID: 7346 RVA: 0x0007E120 File Offset: 0x0007C320
		public int GetInt(string name)
		{
			int num;
			if (this.HasMember(name) && int.TryParse(this.members[name], out num))
			{
				return num;
			}
			return 0;
		}

		// Token: 0x06001CB3 RID: 7347 RVA: 0x0007E14E File Offset: 0x0007C34E
		public void SetBool(string name, bool value)
		{
			this.SetValue(name, value);
			LuaInterfaceEvent onBoolSet = this.OnBoolSet;
			if (onBoolSet == null)
			{
				return;
			}
			onBoolSet.InvokeEvent(new object[] { this, name, value });
		}

		// Token: 0x06001CB4 RID: 7348 RVA: 0x0007E184 File Offset: 0x0007C384
		public bool GetBool(string name)
		{
			bool flag;
			return this.HasMember(name) && bool.TryParse(this.members[name], out flag) && flag;
		}

		// Token: 0x06001CB5 RID: 7349 RVA: 0x0007E1B2 File Offset: 0x0007C3B2
		public void SetString(string name, string value)
		{
			this.SetValue(name, value);
			LuaInterfaceEvent onStringSet = this.OnStringSet;
			if (onStringSet == null)
			{
				return;
			}
			onStringSet.InvokeEvent(new object[] { this, name, value });
		}

		// Token: 0x06001CB6 RID: 7350 RVA: 0x0007E1DE File Offset: 0x0007C3DE
		public string GetString(string name)
		{
			if (this.HasMember(name))
			{
				return this.members[name];
			}
			return string.Empty;
		}

		// Token: 0x06001CB7 RID: 7351 RVA: 0x0007E1FB File Offset: 0x0007C3FB
		public bool HasMember(string name)
		{
			return this.members.ContainsKey(name);
		}

		// Token: 0x06001CB8 RID: 7352 RVA: 0x0007E20C File Offset: 0x0007C40C
		private void SetValue(string name, object value)
		{
			string text = value.ToString();
			if (!this.members.TryAdd(name, text))
			{
				this.members[name] = text;
			}
		}

		// Token: 0x06001CB9 RID: 7353 RVA: 0x0007E23C File Offset: 0x0007C43C
		internal void LoadFromJson(string loadedState)
		{
			if (loadedState != null)
			{
				this.members = JsonConvert.DeserializeObject<Dictionary<string, string>>(loadedState);
			}
		}

		// Token: 0x06001CBA RID: 7354 RVA: 0x0007E24D File Offset: 0x0007C44D
		internal string ToJson()
		{
			return JsonConvert.SerializeObject(this.members);
		}

		// Token: 0x06001CBB RID: 7355 RVA: 0x0007E25C File Offset: 0x0007C45C
		public object TryGetComponent(string componentName)
		{
			if (this.WorldObject == null)
			{
				Debug.LogError("I DONT HAVE A WORLD OBJECT");
			}
			if (componentName == "PhysicsComponent")
			{
				IPhysicsTaker physicsTaker = this.WorldObject.GetComponentInChildren(typeof(IPhysicsTaker)) as IPhysicsTaker;
				if (physicsTaker != null)
				{
					return physicsTaker.LuaObject;
				}
				return null;
			}
			else
			{
				if (!(componentName == "Player"))
				{
					Type type = null;
					ComponentDefinition componentDefinition;
					BaseTypeDefinition baseTypeDefinition;
					if (MonoBehaviourSingleton<StageManager>.Instance.ComponentList.TryGetDefinition(componentName, out componentDefinition))
					{
						type = componentDefinition.ComponentBase.GetType();
					}
					else if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(componentName, out baseTypeDefinition))
					{
						type = baseTypeDefinition.ComponentReferenceType;
						IScriptInjector scriptInjector = this.WorldObject.BaseType as IScriptInjector;
						if (scriptInjector != null && scriptInjector.AllowLuaReference)
						{
							return scriptInjector.LuaObject;
						}
					}
					object obj;
					if (type != null && this.WorldObject.TryGetUserComponent(type, out obj))
					{
						IScriptInjector scriptInjector2 = obj as IScriptInjector;
						if (scriptInjector2 != null && scriptInjector2.AllowLuaReference)
						{
							return scriptInjector2.LuaObject;
						}
					}
					return null;
				}
				PlayerLuaComponent playerLuaComponent;
				if (!this.WorldObject.TryGetUserComponent<PlayerLuaComponent>(out playerLuaComponent))
				{
					return null;
				}
				return playerLuaComponent.LuaObject;
			}
		}

		// Token: 0x06001CBC RID: 7356 RVA: 0x0007E383 File Offset: 0x0007C583
		public override string ToString()
		{
			if (this.WorldObject != null)
			{
				return this.WorldObject.gameObject.name;
			}
			return "Context with no WorldObject!?";
		}

		// Token: 0x040016AE RID: 5806
		private Dictionary<string, string> members = new Dictionary<string, string>();

		// Token: 0x040016AF RID: 5807
		public LuaInterfaceEvent OnBoolSet = new LuaInterfaceEvent();

		// Token: 0x040016B0 RID: 5808
		public LuaInterfaceEvent OnIntSet = new LuaInterfaceEvent();

		// Token: 0x040016B1 RID: 5809
		public LuaInterfaceEvent OnFloatSet = new LuaInterfaceEvent();

		// Token: 0x040016B2 RID: 5810
		public LuaInterfaceEvent OnStringSet = new LuaInterfaceEvent();

		// Token: 0x040016B3 RID: 5811
		public LuaInterfaceEvent OnTableSet = new LuaInterfaceEvent();
	}
}
