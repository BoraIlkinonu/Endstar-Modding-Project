using System;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay
{
	// Token: 0x02000153 RID: 339
	public class VelocityTracker
	{
		// Token: 0x060007FA RID: 2042 RVA: 0x00025754 File Offset: 0x00023954
		public VelocityTracker(Transform transform, Animator animator, NavMeshAgent agent, NpcEntity entity, MonoBehaviorProxy proxy, IndividualStateUpdater stateUpdater)
		{
			this.transform = transform;
			this.animator = animator;
			this.agent = agent;
			this.entity = entity;
			stateUpdater.OnUpdateState += this.HandleOnUpdateState;
			stateUpdater.OnWriteState += this.OnWriteState;
			proxy.OnUpdate += this.HandleOnUpdate;
			this.lastPosition = transform.position;
			this.lastRotation = transform.rotation.eulerAngles.y;
		}

		// Token: 0x1700017F RID: 383
		// (get) Token: 0x060007FB RID: 2043 RVA: 0x000257EF File Offset: 0x000239EF
		public float SmoothedSpeed
		{
			get
			{
				return this.smoothedDisplacement.magnitude / (this.entity.AnimationSpeed * Time.deltaTime);
			}
		}

		// Token: 0x060007FC RID: 2044 RVA: 0x0002580E File Offset: 0x00023A0E
		public Vector3 GetVelocity()
		{
			Vector3CircularBuffer vector3CircularBuffer = this.buffer;
			Vector3 vector = vector3CircularBuffer[vector3CircularBuffer.Count - 1];
			Vector3CircularBuffer vector3CircularBuffer2 = this.buffer;
			return (vector - vector3CircularBuffer2[vector3CircularBuffer2.Count - 2]) / NetClock.FixedDeltaTime;
		}

		// Token: 0x060007FD RID: 2045 RVA: 0x00025845 File Offset: 0x00023A45
		public Vector3 GetSmoothVelocity()
		{
			Vector3CircularBuffer vector3CircularBuffer = this.buffer;
			return (vector3CircularBuffer[vector3CircularBuffer.Count - 1] - this.buffer[0]) / ((float)this.buffer.Count * NetClock.FixedDeltaTime);
		}

		// Token: 0x060007FE RID: 2046 RVA: 0x00025882 File Offset: 0x00023A82
		private void HandleOnUpdateState(uint obj)
		{
			this.buffer.Add(this.transform.position);
		}

		// Token: 0x060007FF RID: 2047 RVA: 0x0002589C File Offset: 0x00023A9C
		private void OnWriteState(ref NpcState currentState)
		{
			currentState.VelX = this.animator.GetFloat(NpcAnimator.VelX);
			currentState.VelY = this.animator.GetFloat(NpcAnimator.VelY);
			currentState.VelZ = this.animator.GetFloat(NpcAnimator.VelZ);
			currentState.AngularVelocity = this.animator.GetFloat(NpcAnimator.AngularVelocity);
			currentState.HorizVelMagnitude = this.animator.GetFloat(NpcAnimator.HorizVelMagnitude);
		}

		// Token: 0x06000800 RID: 2048 RVA: 0x00025918 File Offset: 0x00023B18
		private void HandleOnUpdate()
		{
			float y = this.transform.rotation.eulerAngles.y;
			Vector3 position = this.transform.position;
			float num = Mathf.DeltaAngle(this.lastRotation, y);
			Vector3 vector = position - this.lastPosition;
			Vector3 vector2 = this.transform.InverseTransformDirection(vector) / (this.entity.AnimationSpeed * Time.deltaTime);
			float num2 = (position.y - this.lastPosition.y) / Time.deltaTime;
			float num3 = num / Time.deltaTime;
			float num4 = num3 / this.agent.angularSpeed;
			this.animatorAngularVelocity = Mathf.SmoothDamp(this.animatorAngularVelocity, num4, ref this.animatorAngularVelocityVelocity, 0.125f);
			this.animatorVelocityX = Mathf.SmoothDamp(this.animatorVelocityX, vector2.x, ref this.animatorVelocityXVelocity, 0.125f);
			this.animatorVelocityZ = Mathf.SmoothDamp(this.animatorVelocityZ, vector2.z, ref this.animatorVelocityZVelocity, 0.125f);
			this.animatorVelocityY = Mathf.SmoothDamp(this.animatorVelocityY, num2, ref this.animatorVelocityYVelocity, 0.125f);
			this.smoothedDisplacement = new Vector2(this.animatorVelocityX, this.animatorVelocityZ);
			this.lastRotation = y;
			this.lastPosition = position;
			this.animator.SetFloat(NpcAnimator.VelX, this.animatorVelocityX);
			this.animator.SetFloat(NpcAnimator.VelY, this.animatorVelocityY);
			this.animator.SetFloat(NpcAnimator.VelZ, this.animatorVelocityZ);
			this.animator.SetFloat(NpcAnimator.AngularVelocity, num3);
			this.animator.SetFloat(NpcAnimator.HorizVelMagnitude, this.SmoothedSpeed);
		}

		// Token: 0x04000651 RID: 1617
		private const float VELOCITY_SMOOTH_TIME = 0.125f;

		// Token: 0x04000652 RID: 1618
		private const float ANGULAR_VELOCITY_SMOOTH_TIME = 0.125f;

		// Token: 0x04000653 RID: 1619
		private readonly Vector3CircularBuffer buffer = new Vector3CircularBuffer(10);

		// Token: 0x04000654 RID: 1620
		private float animatorAngularVelocity;

		// Token: 0x04000655 RID: 1621
		private float animatorVelocityX;

		// Token: 0x04000656 RID: 1622
		private float animatorVelocityY;

		// Token: 0x04000657 RID: 1623
		private float animatorVelocityZ;

		// Token: 0x04000658 RID: 1624
		private float animatorAngularVelocityVelocity;

		// Token: 0x04000659 RID: 1625
		private float animatorVelocityXVelocity;

		// Token: 0x0400065A RID: 1626
		private float animatorVelocityYVelocity;

		// Token: 0x0400065B RID: 1627
		private float animatorVelocityZVelocity;

		// Token: 0x0400065C RID: 1628
		private float lastRotation;

		// Token: 0x0400065D RID: 1629
		private Vector3 lastPosition;

		// Token: 0x0400065E RID: 1630
		private Vector2 smoothedDisplacement;

		// Token: 0x0400065F RID: 1631
		private readonly Animator animator;

		// Token: 0x04000660 RID: 1632
		private readonly Transform transform;

		// Token: 0x04000661 RID: 1633
		private readonly NavMeshAgent agent;

		// Token: 0x04000662 RID: 1634
		private readonly NpcEntity entity;
	}
}
