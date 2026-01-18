using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.Screenshots
{
	// Token: 0x0200000F RID: 15
	public class ScreenshotRecorder
	{
		// Token: 0x06000038 RID: 56 RVA: 0x000035E5 File Offset: 0x000017E5
		public ScreenshotRecorder()
		{
			this.operationPool = new List<ScreenshotRecorder.ScreenshotOperation>();
		}

		// Token: 0x06000039 RID: 57 RVA: 0x000035F8 File Offset: 0x000017F8
		private ScreenshotRecorder.ScreenshotOperation GetOperation()
		{
			foreach (ScreenshotRecorder.ScreenshotOperation screenshotOperation in this.operationPool)
			{
				if (!screenshotOperation.IsInUse)
				{
					screenshotOperation.IsInUse = true;
					return screenshotOperation;
				}
			}
			ScreenshotRecorder.ScreenshotOperation screenshotOperation2 = new ScreenshotRecorder.ScreenshotOperation();
			screenshotOperation2.IsInUse = true;
			this.operationPool.Add(screenshotOperation2);
			return screenshotOperation2;
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00003674 File Offset: 0x00001874
		public void Screenshot(int maximumWidth, int maximumHeight, ScreenshotType type, Action<byte[], object> callback, object state)
		{
			Texture2D texture2D = ScreenCapture.CaptureScreenshotAsTexture();
			this.Screenshot(texture2D, maximumWidth, maximumHeight, type, callback, state);
		}

		// Token: 0x0600003B RID: 59 RVA: 0x00003698 File Offset: 0x00001898
		public void Screenshot(Camera source, int maximumWidth, int maximumHeight, ScreenshotType type, Action<byte[], object> callback, object state)
		{
			RenderTexture renderTexture = new RenderTexture(maximumWidth, maximumHeight, 24);
			RenderTexture targetTexture = source.targetTexture;
			source.targetTexture = renderTexture;
			source.Render();
			source.targetTexture = targetTexture;
			this.Screenshot(renderTexture, maximumWidth, maximumHeight, type, callback, state);
		}

		// Token: 0x0600003C RID: 60 RVA: 0x000036D9 File Offset: 0x000018D9
		public void Screenshot(RenderTexture source, int maximumWidth, int maximumHeight, ScreenshotType type, Action<byte[], object> callback, object state)
		{
			this.ScreenshotInternal(source, maximumWidth, maximumHeight, type, callback, state);
		}

		// Token: 0x0600003D RID: 61 RVA: 0x000036EA File Offset: 0x000018EA
		public void Screenshot(Texture2D source, int maximumWidth, int maximumHeight, ScreenshotType type, Action<byte[], object> callback, object state)
		{
			this.ScreenshotInternal(source, maximumWidth, maximumHeight, type, callback, state);
		}

		// Token: 0x0600003E RID: 62 RVA: 0x000036FC File Offset: 0x000018FC
		private void ScreenshotInternal(Texture source, int maximumWidth, int maximumHeight, ScreenshotType type, Action<byte[], object> callback, object state)
		{
			ScreenshotRecorder.ScreenshotOperation operation = this.GetOperation();
			operation.Identifier = ScreenshotRecorder.nextIdentifier++;
			operation.Source = source;
			operation.MaximumWidth = maximumWidth;
			operation.MaximumHeight = maximumHeight;
			operation.Type = type;
			operation.Callback = callback;
			operation.State = state;
			AsyncGPUReadback.Request(source, 0, TextureFormat.RGBA32, operation.ScreenshotCallbackDelegate);
		}

		// Token: 0x0400002A RID: 42
		private static int nextIdentifier;

		// Token: 0x0400002B RID: 43
		private List<ScreenshotRecorder.ScreenshotOperation> operationPool;

		// Token: 0x0200003C RID: 60
		private class ScreenshotOperation
		{
			// Token: 0x060001F4 RID: 500 RVA: 0x000091BC File Offset: 0x000073BC
			public ScreenshotOperation()
			{
				this.ScreenshotCallbackDelegate = new Action<AsyncGPUReadbackRequest>(this.ScreenshotCallback);
				this.EncodeCallbackDelegate = new WaitCallback(this.EncodeCallback);
			}

			// Token: 0x1700006D RID: 109
			// (get) Token: 0x060001F5 RID: 501 RVA: 0x000091E8 File Offset: 0x000073E8
			// (set) Token: 0x060001F6 RID: 502 RVA: 0x000091F0 File Offset: 0x000073F0
			public Action<byte[], object> Callback { get; set; }

			// Token: 0x1700006E RID: 110
			// (get) Token: 0x060001F7 RID: 503 RVA: 0x000091F9 File Offset: 0x000073F9
			// (set) Token: 0x060001F8 RID: 504 RVA: 0x00009201 File Offset: 0x00007401
			public int Height { get; set; }

			// Token: 0x1700006F RID: 111
			// (get) Token: 0x060001F9 RID: 505 RVA: 0x0000920A File Offset: 0x0000740A
			// (set) Token: 0x060001FA RID: 506 RVA: 0x00009212 File Offset: 0x00007412
			public int Identifier { get; set; }

			// Token: 0x17000070 RID: 112
			// (get) Token: 0x060001FB RID: 507 RVA: 0x0000921B File Offset: 0x0000741B
			// (set) Token: 0x060001FC RID: 508 RVA: 0x00009223 File Offset: 0x00007423
			public bool IsInUse { get; set; }

			// Token: 0x17000071 RID: 113
			// (get) Token: 0x060001FD RID: 509 RVA: 0x0000922C File Offset: 0x0000742C
			// (set) Token: 0x060001FE RID: 510 RVA: 0x00009234 File Offset: 0x00007434
			public int MaximumHeight { get; set; }

			// Token: 0x17000072 RID: 114
			// (get) Token: 0x060001FF RID: 511 RVA: 0x0000923D File Offset: 0x0000743D
			// (set) Token: 0x06000200 RID: 512 RVA: 0x00009245 File Offset: 0x00007445
			public int MaximumWidth { get; set; }

			// Token: 0x17000073 RID: 115
			// (get) Token: 0x06000201 RID: 513 RVA: 0x0000924E File Offset: 0x0000744E
			// (set) Token: 0x06000202 RID: 514 RVA: 0x00009256 File Offset: 0x00007456
			public NativeArray<byte> NativeData { get; set; }

			// Token: 0x17000074 RID: 116
			// (get) Token: 0x06000203 RID: 515 RVA: 0x0000925F File Offset: 0x0000745F
			// (set) Token: 0x06000204 RID: 516 RVA: 0x00009267 File Offset: 0x00007467
			public Texture Source { get; set; }

			// Token: 0x17000075 RID: 117
			// (get) Token: 0x06000205 RID: 517 RVA: 0x00009270 File Offset: 0x00007470
			// (set) Token: 0x06000206 RID: 518 RVA: 0x00009278 File Offset: 0x00007478
			public object State { get; set; }

			// Token: 0x17000076 RID: 118
			// (get) Token: 0x06000207 RID: 519 RVA: 0x00009281 File Offset: 0x00007481
			// (set) Token: 0x06000208 RID: 520 RVA: 0x00009289 File Offset: 0x00007489
			public ScreenshotType Type { get; set; }

			// Token: 0x17000077 RID: 119
			// (get) Token: 0x06000209 RID: 521 RVA: 0x00009292 File Offset: 0x00007492
			// (set) Token: 0x0600020A RID: 522 RVA: 0x0000929A File Offset: 0x0000749A
			public int Width { get; set; }

			// Token: 0x0600020B RID: 523 RVA: 0x000092A4 File Offset: 0x000074A4
			private void EncodeCallback(object state)
			{
				byte[] array = this.NativeData.ToArray();
				int num;
				array = Downsampler.Downsample(array, this.Width * 4, this.MaximumWidth, this.MaximumHeight, out num);
				if (this.Type == ScreenshotType.Png)
				{
					array = PngEncoder.Encode(array, num);
				}
				if (this.Callback != null)
				{
					this.Callback(array, this.State);
				}
				this.NativeData.Dispose();
				this.IsInUse = false;
			}

			// Token: 0x0600020C RID: 524 RVA: 0x0000931D File Offset: 0x0000751D
			private void SavePngToDisk(byte[] byteData)
			{
				if (!Directory.Exists("Screenshots"))
				{
					Directory.CreateDirectory("Screenshots");
				}
				File.WriteAllBytes(string.Format("Screenshots/{0}.png", this.Identifier % 60), byteData);
			}

			// Token: 0x0600020D RID: 525 RVA: 0x00009354 File Offset: 0x00007554
			private void ScreenshotCallback(AsyncGPUReadbackRequest request)
			{
				if (!request.hasError)
				{
					NativeLeakDetection.Mode = NativeLeakDetectionMode.Disabled;
					NativeArray<byte> data = request.GetData<byte>(0);
					NativeArray<byte> nativeArray = new NativeArray<byte>(data, Allocator.Persistent);
					this.Width = request.width;
					this.Height = request.height;
					this.NativeData = nativeArray;
					ThreadPool.QueueUserWorkItem(this.EncodeCallbackDelegate, null);
				}
				else if (this.Callback != null)
				{
					this.Callback(null, this.State);
				}
				if (this.Source != null)
				{
					global::UnityEngine.Object.Destroy(this.Source);
				}
			}

			// Token: 0x040000EB RID: 235
			public WaitCallback EncodeCallbackDelegate;

			// Token: 0x040000EC RID: 236
			public Action<AsyncGPUReadbackRequest> ScreenshotCallbackDelegate;
		}
	}
}
