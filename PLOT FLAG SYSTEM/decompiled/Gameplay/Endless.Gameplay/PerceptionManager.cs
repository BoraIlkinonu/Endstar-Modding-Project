using System.Collections;
using System.Collections.Generic;
using Endless.Shared;
using Unity.Collections;
using UnityEngine;

namespace Endless.Gameplay;

public class PerceptionManager : MonoBehaviourSingleton<PerceptionManager>
{
	private readonly List<PerceptionRequest> perceptionRequests = new List<PerceptionRequest>();

	private readonly List<PerceptionRequest> boxcastRequests = new List<PerceptionRequest>();

	private NativeArray<BoxcastCommand> boxcastCommands;

	private readonly List<TargetDatum> targetData = new List<TargetDatum>();

	private Coroutine boxcastCompletionChecker;

	private Coroutine perceptionCompletionChecker;

	[field: SerializeField]
	public LayerMask CharacterMask { get; private set; }

	[field: SerializeField]
	public LayerMask ObstacleMask { get; private set; }

	public HashSet<HittableComponent> Perceivables { get; } = new HashSet<HittableComponent>();

	private void Start()
	{
		UnifiedStateUpdater.OnProcessRequests += ProcessPerceptionRequests;
		UnifiedStateUpdater.OnProcessRequests += ProcessBoxcastRequests;
	}

	private void ProcessBoxcastRequests()
	{
		if (boxcastRequests.Count != 0)
		{
			BoxcastJobWrapper job = new BoxcastJobWrapper(boxcastRequests);
			StartCoroutine(JobCompletionChecker(job));
			boxcastRequests.Clear();
		}
	}

	public void RequestPerception(PerceptionRequest request)
	{
		if (request.IsBoxcast)
		{
			boxcastRequests.Add(request);
		}
		else
		{
			perceptionRequests.Add(request);
		}
	}

	private void ProcessPerceptionRequests()
	{
		if (perceptionRequests.Count != 0)
		{
			UpdateTargetData();
			if (targetData.Count == 0)
			{
				perceptionRequests.Clear();
				return;
			}
			PerceptionJobWrapper job = new PerceptionJobWrapper(perceptionRequests, targetData);
			StartCoroutine(JobCompletionChecker(job));
			perceptionRequests.Clear();
		}
	}

	private void UpdateTargetData()
	{
		targetData.Clear();
		foreach (HittableComponent perceivable in Perceivables)
		{
			targetData.AddRange(perceivable.GetTargetableColliderData());
		}
	}

	private static IEnumerator JobCompletionChecker(IPerceptionJob job)
	{
		int frameCount = 0;
		while (!job.IsComplete)
		{
			yield return null;
			int num = frameCount + 1;
			frameCount = num;
			if (num > 4)
			{
				job.RespondToRequests();
				yield break;
			}
		}
		job.RespondToRequests();
	}
}
