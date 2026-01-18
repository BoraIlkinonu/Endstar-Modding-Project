using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay;

public class TriggerDetector : NpcComponent, IStartSubscriber
{
	[SerializeField]
	private LayerMask worldEffectMask;

	[SerializeField]
	private float detectionRadius;

	private int hitCount;

	private readonly Collider[] hits = new Collider[5];

	private void HandleOnCheckWorldTriggers(uint frame)
	{
		HandleOnCheckWorldTriggers(frame, onDestroy: false);
	}

	private void HandleOnCheckWorldTriggers(uint frame, bool onDestroy)
	{
		Vector3 vector = base.transform.position + Vector3.down * 0.5f;
		Vector3 point = vector + Vector3.up;
		hitCount = Physics.OverlapCapsuleNonAlloc(vector, point, detectionRadius, hits, worldEffectMask, QueryTriggerInteraction.Collide);
		for (int i = 0; i < hitCount; i++)
		{
			WorldTrigger worldTrigger = hits[i].GetComponent<WorldTriggerCollider>()?.WorldTrigger;
			if ((bool)worldTrigger)
			{
				if (onDestroy)
				{
					worldTrigger.DestroyOverlap(base.Components.WorldCollidable, frame);
				}
				else
				{
					worldTrigger.Overlapped(base.Components.WorldCollidable, frame);
				}
			}
		}
	}

	protected override void OnDestroy()
	{
		if (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
		{
			HandleOnCheckWorldTriggers(NetClock.CurrentFrame, onDestroy: true);
		}
		base.OnDestroy();
	}

	public void EndlessStart()
	{
		base.Components.IndividualStateUpdater.OnCheckWorldTriggers += HandleOnCheckWorldTriggers;
	}
}
