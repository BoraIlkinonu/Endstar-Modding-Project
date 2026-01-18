using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000B8 RID: 184
	public class AudioUtility
	{
		// Token: 0x0600034A RID: 842 RVA: 0x00011D2F File Offset: 0x0000FF2F
		public static float DecibelToVolume(float dB)
		{
			return Mathf.Clamp01(Mathf.Pow(10f, dB / 20f));
		}

		// Token: 0x0600034B RID: 843 RVA: 0x00011D48 File Offset: 0x0000FF48
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
}
