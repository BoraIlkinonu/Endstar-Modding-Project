using System;
using System.Net;
using System.Text;

namespace Networking.UDP.Layers
{
	// Token: 0x02000048 RID: 72
	public class XorEncryptLayer : PacketLayerBase
	{
		// Token: 0x06000287 RID: 647 RVA: 0x0000BCB9 File Offset: 0x00009EB9
		public XorEncryptLayer()
			: base(0)
		{
		}

		// Token: 0x06000288 RID: 648 RVA: 0x0000BCC2 File Offset: 0x00009EC2
		public XorEncryptLayer(byte[] key)
			: this()
		{
			this.SetKey(key);
		}

		// Token: 0x06000289 RID: 649 RVA: 0x0000BCD1 File Offset: 0x00009ED1
		public XorEncryptLayer(string key)
			: this()
		{
			this.SetKey(key);
		}

		// Token: 0x0600028A RID: 650 RVA: 0x0000BCE0 File Offset: 0x00009EE0
		public void SetKey(string key)
		{
			this._byteKey = Encoding.UTF8.GetBytes(key);
		}

		// Token: 0x0600028B RID: 651 RVA: 0x0000BCF3 File Offset: 0x00009EF3
		public void SetKey(byte[] key)
		{
			if (this._byteKey == null || this._byteKey.Length != key.Length)
			{
				this._byteKey = new byte[key.Length];
			}
			Buffer.BlockCopy(key, 0, this._byteKey, 0, key.Length);
		}

		// Token: 0x0600028C RID: 652 RVA: 0x0000BD2C File Offset: 0x00009F2C
		public override void ProcessInboundPacket(ref IPEndPoint endPoint, ref byte[] data, ref int length)
		{
			if (this._byteKey == null)
			{
				return;
			}
			for (int i = 0; i < length; i++)
			{
				data[i] ^= this._byteKey[i % this._byteKey.Length];
			}
		}

		// Token: 0x0600028D RID: 653 RVA: 0x0000BD6C File Offset: 0x00009F6C
		public override void ProcessOutBoundPacket(ref IPEndPoint endPoint, ref byte[] data, ref int offset, ref int length)
		{
			if (this._byteKey == null)
			{
				return;
			}
			int num = offset;
			int i = 0;
			while (i < length)
			{
				data[num] ^= this._byteKey[i % this._byteKey.Length];
				i++;
				num++;
			}
		}

		// Token: 0x0400018C RID: 396
		private byte[] _byteKey;
	}
}
