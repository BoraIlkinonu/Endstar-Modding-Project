using System;
using Endless.Shared.Debugging;
using TMPro;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000272 RID: 626
	public class UIText : UIGameObject, IPoolableT
	{
		// Token: 0x170002F5 RID: 757
		// (get) Token: 0x06000FB6 RID: 4022 RVA: 0x0004378C File Offset: 0x0004198C
		// (set) Token: 0x06000FB7 RID: 4023 RVA: 0x00043794 File Offset: 0x00041994
		public TextMeshProUGUI TextMeshPro { get; private set; }

		// Token: 0x170002F6 RID: 758
		// (get) Token: 0x06000FB8 RID: 4024 RVA: 0x0004379D File Offset: 0x0004199D
		// (set) Token: 0x06000FB9 RID: 4025 RVA: 0x000437A5 File Offset: 0x000419A5
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x170002F7 RID: 759
		// (get) Token: 0x06000FBA RID: 4026 RVA: 0x000050D2 File Offset: 0x000032D2
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170002F8 RID: 760
		// (get) Token: 0x06000FBB RID: 4027 RVA: 0x000437AE File Offset: 0x000419AE
		// (set) Token: 0x06000FBC RID: 4028 RVA: 0x000437BB File Offset: 0x000419BB
		public string Value
		{
			get
			{
				return this.TextMeshPro.text;
			}
			set
			{
				if (!string.Equals(this.TextMeshPro.text, value))
				{
					this.TextMeshPro.SetText(value, true);
					if (this.layoutables.Length != 0)
					{
						this.layoutables.RequestLayout();
					}
				}
			}
		}

		// Token: 0x170002F9 RID: 761
		// (get) Token: 0x06000FBD RID: 4029 RVA: 0x000437F4 File Offset: 0x000419F4
		public bool IsNullOrEmptyOrWhiteSpace
		{
			get
			{
				return this.Value.IsNullOrEmptyOrWhiteSpace();
			}
		}

		// Token: 0x06000FBE RID: 4030 RVA: 0x00043804 File Offset: 0x00041A04
		public bool WouldOverflow(string input)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "WouldOverflow", new object[] { input });
			}
			if (string.IsNullOrEmpty(input))
			{
				return false;
			}
			Vector2 preferredValues = this.TextMeshPro.GetPreferredValues(input);
			float width = this.TextMeshPro.rectTransform.rect.width;
			if (preferredValues.x > width)
			{
				return true;
			}
			float height = this.TextMeshPro.rectTransform.rect.height;
			return preferredValues.y > height;
		}

		// Token: 0x06000FBF RID: 4031 RVA: 0x0004388E File Offset: 0x00041A8E
		public void Clear()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			this.Value = string.Empty;
		}

		// Token: 0x04000A0A RID: 2570
		[SerializeField]
		private InterfaceReference<IUILayoutable>[] layoutables = Array.Empty<InterfaceReference<IUILayoutable>>();

		// Token: 0x04000A0B RID: 2571
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
