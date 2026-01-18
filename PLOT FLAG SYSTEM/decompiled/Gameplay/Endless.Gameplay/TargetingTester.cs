using UnityEngine;

namespace Endless.Gameplay;

public class TargetingTester : MonoBehaviour
{
	[SerializeField]
	private TargeterComponent targeterComponent;

	private void Awake()
	{
		targeterComponent.OnTargetChanged += delegate(HittableComponent target)
		{
			Debug.Log("Target has changed to " + (target ? target.gameObject.name : "null"));
		};
		targeterComponent.OnTargetChanging += delegate(HittableComponent target)
		{
			Debug.Log("Target is changing from " + (target ? target.gameObject.name : "null"));
		};
	}
}
