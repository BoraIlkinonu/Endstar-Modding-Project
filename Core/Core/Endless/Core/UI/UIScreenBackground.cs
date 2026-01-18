using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Unity.Cinemachine;
using UnityEngine;

namespace Endless.Core.UI
{
	// Token: 0x02000091 RID: 145
	public class UIScreenBackground : MonoBehaviour
	{
		// Token: 0x17000045 RID: 69
		// (get) Token: 0x060002EF RID: 751 RVA: 0x0000FDE6 File Offset: 0x0000DFE6
		// (set) Token: 0x060002F0 RID: 752 RVA: 0x0000FDEE File Offset: 0x0000DFEE
		protected bool VerboseLogging { get; set; }

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x060002F1 RID: 753 RVA: 0x0000FDF7 File Offset: 0x0000DFF7
		public UIBaseScreenView Key
		{
			get
			{
				return this.key;
			}
		}

		// Token: 0x060002F2 RID: 754 RVA: 0x0000FDFF File Offset: 0x0000DFFF
		public virtual void Display()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Display", this);
			}
			this.cinemachineCamera.gameObject.SetActive(true);
		}

		// Token: 0x060002F3 RID: 755 RVA: 0x0000FE25 File Offset: 0x0000E025
		public virtual void Hide()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Hide", this);
			}
			this.cinemachineCamera.gameObject.SetActive(false);
		}

		// Token: 0x0400022B RID: 555
		[Header("UIScreenBackground")]
		[SerializeField]
		private UIBaseScreenView key;

		// Token: 0x0400022C RID: 556
		[SerializeField]
		private CinemachineCamera cinemachineCamera;
	}
}
