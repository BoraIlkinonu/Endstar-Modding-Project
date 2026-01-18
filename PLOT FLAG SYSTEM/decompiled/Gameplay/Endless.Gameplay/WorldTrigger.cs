using System;
using System.Collections.Generic;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay;

public class WorldTrigger : MonoBehaviour, NetClock.ISimulateFrameEarlySubscriber, NetClock.ISimulateFrameLateSubscriber, NetClock.IRollbackSubscriber
{
	private struct FrameOverlaps : IFrameInfo
	{
		public List<WorldCollidable> previousOverlaps;

		public List<WorldCollidable> overlapsThisFrame;

		public uint NetFrame { get; set; }

		public void Clear()
		{
			overlapsThisFrame.Clear();
			previousOverlaps.Clear();
		}

		public void Initialize()
		{
			previousOverlaps = new List<WorldCollidable>();
			overlapsThisFrame = new List<WorldCollidable>();
		}

		public void ClearAndCopyPrevious(FrameOverlaps previous)
		{
			Clear();
			previousOverlaps = previous.overlapsThisFrame.GetRange(0, previous.overlapsThisFrame.Count);
		}
	}

	[SerializeField]
	private bool drawDebugs;

	public UnityEvent<WorldCollidable> OnPreloadOverlap = new UnityEvent<WorldCollidable>();

	public UnityEvent<WorldCollidable, bool> OnTriggerEnter = new UnityEvent<WorldCollidable, bool>();

	public UnityEvent<WorldCollidable, bool> OnTriggerStay = new UnityEvent<WorldCollidable, bool>();

	public UnityEvent<WorldCollidable, bool> OnTriggerExit = new UnityEvent<WorldCollidable, bool>();

	public UnityEvent<WorldCollidable> OnTriggerEnter_Unsimulated = new UnityEvent<WorldCollidable>();

	public UnityEvent<WorldCollidable> OnTriggerStay_Unsimulated = new UnityEvent<WorldCollidable>();

	public UnityEvent<WorldCollidable> OnTriggerExit_Unsimulated = new UnityEvent<WorldCollidable>();

	private RingBuffer<FrameOverlaps> frameOverlapRingBuffer = new RingBuffer<FrameOverlaps>(30);

	public Func<WorldCollidable, bool> AllowInteractionChecker { get; set; }

	public bool DrawDebugs => drawDebugs;

	protected virtual bool AllowInteraction(WorldCollidable worldCollidable)
	{
		if (AllowInteractionChecker != null)
		{
			return AllowInteractionChecker(worldCollidable);
		}
		return true;
	}

	protected virtual void TriggerEnter(WorldCollidable worldCollidable)
	{
		if (worldCollidable.IsSimulated)
		{
			OnTriggerEnter.Invoke(worldCollidable, !NetworkBehaviourSingleton<NetClock>.Instance.IsServer && NetClock.CurrentSimulationFrame != NetClock.CurrentFrame);
		}
		else
		{
			OnTriggerEnter_Unsimulated.Invoke(worldCollidable);
		}
	}

	protected virtual void TriggerStay(WorldCollidable worldCollidable)
	{
		if (worldCollidable.IsSimulated)
		{
			OnTriggerStay.Invoke(worldCollidable, !NetworkBehaviourSingleton<NetClock>.Instance.IsServer && NetClock.CurrentSimulationFrame != NetClock.CurrentFrame);
		}
		else
		{
			OnTriggerStay_Unsimulated.Invoke(worldCollidable);
		}
	}

	protected virtual void TriggerExit(WorldCollidable worldCollidable)
	{
		if (worldCollidable.IsSimulated)
		{
			OnTriggerExit.Invoke(worldCollidable, !NetworkBehaviourSingleton<NetClock>.Instance.IsServer && NetClock.CurrentSimulationFrame != NetClock.CurrentFrame);
		}
		else
		{
			OnTriggerExit_Unsimulated.Invoke(worldCollidable);
		}
	}

	private void OnEnable()
	{
		NetClock.Register(this);
	}

	private void OnDisable()
	{
		NetClock.Unregister(this);
	}

	public void PreloadOverlap(WorldCollidable worldCollidable)
	{
		OnPreloadOverlap.Invoke(worldCollidable);
		for (uint num = 0u; num < 2; num++)
		{
			OverlappedWithoutEvents(worldCollidable, NetClock.CurrentFrame - num);
		}
	}

	private void OverlappedWithoutEvents(WorldCollidable worldCollidable, uint frame)
	{
		Overlapped(worldCollidable, frame, fireEvents: false);
	}

	public void Overlapped(WorldCollidable worldCollidable, uint frame, bool fireEvents = true)
	{
		if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying || !AllowInteraction(worldCollidable))
		{
			return;
		}
		frameOverlapRingBuffer.GetAtPosition(frame).overlapsThisFrame.Add(worldCollidable);
		if (frameOverlapRingBuffer.GetAtPosition(frame).previousOverlaps.Contains(worldCollidable))
		{
			if (fireEvents)
			{
				TriggerStay(worldCollidable);
			}
			frameOverlapRingBuffer.GetAtPosition(frame).previousOverlaps.Remove(worldCollidable);
		}
		else if (fireEvents)
		{
			TriggerEnter(worldCollidable);
		}
	}

	public void DestroyOverlap(WorldCollidable worldCollidable, uint frame)
	{
		if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying || !frameOverlapRingBuffer.GetAtPosition(frame).overlapsThisFrame.Contains(worldCollidable))
		{
			return;
		}
		if (worldCollidable != null)
		{
			frameOverlapRingBuffer.GetAtPosition(frame).overlapsThisFrame.Remove(worldCollidable);
			TriggerExit(worldCollidable);
		}
		else
		{
			frameOverlapRingBuffer.GetAtPosition(frame).overlapsThisFrame.RemoveAll((WorldCollidable wc) => wc == null);
		}
	}

	public void SimulateFrameEarly(uint frame)
	{
		frameOverlapRingBuffer.GetAtPosition(frame).ClearAndCopyPrevious(frameOverlapRingBuffer.GetAtPosition(frame - 1));
		frameOverlapRingBuffer.GetAtPosition(frame).NetFrame = frame;
	}

	public void SimulateFrameLate(uint frame)
	{
		foreach (WorldCollidable previousOverlap in frameOverlapRingBuffer.GetAtPosition(frame).previousOverlaps)
		{
			TriggerExit(previousOverlap);
		}
	}

	public void Rollback(uint frame)
	{
		if (frameOverlapRingBuffer.GetAtPosition(frame).NetFrame != frame)
		{
			frameOverlapRingBuffer.GetAtPosition(frame).Clear();
			frameOverlapRingBuffer.GetAtPosition(frame).NetFrame = frame;
		}
	}
}
