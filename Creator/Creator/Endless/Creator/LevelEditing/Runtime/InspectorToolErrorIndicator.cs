using System;
using System.Collections.Generic;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using TMPro;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x0200035D RID: 861
	public class InspectorToolErrorIndicator : PooledGameObject
	{
		// Token: 0x06001059 RID: 4185 RVA: 0x0004D618 File Offset: 0x0004B818
		public void InitializeFor(SerializableGuid instanceId)
		{
			this.activeInstanceId = instanceId;
			IEnumerable<ConsoleMessage> consoleMessagesForInstanceId = NetworkBehaviourSingleton<UserScriptingConsole>.Instance.GetConsoleMessagesForInstanceId(instanceId);
			int num = 0;
			int num2 = 0;
			foreach (ConsoleMessage consoleMessage in consoleMessagesForInstanceId)
			{
				if (consoleMessage.LogType == LogType.Warning)
				{
					num++;
				}
				else
				{
					LogType logType = consoleMessage.LogType;
					if (logType == LogType.Assert || logType == LogType.Error || logType == LogType.Exception)
					{
						num2++;
					}
				}
			}
			this.warningLine.SetActive(num > 0);
			this.warningCountLabel.text = num.ToString();
			this.errorLine.SetActive(num > 0);
			this.errorCountLabel.text = num2.ToString();
		}

		// Token: 0x0600105A RID: 4186 RVA: 0x0004D6DC File Offset: 0x0004B8DC
		public void Release()
		{
			this.activeInstanceId = SerializableGuid.Empty;
		}

		// Token: 0x0600105B RID: 4187 RVA: 0x0004D6EC File Offset: 0x0004B8EC
		private void OnMouseDown()
		{
			Debug.Log("Hey! We had a mouse down here!");
			InspectorTool inspectorTool = MonoBehaviourSingleton<ToolManager>.Instance.GetTool(ToolType.Inspector) as InspectorTool;
			if (inspectorTool)
			{
				inspectorTool.ShowConsoleLogFor(this.activeInstanceId);
				return;
			}
			Debug.LogError("Failed to acquire the Inspector Tool from the tool manager! This should *never* happen.");
		}

		// Token: 0x0600105C RID: 4188 RVA: 0x0004D733 File Offset: 0x0004B933
		private void Update()
		{
			base.transform.LookAt(Camera.main.transform.position, global::UnityEngine.Vector3.up);
		}

		// Token: 0x04000D7F RID: 3455
		private SerializableGuid activeInstanceId;

		// Token: 0x04000D80 RID: 3456
		[SerializeField]
		private float multipleOffset = 0.3f;

		// Token: 0x04000D81 RID: 3457
		[SerializeField]
		private GameObject warningLine;

		// Token: 0x04000D82 RID: 3458
		[SerializeField]
		private TextMeshPro warningCountLabel;

		// Token: 0x04000D83 RID: 3459
		[SerializeField]
		private GameObject errorLine;

		// Token: 0x04000D84 RID: 3460
		[SerializeField]
		private TextMeshPro errorCountLabel;
	}
}
