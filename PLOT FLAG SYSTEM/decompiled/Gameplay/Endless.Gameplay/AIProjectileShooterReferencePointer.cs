using System;
using UnityEngine;

namespace Endless.Gameplay;

public class AIProjectileShooterReferencePointer : MonoBehaviour
{
	[field: SerializeField]
	public Transform FirePoint { get; private set; }

	[field: SerializeField]
	public ParticleSystem MuzzleFlashEffect { get; private set; }

	[field: SerializeField]
	public ParticleSystem EjectionEffect { get; private set; }

	[Obsolete("Use HitEffectPrefab")]
	[field: SerializeField]
	public ParticleSystem HitEffect { get; private set; }

	[field: SerializeField]
	public HitEffect HitEffectPrefab { get; private set; }
}
