using System;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x02000137 RID: 311
	public class IndividualStateUpdater : NpcComponent, IStartSubscriber
	{
		// Token: 0x14000003 RID: 3
		// (add) Token: 0x0600072F RID: 1839 RVA: 0x00022068 File Offset: 0x00020268
		// (remove) Token: 0x06000730 RID: 1840 RVA: 0x000220A0 File Offset: 0x000202A0
		public event Action OnCleanupTriggers;

		// Token: 0x14000004 RID: 4
		// (add) Token: 0x06000731 RID: 1841 RVA: 0x000220D8 File Offset: 0x000202D8
		// (remove) Token: 0x06000732 RID: 1842 RVA: 0x00022110 File Offset: 0x00020310
		public event Action<uint> OnCheckWorldTriggers;

		// Token: 0x14000005 RID: 5
		// (add) Token: 0x06000733 RID: 1843 RVA: 0x00022148 File Offset: 0x00020348
		// (remove) Token: 0x06000734 RID: 1844 RVA: 0x00022180 File Offset: 0x00020380
		public event Action OnTickAi;

		// Token: 0x14000006 RID: 6
		// (add) Token: 0x06000735 RID: 1845 RVA: 0x000221B8 File Offset: 0x000203B8
		// (remove) Token: 0x06000736 RID: 1846 RVA: 0x000221F0 File Offset: 0x000203F0
		public event Action OnProcessTransitions;

		// Token: 0x14000007 RID: 7
		// (add) Token: 0x06000737 RID: 1847 RVA: 0x00022228 File Offset: 0x00020428
		// (remove) Token: 0x06000738 RID: 1848 RVA: 0x00022260 File Offset: 0x00020460
		public event Action<uint> OnUpdateState;

		// Token: 0x14000008 RID: 8
		// (add) Token: 0x06000739 RID: 1849 RVA: 0x00022298 File Offset: 0x00020498
		// (remove) Token: 0x0600073A RID: 1850 RVA: 0x000222D0 File Offset: 0x000204D0
		public event UnifiedStateUpdater.ProcessAiState OnWriteState;

		// Token: 0x14000009 RID: 9
		// (add) Token: 0x0600073B RID: 1851 RVA: 0x00022308 File Offset: 0x00020508
		// (remove) Token: 0x0600073C RID: 1852 RVA: 0x00022340 File Offset: 0x00020540
		public event UnifiedStateUpdater.ConsumeAiState OnReadState;

		// Token: 0x1400000A RID: 10
		// (add) Token: 0x0600073D RID: 1853 RVA: 0x00022378 File Offset: 0x00020578
		// (remove) Token: 0x0600073E RID: 1854 RVA: 0x000223B0 File Offset: 0x000205B0
		public event Action<NpcState> OnStateInterpolated;

		// Token: 0x1400000B RID: 11
		// (add) Token: 0x0600073F RID: 1855 RVA: 0x000223E8 File Offset: 0x000205E8
		// (remove) Token: 0x06000740 RID: 1856 RVA: 0x00022420 File Offset: 0x00020620
		public event Action OnCheckGroundState;

		// Token: 0x06000741 RID: 1857 RVA: 0x00022455 File Offset: 0x00020655
		private void WriteState(ref NpcState netState)
		{
			UnifiedStateUpdater.ProcessAiState onWriteState = this.OnWriteState;
			if (onWriteState == null)
			{
				return;
			}
			onWriteState(ref netState);
		}

		// Token: 0x06000742 RID: 1858 RVA: 0x00022468 File Offset: 0x00020668
		private void ReadState(ref NpcState state)
		{
			UnifiedStateUpdater.ConsumeAiState onReadState = this.OnReadState;
			if (onReadState == null)
			{
				return;
			}
			onReadState(ref state);
		}

		// Token: 0x06000743 RID: 1859 RVA: 0x0002247B File Offset: 0x0002067B
		private void StateInterpolated(NpcState npcState)
		{
			Action<NpcState> onStateInterpolated = this.OnStateInterpolated;
			if (onStateInterpolated == null)
			{
				return;
			}
			onStateInterpolated(npcState);
		}

		// Token: 0x06000744 RID: 1860 RVA: 0x0002248E File Offset: 0x0002068E
		protected void OnDisable()
		{
			this.UnbindFromEvents();
		}

		// Token: 0x06000745 RID: 1861 RVA: 0x00022498 File Offset: 0x00020698
		private void UnbindFromEvents()
		{
			UnifiedStateUpdater.OnCheckWorldTriggers -= this.HandleOnCheckWorldTriggers;
			UnifiedStateUpdater.OnProcessTransitions -= this.HandleOnProcessTransitions;
			UnifiedStateUpdater.OnCleanupTriggers -= this.HandleOnCleanupTriggers;
			UnifiedStateUpdater.OnTickAi -= this.HandleOnTickAi;
			UnifiedStateUpdater.OnUpdateState -= this.HandleOnUpdateState;
			UnifiedStateUpdater.OnWriteState -= this.HandleOnWriteState;
			UnifiedStateUpdater.OnSendState -= this.HandleOnSendState;
			UnifiedStateUpdater.OnReadState -= this.HandleOnReadState;
			UnifiedStateUpdater.OnInterpolateState -= this.HandleOnInterpolateState;
			UnifiedStateUpdater.OnCheckGrounding -= this.HandleOnCheckGrounding;
		}

		// Token: 0x06000746 RID: 1862 RVA: 0x0002254F File Offset: 0x0002074F
		private void HandleOnCheckGrounding()
		{
			Action onCheckGroundState = this.OnCheckGroundState;
			if (onCheckGroundState == null)
			{
				return;
			}
			onCheckGroundState();
		}

		// Token: 0x06000747 RID: 1863 RVA: 0x00022564 File Offset: 0x00020764
		private void BindToServerEvents()
		{
			this.hasReceivedData = base.IsServer;
			UnifiedStateUpdater.OnCheckWorldTriggers += this.HandleOnCheckWorldTriggers;
			UnifiedStateUpdater.OnProcessTransitions += this.HandleOnProcessTransitions;
			UnifiedStateUpdater.OnCleanupTriggers += this.HandleOnCleanupTriggers;
			UnifiedStateUpdater.OnTickAi += this.HandleOnTickAi;
			UnifiedStateUpdater.OnUpdateState += this.HandleOnUpdateState;
			UnifiedStateUpdater.OnWriteState += this.HandleOnWriteState;
			UnifiedStateUpdater.OnSendState += this.HandleOnSendState;
			UnifiedStateUpdater.OnCheckGrounding += this.HandleOnCheckGrounding;
		}

		// Token: 0x06000748 RID: 1864 RVA: 0x00022608 File Offset: 0x00020808
		private void BindToClientEvents()
		{
			this.aiStates.OnStatesShifted.AddListener(new UnityAction<NpcState, NpcState>(this.HandleOnStatesShifted));
			UnifiedStateUpdater.OnUpdateState += this.HandleOnUpdateState;
			UnifiedStateUpdater.OnCheckGrounding += this.HandleOnCheckGrounding;
			UnifiedStateUpdater.OnInterpolateState += this.HandleOnInterpolateState;
			UnifiedStateUpdater.OnReadState += this.HandleOnReadState;
		}

		// Token: 0x06000749 RID: 1865 RVA: 0x00022678 File Offset: 0x00020878
		private void HandleOnStatesShifted(NpcState previousState, NpcState newState)
		{
			base.NpcEntity.Components.Animator.SetBool(NpcAnimator.Moving, newState.isMoving);
			base.NpcEntity.Components.Animator.SetFloat(NpcAnimator.FallTime, newState.fallTime);
			base.NpcEntity.Components.Animator.SetBool(NpcAnimator.Grounded, newState.isGrounded);
			if (newState.LargePush)
			{
				base.NpcEntity.Components.Animator.SetTrigger(NpcAnimator.LargePush);
			}
			if (newState.jumped)
			{
				base.NpcEntity.Components.Animator.SetTrigger(NpcAnimator.Jump);
			}
			if (newState.landed)
			{
				base.NpcEntity.Components.Animator.SetTrigger(NpcAnimator.Landed);
			}
			if (newState.PhysicsForceExit)
			{
				base.NpcEntity.Components.Animator.SetTrigger(NpcAnimator.PhysicsForceExit);
			}
			if (newState.SmallPush)
			{
				base.NpcEntity.Components.Animator.SetTrigger(NpcAnimator.SmallPush);
			}
			if (newState.EndSmallPush)
			{
				base.NpcEntity.Components.Animator.SetTrigger(NpcAnimator.EndSmallPush);
			}
			if (newState.LoopSmallPush)
			{
				base.NpcEntity.Components.Animator.SetTrigger(NpcAnimator.LoopSmallPush);
			}
			if (newState.ImminentlyAttacking)
			{
				base.NpcEntity.Components.AttackAlert.ImminentlyAttacking(0U, false);
			}
		}

		// Token: 0x0600074A RID: 1866 RVA: 0x000227F2 File Offset: 0x000209F2
		private void HandleOnCheckWorldTriggers(uint frame)
		{
			Action<uint> onCheckWorldTriggers = this.OnCheckWorldTriggers;
			if (onCheckWorldTriggers == null)
			{
				return;
			}
			onCheckWorldTriggers(frame);
		}

		// Token: 0x0600074B RID: 1867 RVA: 0x00022805 File Offset: 0x00020A05
		private void HandleOnProcessTransitions()
		{
			Action onProcessTransitions = this.OnProcessTransitions;
			if (onProcessTransitions == null)
			{
				return;
			}
			onProcessTransitions();
		}

		// Token: 0x0600074C RID: 1868 RVA: 0x00022817 File Offset: 0x00020A17
		private void HandleOnCleanupTriggers()
		{
			Action onCleanupTriggers = this.OnCleanupTriggers;
			if (onCleanupTriggers == null)
			{
				return;
			}
			onCleanupTriggers();
		}

		// Token: 0x0600074D RID: 1869 RVA: 0x00022829 File Offset: 0x00020A29
		private void HandleOnTickAi()
		{
			Action onTickAi = this.OnTickAi;
			if (onTickAi == null)
			{
				return;
			}
			onTickAi();
		}

		// Token: 0x0600074E RID: 1870 RVA: 0x0002283B File Offset: 0x00020A3B
		private void HandleOnUpdateState(uint frame)
		{
			Action<uint> onUpdateState = this.OnUpdateState;
			if (onUpdateState == null)
			{
				return;
			}
			onUpdateState(frame);
		}

		// Token: 0x0600074F RID: 1871 RVA: 0x00022850 File Offset: 0x00020A50
		private void HandleOnWriteState(uint frame)
		{
			ref NpcState ptr = ref this.GetCurrentState();
			ptr.NetFrame = frame;
			this.WriteState(ref ptr);
			this.aiStates.UpdateValue(ref ptr);
		}

		// Token: 0x06000750 RID: 1872 RVA: 0x0002287E File Offset: 0x00020A7E
		private void HandleOnSendState(uint obj)
		{
			GameplayMessagingManager.SendAiState(this.currentState, (uint)base.NpcEntity.WorldObject.NetworkObject.NetworkObjectId);
			this.currentState.Clear();
		}

		// Token: 0x06000751 RID: 1873 RVA: 0x000228AC File Offset: 0x00020AAC
		private void HandleOnReadState(uint frame)
		{
			ref NpcState referenceFromBuffer = ref this.aiStates.GetReferenceFromBuffer(frame);
			this.ReadState(ref referenceFromBuffer);
			referenceFromBuffer.Clear();
		}

		// Token: 0x06000752 RID: 1874 RVA: 0x000228D4 File Offset: 0x00020AD4
		private void HandleOnInterpolateState(double interpolationTime)
		{
			if (!this.hasReceivedData)
			{
				return;
			}
			this.aiStates.ActiveInterpolationTime = interpolationTime;
			if (Vector3.Distance(this.aiStates.PastInterpolationState.Position, this.aiStates.NextInterpolationState.Position) > 1f)
			{
				this.aiStates.ActiveInterpolatedState.Position = this.aiStates.NextInterpolationState.Position;
			}
			else
			{
				this.aiStates.ActiveInterpolatedState.Position = Vector3.Lerp(this.aiStates.PastInterpolationState.Position, this.aiStates.NextInterpolationState.Position, this.aiStates.ActiveStateLerpTime);
			}
			this.aiStates.ActiveInterpolatedState.Rotation = Mathf.Lerp(this.aiStates.PastInterpolationState.Rotation, this.aiStates.NextInterpolationState.Rotation, this.aiStates.ActiveStateLerpTime);
			this.aiStates.ActiveInterpolatedState.slopeAngle = Mathf.Lerp(this.aiStates.PastInterpolationState.slopeAngle, this.aiStates.NextInterpolationState.slopeAngle, this.aiStates.ActiveStateLerpTime);
			this.aiStates.ActiveInterpolatedState.VelX = Mathf.Lerp(this.aiStates.PastInterpolationState.VelX, this.aiStates.NextInterpolationState.VelX, this.aiStates.ActiveStateLerpTime);
			this.aiStates.ActiveInterpolatedState.VelY = Mathf.Lerp(this.aiStates.PastInterpolationState.VelY, this.aiStates.NextInterpolationState.VelY, this.aiStates.ActiveStateLerpTime);
			this.aiStates.ActiveInterpolatedState.VelZ = Mathf.Lerp(this.aiStates.PastInterpolationState.VelZ, this.aiStates.NextInterpolationState.VelZ, this.aiStates.ActiveStateLerpTime);
			this.aiStates.ActiveInterpolatedState.AngularVelocity = Mathf.Lerp(this.aiStates.PastInterpolationState.AngularVelocity, this.aiStates.NextInterpolationState.AngularVelocity, this.aiStates.ActiveStateLerpTime);
			this.aiStates.ActiveInterpolatedState.HorizVelMagnitude = Mathf.Lerp(this.aiStates.PastInterpolationState.HorizVelMagnitude, this.aiStates.NextInterpolationState.HorizVelMagnitude, this.aiStates.ActiveStateLerpTime);
			this.StateInterpolated(this.aiStates.ActiveInterpolatedState);
		}

		// Token: 0x06000753 RID: 1875 RVA: 0x00022B58 File Offset: 0x00020D58
		public static void ReceiveAiStates(uint key, NpcState simulatedState)
		{
			WorldObject worldObject;
			if (!MonoBehaviourSingleton<NetworkedWorldObjectMap>.Instance.ObjectMap.TryGetValue(key, out worldObject))
			{
				return;
			}
			worldObject.GetUserComponent<NpcEntity>().Components.IndividualStateUpdater.UpdateStates(simulatedState);
		}

		// Token: 0x06000754 RID: 1876 RVA: 0x00022B90 File Offset: 0x00020D90
		private void UpdateStates(NpcState simulatedState)
		{
			this.hasReceivedData = true;
			this.aiStates.UpdateValue(ref simulatedState);
		}

		// Token: 0x06000755 RID: 1877 RVA: 0x00022BA6 File Offset: 0x00020DA6
		public ref NpcState GetCurrentState()
		{
			return ref this.currentState;
		}

		// Token: 0x06000756 RID: 1878 RVA: 0x00022BAE File Offset: 0x00020DAE
		public void EndlessStart()
		{
			if (base.IsServer)
			{
				this.BindToServerEvents();
				return;
			}
			this.BindToClientEvents();
		}

		// Token: 0x040005BE RID: 1470
		private NpcState currentState;

		// Token: 0x040005BF RID: 1471
		private readonly InterpolationRingBuffer<NpcState> aiStates = new InterpolationRingBuffer<NpcState>(30);

		// Token: 0x040005C0 RID: 1472
		public readonly RingBuffer<NpcState> ClientStates = new RingBuffer<NpcState>(30);

		// Token: 0x040005C1 RID: 1473
		private bool isBoundToEvents;

		// Token: 0x040005C2 RID: 1474
		private bool hasReceivedData;
	}
}
