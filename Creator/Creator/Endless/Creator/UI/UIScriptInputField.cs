using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Endless.Creator.UI
{
	// Token: 0x020002E0 RID: 736
	public class UIScriptInputField : UIInputField
	{
		// Token: 0x1700019C RID: 412
		// (get) Token: 0x06000C80 RID: 3200 RVA: 0x0001BF89 File Offset: 0x0001A189
		public override bool CanSelectNextUiOnTab
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700019D RID: 413
		// (get) Token: 0x06000C81 RID: 3201 RVA: 0x0003B9D4 File Offset: 0x00039BD4
		public UnityEvent OnTabInputUnityEvent { get; } = new UnityEvent();

		// Token: 0x1700019E RID: 414
		// (get) Token: 0x06000C82 RID: 3202 RVA: 0x0003B9DC File Offset: 0x00039BDC
		public UnityEvent OnNewLineInputUnityEvent { get; } = new UnityEvent();

		// Token: 0x06000C83 RID: 3203 RVA: 0x0003B9E4 File Offset: 0x00039BE4
		protected override bool IsValidChar(char character)
		{
			if (this.blockNextIsValidChar)
			{
				this.blockNextIsValidChar = false;
				return false;
			}
			bool key = EndlessInput.GetKey(Key.Tab);
			bool key2 = EndlessInput.GetKey(Key.Enter);
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "IsValidChar", new object[] { character });
				DebugUtility.Log(string.Format("{0}:{1}", "AutoComplete", this.model.AutoComplete.Count), this);
				DebugUtility.Log(string.Format("{0}:{1}", "isTab", key), this);
				DebugUtility.Log(string.Format("{0}:{1}", "isNewLine", key2), this);
			}
			if (this.model.AutoComplete.Count == 0)
			{
				return base.IsValidChar(character);
			}
			if (key)
			{
				this.OnTabInputUnityEvent.Invoke();
				this.blockNextIsValidChar = true;
				return false;
			}
			if (key2)
			{
				this.OnNewLineInputUnityEvent.Invoke();
				this.blockNextIsValidChar = true;
				return false;
			}
			return base.IsValidChar(character);
		}

		// Token: 0x04000ABF RID: 2751
		[Header("UIScriptInputField")]
		[SerializeField]
		private UIScriptWindowModel model;

		// Token: 0x04000AC0 RID: 2752
		private bool blockNextIsValidChar;
	}
}
