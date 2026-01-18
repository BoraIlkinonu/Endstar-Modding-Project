using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000247 RID: 583
	public class UIRolesRadio : UIBaseEnumRadio<Roles>
	{
		// Token: 0x170002CD RID: 717
		// (get) Token: 0x06000EDD RID: 3805 RVA: 0x0003FF94 File Offset: 0x0003E194
		// (set) Token: 0x06000EDE RID: 3806 RVA: 0x0003FF9C File Offset: 0x0003E19C
		public Roles OriginalValue { get; private set; } = Roles.None;

		// Token: 0x170002CE RID: 718
		// (get) Token: 0x06000EDF RID: 3807 RVA: 0x0003FFA5 File Offset: 0x0003E1A5
		protected override Roles[] Values
		{
			get
			{
				return this.rolesInOrder;
			}
		}

		// Token: 0x06000EE0 RID: 3808 RVA: 0x0003FFB0 File Offset: 0x0003E1B0
		public override void Validate()
		{
			base.Validate();
			if (this.onVisuals.Length != this.originalValueVisuals.Length)
			{
				DebugUtility.LogError(string.Format("There must be the same amount of items in {0} ({1}) and {2} ({3})!", new object[]
				{
					"onVisuals",
					this.onVisuals.Length,
					"originalValueVisuals",
					this.originalValueVisuals.Length
				}), this);
			}
			DebugUtility.DebugHasNullItem<GameObject>(this.onVisuals, "onVisuals", this);
			DebugUtility.DebugHasNullItem<GameObject>(this.originalValueVisuals, "originalValueVisuals", this);
		}

		// Token: 0x06000EE1 RID: 3809 RVA: 0x0004003F File Offset: 0x0003E23F
		public void SetOriginalValue(Roles originalValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetOriginalValue", new object[] { originalValue });
			}
			this.OriginalValue = originalValue;
			this.HandleOriginalAndActiveValueVisuals();
		}

		// Token: 0x06000EE2 RID: 3810 RVA: 0x00040070 File Offset: 0x0003E270
		public void DisableControls(int inclusiveIndexStart)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "DisableControls", "inclusiveIndexStart", inclusiveIndexStart), this);
			}
			for (int i = 0; i < this.rolesInOrder.Length; i++)
			{
				bool flag = i < inclusiveIndexStart;
				base.RadioButtons[i].SetInteractable(flag);
				if (base.VerboseLogging)
				{
					DebugUtility.Log(string.Format("{0} enabled: {1}", this.rolesInOrder[i], flag), this);
				}
			}
		}

		// Token: 0x06000EE3 RID: 3811 RVA: 0x000400FA File Offset: 0x0003E2FA
		protected override void Initialize()
		{
			base.Initialize();
			this.HandleOriginalAndActiveValueVisuals();
		}

		// Token: 0x06000EE4 RID: 3812 RVA: 0x00040108 File Offset: 0x0003E308
		private void HandleOriginalAndActiveValueVisuals()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleOriginalAndActiveValueVisuals", Array.Empty<object>());
			}
			for (int i = 0; i < this.rolesInOrder.Length; i++)
			{
				bool flag = this.rolesInOrder[i] == base.Value;
				this.onVisuals[i].SetActive(flag);
				this.originalValueVisuals[i].SetActive(i == this.OriginalValue.Level());
			}
		}

		// Token: 0x04000956 RID: 2390
		[Header("UIRolesRadio")]
		[SerializeField]
		private GameObject[] onVisuals = Array.Empty<GameObject>();

		// Token: 0x04000957 RID: 2391
		[SerializeField]
		private GameObject[] originalValueVisuals = Array.Empty<GameObject>();

		// Token: 0x04000958 RID: 2392
		private readonly Roles[] rolesInOrder = new Roles[]
		{
			Roles.None,
			Roles.Viewer,
			Roles.Editor,
			Roles.Publisher,
			Roles.Owner
		};
	}
}
