using System;
using System.Linq;
using Endless.Shared.Debugging;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000139 RID: 313
	public class UIDropdownSetActiveHandler : UIGameObject, IValidatable
	{
		// Token: 0x060007E6 RID: 2022 RVA: 0x000215B5 File Offset: 0x0001F7B5
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.dropdown.Interface.OnValueChanged.AddListener(new UnityAction(this.SetActive));
		}

		// Token: 0x060007E7 RID: 2023 RVA: 0x000215F0 File Offset: 0x0001F7F0
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			if (!this.dropdown)
			{
				DebugUtility.LogError("There must be a dropdown!", this);
				return;
			}
			foreach (GameObject gameObject in this.toSetActive)
			{
				DebugUtility.LogError("There is a null item in toSetActive!", this);
			}
			if (this.toSetActive.Length != this.dropdown.Interface.OptionsCount)
			{
				DebugUtility.LogError(string.Format("There must be as many items in {0} ({1}) as are in {2} ({3})!", new object[]
				{
					"toSetActive",
					this.toSetActive.Length,
					"dropdown",
					this.dropdown.Interface.OptionsCount
				}), this);
			}
		}

		// Token: 0x060007E8 RID: 2024 RVA: 0x000216BC File Offset: 0x0001F8BC
		private void SetActive()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetActive", Array.Empty<object>());
			}
			int num = this.dropdown.Interface.ValueIndices.FirstOrDefault<int>();
			for (int i = 0; i < this.toSetActive.Length; i++)
			{
				this.toSetActive[i].SetActive(i == num);
			}
		}

		// Token: 0x040004B3 RID: 1203
		[SerializeField]
		private GameObject[] toSetActive = Array.Empty<GameObject>();

		// Token: 0x040004B4 RID: 1204
		[SerializeField]
		private InterfaceReference<IUIDropdownable> dropdown;

		// Token: 0x040004B5 RID: 1205
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
