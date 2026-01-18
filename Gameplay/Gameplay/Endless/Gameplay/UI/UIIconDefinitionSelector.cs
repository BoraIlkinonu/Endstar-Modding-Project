using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003AC RID: 940
	public class UIIconDefinitionSelector : UIGameObject
	{
		// Token: 0x170004EF RID: 1263
		// (get) Token: 0x060017FC RID: 6140 RVA: 0x0006F71A File Offset: 0x0006D91A
		public UnityEvent<IconDefinition> OnSelectedUnityEvent { get; } = new UnityEvent<IconDefinition>();

		// Token: 0x060017FD RID: 6141 RVA: 0x0006F722 File Offset: 0x0006D922
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.listModel.ItemSelectedUnityEvent.AddListener(new UnityAction<int>(this.OnSelection));
		}

		// Token: 0x060017FE RID: 6142 RVA: 0x0006F758 File Offset: 0x0006D958
		public void Initialize(IconDefinition selection, bool triggerOnSelected)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", new object[] { selection, triggerOnSelected });
			}
			this.listModel.ClearSelected(true);
			if (!this.listModel.Initialized)
			{
				this.listModel.Initialize();
			}
			if (selection == null)
			{
				return;
			}
			int num = this.listModel.ReadOnlyList.IndexOf(selection);
			if (num < 0)
			{
				DebugUtility.LogError(string.Format("Could not find {0} in list!", selection), this);
				return;
			}
			this.listModel.Select(num, triggerOnSelected);
		}

		// Token: 0x060017FF RID: 6143 RVA: 0x0006F7F0 File Offset: 0x0006D9F0
		private void OnSelection(int index)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSelection", new object[] { index });
			}
			IconDefinition iconDefinition = this.listModel[index];
			this.OnSelectedUnityEvent.Invoke(iconDefinition);
		}

		// Token: 0x04001342 RID: 4930
		[SerializeField]
		private UIIconDefinitionListModel listModel;

		// Token: 0x04001343 RID: 4931
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
