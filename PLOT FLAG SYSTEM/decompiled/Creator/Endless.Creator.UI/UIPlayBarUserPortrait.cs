using System;
using Endless.Gameplay.UI;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIPlayBarUserPortrait : UIGameObject, IPoolableT
{
	[SerializeField]
	private UserPortrait userPortrait;

	[SerializeField]
	private TweenPosition tweenPosition;

	[SerializeField]
	private TweenSizeDelta tweenSizeDelta;

	[SerializeField]
	private TweenCollection initializeTweenCollection;

	[SerializeField]
	private TweenCollection onTweenPositionAndSizeDeltaCompleteTweenCollection;

	[SerializeField]
	private TweenCollection arrangementChangeHideTweenCollection;

	[SerializeField]
	private TweenCollection arrangementChangeDisplayTweenCollection;

	[SerializeField]
	private TweenCollection despawnTweenCollection;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private bool tweenPositionAndSizeDeltaComplete = true;

	public MonoBehaviour Prefab { get; set; }

	public bool IsUi => true;

	public RectTransform Target { get; private set; }

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		arrangementChangeHideTweenCollection.OnAllTweenCompleted.AddListener(arrangementChangeDisplayTweenCollection.Tween);
		arrangementChangeHideTweenCollection.OnAllTweenCompleted.AddListener(TweenPositionAndSizeDelta);
		despawnTweenCollection.OnAllTweenCompleted.AddListener(Despawn);
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
		CancelAllTweens();
		tweenPositionAndSizeDeltaComplete = true;
	}

	public void Initialize(int userId, RectTransform target, bool showHostAndParty = true)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize", userId, target.DebugSafeName(), showHostAndParty);
		}
		userPortrait.Initialize(userId, showHostAndParty);
		Target = target;
		LayoutRebuilder.ForceRebuildLayoutImmediate(target.parent as RectTransform);
		base.RectTransform.position = target.position;
		base.RectTransform.sizeDelta = target.sizeDelta;
		initializeTweenCollection.Tween();
	}

	public void SetTargetAndTweenPositionAndSizeDelta(RectTransform target, bool tweenPositionAndSizeDeltaComplete)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetTargetAndTweenPositionAndSizeDelta", target.DebugSafeName(), tweenPositionAndSizeDeltaComplete);
			DebugUtility.Log(target.DebugSafeName(), target);
		}
		Target = target;
		base.transform.SetSiblingIndex(Target.GetSiblingIndex());
		this.tweenPositionAndSizeDeltaComplete = tweenPositionAndSizeDeltaComplete;
		TweenPositionAndSizeDelta();
	}

	public void ChangeArrangement(RectTransform target)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ChangeArrangement", target.DebugSafeName());
		}
		Target = target;
		tweenPositionAndSizeDeltaComplete = false;
		float delay = 0.25f * (float)base.transform.GetSiblingIndex();
		arrangementChangeDisplayTweenCollection.SetDelay(delay);
		arrangementChangeHideTweenCollection.Tween();
	}

	public void ShrinkAwayAndDespawn()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ShrinkAwayAndDespawn");
		}
		despawnTweenCollection.Tween();
	}

	public bool IsAtTargetPositionAndSize()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "IsAtTargetPositionAndSize");
		}
		if (!Target)
		{
			return true;
		}
		bool num = Vector3.Distance(base.RectTransform.position, Target.position) < 1f;
		bool flag = Vector2.Distance(base.RectTransform.sizeDelta, Target.sizeDelta) < 1f;
		return num && flag;
	}

	public void CancelPositionAndSizeTweens()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "CancelPositionAndSizeTweens");
		}
		tweenPosition.Cancel();
		tweenSizeDelta.Cancel();
	}

	private void TweenPositionAndSizeDelta()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "TweenPositionAndSizeDelta");
		}
		tweenPosition.To = Target.position;
		tweenSizeDelta.To = Target.sizeDelta;
		tweenPosition.Tween(tweenPositionAndSizeDeltaComplete ? new Action(onTweenPositionAndSizeDeltaCompleteTweenCollection.Tween) : null);
		tweenSizeDelta.Tween();
		tweenPositionAndSizeDeltaComplete = false;
	}

	private void CancelAllTweens()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "CancelAllTweens");
		}
		tweenPosition.Cancel();
		tweenSizeDelta.Cancel();
		initializeTweenCollection.Cancel();
		onTweenPositionAndSizeDeltaCompleteTweenCollection.Cancel();
		arrangementChangeHideTweenCollection.Cancel();
		arrangementChangeDisplayTweenCollection.Cancel();
		despawnTweenCollection.Cancel();
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
