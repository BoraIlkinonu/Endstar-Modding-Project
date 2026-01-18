using System.Collections.Generic;
using System.Linq;

namespace Endless.Gameplay;

public readonly struct PerceptionDebuggingData
{
	private readonly List<PerceptionResult> perceptionResults;

	private readonly Dictionary<HittableComponent, float> currentAwarenessValues;

	private readonly List<(float score, HittableComponent hittableComponent)> finalScores;

	public PerceptionDebuggingData(List<PerceptionResult> perception, Dictionary<HittableComponent, float> awarenessValues, List<(float score, HittableComponent hittableComponent)> scores)
	{
		perceptionResults = perception;
		currentAwarenessValues = awarenessValues;
		finalScores = scores;
	}

	public Dictionary<HittableComponent, PerceptionDebuggingDatum> GetFilteredData()
	{
		Dictionary<HittableComponent, PerceptionDebuggingDatum> dictionary = new Dictionary<HittableComponent, PerceptionDebuggingDatum>();
		foreach (HittableComponent hittableComponent in currentAwarenessValues.Keys)
		{
			float awarenessThisFrame = -1f;
			float finalScore = -1f;
			float currentAwareness = currentAwarenessValues[hittableComponent];
			foreach (PerceptionResult item in perceptionResults.Where((PerceptionResult result) => result.HittableComponent == hittableComponent))
			{
				awarenessThisFrame = item.Awareness;
			}
			foreach (var item2 in finalScores.Where(((float score, HittableComponent hittableComponent) score) => score.hittableComponent == hittableComponent))
			{
				finalScore = item2.score;
			}
			dictionary.Add(hittableComponent, new PerceptionDebuggingDatum
			{
				AwarenessThisFrame = awarenessThisFrame,
				CurrentAwareness = currentAwareness,
				FinalScore = finalScore
			});
		}
		return dictionary;
	}
}
