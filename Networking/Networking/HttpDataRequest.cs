using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Endless.Networking.Http;

namespace Endless.Networking
{
	// Token: 0x02000009 RID: 9
	public class HttpDataRequest
	{
		// Token: 0x14000001 RID: 1
		// (add) Token: 0x0600002E RID: 46 RVA: 0x00002968 File Offset: 0x00000B68
		// (remove) Token: 0x0600002F RID: 47 RVA: 0x000029A0 File Offset: 0x00000BA0
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<byte[]> OnDataAvailable;

		// Token: 0x06000030 RID: 48 RVA: 0x000029D8 File Offset: 0x00000BD8
		public HttpDataRequest(string getPrefix, int getPort, string uri, string logFile)
		{
			this.requestUrl = getPrefix;
			bool flag = getPort >= 0;
			if (flag)
			{
				this.requestUrl += string.Format(":{0}", getPort);
			}
			this.requestUrl = this.requestUrl + "/" + uri;
		}

		// Token: 0x06000031 RID: 49 RVA: 0x00002A40 File Offset: 0x00000C40
		public byte[] GetData()
		{
			bool flag = this.gettingData;
			byte[] array;
			if (flag)
			{
				array = null;
			}
			else
			{
				try
				{
					Task<byte[]> byteArrayAsync = HTTPClient.Client.GetByteArrayAsync(this.requestUrl);
					byteArrayAsync.Wait();
					byte[] result = byteArrayAsync.Result;
					array = result;
				}
				catch (Exception ex)
				{
					Logger.Log(NetConfig.LogFileName, ex.ToString(), true);
					array = null;
				}
			}
			return array;
		}

		// Token: 0x06000032 RID: 50 RVA: 0x00002AAC File Offset: 0x00000CAC
		public void GetDataAsync()
		{
			bool flag = this.gettingData;
			if (!flag)
			{
				this.gettingData = true;
				this.getDataAsync();
			}
		}

		// Token: 0x06000033 RID: 51 RVA: 0x00002AD4 File Offset: 0x00000CD4
		private async void getDataAsync()
		{
			try
			{
				byte[] array = await HTTPClient.Client.GetByteArrayAsync(this.requestUrl);
				byte[] data = array;
				array = null;
				this.gettingData = false;
				Action<byte[]> onDataAvailable = this.OnDataAvailable;
				if (onDataAvailable != null)
				{
					onDataAvailable(data);
				}
				data = null;
			}
			catch (Exception e)
			{
				Logger.Log(NetConfig.LogFileName, e.ToString(), true);
				this.gettingData = false;
				Action<byte[]> onDataAvailable2 = this.OnDataAvailable;
				if (onDataAvailable2 != null)
				{
					onDataAvailable2(null);
				}
			}
		}

		// Token: 0x04000017 RID: 23
		private readonly string requestUrl;

		// Token: 0x04000018 RID: 24
		private bool gettingData = false;
	}
}
