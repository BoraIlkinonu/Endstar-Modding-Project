using System;
using Newtonsoft.Json;

namespace Endless.Shared
{
	// Token: 0x02000098 RID: 152
	public class BuildManifest
	{
		// Token: 0x170000BC RID: 188
		// (get) Token: 0x0600044C RID: 1100 RVA: 0x0001297E File Offset: 0x00010B7E
		// (set) Token: 0x0600044D RID: 1101 RVA: 0x00012986 File Offset: 0x00010B86
		[JsonProperty("scmCommitId")]
		public string CommitId { get; set; } = string.Empty;

		// Token: 0x170000BD RID: 189
		// (get) Token: 0x0600044E RID: 1102 RVA: 0x0001298F File Offset: 0x00010B8F
		// (set) Token: 0x0600044F RID: 1103 RVA: 0x00012997 File Offset: 0x00010B97
		[JsonProperty("scmBranch")]
		public string Branch { get; set; } = string.Empty;

		// Token: 0x170000BE RID: 190
		// (get) Token: 0x06000450 RID: 1104 RVA: 0x000129A0 File Offset: 0x00010BA0
		// (set) Token: 0x06000451 RID: 1105 RVA: 0x000129A8 File Offset: 0x00010BA8
		[JsonProperty("buildNumber")]
		public string BuildNumber { get; set; } = string.Empty;

		// Token: 0x170000BF RID: 191
		// (get) Token: 0x06000452 RID: 1106 RVA: 0x000129B1 File Offset: 0x00010BB1
		// (set) Token: 0x06000453 RID: 1107 RVA: 0x000129B9 File Offset: 0x00010BB9
		[JsonProperty("buildStartTime")]
		public string BuildStartTime { get; set; } = string.Empty;

		// Token: 0x170000C0 RID: 192
		// (get) Token: 0x06000454 RID: 1108 RVA: 0x000129C2 File Offset: 0x00010BC2
		// (set) Token: 0x06000455 RID: 1109 RVA: 0x000129CA File Offset: 0x00010BCA
		[JsonProperty("projectId")]
		public string ProjectId { get; set; } = string.Empty;

		// Token: 0x170000C1 RID: 193
		// (get) Token: 0x06000456 RID: 1110 RVA: 0x000129D3 File Offset: 0x00010BD3
		// (set) Token: 0x06000457 RID: 1111 RVA: 0x000129DB File Offset: 0x00010BDB
		[JsonProperty("bundleId")]
		public string BundleId { get; set; } = string.Empty;

		// Token: 0x170000C2 RID: 194
		// (get) Token: 0x06000458 RID: 1112 RVA: 0x000129E4 File Offset: 0x00010BE4
		// (set) Token: 0x06000459 RID: 1113 RVA: 0x000129EC File Offset: 0x00010BEC
		[JsonProperty("unityVersion")]
		public string UnityVersion { get; set; } = string.Empty;

		// Token: 0x170000C3 RID: 195
		// (get) Token: 0x0600045A RID: 1114 RVA: 0x000129F5 File Offset: 0x00010BF5
		// (set) Token: 0x0600045B RID: 1115 RVA: 0x000129FD File Offset: 0x00010BFD
		[JsonProperty("xcodeVersion")]
		public string XCodeVersion { get; set; } = string.Empty;

		// Token: 0x170000C4 RID: 196
		// (get) Token: 0x0600045C RID: 1116 RVA: 0x00012A06 File Offset: 0x00010C06
		// (set) Token: 0x0600045D RID: 1117 RVA: 0x00012A0E File Offset: 0x00010C0E
		[JsonProperty("cloudBuildTargetName")]
		public string Target { get; set; } = string.Empty;

		// Token: 0x170000C5 RID: 197
		// (get) Token: 0x0600045E RID: 1118 RVA: 0x00012A17 File Offset: 0x00010C17
		// (set) Token: 0x0600045F RID: 1119 RVA: 0x00012A1F File Offset: 0x00010C1F
		public string Version { get; set; } = string.Empty;
	}
}
