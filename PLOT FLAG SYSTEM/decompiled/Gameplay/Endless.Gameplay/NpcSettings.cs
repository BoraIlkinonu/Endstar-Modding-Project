using UnityEngine;

namespace Endless.Gameplay;

[CreateAssetMenu(menuName = "ScriptableObject/NpcSettings", fileName = "NpcSettings")]
public class NpcSettings : ScriptableObject
{
	[field: SerializeField]
	public float WalkSpeed { get; private set; }

	[field: SerializeField]
	public float RunSpeed { get; private set; }

	[field: SerializeField]
	public float SprintSpeed { get; private set; }

	[field: SerializeField]
	public float StrafingSpeed { get; private set; }

	[field: SerializeField]
	public float RotationSpeed { get; private set; }

	[field: SerializeField]
	public uint DownFramesLimit { get; private set; }

	[field: SerializeField]
	public uint ReplanFrames { get; private set; }

	[field: SerializeField]
	public LayerMask CharacterCollisionMask { get; private set; }

	[field: SerializeField]
	public LayerMask GroundCollisionMask { get; private set; }

	[field: SerializeField]
	public AnimationCurve GravityCurve { get; private set; }

	[field: SerializeField]
	public int FramesToTerminalVelocity { get; private set; }

	[field: SerializeField]
	public float TerminalVelocity { get; private set; }

	[field: SerializeField]
	public float PredictionTime { get; private set; }
}
