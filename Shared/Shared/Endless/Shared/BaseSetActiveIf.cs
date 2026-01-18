using System;
using Endless.Shared.Debugging;
using Endless.Shared.Validation;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x0200008C RID: 140
	public abstract class BaseSetActiveIf : MonoBehaviour, IValidatable
	{
		// Token: 0x170000AD RID: 173
		// (get) Token: 0x060003FF RID: 1023 RVA: 0x000117B6 File Offset: 0x0000F9B6
		// (set) Token: 0x06000400 RID: 1024 RVA: 0x000117BE File Offset: 0x0000F9BE
		protected bool VerboseLogging { get; set; }

		// Token: 0x06000401 RID: 1025 RVA: 0x000117C7 File Offset: 0x0000F9C7
		[ContextMenu("Validate")]
		public virtual void Validate()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Validate", this);
			}
			DebugUtility.DebugHasNullItem<GameObject>(this.Targets, "Targets", this);
		}

		// Token: 0x06000402 RID: 1026 RVA: 0x000117F0 File Offset: 0x0000F9F0
		protected void SetActive(bool value)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetActive", "value", value), this);
			}
			if (this.Inverse)
			{
				value = !value;
			}
			for (int i = 0; i < this.Targets.Length; i++)
			{
				if (this.Targets[i] == null)
				{
					DebugUtility.LogError(string.Format("{0}[{1}] is null!", "Targets", i), this);
				}
				else
				{
					this.Targets[i].SetActive(value);
				}
			}
		}

		// Token: 0x040001EB RID: 491
		[Tooltip("Defaults to self if null")]
		[SerializeField]
		protected GameObject[] Targets = Array.Empty<GameObject>();

		// Token: 0x040001EC RID: 492
		[SerializeField]
		protected bool Inverse;
	}
}
