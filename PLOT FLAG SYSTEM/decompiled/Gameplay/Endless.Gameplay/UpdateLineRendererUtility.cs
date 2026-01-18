using UnityEngine;

namespace Endless.Gameplay;

public class UpdateLineRendererUtility : MonoBehaviour
{
	[SerializeField]
	private Transform[] transformTargets;

	[SerializeField]
	private LineRenderer lineRenderer;

	private void Update()
	{
		UpdateLineRenderers();
	}

	public void UpdateLineRenderers()
	{
		for (int i = 0; i < transformTargets.Length; i++)
		{
			Transform transform = transformTargets[i];
			if (!(transform == null))
			{
				lineRenderer.SetPosition(i, transform.position);
			}
		}
	}
}
