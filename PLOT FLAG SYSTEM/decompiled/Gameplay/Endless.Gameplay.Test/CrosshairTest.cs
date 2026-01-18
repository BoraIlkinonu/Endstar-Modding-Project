using System.Collections;
using Endless.Gameplay.UI;
using UnityEngine;

namespace Endless.Gameplay.Test;

public class CrosshairTest : MonoBehaviour
{
	public CrosshairUI crosshairUI;

	public CrosshairBase crosshairPrefab;

	public CrosshairSettings crosshairOverrideSettings;

	public int iterations = 10;

	public int minShots = 1;

	public int maxShots = 3;

	public float initialDelay = 1.5f;

	public float minDelay = 0.75f;

	public float maxDelay = 2.25f;

	private YieldInstruction shotWait;

	private void Start()
	{
		crosshairUI.CreateCrosshair(crosshairPrefab, crosshairOverrideSettings, showImmediately: true);
		crosshairUI.SetHasAmmo(hasAmmo: true);
		shotWait = new WaitForSeconds(0.05f);
		StartCoroutine(DoFakeSpread());
	}

	private IEnumerator DoFakeSpread()
	{
		yield return new WaitForSeconds(initialDelay);
		for (int i = 0; i < iterations; i++)
		{
			yield return FireShots();
			yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
		}
	}

	private IEnumerator FireShots()
	{
		int shotCount = Random.Range(minShots, maxShots + 1);
		for (int i = 0; i < shotCount; i++)
		{
			ApplySpread();
			yield return shotWait;
		}
	}

	private void ApplySpread()
	{
		crosshairUI.ApplySpread(1f, crosshairOverrideSettings.weaponStrength, crosshairOverrideSettings.maxSpread, crosshairOverrideSettings.resetSpeed);
	}
}
