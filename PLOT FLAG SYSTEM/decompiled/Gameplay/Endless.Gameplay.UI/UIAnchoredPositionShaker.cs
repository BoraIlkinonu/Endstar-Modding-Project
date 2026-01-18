using System.Collections;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIAnchoredPositionShaker : MonoBehaviour
{
	public RectTransform Target;

	public Vector2 ShakeRangeX = new Vector2(-5f, 5f);

	public Vector2 ShakeRangeY = new Vector2(-5f, 5f);

	[Tooltip("In seconds")]
	public float Delay;

	[Tooltip("In seconds")]
	public float ShakeIterations = 0.1f;

	[Tooltip("In seconds")]
	public float ShakeDuration = 0.5f;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private Vector2 anchoredPositionCache = Vector2.zero;

	private Coroutine shakeCoroutineCache;

	private Coroutine shakeEndCoroutineCache;

	public bool IsShaking
	{
		get
		{
			if (shakeCoroutineCache == null)
			{
				return shakeEndCoroutineCache != null;
			}
			return true;
		}
	}

	private void OnDestroy()
	{
		if (IsShaking)
		{
			MonoBehaviourSingleton<UICoroutineManager>.Instance.StopThisCoroutine(shakeCoroutineCache);
			MonoBehaviourSingleton<UICoroutineManager>.Instance.StopThisCoroutine(shakeEndCoroutineCache);
			shakeCoroutineCache = null;
			shakeEndCoroutineCache = null;
		}
	}

	public void Shake()
	{
		if (verboseLogging)
		{
			DebugUtility.Log("Shake", this);
		}
		if (IsShaking)
		{
			Stop();
		}
		anchoredPositionCache = Target.anchoredPosition;
		shakeCoroutineCache = MonoBehaviourSingleton<UICoroutineManager>.Instance.StartThisCoroutine(ShakeCoroutine());
		shakeEndCoroutineCache = MonoBehaviourSingleton<UICoroutineManager>.Instance.StartThisCoroutine(ShakeEndCoroutine());
	}

	public void Stop()
	{
		if (verboseLogging)
		{
			DebugUtility.Log("Stop", this);
		}
		MonoBehaviourSingleton<UICoroutineManager>.Instance.StopThisCoroutine(shakeCoroutineCache);
		MonoBehaviourSingleton<UICoroutineManager>.Instance.StopThisCoroutine(shakeEndCoroutineCache);
		shakeCoroutineCache = null;
		shakeEndCoroutineCache = null;
		Target.anchoredPosition = anchoredPositionCache;
	}

	private IEnumerator ShakeCoroutine()
	{
		yield return new WaitForSeconds(Delay);
		while (true)
		{
			Target.anchoredPosition = new Vector2(anchoredPositionCache.x + Random.Range(ShakeRangeX.x, ShakeRangeX.y), anchoredPositionCache.y + Random.Range(ShakeRangeY.x, ShakeRangeY.y));
			yield return new WaitForSeconds(ShakeIterations);
		}
	}

	private IEnumerator ShakeEndCoroutine()
	{
		yield return new WaitForSeconds(Delay);
		yield return new WaitForSeconds(ShakeDuration);
		Stop();
	}
}
