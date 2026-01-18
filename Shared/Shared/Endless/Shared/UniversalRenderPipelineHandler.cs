using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Endless.Shared
{
	// Token: 0x02000091 RID: 145
	public class UniversalRenderPipelineHandler
	{
		// Token: 0x0600040C RID: 1036 RVA: 0x00011948 File Offset: 0x0000FB48
		public UniversalRenderPipelineHandler(UniversalRenderPipelineAsset target)
		{
			Type typeFromHandle = typeof(UniversalRenderPipelineAsset);
			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
			this.mainLightCastShadows = typeFromHandle.GetField("m_MainLightShadowsSupported", bindingFlags);
			this.additionalLightsCastShadows = typeFromHandle.GetField("m_AdditionalLightShadowsSupported", bindingFlags);
			this.additionalLightsRenderingMode = typeFromHandle.GetField("m_AdditionalLightsRenderingMode", bindingFlags);
			this.mainLightShadowmapResolution = typeFromHandle.GetField("m_MainLightShadowmapResolution", bindingFlags);
			this.additionalLightsShadowmapResolution = typeFromHandle.GetField("m_AdditionalLightsShadowmapResolution", bindingFlags);
			this.cascade2Split = typeFromHandle.GetField("m_Cascade2Split", bindingFlags);
			this.cascade3Split = typeFromHandle.GetField("m_Cascade3Split", bindingFlags);
			this.cascade4Split = typeFromHandle.GetField("m_Cascade4Split", bindingFlags);
			this.softShadowsEnabled = typeFromHandle.GetField("m_SoftShadowsSupported", bindingFlags);
			this.shadowDistance = typeFromHandle.GetField("m_ShadowDistance", bindingFlags);
			this.shadowCascades = typeFromHandle.GetField("m_ShadowCascadeCount", bindingFlags);
			this.target = target;
		}

		// Token: 0x170000AE RID: 174
		// (get) Token: 0x0600040D RID: 1037 RVA: 0x00011A36 File Offset: 0x0000FC36
		public UniversalRenderPipelineAsset Target
		{
			get
			{
				return this.target;
			}
		}

		// Token: 0x0600040E RID: 1038 RVA: 0x00011A3E File Offset: 0x0000FC3E
		public void SetTarget(UniversalRenderPipelineAsset target)
		{
			this.target = target;
		}

		// Token: 0x170000AF RID: 175
		// (get) Token: 0x0600040F RID: 1039 RVA: 0x00011A47 File Offset: 0x0000FC47
		// (set) Token: 0x06000410 RID: 1040 RVA: 0x00011A5F File Offset: 0x0000FC5F
		public bool MainLightCastShadows
		{
			get
			{
				return (bool)this.mainLightCastShadows.GetValue(this.target);
			}
			set
			{
				this.mainLightCastShadows.SetValue(this.target, value);
			}
		}

		// Token: 0x170000B0 RID: 176
		// (get) Token: 0x06000411 RID: 1041 RVA: 0x00011A78 File Offset: 0x0000FC78
		// (set) Token: 0x06000412 RID: 1042 RVA: 0x00011A90 File Offset: 0x0000FC90
		public bool AdditionalLightsCastShadows
		{
			get
			{
				return (bool)this.additionalLightsCastShadows.GetValue(this.target);
			}
			set
			{
				this.additionalLightsCastShadows.SetValue(this.target, value);
			}
		}

		// Token: 0x170000B1 RID: 177
		// (get) Token: 0x06000413 RID: 1043 RVA: 0x00011AA9 File Offset: 0x0000FCA9
		// (set) Token: 0x06000414 RID: 1044 RVA: 0x00011AC1 File Offset: 0x0000FCC1
		public LightRenderingMode AdditionalLightsRenderingMode
		{
			get
			{
				return (LightRenderingMode)this.additionalLightsRenderingMode.GetValue(this.target);
			}
			set
			{
				this.additionalLightsRenderingMode.SetValue(this.target, value);
			}
		}

		// Token: 0x170000B2 RID: 178
		// (get) Token: 0x06000415 RID: 1045 RVA: 0x00011ADA File Offset: 0x0000FCDA
		// (set) Token: 0x06000416 RID: 1046 RVA: 0x00011AF2 File Offset: 0x0000FCF2
		public global::UnityEngine.Rendering.Universal.ShadowResolution MainLightShadowResolution
		{
			get
			{
				return (global::UnityEngine.Rendering.Universal.ShadowResolution)this.mainLightShadowmapResolution.GetValue(this.target);
			}
			set
			{
				this.mainLightShadowmapResolution.SetValue(this.target, value);
			}
		}

		// Token: 0x170000B3 RID: 179
		// (get) Token: 0x06000417 RID: 1047 RVA: 0x00011B0B File Offset: 0x0000FD0B
		// (set) Token: 0x06000418 RID: 1048 RVA: 0x00011B23 File Offset: 0x0000FD23
		public global::UnityEngine.Rendering.Universal.ShadowResolution AdditionalLightsShadowResolution
		{
			get
			{
				return (global::UnityEngine.Rendering.Universal.ShadowResolution)this.additionalLightsShadowmapResolution.GetValue(this.target);
			}
			set
			{
				this.additionalLightsShadowmapResolution.SetValue(this.target, value);
			}
		}

		// Token: 0x170000B4 RID: 180
		// (get) Token: 0x06000419 RID: 1049 RVA: 0x00011B3C File Offset: 0x0000FD3C
		// (set) Token: 0x0600041A RID: 1050 RVA: 0x00011B54 File Offset: 0x0000FD54
		public float Cascade2Split
		{
			get
			{
				return (float)this.cascade2Split.GetValue(this.target);
			}
			set
			{
				this.cascade2Split.SetValue(this.target, value);
			}
		}

		// Token: 0x170000B5 RID: 181
		// (get) Token: 0x0600041B RID: 1051 RVA: 0x00011B6D File Offset: 0x0000FD6D
		// (set) Token: 0x0600041C RID: 1052 RVA: 0x00011B85 File Offset: 0x0000FD85
		public Vector2 Cascade3Split
		{
			get
			{
				return (Vector2)this.cascade3Split.GetValue(this.target);
			}
			set
			{
				this.cascade3Split.SetValue(this.target, value);
			}
		}

		// Token: 0x170000B6 RID: 182
		// (get) Token: 0x0600041D RID: 1053 RVA: 0x00011B9E File Offset: 0x0000FD9E
		// (set) Token: 0x0600041E RID: 1054 RVA: 0x00011BB6 File Offset: 0x0000FDB6
		public Vector3 Cascade4Split
		{
			get
			{
				return (Vector3)this.cascade4Split.GetValue(this.target);
			}
			set
			{
				this.cascade4Split.SetValue(this.target, value);
			}
		}

		// Token: 0x170000B7 RID: 183
		// (get) Token: 0x0600041F RID: 1055 RVA: 0x00011BCF File Offset: 0x0000FDCF
		// (set) Token: 0x06000420 RID: 1056 RVA: 0x00011BE7 File Offset: 0x0000FDE7
		public bool SoftShadowsEnabled
		{
			get
			{
				return (bool)this.softShadowsEnabled.GetValue(this.target);
			}
			set
			{
				this.softShadowsEnabled.SetValue(this.target, value);
			}
		}

		// Token: 0x170000B8 RID: 184
		// (get) Token: 0x06000421 RID: 1057 RVA: 0x00011C00 File Offset: 0x0000FE00
		// (set) Token: 0x06000422 RID: 1058 RVA: 0x00011C18 File Offset: 0x0000FE18
		public float ShadowDistance
		{
			get
			{
				return (float)this.shadowDistance.GetValue(this.target);
			}
			set
			{
				this.shadowDistance.SetValue(this.target, value);
			}
		}

		// Token: 0x170000B9 RID: 185
		// (get) Token: 0x06000423 RID: 1059 RVA: 0x00011C31 File Offset: 0x0000FE31
		// (set) Token: 0x06000424 RID: 1060 RVA: 0x00011C49 File Offset: 0x0000FE49
		public int ShadowCascades
		{
			get
			{
				return (int)this.shadowCascades.GetValue(this.target);
			}
			set
			{
				this.shadowCascades.SetValue(this.target, value);
			}
		}

		// Token: 0x040001EE RID: 494
		private FieldInfo mainLightCastShadows;

		// Token: 0x040001EF RID: 495
		private FieldInfo additionalLightsCastShadows;

		// Token: 0x040001F0 RID: 496
		private FieldInfo additionalLightsRenderingMode;

		// Token: 0x040001F1 RID: 497
		private FieldInfo additionalLightsShadowmapResolution;

		// Token: 0x040001F2 RID: 498
		private FieldInfo mainLightShadowmapResolution;

		// Token: 0x040001F3 RID: 499
		private FieldInfo cascade2Split;

		// Token: 0x040001F4 RID: 500
		private FieldInfo cascade3Split;

		// Token: 0x040001F5 RID: 501
		private FieldInfo cascade4Split;

		// Token: 0x040001F6 RID: 502
		private FieldInfo softShadowsEnabled;

		// Token: 0x040001F7 RID: 503
		private FieldInfo shadowDistance;

		// Token: 0x040001F8 RID: 504
		private FieldInfo shadowCascades;

		// Token: 0x040001F9 RID: 505
		private UniversalRenderPipelineAsset target;
	}
}
