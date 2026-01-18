using System;
using System.Net;
using System.Threading;
using Networking.UDP.Utils;

namespace Networking.UDP
{
	// Token: 0x0200000C RID: 12
	public class ConnectionRequest
	{
		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000011 RID: 17 RVA: 0x000021B9 File Offset: 0x000003B9
		public NetDataReader Data
		{
			get
			{
				return this.InternalPacket.Data;
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000012 RID: 18 RVA: 0x000021C6 File Offset: 0x000003C6
		// (set) Token: 0x06000013 RID: 19 RVA: 0x000021CE File Offset: 0x000003CE
		internal ConnectionRequestResult Result { get; private set; }

		// Token: 0x06000014 RID: 20 RVA: 0x000021D8 File Offset: 0x000003D8
		internal void UpdateRequest(NetConnectRequestPacket connectRequest)
		{
			if (connectRequest.ConnectionTime < this.InternalPacket.ConnectionTime)
			{
				return;
			}
			if (connectRequest.ConnectionTime == this.InternalPacket.ConnectionTime && connectRequest.ConnectionNumber == this.InternalPacket.ConnectionNumber)
			{
				return;
			}
			this.InternalPacket = connectRequest;
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00002227 File Offset: 0x00000427
		private bool TryActivate()
		{
			return Interlocked.CompareExchange(ref this._used, 1, 0) == 0;
		}

		// Token: 0x06000016 RID: 22 RVA: 0x00002239 File Offset: 0x00000439
		internal ConnectionRequest(IPEndPoint remoteEndPoint, NetConnectRequestPacket requestPacket, NetManager listener)
		{
			this.InternalPacket = requestPacket;
			this.RemoteEndPoint = remoteEndPoint;
			this._listener = listener;
		}

		// Token: 0x06000017 RID: 23 RVA: 0x00002258 File Offset: 0x00000458
		public NetPeer AcceptIfKey(string key)
		{
			if (!this.TryActivate())
			{
				return null;
			}
			try
			{
				if (this.Data.GetString() == key)
				{
					this.Result = ConnectionRequestResult.Accept;
				}
			}
			catch
			{
				NetDebug.WriteError("[AC] Invalid incoming data");
			}
			if (this.Result == ConnectionRequestResult.Accept)
			{
				return this._listener.OnConnectionSolved(this, null, 0, 0);
			}
			this.Result = ConnectionRequestResult.Reject;
			this._listener.OnConnectionSolved(this, null, 0, 0);
			return null;
		}

		// Token: 0x06000018 RID: 24 RVA: 0x000022DC File Offset: 0x000004DC
		public NetPeer Accept()
		{
			if (!this.TryActivate())
			{
				return null;
			}
			this.Result = ConnectionRequestResult.Accept;
			return this._listener.OnConnectionSolved(this, null, 0, 0);
		}

		// Token: 0x06000019 RID: 25 RVA: 0x000022FE File Offset: 0x000004FE
		public void Reject(byte[] rejectData, int start, int length, bool force)
		{
			if (!this.TryActivate())
			{
				return;
			}
			this.Result = (force ? ConnectionRequestResult.RejectForce : ConnectionRequestResult.Reject);
			this._listener.OnConnectionSolved(this, rejectData, start, length);
		}

		// Token: 0x0600001A RID: 26 RVA: 0x00002327 File Offset: 0x00000527
		public void Reject(byte[] rejectData, int start, int length)
		{
			this.Reject(rejectData, start, length, false);
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00002333 File Offset: 0x00000533
		public void RejectForce(byte[] rejectData, int start, int length)
		{
			this.Reject(rejectData, start, length, true);
		}

		// Token: 0x0600001C RID: 28 RVA: 0x0000233F File Offset: 0x0000053F
		public void RejectForce()
		{
			this.Reject(null, 0, 0, true);
		}

		// Token: 0x0600001D RID: 29 RVA: 0x0000234B File Offset: 0x0000054B
		public void RejectForce(byte[] rejectData)
		{
			this.Reject(rejectData, 0, rejectData.Length, true);
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00002359 File Offset: 0x00000559
		public void RejectForce(NetDataWriter rejectData)
		{
			this.Reject(rejectData.Data, 0, rejectData.Length, true);
		}

		// Token: 0x0600001F RID: 31 RVA: 0x0000236F File Offset: 0x0000056F
		public void Reject()
		{
			this.Reject(null, 0, 0, false);
		}

		// Token: 0x06000020 RID: 32 RVA: 0x0000237B File Offset: 0x0000057B
		public void Reject(byte[] rejectData)
		{
			this.Reject(rejectData, 0, rejectData.Length, false);
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00002389 File Offset: 0x00000589
		public void Reject(NetDataWriter rejectData)
		{
			this.Reject(rejectData.Data, 0, rejectData.Length, false);
		}

		// Token: 0x04000012 RID: 18
		private readonly NetManager _listener;

		// Token: 0x04000013 RID: 19
		private int _used;

		// Token: 0x04000015 RID: 21
		internal NetConnectRequestPacket InternalPacket;

		// Token: 0x04000016 RID: 22
		public readonly IPEndPoint RemoteEndPoint;
	}
}
