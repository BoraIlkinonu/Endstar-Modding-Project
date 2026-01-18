using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000F4 RID: 244
	[Serializable]
	public class InstanceReference : InspectorPropReference
	{
		// Token: 0x06000564 RID: 1380 RVA: 0x0001B7B7 File Offset: 0x000199B7
		internal InstanceReference()
		{
		}

		// Token: 0x06000565 RID: 1381 RVA: 0x0001B7BF File Offset: 0x000199BF
		internal InstanceReference(SerializableGuid instanceId, bool useContext)
		{
			this.Id = instanceId;
			this.useContext = useContext;
		}

		// Token: 0x06000566 RID: 1382 RVA: 0x0001B7D8 File Offset: 0x000199D8
		public Context GetContext()
		{
			if (this.useContext)
			{
				return Context.StaticLastContext;
			}
			GameObject instanceObject = this.GetInstanceObject();
			if (!instanceObject)
			{
				return null;
			}
			return instanceObject.GetComponent<WorldObject>().Context;
		}

		// Token: 0x06000567 RID: 1383 RVA: 0x0001B810 File Offset: 0x00019A10
		internal GameObject GetInstanceObject()
		{
			if (this.useContext)
			{
				return Context.StaticLastContext.WorldObject.gameObject;
			}
			if (MonoBehaviourSingleton<StageManager>.Instance && MonoBehaviourSingleton<StageManager>.Instance.ActiveStage && this.Id != SerializableGuid.Empty)
			{
				return MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(this.Id);
			}
			return null;
		}

		// Token: 0x06000568 RID: 1384 RVA: 0x0001B87C File Offset: 0x00019A7C
		internal SerializableGuid GetInstanceDefintionId()
		{
			PropEntry reference = this.GetReference();
			if (reference != null)
			{
				return reference.AssetId;
			}
			return SerializableGuid.Empty;
		}

		// Token: 0x06000569 RID: 1385 RVA: 0x0001B8A0 File Offset: 0x00019AA0
		private PropEntry GetReference()
		{
			if (this.useContext)
			{
				return null;
			}
			PropEntry propEntry;
			if (MonoBehaviourSingleton<StageManager>.Instance && MonoBehaviourSingleton<StageManager>.Instance.ActiveStage && MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.TryGetPropEntry(this.Id, out propEntry))
			{
				return propEntry;
			}
			return null;
		}

		// Token: 0x0600056A RID: 1386 RVA: 0x0001B8F8 File Offset: 0x00019AF8
		public string GetReferenceName()
		{
			if (this.useContext)
			{
				return "Use Context";
			}
			PropEntry propEntry;
			if (MonoBehaviourSingleton<StageManager>.Instance && MonoBehaviourSingleton<StageManager>.Instance.ActiveStage && MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.TryGetPropEntry(this.Id, out propEntry))
			{
				return propEntry.Label;
			}
			return "None";
		}

		// Token: 0x170000E6 RID: 230
		// (get) Token: 0x0600056B RID: 1387 RVA: 0x00017586 File Offset: 0x00015786
		internal override ReferenceFilter Filter
		{
			get
			{
				return ReferenceFilter.NonStatic;
			}
		}

		// Token: 0x0600056C RID: 1388 RVA: 0x0001B95A File Offset: 0x00019B5A
		public override string ToString()
		{
			return string.Format("{0}, {1}: {2}", base.ToString(), "useContext", this.useContext);
		}

		// Token: 0x04000420 RID: 1056
		public bool useContext;
	}
}
