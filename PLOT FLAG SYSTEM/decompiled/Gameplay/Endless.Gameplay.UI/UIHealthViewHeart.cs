using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay.UI;

public class UIHealthViewHeart : UIGameObject, IPoolableT
{
	[Header("Heart Visuals")]
	[SerializeField]
	private Image heartImage;

	[Header("Spawn Tweens")]
	[SerializeField]
	private TweenCollection spawnTweens;

	[SerializeField]
	[Tooltip("In seconds")]
	private float spawnTweenDelay = 0.125f;

	[Header("Toggle On Tweens")]
	[SerializeField]
	private TweenCollection toggledOnTweens;

	[SerializeField]
	[Tooltip("In seconds")]
	private float toggledOnTweenDelay = 0.125f;

	[Header("Toggle Off Tweens")]
	[SerializeField]
	private TweenCollection toggledOffTweens;

	[SerializeField]
	[Tooltip("In seconds")]
	private float toggledOffTweenDelay = 0.125f;

	[SerializeField]
	private UIAnchoredPositionShaker toggledOffAnchoredPositionShaker;

	[Header("Before Despawn Tweens")]
	[SerializeField]
	private TweenCollection beforeDespawnTweens;

	[SerializeField]
	[Tooltip("In seconds")]
	private float beforeDespawnTweenDelay = 0.125f;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private Color defaultColor = Color.white;

	private bool isLeft;

	public MonoBehaviour Prefab { get; set; }

	public bool IsUi => true;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		beforeDespawnTweens.OnAllTweenCompleted.AddListener(Despawn);
		defaultColor = heartImage.color;
	}

	public void OnSpawn()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnSpawn");
		}
	}

	public void OnDespawn()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDespawn");
		}
		ForceDoneAnyTweensInProgress();
		heartImage.color = defaultColor;
	}

	public void Initialize(int healthPoint, int delayIndex)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize", healthPoint, delayIndex);
		}
		isLeft = healthPoint == 0 || healthPoint % 2 == 0;
		heartImage.rectTransform.SetPivot(isLeft ? PivotPresets.MiddleRight : PivotPresets.MiddleLeft);
		BaseTween[] tweens = spawnTweens.Tweens;
		for (int i = 0; i < tweens.Length; i++)
		{
			tweens[i].Delay = (float)delayIndex * spawnTweenDelay;
		}
		spawnTweens.Tween();
	}

	public void Toggle(bool state, int delayIndex)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Toggle", state, delayIndex);
		}
		ForceDoneAnyTweensInProgress();
		BaseTween[] tweens;
		if (state)
		{
			tweens = toggledOnTweens.Tweens;
			for (int i = 0; i < tweens.Length; i++)
			{
				tweens[i].Delay = (float)delayIndex * toggledOnTweenDelay;
			}
			toggledOnTweens.Tween();
			return;
		}
		toggledOffAnchoredPositionShaker.Delay = (float)delayIndex * toggledOffTweenDelay;
		toggledOffAnchoredPositionShaker.Shake();
		tweens = toggledOffTweens.Tweens;
		for (int i = 0; i < tweens.Length; i++)
		{
			tweens[i].Delay = (float)delayIndex * toggledOffTweenDelay;
		}
		toggledOffTweens.Tween();
	}

	public void TweenAwayAndDespawn(int delayIndex)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "TweenAwayAndDespawn", delayIndex);
		}
		ForceDoneAnyTweensInProgress();
		BaseTween[] tweens = beforeDespawnTweens.Tweens;
		for (int i = 0; i < tweens.Length; i++)
		{
			tweens[i].Delay = (float)delayIndex * beforeDespawnTweenDelay;
		}
		beforeDespawnTweens.Tween();
	}

	private void ForceDoneAnyTweensInProgress()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ForceDoneAnyTweensInProgress");
		}
		if (spawnTweens.IsAnyTweening())
		{
			spawnTweens.ForceDone();
		}
		if (toggledOnTweens.IsAnyTweening())
		{
			toggledOnTweens.ForceDone();
		}
		if (toggledOffTweens.IsAnyTweening())
		{
			toggledOffTweens.ForceDone();
			if (toggledOffAnchoredPositionShaker.IsShaking)
			{
				toggledOffAnchoredPositionShaker.Stop();
			}
		}
		if (beforeDespawnTweens.IsAnyTweening())
		{
			beforeDespawnTweens.ForceDone(triggerOnDoneEvents: false);
		}
	}

	private void Despawn()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Despawn");
		}
		MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(this);
	}
}
