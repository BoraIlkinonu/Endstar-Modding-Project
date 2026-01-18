using UnityEngine;

namespace Endless.Gameplay;

public class AudioUtility
{
	public static float DecibelToVolume(float dB)
	{
		return Mathf.Clamp01(Mathf.Pow(10f, dB / 20f));
	}

	public static float VolumeToDecibel(float volume)
	{
		float num = Mathf.Clamp01(volume);
		if (num == 0f)
		{
			return -144f;
		}
		return 20f * Mathf.Log10(num);
	}
}
