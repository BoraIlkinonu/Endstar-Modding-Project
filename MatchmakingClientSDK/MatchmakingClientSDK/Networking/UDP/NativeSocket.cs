using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Networking.UDP
{
	// Token: 0x02000017 RID: 23
	internal static class NativeSocket
	{
		// Token: 0x0600005D RID: 93 RVA: 0x00002BD4 File Offset: 0x00000DD4
		static NativeSocket()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				NativeSocket.IsSupported = true;
				NativeSocket.UnixMode = true;
				return;
			}
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				NativeSocket.IsSupported = true;
			}
		}

		// Token: 0x0600005E RID: 94 RVA: 0x00002E3E File Offset: 0x0000103E
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int RecvFrom(IntPtr socketHandle, byte[] pinnedBuffer, int len, byte[] socketAddress, ref int socketAddressSize)
		{
			if (!NativeSocket.UnixMode)
			{
				return NativeSocket.WinSock.recvfrom(socketHandle, pinnedBuffer, len, SocketFlags.None, socketAddress, ref socketAddressSize);
			}
			return NativeSocket.UnixSock.recvfrom(socketHandle, pinnedBuffer, len, SocketFlags.None, socketAddress, ref socketAddressSize);
		}

		// Token: 0x0600005F RID: 95 RVA: 0x00002E60 File Offset: 0x00001060
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static int SendTo(IntPtr socketHandle, byte* pinnedBuffer, int len, byte[] socketAddress, int socketAddressSize)
		{
			if (!NativeSocket.UnixMode)
			{
				return NativeSocket.WinSock.sendto(socketHandle, pinnedBuffer, len, SocketFlags.None, socketAddress, socketAddressSize);
			}
			return NativeSocket.UnixSock.sendto(socketHandle, pinnedBuffer, len, SocketFlags.None, socketAddress, socketAddressSize);
		}

		// Token: 0x06000060 RID: 96 RVA: 0x00002E84 File Offset: 0x00001084
		public static SocketError GetSocketError()
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (!NativeSocket.UnixMode)
			{
				return (SocketError)lastWin32Error;
			}
			SocketError socketError;
			if (!NativeSocket.NativeErrorToSocketError.TryGetValue(lastWin32Error, out socketError))
			{
				return SocketError.SocketError;
			}
			return socketError;
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00002EB4 File Offset: 0x000010B4
		public static SocketException GetSocketException()
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (!NativeSocket.UnixMode)
			{
				return new SocketException(lastWin32Error);
			}
			SocketError socketError;
			if (!NativeSocket.NativeErrorToSocketError.TryGetValue(lastWin32Error, out socketError))
			{
				return new SocketException(-1);
			}
			return new SocketException((int)socketError);
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00002EF1 File Offset: 0x000010F1
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static short GetNativeAddressFamily(IPEndPoint remoteEndPoint)
		{
			if (!NativeSocket.UnixMode)
			{
				return (short)remoteEndPoint.AddressFamily;
			}
			return (remoteEndPoint.AddressFamily == AddressFamily.InterNetwork) ? 2 : 10;
		}

		// Token: 0x0400003F RID: 63
		public static readonly bool IsSupported = false;

		// Token: 0x04000040 RID: 64
		public static readonly bool UnixMode = false;

		// Token: 0x04000041 RID: 65
		public const int IPv4AddrSize = 16;

		// Token: 0x04000042 RID: 66
		public const int IPv6AddrSize = 28;

		// Token: 0x04000043 RID: 67
		public const int AF_INET = 2;

		// Token: 0x04000044 RID: 68
		public const int AF_INET6 = 10;

		// Token: 0x04000045 RID: 69
		private static readonly Dictionary<int, SocketError> NativeErrorToSocketError = new Dictionary<int, SocketError>
		{
			{
				13,
				SocketError.AccessDenied
			},
			{
				98,
				SocketError.AddressAlreadyInUse
			},
			{
				99,
				SocketError.AddressNotAvailable
			},
			{
				97,
				SocketError.AddressFamilyNotSupported
			},
			{
				11,
				SocketError.WouldBlock
			},
			{
				114,
				SocketError.AlreadyInProgress
			},
			{
				9,
				SocketError.OperationAborted
			},
			{
				125,
				SocketError.OperationAborted
			},
			{
				103,
				SocketError.ConnectionAborted
			},
			{
				111,
				SocketError.ConnectionRefused
			},
			{
				104,
				SocketError.ConnectionReset
			},
			{
				89,
				SocketError.DestinationAddressRequired
			},
			{
				14,
				SocketError.Fault
			},
			{
				112,
				SocketError.HostDown
			},
			{
				6,
				SocketError.HostNotFound
			},
			{
				113,
				SocketError.HostUnreachable
			},
			{
				115,
				SocketError.InProgress
			},
			{
				4,
				SocketError.Interrupted
			},
			{
				22,
				SocketError.InvalidArgument
			},
			{
				106,
				SocketError.IsConnected
			},
			{
				24,
				SocketError.TooManyOpenSockets
			},
			{
				90,
				SocketError.MessageSize
			},
			{
				100,
				SocketError.NetworkDown
			},
			{
				102,
				SocketError.NetworkReset
			},
			{
				101,
				SocketError.NetworkUnreachable
			},
			{
				23,
				SocketError.TooManyOpenSockets
			},
			{
				105,
				SocketError.NoBufferSpaceAvailable
			},
			{
				61,
				SocketError.NoData
			},
			{
				2,
				SocketError.AddressNotAvailable
			},
			{
				92,
				SocketError.ProtocolOption
			},
			{
				107,
				SocketError.NotConnected
			},
			{
				88,
				SocketError.NotSocket
			},
			{
				3440,
				SocketError.OperationNotSupported
			},
			{
				1,
				SocketError.AccessDenied
			},
			{
				32,
				SocketError.Shutdown
			},
			{
				96,
				SocketError.ProtocolFamilyNotSupported
			},
			{
				93,
				SocketError.ProtocolNotSupported
			},
			{
				91,
				SocketError.ProtocolType
			},
			{
				94,
				SocketError.SocketNotSupported
			},
			{
				108,
				SocketError.Disconnecting
			},
			{
				110,
				SocketError.TimedOut
			},
			{
				0,
				SocketError.Success
			}
		};

		// Token: 0x0200007C RID: 124
		private static class WinSock
		{
			// Token: 0x0600045B RID: 1115
			[DllImport("ws2_32.dll", SetLastError = true)]
			public static extern int recvfrom(IntPtr socketHandle, [In] [Out] byte[] pinnedBuffer, [In] int len, [In] SocketFlags socketFlags, [Out] byte[] socketAddress, [In] [Out] ref int socketAddressSize);

			// Token: 0x0600045C RID: 1116
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal unsafe static extern int sendto(IntPtr socketHandle, byte* pinnedBuffer, [In] int len, [In] SocketFlags socketFlags, [In] byte[] socketAddress, [In] int socketAddressSize);

			// Token: 0x040002B6 RID: 694
			private const string LibName = "ws2_32.dll";
		}

		// Token: 0x0200007D RID: 125
		private static class UnixSock
		{
			// Token: 0x0600045D RID: 1117
			[DllImport("libc", SetLastError = true)]
			public static extern int recvfrom(IntPtr socketHandle, [In] [Out] byte[] pinnedBuffer, [In] int len, [In] SocketFlags socketFlags, [Out] byte[] socketAddress, [In] [Out] ref int socketAddressSize);

			// Token: 0x0600045E RID: 1118
			[DllImport("libc", SetLastError = true)]
			internal unsafe static extern int sendto(IntPtr socketHandle, byte* pinnedBuffer, [In] int len, [In] SocketFlags socketFlags, [In] byte[] socketAddress, [In] int socketAddressSize);

			// Token: 0x040002B7 RID: 695
			private const string LibName = "libc";
		}
	}
}
