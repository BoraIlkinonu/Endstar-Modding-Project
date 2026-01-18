using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.Scripting;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000310 RID: 784
	public class UIScriptDataWizard : MonoBehaviourSingleton<UIScriptDataWizard>
	{
		// Token: 0x1700020B RID: 523
		// (get) Token: 0x06000E30 RID: 3632 RVA: 0x000436AD File Offset: 0x000418AD
		public UnityEvent<Script> OnUpdatedScript { get; } = new UnityEvent<Script>();

		// Token: 0x1700020C RID: 524
		// (get) Token: 0x06000E31 RID: 3633 RVA: 0x000436B5 File Offset: 0x000418B5
		public SerializableGuid PropAssetId
		{
			get
			{
				return this.prop.AssetID;
			}
		}

		// Token: 0x06000E32 RID: 3634 RVA: 0x000436C8 File Offset: 0x000418C8
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			foreach (Type type in EndlessScriptComponent.LuaEnums)
			{
				this.invalidNames.Add(type.Name);
			}
		}

		// Token: 0x06000E33 RID: 3635 RVA: 0x00043718 File Offset: 0x00041918
		public bool IsValidScriptingName(string input)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "IsValidScriptingName", new object[] { input });
			}
			if (string.IsNullOrEmpty(input))
			{
				return false;
			}
			if (string.IsNullOrWhiteSpace(input))
			{
				return false;
			}
			if (input.Length == 0)
			{
				return false;
			}
			if (char.IsNumber(input[0]))
			{
				return false;
			}
			return !input.Any((char character) => !char.IsLetterOrDigit(character)) && !this.invalidNames.Contains(input);
		}

		// Token: 0x06000E34 RID: 3636 RVA: 0x000437AA File Offset: 0x000419AA
		public Prop GetCloneOfProp()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetCloneOfProp", Array.Empty<object>());
			}
			return this.prop.Clone();
		}

		// Token: 0x06000E35 RID: 3637 RVA: 0x000437CF File Offset: 0x000419CF
		public Script GetCloneOfScript()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetCloneOfScript", Array.Empty<object>());
			}
			return this.script.Clone();
		}

		// Token: 0x06000E36 RID: 3638 RVA: 0x000437F4 File Offset: 0x000419F4
		public void Initialize(Prop prop, Script script)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", new object[] { prop.AssetID, script.AssetID });
			}
			this.prop = prop;
			this.script = script;
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.scriptDataTypeSelectionModalSource, UIModalManagerStackActions.MaintainStack, Array.Empty<object>());
		}

		// Token: 0x06000E37 RID: 3639 RVA: 0x00043850 File Offset: 0x00041A50
		public void SetScriptDataType(ScriptDataTypes newScriptDataType)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetScriptDataType", new object[] { this.scriptDataType });
			}
			this.scriptDataType = newScriptDataType;
			bool flag = false;
			switch (this.scriptDataType)
			{
			case ScriptDataTypes.Enum:
				DebugUtility.LogException(new NotImplementedException(), this);
				return;
			case ScriptDataTypes.InspectorScriptValue:
				MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.inspectorScriptValueTypeSelectionModalSource, UIModalManagerStackActions.MaintainStack, Array.Empty<object>());
				return;
			case ScriptDataTypes.Event:
				flag = true;
				MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.scriptEventInputModalSource, UIModalManagerStackActions.MaintainStack, new object[] { flag });
				return;
			case ScriptDataTypes.Receiver:
				MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.scriptEventInputModalSource, UIModalManagerStackActions.MaintainStack, new object[] { flag });
				return;
			case ScriptDataTypes.ScriptReferences:
				MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.scriptReferenceInputModalSource, UIModalManagerStackActions.MaintainStack, Array.Empty<object>());
				return;
			default:
				DebugUtility.LogNoEnumSupportError<ScriptDataTypes>(this, "SetScriptDataType", this.scriptDataType, new object[] { this.scriptDataType });
				return;
			}
		}

		// Token: 0x06000E38 RID: 3640 RVA: 0x00043953 File Offset: 0x00041B53
		public void Back()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Back", Array.Empty<object>());
			}
		}

		// Token: 0x06000E39 RID: 3641 RVA: 0x0004396D File Offset: 0x00041B6D
		public void Exit()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Exit", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		}

		// Token: 0x06000E3A RID: 3642 RVA: 0x00043991 File Offset: 0x00041B91
		public void UpdateScriptAndExit(Script script)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateScriptAndExit", new object[] { script.AssetID });
			}
			this.OnUpdatedScript.Invoke(script);
			this.Exit();
		}

		// Token: 0x06000E3B RID: 3643 RVA: 0x000439C7 File Offset: 0x00041BC7
		public void UpdateScript(Script script)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateScript", new object[] { script.AssetID });
			}
			this.script = script;
			this.OnUpdatedScript.Invoke(script);
		}

		// Token: 0x04000C0C RID: 3084
		[SerializeField]
		private UIScriptDataTypeSelectionModalView scriptDataTypeSelectionModalSource;

		// Token: 0x04000C0D RID: 3085
		[SerializeField]
		private UIInspectorScriptValueTypeSelectionModalView inspectorScriptValueTypeSelectionModalSource;

		// Token: 0x04000C0E RID: 3086
		[SerializeField]
		private UIInspectorScriptValueInputModalView inspectorScriptValueInputModalSource;

		// Token: 0x04000C0F RID: 3087
		[SerializeField]
		private UIScriptEventInputModalView scriptEventInputModalSource;

		// Token: 0x04000C10 RID: 3088
		[SerializeField]
		private UIScriptReferenceInputModalView scriptReferenceInputModalSource;

		// Token: 0x04000C11 RID: 3089
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000C12 RID: 3090
		[SerializeField]
		private bool superVerboseLogging;

		// Token: 0x04000C13 RID: 3091
		private ScriptDataTypes scriptDataType;

		// Token: 0x04000C14 RID: 3092
		private Prop prop;

		// Token: 0x04000C15 RID: 3093
		private Script script;

		// Token: 0x04000C16 RID: 3094
		private readonly HashSet<string> invalidNames = new HashSet<string>();
	}
}
