using System;
using UnityEngine.Audio;

namespace Endless.Core
{
	// Token: 0x02000027 RID: 39
	public static class AudioMixerGroupExtensions
	{
		// Token: 0x06000089 RID: 137 RVA: 0x00004A26 File Offset: 0x00002C26
		public static string VolumePropertyName(this AudioMixerGroup audioMixerGroup)
		{
			return "Volume" + audioMixerGroup.name;
		}
	}
}
