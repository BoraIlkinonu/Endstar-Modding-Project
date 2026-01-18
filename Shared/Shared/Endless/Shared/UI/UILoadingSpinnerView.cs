using System;
using Endless.Shared.Debugging;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x020001C3 RID: 451
	[DefaultExecutionOrder(-2147483648)]
	public class UILoadingSpinnerView : UIGameObject, IValidatable
	{
		// Token: 0x06000B45 RID: 2885 RVA: 0x00030BE8 File Offset: 0x0002EDE8
		private void Awake()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Awake", Array.Empty<object>());
			}
			if (this.target)
			{
				foreach (IUILoadingSpinnerViewCompatible iuiloadingSpinnerViewCompatible in this.target.GetComponents<IUILoadingSpinnerViewCompatible>())
				{
					iuiloadingSpinnerViewCompatible.OnLoadingStarted.AddListener(new UnityAction(this.displayAndHideHandler.Display));
					iuiloadingSpinnerViewCompatible.OnLoadingEnded.AddListener(new UnityAction(this.displayAndHideHandler.Hide));
				}
			}
			this.displayAndHideHandler.SetToHideEnd(true);
			this.displayAndHideHandler.OnDisplayStart.AddListener(new UnityAction(this.OnDisplayStart));
			this.displayAndHideHandler.OnDisplayComplete.AddListener(new UnityAction(this.OnDisplayComplete));
			this.displayAndHideHandler.OnHideComplete.AddListener(new UnityAction(this.OnHideComplete));
		}

		// Token: 0x06000B46 RID: 2886 RVA: 0x00030CD0 File Offset: 0x0002EED0
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			if (this.ignoreValidation)
			{
				return;
			}
			if (!this.target)
			{
				return;
			}
			IUILoadingSpinnerViewCompatible iuiloadingSpinnerViewCompatible;
			if (!this.target.TryGetComponent<IUILoadingSpinnerViewCompatible>(out iuiloadingSpinnerViewCompatible))
			{
				DebugUtility.LogError("target (" + this.target.gameObject.name + ") must have a Component on it that implements the IUILoadingSpinnerViewCompatible interface!", this);
			}
		}

		// Token: 0x06000B47 RID: 2887 RVA: 0x00030D40 File Offset: 0x0002EF40
		private void OnDisplayStart()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisplayStart", Array.Empty<object>());
			}
			base.RectTransform.SetAnchor(AnchorPresets.StretchAll, 0f, 0f, 0f, 0f);
		}

		// Token: 0x06000B48 RID: 2888 RVA: 0x00030D7B File Offset: 0x0002EF7B
		private void OnDisplayComplete()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisplayComplete", Array.Empty<object>());
			}
			this.imageSpriteSequencePlayer.Play();
		}

		// Token: 0x06000B49 RID: 2889 RVA: 0x00030DA0 File Offset: 0x0002EFA0
		private void OnHideComplete()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnHideComplete", Array.Empty<object>());
			}
			this.imageSpriteSequencePlayer.Stop();
		}

		// Token: 0x04000732 RID: 1842
		[SerializeField]
		private GameObject target;

		// Token: 0x04000733 RID: 1843
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x04000734 RID: 1844
		[SerializeField]
		private UIImageSpriteSequencePlayer imageSpriteSequencePlayer;

		// Token: 0x04000735 RID: 1845
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000736 RID: 1846
		[Tooltip("Use this only if you know what you are doing :P")]
		[SerializeField]
		private bool ignoreValidation;
	}
}
