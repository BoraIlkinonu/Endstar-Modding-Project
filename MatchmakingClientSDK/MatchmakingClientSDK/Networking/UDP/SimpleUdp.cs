using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using Networking.UDP.Utils;

namespace Networking.UDP
{
	// Token: 0x02000036 RID: 54
	public class SimpleUdp : IDisposable
	{
		// Token: 0x0600016E RID: 366 RVA: 0x000092A0 File Offset: 0x000074A0
		private static SimpleUdp.Packet GetPacket()
		{
			SimpleUdp.Packet packet;
			if (SimpleUdp.packetPool.TryDequeue(out packet))
			{
				return packet;
			}
			return new SimpleUdp.Packet();
		}

		// Token: 0x0600016F RID: 367 RVA: 0x000092C2 File Offset: 0x000074C2
		private static void RecyclePacket(SimpleUdp.Packet packet)
		{
			packet.Target = null;
			packet.Buffer = null;
			packet.AttemptCount = 0;
			packet.AttemptIndex = 0;
			SimpleUdp.packetPool.Enqueue(packet);
		}

		// Token: 0x1400000D RID: 13
		// (add) Token: 0x06000170 RID: 368 RVA: 0x000092EC File Offset: 0x000074EC
		// (remove) Token: 0x06000171 RID: 369 RVA: 0x00009324 File Offset: 0x00007524
		public event Action<SimpleUdp, EndPoint, byte[], int> OnDataReceived;

		// Token: 0x1400000E RID: 14
		// (add) Token: 0x06000172 RID: 370 RVA: 0x0000935C File Offset: 0x0000755C
		// (remove) Token: 0x06000173 RID: 371 RVA: 0x00009394 File Offset: 0x00007594
		public event Action<SimpleUdp> OnDataSent;

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x06000174 RID: 372 RVA: 0x000093C9 File Offset: 0x000075C9
		public bool Disposed
		{
			get
			{
				return Interlocked.CompareExchange(ref this.disposed, 0, 0) != 0;
			}
		}

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x06000175 RID: 373 RVA: 0x000093DB File Offset: 0x000075DB
		public bool Receiving
		{
			get
			{
				return this.receiving;
			}
		}

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x06000176 RID: 374 RVA: 0x000093E5 File Offset: 0x000075E5
		public bool IPv6 { get; }

		// Token: 0x06000177 RID: 375 RVA: 0x000093F0 File Offset: 0x000075F0
		public SimpleUdp(bool useIPv6 = false, byte id = 0, Action<string, bool> log = null)
		{
			this.Id = id;
			this.log = log;
			this.IPv6 = useIPv6;
			this.socket = new Socket(this.IPv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			try
			{
				this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				if (this.IPv6)
				{
					this.socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, !Socket.OSSupportsIPv4);
				}
			}
			catch (Exception ex)
			{
				if (log != null)
				{
					log(string.Format("Failed to set socket options: {0}", ex), true);
				}
			}
		}

		// Token: 0x06000178 RID: 376 RVA: 0x000094B0 File Offset: 0x000076B0
		public void Server(int port)
		{
			if (this.Disposed)
			{
				return;
			}
			this.socket.Bind(new IPEndPoint(this.IPv6 ? IPAddress.IPv6Any : IPAddress.Any, port));
			this.Receive();
		}

		// Token: 0x06000179 RID: 377 RVA: 0x000094E6 File Offset: 0x000076E6
		public void Client()
		{
			if (this.Disposed)
			{
				return;
			}
			this.Receive();
		}

		// Token: 0x0600017A RID: 378 RVA: 0x000094F8 File Offset: 0x000076F8
		public void Send(NetDataWriter buffer, EndPoint target, int attempts = 1)
		{
			if (this.Disposed)
			{
				return;
			}
			if (attempts <= 0)
			{
				attempts = 1;
			}
			SimpleUdp.Packet packet = SimpleUdp.GetPacket();
			packet.Target = target;
			packet.Buffer = buffer;
			packet.AttemptCount = attempts;
			packet.AttemptIndex = 0;
			this.SendPacket(packet);
		}

		// Token: 0x0600017B RID: 379 RVA: 0x00009540 File Offset: 0x00007740
		private void SendPacket(SimpleUdp.Packet packet)
		{
			try
			{
				this.socket.BeginSendTo(packet.Buffer.Data, 0, packet.Buffer.Length, SocketFlags.None, packet.Target, new AsyncCallback(this.OnSent), packet);
			}
			catch (Exception ex)
			{
				Action<string, bool> action = this.log;
				if (action != null)
				{
					action(string.Format("Failed to begin sending a packet to {0}: {1}", packet.Target, ex), true);
				}
				NetDataWriterPool.RecycleBuffer(packet.Buffer);
				SimpleUdp.RecyclePacket(packet);
				this.EndSend();
			}
		}

		// Token: 0x0600017C RID: 380 RVA: 0x000095D4 File Offset: 0x000077D4
		private void OnSent(IAsyncResult ar)
		{
			try
			{
				this.socket.EndSendTo(ar);
				if (ar != null)
				{
					SimpleUdp.Packet packet = ar.AsyncState as SimpleUdp.Packet;
					if (packet != null)
					{
						packet.AttemptIndex++;
						if (packet.AttemptIndex >= packet.AttemptCount)
						{
							NetDataWriter buffer = packet.Buffer;
							SimpleUdp.RecyclePacket(packet);
							NetDataWriterPool.RecycleBuffer(buffer);
							this.EndSend();
							Action<SimpleUdp> onDataSent = this.OnDataSent;
							if (onDataSent != null)
							{
								onDataSent(this);
							}
							return;
						}
						this.SendPacket(packet);
						return;
					}
				}
				this.EndSend();
			}
			catch (Exception ex)
			{
				Action<string, bool> action = this.log;
				if (action != null)
				{
					action(string.Format("Failed to complete sending a packet: {0}", ex), true);
				}
				if (ar != null)
				{
					SimpleUdp.Packet packet2 = ar.AsyncState as SimpleUdp.Packet;
					if (packet2 != null)
					{
						NetDataWriter buffer2 = packet2.Buffer;
						SimpleUdp.RecyclePacket(packet2);
						NetDataWriterPool.RecycleBuffer(buffer2);
					}
				}
				this.EndSend();
			}
		}

		// Token: 0x0600017D RID: 381 RVA: 0x000096B8 File Offset: 0x000078B8
		private void EndSend()
		{
		}

		// Token: 0x0600017E RID: 382 RVA: 0x000096BA File Offset: 0x000078BA
		private void Receive()
		{
			if (this.Disposed)
			{
				return;
			}
			if (this.receiving)
			{
				return;
			}
			this.receiving = true;
			ThreadPool.QueueUserWorkItem(new WaitCallback(this.ReceiveCallback));
		}

		// Token: 0x0600017F RID: 383 RVA: 0x000096EC File Offset: 0x000078EC
		[NullableContext(2)]
		private void ReceiveCallback(object state)
		{
			try
			{
				this.socket.BeginReceiveFrom(this.receiveArray, 0, 65536, SocketFlags.None, ref this.receiveEndPoint, new AsyncCallback(this.OnReceived), null);
			}
			catch (Exception ex)
			{
				Action<string, bool> action = this.log;
				if (action != null)
				{
					action(string.Format("Failed to begin receiving a packet: {0}", ex), true);
				}
				this.EndReceive(false);
			}
		}

		// Token: 0x06000180 RID: 384 RVA: 0x00009760 File Offset: 0x00007960
		private void OnReceived(IAsyncResult ar)
		{
			try
			{
				int num = this.socket.EndReceiveFrom(ar, ref this.receiveEndPoint);
				Action<SimpleUdp, EndPoint, byte[], int> onDataReceived = this.OnDataReceived;
				if (onDataReceived != null)
				{
					onDataReceived(this, this.receiveEndPoint, this.receiveArray, num);
				}
				this.EndReceive(true);
			}
			catch (Exception ex)
			{
				Action<string, bool> action = this.log;
				if (action != null)
				{
					action(string.Format("Failed to complete receiving a packet: {0}", ex), true);
				}
				this.EndReceive(false);
			}
		}

		// Token: 0x06000181 RID: 385 RVA: 0x000097E0 File Offset: 0x000079E0
		private void EndReceive(bool receive = true)
		{
			this.receiving = false;
			if (receive)
			{
				this.Receive();
			}
		}

		// Token: 0x06000182 RID: 386 RVA: 0x000097F4 File Offset: 0x000079F4
		public void Dispose()
		{
			if (Interlocked.CompareExchange(ref this.disposed, 1, 0) != 0)
			{
				return;
			}
			try
			{
				this.socket.Close();
				this.socket.Dispose();
			}
			catch (Exception ex)
			{
				Action<string, bool> action = this.log;
				if (action != null)
				{
					action(string.Format("Failed to dispose SimpleUdp socket: {0}", ex), true);
				}
			}
		}

		// Token: 0x0400015C RID: 348
		public const int MAX_PACKET_SIZE = 65536;

		// Token: 0x0400015D RID: 349
		private static readonly ConcurrentQueue<SimpleUdp.Packet> packetPool = new ConcurrentQueue<SimpleUdp.Packet>();

		// Token: 0x0400015E RID: 350
		public readonly byte Id;

		// Token: 0x0400015F RID: 351
		private readonly Action<string, bool> log;

		// Token: 0x04000160 RID: 352
		private readonly Socket socket;

		// Token: 0x04000161 RID: 353
		private readonly byte[] receiveArray = new byte[65536];

		// Token: 0x04000165 RID: 357
		private int disposed;

		// Token: 0x04000166 RID: 358
		private volatile bool receiving;

		// Token: 0x04000167 RID: 359
		private EndPoint receiveEndPoint = new IPEndPoint(IPAddress.Any, 0);

		// Token: 0x0200008C RID: 140
		private class Packet
		{
			// Token: 0x040002E2 RID: 738
			public EndPoint Target;

			// Token: 0x040002E3 RID: 739
			public NetDataWriter Buffer;

			// Token: 0x040002E4 RID: 740
			public int AttemptCount;

			// Token: 0x040002E5 RID: 741
			public int AttemptIndex;
		}
	}
}
