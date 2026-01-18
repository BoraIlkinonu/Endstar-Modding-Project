using System;
using System.Collections.Generic;
using System.Linq;

namespace Endless.Core.Test
{
	// Token: 0x020000DB RID: 219
	public class FpsReport
	{
		// Token: 0x060004ED RID: 1261 RVA: 0x00017DAC File Offset: 0x00015FAC
		public FpsReport(string sceneLabel, FpsInfo[] data)
		{
			this.SceneLabel = sceneLabel;
			this.RawData = data;
			foreach (FpsInfo fpsInfo in this.RawData)
			{
				FpsReportSection fpsReportSection = new FpsReportSection
				{
					SectionTitle = fpsInfo.SectionName,
					SectionType = fpsInfo.TestType.ToString(),
					SectionInfo = new List<string[]>()
				};
				float num = float.MaxValue;
				float num2 = float.MinValue;
				int num3 = 0;
				int num4 = 0;
				int count = fpsInfo.Frames.Count;
				float num5 = 0f;
				FpsTestType fpsTestType;
				foreach (float num6 in fpsInfo.Frames)
				{
					if (num6 > num2)
					{
						num2 = num6;
					}
					if (num6 > 0.033333335f)
					{
						num3++;
						num4++;
					}
					else if (num6 > 0.016666668f)
					{
						num4++;
					}
					fpsTestType = fpsInfo.TestType;
					if (fpsTestType != FpsTestType.Load && fpsTestType - FpsTestType.Static <= 1)
					{
						num5 += num6;
						if (num6 < num)
						{
							num = num6;
						}
					}
				}
				fpsTestType = fpsInfo.TestType;
				if (fpsTestType != FpsTestType.Load)
				{
					if (fpsTestType - FpsTestType.Static <= 1)
					{
						num5 /= (float)count;
						fpsReportSection.SectionInfo.Add(new string[]
						{
							"Average Framerate",
							(1f / num5).ToString("n2")
						});
						fpsReportSection.SectionInfo.Add(new string[]
						{
							"Best FPS",
							(1f / num).ToString("n2")
						});
					}
				}
				else
				{
					fpsReportSection.SectionInfo.Add(new string[]
					{
						"Load time (sec)",
						fpsInfo.Frames.Sum().ToString("n2"),
						"Load time (frames)",
						fpsInfo.Frames.Count.ToString()
					});
				}
				fpsReportSection.SectionInfo.Add(new string[]
				{
					"Worst FPS",
					(1f / num2).ToString("n2")
				});
				fpsReportSection.SectionInfo.Add(new string[]
				{
					"Frames < 30",
					num3.ToString(),
					"Frame Percent",
					((float)num3 / (float)count).ToString("P2")
				});
				fpsReportSection.SectionInfo.Add(new string[]
				{
					"Frames < 60",
					num4.ToString(),
					"Frame Percent",
					((float)num4 / (float)count).ToString("P2")
				});
				this.DisplayInfo.Add(fpsReportSection);
			}
		}

		// Token: 0x04000353 RID: 851
		private const float lowFrameRate = 0.033333335f;

		// Token: 0x04000354 RID: 852
		private const float mediumFrameRate = 0.016666668f;

		// Token: 0x04000355 RID: 853
		public string SceneLabel;

		// Token: 0x04000356 RID: 854
		public List<FpsReportSection> DisplayInfo = new List<FpsReportSection>();

		// Token: 0x04000357 RID: 855
		public FpsInfo[] RawData;
	}
}
