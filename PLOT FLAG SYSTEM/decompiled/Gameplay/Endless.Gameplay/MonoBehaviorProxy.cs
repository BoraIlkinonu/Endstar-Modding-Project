using System;
using System.Collections;
using UnityEngine;

namespace Endless.Gameplay;

public class MonoBehaviorProxy : MonoBehaviour
{
	public event Action OnUpdate;

	public Coroutine StartMonoBehaviorRoutine(IEnumerator routine)
	{
		return StartCoroutine(routine);
	}

	public void StopMonoBehaviorRoutine(Coroutine coroutine)
	{
		StopCoroutine(coroutine);
	}

	private void Update()
	{
		this.OnUpdate?.Invoke();
	}
}
