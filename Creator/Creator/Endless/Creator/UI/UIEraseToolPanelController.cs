using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002A1 RID: 673
	public class UIEraseToolPanelController : UIGameObject
	{
		// Token: 0x06000B37 RID: 2871 RVA: 0x00034844 File Offset: 0x00032A44
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.eraseTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<EraseTool>();
			this.terrain.OnChange.AddListener(new UnityAction<bool>(this.ToggleTerrain));
			this.unwiredProps.OnChange.AddListener(new UnityAction<bool>(this.ToggleUnwiredProps));
			this.wiredProps.OnChange.AddListener(new UnityAction<bool>(this.ToggleWiredProps));
		}

		// Token: 0x06000B38 RID: 2872 RVA: 0x000348CD File Offset: 0x00032ACD
		private void ToggleTerrain(bool state)
		{
			if (this.verboseLogging)
			{
				Debug.LogFormat(this, "ToggleTerrain ( state: {0} )", new object[] { state });
			}
			this.eraseTool.ToggleCurrentFunction(EraseToolFunction.Terrain, state);
		}

		// Token: 0x06000B39 RID: 2873 RVA: 0x000348FE File Offset: 0x00032AFE
		private void ToggleUnwiredProps(bool state)
		{
			if (this.verboseLogging)
			{
				Debug.LogFormat(this, "ToggleUnwiredProps ( state: {0} )", new object[] { state });
			}
			this.eraseTool.ToggleCurrentFunction(EraseToolFunction.UnwiredProps, state);
		}

		// Token: 0x06000B3A RID: 2874 RVA: 0x00034930 File Offset: 0x00032B30
		private void ToggleWiredProps(bool state)
		{
			if (this.verboseLogging)
			{
				Debug.LogFormat(this, "ToggleWiredProps ( state: {0} )", new object[] { state });
			}
			this.eraseTool.ToggleCurrentFunction(EraseToolFunction.WiredProps, state);
			if (!state)
			{
				this.eraseTool.ToggleCurrentFunction(EraseToolFunction.UnwiredProps, true);
			}
		}

		// Token: 0x04000976 RID: 2422
		[SerializeField]
		private UIToggle terrain;

		// Token: 0x04000977 RID: 2423
		[SerializeField]
		private UIToggle unwiredProps;

		// Token: 0x04000978 RID: 2424
		[SerializeField]
		private UIToggle wiredProps;

		// Token: 0x04000979 RID: 2425
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400097A RID: 2426
		private EraseTool eraseTool;
	}
}
