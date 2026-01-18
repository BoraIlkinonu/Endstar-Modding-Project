using System;
using System.Collections.Generic;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x02000379 RID: 889
	public class WorldTrigger : MonoBehaviour, NetClock.ISimulateFrameEarlySubscriber, NetClock.ISimulateFrameLateSubscriber, NetClock.IRollbackSubscriber
	{
		// Token: 0x170004C9 RID: 1225
		// (get) Token: 0x060016C5 RID: 5829 RVA: 0x0006AB15 File Offset: 0x00068D15
		// (set) Token: 0x060016C6 RID: 5830 RVA: 0x0006AB1D File Offset: 0x00068D1D
		public Func<WorldCollidable, bool> AllowInteractionChecker { get; set; }

		// Token: 0x170004CA RID: 1226
		// (get) Token: 0x060016C7 RID: 5831 RVA: 0x0006AB26 File Offset: 0x00068D26
		public bool DrawDebugs
		{
			get
			{
				return this.drawDebugs;
			}
		}

		// Token: 0x060016C8 RID: 5832 RVA: 0x0006AB2E File Offset: 0x00068D2E
		protected virtual bool AllowInteraction(WorldCollidable worldCollidable)
		{
			return this.AllowInteractionChecker == null || this.AllowInteractionChecker(worldCollidable);
		}

		// Token: 0x060016C9 RID: 5833 RVA: 0x0006AB48 File Offset: 0x00068D48
		protected virtual void TriggerEnter(WorldCollidable worldCollidable)
		{
			if (worldCollidable.IsSimulated)
			{
				this.OnTriggerEnter.Invoke(worldCollidable, !NetworkBehaviourSingleton<NetClock>.Instance.IsServer && NetClock.CurrentSimulationFrame != NetClock.CurrentFrame);
				return;
			}
			this.OnTriggerEnter_Unsimulated.Invoke(worldCollidable);
		}

		// Token: 0x060016CA RID: 5834 RVA: 0x0006AB94 File Offset: 0x00068D94
		protected virtual void TriggerStay(WorldCollidable worldCollidable)
		{
			if (worldCollidable.IsSimulated)
			{
				this.OnTriggerStay.Invoke(worldCollidable, !NetworkBehaviourSingleton<NetClock>.Instance.IsServer && NetClock.CurrentSimulationFrame != NetClock.CurrentFrame);
				return;
			}
			this.OnTriggerStay_Unsimulated.Invoke(worldCollidable);
		}

		// Token: 0x060016CB RID: 5835 RVA: 0x0006ABE0 File Offset: 0x00068DE0
		protected virtual void TriggerExit(WorldCollidable worldCollidable)
		{
			if (worldCollidable.IsSimulated)
			{
				this.OnTriggerExit.Invoke(worldCollidable, !NetworkBehaviourSingleton<NetClock>.Instance.IsServer && NetClock.CurrentSimulationFrame != NetClock.CurrentFrame);
				return;
			}
			this.OnTriggerExit_Unsimulated.Invoke(worldCollidable);
		}

		// Token: 0x060016CC RID: 5836 RVA: 0x0001CAAE File Offset: 0x0001ACAE
		private void OnEnable()
		{
			NetClock.Register(this);
		}

		// Token: 0x060016CD RID: 5837 RVA: 0x0001CAB6 File Offset: 0x0001ACB6
		private void OnDisable()
		{
			NetClock.Unregister(this);
		}

		// Token: 0x060016CE RID: 5838 RVA: 0x0006AC2C File Offset: 0x00068E2C
		public void PreloadOverlap(WorldCollidable worldCollidable)
		{
			this.OnPreloadOverlap.Invoke(worldCollidable);
			for (uint num = 0U; num < 2U; num += 1U)
			{
				this.OverlappedWithoutEvents(worldCollidable, NetClock.CurrentFrame - num);
			}
		}

		// Token: 0x060016CF RID: 5839 RVA: 0x0006AC5F File Offset: 0x00068E5F
		private void OverlappedWithoutEvents(WorldCollidable worldCollidable, uint frame)
		{
			this.Overlapped(worldCollidable, frame, false);
		}

		// Token: 0x060016D0 RID: 5840 RVA: 0x0006AC6C File Offset: 0x00068E6C
		public void Overlapped(WorldCollidable worldCollidable, uint frame, bool fireEvents = true)
		{
			if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
			{
				return;
			}
			if (this.AllowInteraction(worldCollidable))
			{
				this.frameOverlapRingBuffer.GetAtPosition(frame).overlapsThisFrame.Add(worldCollidable);
				if (this.frameOverlapRingBuffer.GetAtPosition(frame).previousOverlaps.Contains(worldCollidable))
				{
					if (fireEvents)
					{
						this.TriggerStay(worldCollidable);
					}
					this.frameOverlapRingBuffer.GetAtPosition(frame).previousOverlaps.Remove(worldCollidable);
					return;
				}
				if (fireEvents)
				{
					this.TriggerEnter(worldCollidable);
				}
			}
		}

		// Token: 0x060016D1 RID: 5841 RVA: 0x0006ACEC File Offset: 0x00068EEC
		public void DestroyOverlap(WorldCollidable worldCollidable, uint frame)
		{
			if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
			{
				return;
			}
			if (this.frameOverlapRingBuffer.GetAtPosition(frame).overlapsThisFrame.Contains(worldCollidable))
			{
				if (worldCollidable != null)
				{
					this.frameOverlapRingBuffer.GetAtPosition(frame).overlapsThisFrame.Remove(worldCollidable);
					this.TriggerExit(worldCollidable);
					return;
				}
				this.frameOverlapRingBuffer.GetAtPosition(frame).overlapsThisFrame.RemoveAll((WorldCollidable wc) => wc == null);
			}
		}

		// Token: 0x060016D2 RID: 5842 RVA: 0x0006AD7E File Offset: 0x00068F7E
		public unsafe void SimulateFrameEarly(uint frame)
		{
			this.frameOverlapRingBuffer.GetAtPosition(frame).ClearAndCopyPrevious(*this.frameOverlapRingBuffer.GetAtPosition(frame - 1U));
			this.frameOverlapRingBuffer.GetAtPosition(frame).NetFrame = frame;
		}

		// Token: 0x060016D3 RID: 5843 RVA: 0x0006ADB8 File Offset: 0x00068FB8
		public void SimulateFrameLate(uint frame)
		{
			foreach (WorldCollidable worldCollidable in this.frameOverlapRingBuffer.GetAtPosition(frame).previousOverlaps)
			{
				this.TriggerExit(worldCollidable);
			}
		}

		// Token: 0x060016D4 RID: 5844 RVA: 0x0006AE18 File Offset: 0x00069018
		public void Rollback(uint frame)
		{
			if (this.frameOverlapRingBuffer.GetAtPosition(frame).NetFrame != frame)
			{
				this.frameOverlapRingBuffer.GetAtPosition(frame).Clear();
				this.frameOverlapRingBuffer.GetAtPosition(frame).NetFrame = frame;
			}
		}

		// Token: 0x0400124A RID: 4682
		[SerializeField]
		private bool drawDebugs;

		// Token: 0x0400124B RID: 4683
		public UnityEvent<WorldCollidable> OnPreloadOverlap = new UnityEvent<WorldCollidable>();

		// Token: 0x0400124C RID: 4684
		public UnityEvent<WorldCollidable, bool> OnTriggerEnter = new UnityEvent<WorldCollidable, bool>();

		// Token: 0x0400124D RID: 4685
		public UnityEvent<WorldCollidable, bool> OnTriggerStay = new UnityEvent<WorldCollidable, bool>();

		// Token: 0x0400124E RID: 4686
		public UnityEvent<WorldCollidable, bool> OnTriggerExit = new UnityEvent<WorldCollidable, bool>();

		// Token: 0x0400124F RID: 4687
		public UnityEvent<WorldCollidable> OnTriggerEnter_Unsimulated = new UnityEvent<WorldCollidable>();

		// Token: 0x04001250 RID: 4688
		public UnityEvent<WorldCollidable> OnTriggerStay_Unsimulated = new UnityEvent<WorldCollidable>();

		// Token: 0x04001251 RID: 4689
		public UnityEvent<WorldCollidable> OnTriggerExit_Unsimulated = new UnityEvent<WorldCollidable>();

		// Token: 0x04001253 RID: 4691
		private RingBuffer<WorldTrigger.FrameOverlaps> frameOverlapRingBuffer = new RingBuffer<WorldTrigger.FrameOverlaps>(30);

		// Token: 0x0200037A RID: 890
		private struct FrameOverlaps : IFrameInfo
		{
			// Token: 0x170004CB RID: 1227
			// (get) Token: 0x060016D6 RID: 5846 RVA: 0x0006AEC1 File Offset: 0x000690C1
			// (set) Token: 0x060016D7 RID: 5847 RVA: 0x0006AEC9 File Offset: 0x000690C9
			public uint NetFrame { readonly get; set; }

			// Token: 0x060016D8 RID: 5848 RVA: 0x0006AED2 File Offset: 0x000690D2
			public void Clear()
			{
				this.overlapsThisFrame.Clear();
				this.previousOverlaps.Clear();
			}

			// Token: 0x060016D9 RID: 5849 RVA: 0x0006AEEA File Offset: 0x000690EA
			public void Initialize()
			{
				this.previousOverlaps = new List<WorldCollidable>();
				this.overlapsThisFrame = new List<WorldCollidable>();
			}

			// Token: 0x060016DA RID: 5850 RVA: 0x0006AF02 File Offset: 0x00069102
			public void ClearAndCopyPrevious(WorldTrigger.FrameOverlaps previous)
			{
				this.Clear();
				this.previousOverlaps = previous.overlapsThisFrame.GetRange(0, previous.overlapsThisFrame.Count);
			}

			// Token: 0x04001254 RID: 4692
			public List<WorldCollidable> previousOverlaps;

			// Token: 0x04001255 RID: 4693
			public List<WorldCollidable> overlapsThisFrame;
		}
	}
}
