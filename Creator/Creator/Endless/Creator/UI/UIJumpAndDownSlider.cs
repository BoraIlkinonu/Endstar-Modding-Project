using System;
using Endless.Gameplay;
using Endless.Gameplay.UI;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Endless.Creator.UI
{
	// Token: 0x020000E1 RID: 225
	public class UIJumpAndDownSlider : UISlider, IValidatable
	{
		// Token: 0x060003C1 RID: 961 RVA: 0x00018210 File Offset: 0x00016410
		protected override void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnEnable", this);
			}
			base.OnEnable();
			if (!Application.isPlaying)
			{
				return;
			}
			PlayerReferenceManager localPlayerObject = MonoBehaviourSingleton<PlayerManager>.Instance.GetLocalPlayerObject();
			this.playerNetworkController = localPlayerObject.PlayerNetworkController;
		}

		// Token: 0x060003C2 RID: 962 RVA: 0x00018255 File Offset: 0x00016455
		protected override void Start()
		{
			base.Start();
			if (!Application.isPlaying)
			{
				return;
			}
			base.onValueChanged.AddListener(new UnityAction<float>(this.OnSlide));
			this.ghostModeSlideRangeDisplayAndHideHandler.SetToHideEnd(true);
		}

		// Token: 0x060003C3 RID: 963 RVA: 0x00018288 File Offset: 0x00016488
		protected override void OnDisable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnDisable", this);
			}
			base.OnDisable();
			this.playerNetworkController = null;
		}

		// Token: 0x060003C4 RID: 964 RVA: 0x000182AC File Offset: 0x000164AC
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			float num = 0f;
			float num2 = 1f;
			if (!Mathf.Approximately(base.minValue, num))
			{
				DebugUtility.LogError(string.Format("{0} expects the {1} to be '{2}'!", "UIJumpAndDownSlider", "minValue", num), this);
			}
			if (!Mathf.Approximately(base.maxValue, num2))
			{
				DebugUtility.LogError(string.Format("{0} expects the {1} to be '{2}'!", "UIJumpAndDownSlider", "maxValue", num2), this);
			}
		}

		// Token: 0x060003C5 RID: 965 RVA: 0x00018339 File Offset: 0x00016539
		public override void OnPointerDown(PointerEventData eventData)
		{
			base.OnPointerDown(eventData);
			if (this.playerNetworkController.Ghost)
			{
				this.ghostModeSlideRangeDisplayAndHideHandler.Display();
				return;
			}
			this.jumpOnScreenButtonHandler.SetButtonState(true);
		}

		// Token: 0x060003C6 RID: 966 RVA: 0x00018367 File Offset: 0x00016567
		public override void OnPointerUp(PointerEventData eventData)
		{
			base.OnPointerUp(eventData);
			this.jumpOnScreenButtonHandler.SetButtonState(false);
			this.downOnScreenButtonHandler.SetButtonState(false);
			base.SetValue(0.5f, true);
			this.ghostModeSlideRangeDisplayAndHideHandler.Hide();
		}

		// Token: 0x060003C7 RID: 967 RVA: 0x000183A0 File Offset: 0x000165A0
		private void OnSlide(float sliderValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnSlide", "sliderValue", sliderValue), this);
			}
			if (this.playerNetworkController.Ghost)
			{
				this.jumpOnScreenButtonHandler.SetButtonState((double)sliderValue >= 0.5);
				this.downOnScreenButtonHandler.SetButtonState(sliderValue < 0.5f);
				return;
			}
			base.SetValue(0.5f, true);
		}

		// Token: 0x040003E1 RID: 993
		[Header("UIJumpAndDownSlider")]
		[SerializeField]
		private UIOnScreenButtonHandler jumpOnScreenButtonHandler;

		// Token: 0x040003E2 RID: 994
		[SerializeField]
		private UIOnScreenButtonHandler downOnScreenButtonHandler;

		// Token: 0x040003E3 RID: 995
		[SerializeField]
		private UIDisplayAndHideHandler ghostModeSlideRangeDisplayAndHideHandler;

		// Token: 0x040003E4 RID: 996
		private PlayerNetworkController playerNetworkController;
	}
}
