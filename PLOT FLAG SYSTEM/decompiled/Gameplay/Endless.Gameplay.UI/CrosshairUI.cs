using System;
using System.Collections;
using Endless.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay.UI;

public class CrosshairUI : MonoBehaviour
{
	private static CrosshairUI instance;

	[SerializeField]
	private bool isSingleton;

	[SerializeField]
	private CrosshairBase defaultCrosshair;

	[SerializeField]
	private Image hitMarker;

	[SerializeField]
	private Image reloadMarker;

	[SerializeField]
	private Image notAvailableMarker;

	[SerializeField]
	private float hitMarkerVisibleTime = 0.2f;

	[SerializeField]
	private bool showMarkers;

	[NonSerialized]
	private CrosshairBase crosshair;

	[NonSerialized]
	private CrosshairSettings crosshairSettings;

	[NonSerialized]
	private bool hasAmmo;

	[NonSerialized]
	private bool isReloading;

	[NonSerialized]
	private bool isNotAvailable;

	[NonSerialized]
	private bool visible;

	public static CrosshairUI Instance => instance;

	private void Awake()
	{
		hitMarker.enabled = false;
		reloadMarker.enabled = false;
		notAvailableMarker.enabled = false;
		if (isSingleton)
		{
			instance = this;
		}
	}

	private void Start()
	{
		if (isSingleton)
		{
			MonoBehaviourSingleton<CameraController>.Instance.CrosshairObject = this;
		}
	}

	public void CreateCrosshair(CrosshairBase newCrosshair, CrosshairSettings settings, bool showImmediately = false)
	{
		DestroyCrosshair();
		crosshair = ((newCrosshair != null) ? UnityEngine.Object.Instantiate(newCrosshair, base.transform) : UnityEngine.Object.Instantiate(defaultCrosshair, base.transform));
		crosshair.Init(settings);
		crosshairSettings = settings;
		crosshair.gameObject.SetActive(value: true);
		if (showImmediately || visible)
		{
			visible = false;
			Show();
		}
		else
		{
			visible = true;
			Hide();
		}
	}

	public void DestroyCrosshair()
	{
		if (crosshair != null)
		{
			UnityEngine.Object.Destroy(crosshair.gameObject);
		}
		isReloading = false;
		isNotAvailable = false;
	}

	public void Show()
	{
		if (!visible)
		{
			visible = true;
			if (crosshair != null)
			{
				crosshair.OnShow();
			}
			UpdateStatusMarkers();
		}
	}

	public void Hide()
	{
		if (visible)
		{
			visible = false;
			if (crosshair != null)
			{
				crosshair.OnHide();
			}
			UpdateStatusMarkers();
		}
	}

	public void ApplySpread(float normalRecoilAmount, float shotStrengthMultiplier, float maxRecoilMultiplier, float recoilSettleMultiplier, float recoilSettleDelay = 0.05f)
	{
		if (crosshair != null)
		{
			crosshair.ApplySpread(normalRecoilAmount, shotStrengthMultiplier, maxRecoilMultiplier, recoilSettleMultiplier, recoilSettleDelay);
		}
	}

	public void OnMoved(float moveSpeedPercent = 1f)
	{
		if (crosshair != null)
		{
			crosshair.OnMoved(moveSpeedPercent);
		}
	}

	public void OnHit()
	{
		if (showMarkers)
		{
			StartCoroutine(ShowHitMarker());
		}
	}

	public void SetHasAmmo(bool hasAmmo)
	{
		this.hasAmmo = hasAmmo;
		UpdateStatusMarkers();
	}

	public void StartReload()
	{
		isReloading = true;
		UpdateStatusMarkers();
	}

	public void FinishReload()
	{
		isReloading = false;
		UpdateStatusMarkers();
	}

	public void SetNotAvailable(bool notAvailable)
	{
		isNotAvailable = notAvailable;
		UpdateStatusMarkers();
	}

	private void UpdateStatusMarkers()
	{
		notAvailableMarker.enabled = showMarkers && visible && (isReloading || isNotAvailable);
		reloadMarker.enabled = showMarkers && visible && !hasAmmo && !isReloading && !isNotAvailable;
	}

	private IEnumerator ShowHitMarker()
	{
		hitMarker.enabled = true;
		for (float t = hitMarkerVisibleTime; t > 0f; t -= Time.deltaTime)
		{
			yield return null;
		}
		hitMarker.enabled = false;
	}
}
