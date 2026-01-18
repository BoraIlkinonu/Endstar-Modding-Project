using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay;

public class InterpolationRingBuffer<T> : RingBuffer<T> where T : IFrameInfo
{
	private const uint MAX_SEARCH_COUNT = 30u;

	public UnityEvent<T, T> OnStatesShifted = new UnityEvent<T, T>();

	public T ActiveInterpolatedState;

	private double _activeInterpolationTime;

	private int pastInterpolationIndex;

	private int nextInterpolationIndex;

	private int searchCount;

	public double ActiveInterpolationTime
	{
		get
		{
			return _activeInterpolationTime;
		}
		set
		{
			for (uint num = 0u; num < searchCount; num++)
			{
				if (!(value > NextStateStartTime))
				{
					break;
				}
				ShiftState();
			}
			_activeInterpolationTime = value;
			ActiveStateLerpTime = (float)((_activeInterpolationTime - PastStateStartTime) / (double)(float)(NextStateStartTime - PastStateStartTime));
		}
	}

	public float ActiveStateLerpTime { get; protected set; }

	public T PastInterpolationState => base.RingBufferArray[pastInterpolationIndex];

	public double PastStateStartTime => NetClock.GetFrameTime(base.RingBufferArray[pastInterpolationIndex].NetFrame);

	public T NextInterpolationState => base.RingBufferArray[nextInterpolationIndex];

	public double NextStateStartTime => NetClock.GetFrameTime(base.RingBufferArray[nextInterpolationIndex].NetFrame);

	public new void UpdateValue(ref T value)
	{
		base.UpdateValue(ref value);
		if (value.NetFrame > PastInterpolationState.NetFrame && value.NetFrame <= NextInterpolationState.NetFrame)
		{
			double frameTime = NetClock.GetFrameTime(value.NetFrame);
			if (ActiveInterpolationTime <= frameTime)
			{
				SetNext(GetUseIndexOfFrame(value.NetFrame));
				OnStatesShifted.Invoke(PastInterpolationState, NextInterpolationState);
			}
			else
			{
				OnStatesShifted.Invoke(PastInterpolationState, value);
				SetPrevious(GetUseIndexOfFrame(value.NetFrame));
			}
		}
	}

	private void ShiftState()
	{
		SetPrevious(nextInterpolationIndex);
		SearchForAndSetNext(NextInterpolationState.NetFrame + 1);
		OnStatesShifted.Invoke(PastInterpolationState, NextInterpolationState);
	}

	private void SetPrevious(int index)
	{
		pastInterpolationIndex = index;
	}

	private void SetNext(int index)
	{
		nextInterpolationIndex = index;
	}

	private void SearchForAndSetNext(uint checkFrame)
	{
		if (lastUpdated <= checkFrame)
		{
			SetNext(pastInterpolationIndex);
		}
		for (uint num = checkFrame; num < checkFrame + searchCount; num++)
		{
			int useIndexOfFrame = GetUseIndexOfFrame(num);
			if (base.RingBufferArray[useIndexOfFrame].NetFrame >= checkFrame)
			{
				SetNext(useIndexOfFrame);
				return;
			}
		}
		SetNext(pastInterpolationIndex);
	}

	public InterpolationRingBuffer(int ringCount)
		: base(ringCount)
	{
		searchCount = Mathf.Min(30, base.RingBufferArray.Length);
	}

	public void InitPastAndNext(uint frame)
	{
		pastInterpolationIndex = GetUseIndexOfFrame(frame);
		nextInterpolationIndex = pastInterpolationIndex;
	}
}
