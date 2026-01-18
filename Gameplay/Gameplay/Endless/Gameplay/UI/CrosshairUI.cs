using System;
using System.Collections;
using Endless.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000392 RID: 914
	public class CrosshairUI : MonoBehaviour
	{
		// Token: 0x170004D0 RID: 1232
		// (get) Token: 0x0600173C RID: 5948 RVA: 0x0006C974 File Offset: 0x0006AB74
		public static CrosshairUI Instance
		{
			get
			{
				return CrosshairUI.instance;
			}
		}

		// Token: 0x0600173D RID: 5949 RVA: 0x0006C97B File Offset: 0x0006AB7B
		private void Awake()
		{
			this.hitMarker.enabled = false;
			this.reloadMarker.enabled = false;
			this.notAvailableMarker.enabled = false;
			if (this.isSingleton)
			{
				CrosshairUI.instance = this;
			}
		}

		// Token: 0x0600173E RID: 5950 RVA: 0x0006C9AF File Offset: 0x0006ABAF
		private void Start()
		{
			if (this.isSingleton)
			{
				MonoBehaviourSingleton<CameraController>.Instance.CrosshairObject = this;
			}
		}

		// Token: 0x0600173F RID: 5951 RVA: 0x0006C9C4 File Offset: 0x0006ABC4
		public void CreateCrosshair(CrosshairBase newCrosshair, CrosshairSettings settings, bool showImmediately = false)
		{
			this.DestroyCrosshair();
			this.crosshair = ((newCrosshair != null) ? global::UnityEngine.Object.Instantiate<CrosshairBase>(newCrosshair, base.transform) : global::UnityEngine.Object.Instantiate<CrosshairBase>(this.defaultCrosshair, base.transform));
			this.crosshair.Init(settings);
			this.crosshairSettings = settings;
			this.crosshair.gameObject.SetActive(true);
			if (showImmediately || this.visible)
			{
				this.visible = false;
				this.Show();
				return;
			}
			this.visible = true;
			this.Hide();
		}

		// Token: 0x06001740 RID: 5952 RVA: 0x0006CA4F File Offset: 0x0006AC4F
		public void DestroyCrosshair()
		{
			if (this.crosshair != null)
			{
				global::UnityEngine.Object.Destroy(this.crosshair.gameObject);
			}
			this.isReloading = false;
			this.isNotAvailable = false;
		}

		// Token: 0x06001741 RID: 5953 RVA: 0x0006CA7D File Offset: 0x0006AC7D
		public void Show()
		{
			if (!this.visible)
			{
				this.visible = true;
				if (this.crosshair != null)
				{
					this.crosshair.OnShow();
				}
				this.UpdateStatusMarkers();
			}
		}

		// Token: 0x06001742 RID: 5954 RVA: 0x0006CAAD File Offset: 0x0006ACAD
		public void Hide()
		{
			if (this.visible)
			{
				this.visible = false;
				if (this.crosshair != null)
				{
					this.crosshair.OnHide();
				}
				this.UpdateStatusMarkers();
			}
		}

		// Token: 0x06001743 RID: 5955 RVA: 0x0006CADD File Offset: 0x0006ACDD
		public void ApplySpread(float normalRecoilAmount, float shotStrengthMultiplier, float maxRecoilMultiplier, float recoilSettleMultiplier, float recoilSettleDelay = 0.05f)
		{
			if (this.crosshair != null)
			{
				this.crosshair.ApplySpread(normalRecoilAmount, shotStrengthMultiplier, maxRecoilMultiplier, recoilSettleMultiplier, recoilSettleDelay);
			}
		}

		// Token: 0x06001744 RID: 5956 RVA: 0x0006CAFF File Offset: 0x0006ACFF
		public void OnMoved(float moveSpeedPercent = 1f)
		{
			if (this.crosshair != null)
			{
				this.crosshair.OnMoved(moveSpeedPercent);
			}
		}

		// Token: 0x06001745 RID: 5957 RVA: 0x0006CB1B File Offset: 0x0006AD1B
		public void OnHit()
		{
			if (this.showMarkers)
			{
				base.StartCoroutine(this.ShowHitMarker());
			}
		}

		// Token: 0x06001746 RID: 5958 RVA: 0x0006CB32 File Offset: 0x0006AD32
		public void SetHasAmmo(bool hasAmmo)
		{
			this.hasAmmo = hasAmmo;
			this.UpdateStatusMarkers();
		}

		// Token: 0x06001747 RID: 5959 RVA: 0x0006CB41 File Offset: 0x0006AD41
		public void StartReload()
		{
			this.isReloading = true;
			this.UpdateStatusMarkers();
		}

		// Token: 0x06001748 RID: 5960 RVA: 0x0006CB50 File Offset: 0x0006AD50
		public void FinishReload()
		{
			this.isReloading = false;
			this.UpdateStatusMarkers();
		}

		// Token: 0x06001749 RID: 5961 RVA: 0x0006CB5F File Offset: 0x0006AD5F
		public void SetNotAvailable(bool notAvailable)
		{
			this.isNotAvailable = notAvailable;
			this.UpdateStatusMarkers();
		}

		// Token: 0x0600174A RID: 5962 RVA: 0x0006CB70 File Offset: 0x0006AD70
		private void UpdateStatusMarkers()
		{
			this.notAvailableMarker.enabled = this.showMarkers && this.visible && (this.isReloading || this.isNotAvailable);
			this.reloadMarker.enabled = this.showMarkers && this.visible && !this.hasAmmo && !this.isReloading && !this.isNotAvailable;
		}

		// Token: 0x0600174B RID: 5963 RVA: 0x0006CBE6 File Offset: 0x0006ADE6
		private IEnumerator ShowHitMarker()
		{
			this.hitMarker.enabled = true;
			for (float t = this.hitMarkerVisibleTime; t > 0f; t -= Time.deltaTime)
			{
				yield return null;
			}
			this.hitMarker.enabled = false;
			yield break;
		}

		// Token: 0x040012AC RID: 4780
		private static CrosshairUI instance;

		// Token: 0x040012AD RID: 4781
		[SerializeField]
		private bool isSingleton;

		// Token: 0x040012AE RID: 4782
		[SerializeField]
		private CrosshairBase defaultCrosshair;

		// Token: 0x040012AF RID: 4783
		[SerializeField]
		private Image hitMarker;

		// Token: 0x040012B0 RID: 4784
		[SerializeField]
		private Image reloadMarker;

		// Token: 0x040012B1 RID: 4785
		[SerializeField]
		private Image notAvailableMarker;

		// Token: 0x040012B2 RID: 4786
		[SerializeField]
		private float hitMarkerVisibleTime = 0.2f;

		// Token: 0x040012B3 RID: 4787
		[SerializeField]
		private bool showMarkers;

		// Token: 0x040012B4 RID: 4788
		[NonSerialized]
		private CrosshairBase crosshair;

		// Token: 0x040012B5 RID: 4789
		[NonSerialized]
		private CrosshairSettings crosshairSettings;

		// Token: 0x040012B6 RID: 4790
		[NonSerialized]
		private bool hasAmmo;

		// Token: 0x040012B7 RID: 4791
		[NonSerialized]
		private bool isReloading;

		// Token: 0x040012B8 RID: 4792
		[NonSerialized]
		private bool isNotAvailable;

		// Token: 0x040012B9 RID: 4793
		[NonSerialized]
		private bool visible;
	}
}
