using System;

namespace Endless.Shared
{
	// Token: 0x0200009E RID: 158
	public static class DateTimeUtility
	{
		// Token: 0x06000474 RID: 1140 RVA: 0x00013378 File Offset: 0x00011578
		public static string GetElapsedTimeString(DateTime dateTime)
		{
			int num = 1;
			int num2 = 60 * num;
			int num3 = 60 * num2;
			int num4 = 24 * num3;
			int num5 = 30 * num4;
			TimeSpan timeSpan = DateTime.Now - dateTime;
			double num6 = Math.Abs(timeSpan.TotalSeconds);
			if (num6 < (double)num2)
			{
				if (timeSpan.Seconds != 1)
				{
					return timeSpan.Seconds.ToString() + " seconds ago";
				}
				return "one second ago";
			}
			else
			{
				if (num6 < (double)(2 * num2))
				{
					return "a minute ago";
				}
				if (num6 < (double)(45 * num2))
				{
					return timeSpan.Minutes.ToString() + " minutes ago";
				}
				if (num6 < (double)(90 * num2))
				{
					return "an hour ago";
				}
				if (num6 < (double)(24 * num3))
				{
					return timeSpan.Hours.ToString() + " hours ago";
				}
				if (num6 < (double)(48 * num3))
				{
					return "yesterday";
				}
				if (num6 < (double)(30 * num4))
				{
					return timeSpan.Days.ToString() + " days ago";
				}
				if (num6 < (double)(12 * num5))
				{
					int num7 = Convert.ToInt32(Math.Floor((double)timeSpan.Days / 30.0));
					if (num7 > 1)
					{
						return num7.ToString() + " months ago";
					}
					return "one month ago";
				}
				else
				{
					int num8 = Convert.ToInt32(Math.Floor((double)timeSpan.Days / 365.0));
					if (num8 > 1)
					{
						return num8.ToString() + " years ago";
					}
					return "one year ago";
				}
			}
		}
	}
}
