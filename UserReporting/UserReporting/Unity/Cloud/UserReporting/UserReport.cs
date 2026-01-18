using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unity.Cloud.UserReporting
{
	// Token: 0x02000018 RID: 24
	public class UserReport : UserReportPreview
	{
		// Token: 0x06000076 RID: 118 RVA: 0x00003E04 File Offset: 0x00002004
		public UserReport()
		{
			base.AggregateMetrics = new List<UserReportMetric>();
			this.Attachments = new List<UserReportAttachment>();
			this.ClientMetrics = new List<UserReportMetric>();
			this.DeviceMetadata = new List<UserReportNamedValue>();
			this.Events = new List<UserReportEvent>();
			this.Fields = new List<UserReportNamedValue>();
			this.Measures = new List<UserReportMeasure>();
			this.Screenshots = new List<UserReportScreenshot>();
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000077 RID: 119 RVA: 0x00003E6F File Offset: 0x0000206F
		// (set) Token: 0x06000078 RID: 120 RVA: 0x00003E77 File Offset: 0x00002077
		public List<UserReportAttachment> Attachments { get; set; }

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000079 RID: 121 RVA: 0x00003E80 File Offset: 0x00002080
		// (set) Token: 0x0600007A RID: 122 RVA: 0x00003E88 File Offset: 0x00002088
		public List<UserReportMetric> ClientMetrics { get; set; }

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x0600007B RID: 123 RVA: 0x00003E91 File Offset: 0x00002091
		// (set) Token: 0x0600007C RID: 124 RVA: 0x00003E99 File Offset: 0x00002099
		public List<UserReportNamedValue> DeviceMetadata { get; set; }

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x0600007D RID: 125 RVA: 0x00003EA2 File Offset: 0x000020A2
		// (set) Token: 0x0600007E RID: 126 RVA: 0x00003EAA File Offset: 0x000020AA
		public List<UserReportEvent> Events { get; set; }

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x0600007F RID: 127 RVA: 0x00003EB3 File Offset: 0x000020B3
		// (set) Token: 0x06000080 RID: 128 RVA: 0x00003EBB File Offset: 0x000020BB
		public List<UserReportNamedValue> Fields { get; set; }

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000081 RID: 129 RVA: 0x00003EC4 File Offset: 0x000020C4
		// (set) Token: 0x06000082 RID: 130 RVA: 0x00003ECC File Offset: 0x000020CC
		public List<UserReportMeasure> Measures { get; set; }

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x06000083 RID: 131 RVA: 0x00003ED5 File Offset: 0x000020D5
		// (set) Token: 0x06000084 RID: 132 RVA: 0x00003EDD File Offset: 0x000020DD
		public List<UserReportScreenshot> Screenshots { get; set; }

		// Token: 0x06000085 RID: 133 RVA: 0x00003EE8 File Offset: 0x000020E8
		public UserReport Clone()
		{
			return new UserReport
			{
				AggregateMetrics = ((base.AggregateMetrics != null) ? base.AggregateMetrics.ToList<UserReportMetric>() : null),
				Attachments = ((this.Attachments != null) ? this.Attachments.ToList<UserReportAttachment>() : null),
				ClientMetrics = ((this.ClientMetrics != null) ? this.ClientMetrics.ToList<UserReportMetric>() : null),
				ContentLength = base.ContentLength,
				DeviceMetadata = ((this.DeviceMetadata != null) ? this.DeviceMetadata.ToList<UserReportNamedValue>() : null),
				Dimensions = base.Dimensions.ToList<UserReportNamedValue>(),
				Events = ((this.Events != null) ? this.Events.ToList<UserReportEvent>() : null),
				ExpiresOn = base.ExpiresOn,
				Fields = ((this.Fields != null) ? this.Fields.ToList<UserReportNamedValue>() : null),
				Identifier = base.Identifier,
				IPAddress = base.IPAddress,
				Measures = ((this.Measures != null) ? this.Measures.ToList<UserReportMeasure>() : null),
				ProjectIdentifier = base.ProjectIdentifier,
				ReceivedOn = base.ReceivedOn,
				Screenshots = ((this.Screenshots != null) ? this.Screenshots.ToList<UserReportScreenshot>() : null),
				Summary = base.Summary,
				Thumbnail = base.Thumbnail
			};
		}

		// Token: 0x06000086 RID: 134 RVA: 0x0000404C File Offset: 0x0000224C
		public void Complete()
		{
			if (this.Screenshots.Count > 0)
			{
				base.Thumbnail = this.Screenshots[this.Screenshots.Count - 1];
			}
			Dictionary<string, UserReportMetric> dictionary = new Dictionary<string, UserReportMetric>();
			foreach (UserReportMeasure userReportMeasure in this.Measures)
			{
				foreach (UserReportMetric userReportMetric in userReportMeasure.Metrics)
				{
					if (!dictionary.ContainsKey(userReportMetric.Name))
					{
						UserReportMetric userReportMetric2 = default(UserReportMetric);
						userReportMetric2.Name = userReportMetric.Name;
						dictionary.Add(userReportMetric.Name, userReportMetric2);
					}
					UserReportMetric userReportMetric3 = dictionary[userReportMetric.Name];
					userReportMetric3.Sample(userReportMetric.Average);
					dictionary[userReportMetric.Name] = userReportMetric3;
				}
			}
			if (base.AggregateMetrics == null)
			{
				base.AggregateMetrics = new List<UserReportMetric>();
			}
			foreach (KeyValuePair<string, UserReportMetric> keyValuePair in dictionary)
			{
				base.AggregateMetrics.Add(keyValuePair.Value);
			}
			base.AggregateMetrics.Sort(new UserReport.UserReportMetricSorter());
		}

		// Token: 0x06000087 RID: 135 RVA: 0x000041DC File Offset: 0x000023DC
		public void Fix()
		{
			base.AggregateMetrics = base.AggregateMetrics ?? new List<UserReportMetric>();
			this.Attachments = this.Attachments ?? new List<UserReportAttachment>();
			this.ClientMetrics = this.ClientMetrics ?? new List<UserReportMetric>();
			this.DeviceMetadata = this.DeviceMetadata ?? new List<UserReportNamedValue>();
			base.Dimensions = base.Dimensions ?? new List<UserReportNamedValue>();
			this.Events = this.Events ?? new List<UserReportEvent>();
			this.Fields = this.Fields ?? new List<UserReportNamedValue>();
			this.Measures = this.Measures ?? new List<UserReportMeasure>();
			this.Screenshots = this.Screenshots ?? new List<UserReportScreenshot>();
		}

		// Token: 0x06000088 RID: 136 RVA: 0x000042A8 File Offset: 0x000024A8
		public string GetDimensionsString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < base.Dimensions.Count; i++)
			{
				UserReportNamedValue userReportNamedValue = base.Dimensions[i];
				stringBuilder.Append(userReportNamedValue.Name);
				stringBuilder.Append(": ");
				stringBuilder.Append(userReportNamedValue.Value);
				if (i != base.Dimensions.Count - 1)
				{
					stringBuilder.Append(", ");
				}
			}
			return stringBuilder.ToString();
		}

		// Token: 0x06000089 RID: 137 RVA: 0x00004328 File Offset: 0x00002528
		public void RemoveScreenshots(int maximumWidth, int maximumHeight, int totalBytes, int ignoreCount)
		{
			int num = 0;
			for (int i = this.Screenshots.Count; i > 0; i--)
			{
				if (i >= ignoreCount)
				{
					UserReportScreenshot userReportScreenshot = this.Screenshots[i];
					num += userReportScreenshot.DataBase64.Length;
					if (num > totalBytes)
					{
						break;
					}
					if (userReportScreenshot.Width > maximumWidth || userReportScreenshot.Height > maximumHeight)
					{
						this.Screenshots.RemoveAt(i);
					}
				}
			}
		}

		// Token: 0x0600008A RID: 138 RVA: 0x00004394 File Offset: 0x00002594
		public UserReportPreview ToPreview()
		{
			return new UserReportPreview
			{
				AggregateMetrics = ((base.AggregateMetrics != null) ? base.AggregateMetrics.ToList<UserReportMetric>() : null),
				ContentLength = base.ContentLength,
				Dimensions = ((base.Dimensions != null) ? base.Dimensions.ToList<UserReportNamedValue>() : null),
				ExpiresOn = base.ExpiresOn,
				Identifier = base.Identifier,
				IPAddress = base.IPAddress,
				ProjectIdentifier = base.ProjectIdentifier,
				ReceivedOn = base.ReceivedOn,
				Summary = base.Summary,
				Thumbnail = base.Thumbnail
			};
		}

		// Token: 0x0200003E RID: 62
		private class UserReportMetricSorter : IComparer<UserReportMetric>
		{
			// Token: 0x06000214 RID: 532 RVA: 0x00009477 File Offset: 0x00007677
			public int Compare(UserReportMetric x, UserReportMetric y)
			{
				return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
			}
		}
	}
}
