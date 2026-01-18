using System;
using System.Collections.Generic;
using Endless.Assets;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Props.Assets
{
	// Token: 0x02000038 RID: 56
	public class AudioAsset : Asset
	{
		// Token: 0x17000054 RID: 84
		// (get) Token: 0x060000DD RID: 221 RVA: 0x000032E9 File Offset: 0x000014E9
		// (set) Token: 0x060000DE RID: 222 RVA: 0x000032F1 File Offset: 0x000014F1
		[JsonProperty("audio_type")]
		public AudioType AudioType { get; private set; }

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x060000DF RID: 223 RVA: 0x000032FA File Offset: 0x000014FA
		// (set) Token: 0x060000E0 RID: 224 RVA: 0x00003302 File Offset: 0x00001502
		[JsonProperty("audio_category")]
		public AudioCategory AudioCategory { get; private set; }

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x060000E1 RID: 225 RVA: 0x0000330B File Offset: 0x0000150B
		[JsonIgnore]
		public int AudioFileInstanceId
		{
			get
			{
				return this.audioFileInstanceId;
			}
		}

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x060000E2 RID: 226 RVA: 0x00003313 File Offset: 0x00001513
		[JsonIgnore]
		public int IconFileInstanceId
		{
			get
			{
				return this.iconFileInstanceId;
			}
		}

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x060000E3 RID: 227 RVA: 0x0000331B File Offset: 0x0000151B
		[JsonIgnore]
		public float Duration
		{
			get
			{
				return this.duration;
			}
		}

		// Token: 0x060000E4 RID: 228 RVA: 0x00003324 File Offset: 0x00001524
		public AudioAsset(int fileInstanceId, int iconFileInstanceId, AudioType type, AudioCategory audioCategory, float duration)
		{
			this.audioFileInstanceId = fileInstanceId;
			this.iconFileInstanceId = iconFileInstanceId;
			this.AudioType = type;
			this.AudioCategory = audioCategory;
			this.AssetType = "audio";
			this.InternalVersion = AudioAsset.INTERNAL_VERSION.ToString();
			this.duration = duration;
		}

		// Token: 0x0400008C RID: 140
		[JsonIgnore]
		public const int ICON_HEIGHT = 256;

		// Token: 0x0400008D RID: 141
		[JsonIgnore]
		public const int ICON_WIDTH = 768;

		// Token: 0x0400008E RID: 142
		[JsonIgnore]
		public static readonly List<AudioType> SupportedAudioTypes = new List<AudioType>
		{
			AudioType.WAV,
			AudioType.MPEG
		};

		// Token: 0x0400008F RID: 143
		public static readonly SemanticVersion INTERNAL_VERSION = new SemanticVersion(1, 0, 0);

		// Token: 0x04000092 RID: 146
		[JsonProperty]
		private int audioFileInstanceId;

		// Token: 0x04000093 RID: 147
		[JsonProperty]
		private int iconFileInstanceId;

		// Token: 0x04000094 RID: 148
		[JsonProperty]
		private float duration;
	}
}
