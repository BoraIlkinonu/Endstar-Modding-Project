using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay;

public class VelocityTracker
{
	private const float VELOCITY_SMOOTH_TIME = 0.125f;

	private const float ANGULAR_VELOCITY_SMOOTH_TIME = 0.125f;

	private readonly Vector3CircularBuffer buffer = new Vector3CircularBuffer(10);

	private float animatorAngularVelocity;

	private float animatorVelocityX;

	private float animatorVelocityY;

	private float animatorVelocityZ;

	private float animatorAngularVelocityVelocity;

	private float animatorVelocityXVelocity;

	private float animatorVelocityYVelocity;

	private float animatorVelocityZVelocity;

	private float lastRotation;

	private Vector3 lastPosition;

	private Vector2 smoothedDisplacement;

	private readonly Animator animator;

	private readonly Transform transform;

	private readonly NavMeshAgent agent;

	private readonly NpcEntity entity;

	public float SmoothedSpeed => smoothedDisplacement.magnitude / (entity.AnimationSpeed * Time.deltaTime);

	public VelocityTracker(Transform transform, Animator animator, NavMeshAgent agent, NpcEntity entity, MonoBehaviorProxy proxy, IndividualStateUpdater stateUpdater)
	{
		this.transform = transform;
		this.animator = animator;
		this.agent = agent;
		this.entity = entity;
		stateUpdater.OnUpdateState += HandleOnUpdateState;
		stateUpdater.OnWriteState += OnWriteState;
		proxy.OnUpdate += HandleOnUpdate;
		lastPosition = transform.position;
		lastRotation = transform.rotation.eulerAngles.y;
	}

	public Vector3 GetVelocity()
	{
		Vector3CircularBuffer vector3CircularBuffer = buffer;
		Vector3 vector = vector3CircularBuffer[vector3CircularBuffer.Count - 1];
		Vector3CircularBuffer vector3CircularBuffer2 = buffer;
		return (vector - vector3CircularBuffer2[vector3CircularBuffer2.Count - 2]) / NetClock.FixedDeltaTime;
	}

	public Vector3 GetSmoothVelocity()
	{
		Vector3CircularBuffer vector3CircularBuffer = buffer;
		return (vector3CircularBuffer[vector3CircularBuffer.Count - 1] - buffer[0]) / ((float)buffer.Count * NetClock.FixedDeltaTime);
	}

	private void HandleOnUpdateState(uint obj)
	{
		buffer.Add(transform.position);
	}

	private void OnWriteState(ref NpcState currentState)
	{
		currentState.VelX = animator.GetFloat(NpcAnimator.VelX);
		currentState.VelY = animator.GetFloat(NpcAnimator.VelY);
		currentState.VelZ = animator.GetFloat(NpcAnimator.VelZ);
		currentState.AngularVelocity = animator.GetFloat(NpcAnimator.AngularVelocity);
		currentState.HorizVelMagnitude = animator.GetFloat(NpcAnimator.HorizVelMagnitude);
	}

	private void HandleOnUpdate()
	{
		float y = transform.rotation.eulerAngles.y;
		Vector3 position = transform.position;
		float num = Mathf.DeltaAngle(lastRotation, y);
		Vector3 direction = position - lastPosition;
		Vector3 vector = transform.InverseTransformDirection(direction) / (entity.AnimationSpeed * Time.deltaTime);
		float target = (position.y - lastPosition.y) / Time.deltaTime;
		float num2 = num / Time.deltaTime;
		float target2 = num2 / agent.angularSpeed;
		animatorAngularVelocity = Mathf.SmoothDamp(animatorAngularVelocity, target2, ref animatorAngularVelocityVelocity, 0.125f);
		animatorVelocityX = Mathf.SmoothDamp(animatorVelocityX, vector.x, ref animatorVelocityXVelocity, 0.125f);
		animatorVelocityZ = Mathf.SmoothDamp(animatorVelocityZ, vector.z, ref animatorVelocityZVelocity, 0.125f);
		animatorVelocityY = Mathf.SmoothDamp(animatorVelocityY, target, ref animatorVelocityYVelocity, 0.125f);
		smoothedDisplacement = new Vector2(animatorVelocityX, animatorVelocityZ);
		lastRotation = y;
		lastPosition = position;
		animator.SetFloat(NpcAnimator.VelX, animatorVelocityX);
		animator.SetFloat(NpcAnimator.VelY, animatorVelocityY);
		animator.SetFloat(NpcAnimator.VelZ, animatorVelocityZ);
		animator.SetFloat(NpcAnimator.AngularVelocity, num2);
		animator.SetFloat(NpcAnimator.HorizVelMagnitude, SmoothedSpeed);
	}
}
