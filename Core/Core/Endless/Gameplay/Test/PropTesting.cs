using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Props.Assets;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay.Test
{
	// Token: 0x02000016 RID: 22
	public class PropTesting : MonoBehaviour
	{
		// Token: 0x06000052 RID: 82 RVA: 0x00003CF8 File Offset: 0x00001EF8
		private void Start()
		{
			Prop prop = this.prop.Clone();
			Script script = this.script.Clone();
			prop.SetComponentIds(this.baseType.ComponentId, this.components.Select((ComponentDefinition component) => component.ComponentId).ToList<string>());
			if (string.IsNullOrEmpty(this.script.AssetID))
			{
				script = null;
				prop.ScriptAsset = null;
			}
			else
			{
				this.script.SetComponentIds(this.baseType.ComponentId, this.components.Select((ComponentDefinition component) => component.ComponentId).ToList<string>());
			}
			MonoBehaviourSingleton<StageManager>.Instance.InjectProp(prop, this.testPrefab, script, this.icon);
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00003DDC File Offset: 0x00001FDC
		[ContextMenu("Setup script org data")]
		public void SetupScriptOrgData()
		{
			this.script.UpdateOrganizationData();
		}

		// Token: 0x0400003B RID: 59
		[SerializeField]
		private Prop prop;

		// Token: 0x0400003C RID: 60
		[SerializeField]
		private Script script;

		// Token: 0x0400003D RID: 61
		[SerializeField]
		private ComponentDefinition baseType;

		// Token: 0x0400003E RID: 62
		[SerializeField]
		private List<ComponentDefinition> components;

		// Token: 0x0400003F RID: 63
		[SerializeField]
		private Sprite icon;

		// Token: 0x04000040 RID: 64
		[SerializeField]
		private IconDefinition abstractIconDefinition;

		// Token: 0x04000041 RID: 65
		[SerializeField]
		private GameObject testPrefab;
	}
}
