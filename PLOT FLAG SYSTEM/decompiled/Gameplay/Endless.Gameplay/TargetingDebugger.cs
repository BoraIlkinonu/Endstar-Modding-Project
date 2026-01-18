using System;
using System.Collections.Generic;
using Endless.Gameplay.UI;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay;

public class TargetingDebugger : MonoBehaviourSingleton<TargetingDebugger>
{
	[SerializeField]
	private Vector3 targeterOffset;

	[SerializeField]
	private UISimpleText simpleTextSource;

	private UISimpleText targeterText;

	private readonly Dictionary<HittableComponent, UISimpleText> targetTexts = new Dictionary<HittableComponent, UISimpleText>();

	private TargeterComponent targeterComponent;

	public TargeterComponent TargeterComponent
	{
		get
		{
			return targeterComponent;
		}
		private set
		{
			if (targeterComponent == value)
			{
				return;
			}
			if ((bool)targeterComponent)
			{
				targeterText.Close();
				targeterComponent.OnScoresUpdated -= TargeterComponentOnScoresUpdated;
			}
			foreach (UISimpleText value2 in targetTexts.Values)
			{
				value2.Close();
			}
			targetTexts.Clear();
			targeterComponent = value;
			if (targeterComponent != null)
			{
				string text = (targeterComponent.Target ? TargeterComponent.Target.WorldObject.gameObject.name : "");
				targeterText = UISimpleText.CreateInstance(simpleTextSource, targeterComponent.transform, MonoBehaviourSingleton<UIGameplayReferenceManager>.Instance.AnchorContainer, text, targeterOffset);
				targeterComponent.OnScoresUpdated += TargeterComponentOnScoresUpdated;
			}
		}
	}

	private void TargeterComponentOnScoresUpdated(PerceptionDebuggingData data)
	{
		string text = (targeterComponent.Target ? TargeterComponent.Target.WorldObject.gameObject.name : "");
		targeterText.UpdateText(text);
		Dictionary<HittableComponent, PerceptionDebuggingDatum> filteredData = data.GetFilteredData();
		foreach (HittableComponent key in filteredData.Keys)
		{
			PerceptionDebuggingDatum perceptionDebuggingDatum = filteredData[key];
			string text2 = $"Awareness this frame: {perceptionDebuggingDatum.AwarenessThisFrame}" + Environment.NewLine;
			text2 = text2 + $"Current Awareness: {perceptionDebuggingDatum.CurrentAwareness}" + Environment.NewLine;
			text2 += $"Final Score: {perceptionDebuggingDatum.FinalScore}";
			if (targetTexts.TryGetValue(key, out var value))
			{
				value.UpdateText(text2);
			}
			else
			{
				targetTexts.Add(key, UISimpleText.CreateInstance(simpleTextSource, key.WorldObject.transform, MonoBehaviourSingleton<UIGameplayReferenceManager>.Instance.AnchorContainer, text2, targeterOffset));
			}
		}
	}

	public void DebugTargeting(TargeterComponent targeter)
	{
		TargeterComponent = targeter;
	}
}
