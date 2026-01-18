using Endless.Shared;
using Endless.Shared.Tweens;
using UnityEngine;

namespace Endless.Gameplay;

public class UISelfPoolingTween : MonoBehaviour, IPoolableT
{
	[SerializeField]
	private TweenCollection tweenCollection;

	[field: SerializeField]
	public MonoBehaviour Prefab { get; set; }

	public bool IsUi => true;

	public void OnSpawn()
	{
	}

	public void Tween()
	{
		tweenCollection.Tween(HandleFinished);
	}

	public void OnDespawn()
	{
		tweenCollection.SetToStart();
	}

	private void HandleFinished()
	{
		MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(this);
	}
}
