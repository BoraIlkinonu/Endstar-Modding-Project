using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Endless.Gameplay;

public class LightCookieScroller : MonoBehaviour
{
	[SerializeField]
	private float scrollSpeed;

	[SerializeField]
	private float scrollWrap;

	[SerializeField]
	private UniversalAdditionalLightData lightData;

	private void OnValidate()
	{
		if (lightData == null)
		{
			lightData = GetComponent<UniversalAdditionalLightData>();
		}
	}

	private void Start()
	{
		if (lightData == null)
		{
			lightData = GetComponent<UniversalAdditionalLightData>();
		}
	}

	private void Update()
	{
		if (!(lightData == null))
		{
			if (lightData.lightCookieOffset.x >= scrollWrap)
			{
				lightData.lightCookieOffset = Vector2.zero;
			}
			else
			{
				lightData.lightCookieOffset += new Vector2(scrollSpeed, 0f) * Time.deltaTime;
			}
		}
	}
}
