using System;
using Endless.Shared.Debugging;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x020001C8 RID: 456
	[RequireComponent(typeof(UIButton))]
	public class UIGenericModalButtonIconAndTextView : UIGameObject, IPoolableT
	{
		// Token: 0x17000223 RID: 547
		// (get) Token: 0x06000B57 RID: 2903 RVA: 0x00031019 File Offset: 0x0002F219
		// (set) Token: 0x06000B58 RID: 2904 RVA: 0x00031021 File Offset: 0x0002F221
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x17000224 RID: 548
		// (get) Token: 0x06000B59 RID: 2905 RVA: 0x000050D2 File Offset: 0x000032D2
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000B5A RID: 2906 RVA: 0x0003102C File Offset: 0x0002F22C
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			UIButton uibutton;
			base.TryGetComponent<UIButton>(out uibutton);
			uibutton.onClick.AddListener(new UnityAction(this.TriggerOnClick));
		}

		// Token: 0x06000B5B RID: 2907 RVA: 0x00031071 File Offset: 0x0002F271
		public void OnSpawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSpawn", Array.Empty<object>());
			}
		}

		// Token: 0x06000B5C RID: 2908 RVA: 0x0003108B File Offset: 0x0002F28B
		public void OnDespawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDespawn", Array.Empty<object>());
			}
		}

		// Token: 0x06000B5D RID: 2909 RVA: 0x000310A5 File Offset: 0x0002F2A5
		public void SetUp(UIModalGenericViewAction modalGenericViewAction)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetUp", new object[] { modalGenericViewAction });
			}
			this.SetUp(modalGenericViewAction.Color, modalGenericViewAction.Text, modalGenericViewAction.OnClick);
		}

		// Token: 0x06000B5E RID: 2910 RVA: 0x000310E4 File Offset: 0x0002F2E4
		public void SetUp(Color color, string text, Action onClick)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetUp", new object[] { color, text, onClick });
			}
			this.background.color = color;
			this.text.text = text;
			bool flag = string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text);
			this.text.gameObject.SetActive(!flag);
			this.onClick = onClick;
		}

		// Token: 0x06000B5F RID: 2911 RVA: 0x00031160 File Offset: 0x0002F360
		private void TriggerOnClick()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "TriggerOnClick", Array.Empty<object>());
			}
			this.onClick();
		}

		// Token: 0x04000741 RID: 1857
		[SerializeField]
		private Image background;

		// Token: 0x04000742 RID: 1858
		[SerializeField]
		private TextMeshProUGUI text;

		// Token: 0x04000743 RID: 1859
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000744 RID: 1860
		private Action onClick;
	}
}
