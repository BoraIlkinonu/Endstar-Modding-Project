using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Creator.LevelEditing;

public class BoundaryGrid : MonoBehaviour
{
	[SerializeField]
	private float fadeDistance = 10f;

	[SerializeField]
	private List<Transform> trackedTransforms;

	[SerializeField]
	private Color baseColor = Color.black;

	[SerializeField]
	private float scrollSpeed = 1f;

	[SerializeField]
	private float lineWidth = 0.02f;

	private Material material;

	private static readonly int LocalObjectPositions = Shader.PropertyToID("_LocalObjectPositions");

	private static readonly int LocalObjectPositionsCount = Shader.PropertyToID("_LocalObjectPositionsCount");

	private static readonly int FadeDistanceId = Shader.PropertyToID("_FadeDistance");

	private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

	private readonly Vector4[] allValues = new Vector4[10];

	private static readonly int LineColor = Shader.PropertyToID("_LineColor");

	private static readonly int ScrollSpeed = Shader.PropertyToID("_ScrollSpeed");

	private static readonly int LineWidth = Shader.PropertyToID("_LineWidth");

	public float FadeDistance => fadeDistance;

	private void Awake()
	{
		material = GetComponent<MeshRenderer>().material;
		material.SetFloat(FadeDistanceId, fadeDistance);
		material.SetColor(BaseColor, baseColor);
		material.SetFloat(ScrollSpeed, scrollSpeed);
		material.SetFloat(LineWidth, lineWidth);
		Array.Fill(allValues, new Vector4(0f, 0f, 0f, 1f));
	}

	private void Update()
	{
		List<Transform> list = trackedTransforms;
		if (list == null || list.Count <= 0)
		{
			return;
		}
		for (int num = trackedTransforms.Count - 1; num >= 0; num--)
		{
			if (trackedTransforms[num] == null)
			{
				trackedTransforms.RemoveAt(num);
			}
		}
		int num2 = 0;
		for (int i = 0; i < trackedTransforms.Count && i < allValues.Length; i++)
		{
			if (trackedTransforms[i].gameObject.activeInHierarchy)
			{
				allValues[i].x = trackedTransforms[i].position.x;
				allValues[i].y = trackedTransforms[i].position.y;
				allValues[i].z = trackedTransforms[i].position.z;
				num2++;
			}
		}
		if (num2 != 0)
		{
			material.SetVectorArray(LocalObjectPositions, allValues);
			material.SetInteger(LocalObjectPositionsCount, num2);
		}
	}

	public void Track(Transform trackedTransform)
	{
		trackedTransforms.Add(trackedTransform);
	}

	public void Untrack(Transform untrackedTransform)
	{
		trackedTransforms.Remove(untrackedTransform);
	}

	public void SetLineColor(Color color)
	{
		material.SetColor(LineColor, color);
	}

	public void SetFadeDistance(float fadeDistance)
	{
		material.SetFloat(FadeDistanceId, fadeDistance);
	}
}
