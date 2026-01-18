using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Screenshots
{
	// Token: 0x0200000E RID: 14
	public class ScreenshotManager
	{
		// Token: 0x06000033 RID: 51 RVA: 0x00003335 File Offset: 0x00001535
		public ScreenshotManager()
		{
			this.screenshotRecorder = new ScreenshotRecorder();
			this.screenshotCallbackDelegate = new Action<byte[], object>(this.ScreenshotCallback);
			this.screenshotOperations = new List<ScreenshotManager.ScreenshotOperation>();
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00003368 File Offset: 0x00001568
		private ScreenshotManager.ScreenshotOperation GetScreenshotOperation()
		{
			foreach (ScreenshotManager.ScreenshotOperation screenshotOperation in this.screenshotOperations)
			{
				if (!screenshotOperation.IsInUse)
				{
					screenshotOperation.Use();
					return screenshotOperation;
				}
			}
			ScreenshotManager.ScreenshotOperation screenshotOperation2 = new ScreenshotManager.ScreenshotOperation();
			screenshotOperation2.Use();
			this.screenshotOperations.Add(screenshotOperation2);
			return screenshotOperation2;
		}

		// Token: 0x06000035 RID: 53 RVA: 0x000033E4 File Offset: 0x000015E4
		public void OnEndOfFrame()
		{
			foreach (ScreenshotManager.ScreenshotOperation screenshotOperation in this.screenshotOperations)
			{
				if (screenshotOperation.IsInUse)
				{
					if (screenshotOperation.IsAwaiting)
					{
						screenshotOperation.IsAwaiting = false;
						if (screenshotOperation.Source == null)
						{
							this.screenshotRecorder.Screenshot(screenshotOperation.MaximumWidth, screenshotOperation.MaximumHeight, ScreenshotType.Png, this.screenshotCallbackDelegate, screenshotOperation);
						}
						else if (screenshotOperation.Source is Camera)
						{
							this.screenshotRecorder.Screenshot(screenshotOperation.Source as Camera, screenshotOperation.MaximumWidth, screenshotOperation.MaximumHeight, ScreenshotType.Png, this.screenshotCallbackDelegate, screenshotOperation);
						}
						else if (screenshotOperation.Source is RenderTexture)
						{
							this.screenshotRecorder.Screenshot(screenshotOperation.Source as RenderTexture, screenshotOperation.MaximumWidth, screenshotOperation.MaximumHeight, ScreenshotType.Png, this.screenshotCallbackDelegate, screenshotOperation);
						}
						else if (screenshotOperation.Source is Texture2D)
						{
							this.screenshotRecorder.Screenshot(screenshotOperation.Source as Texture2D, screenshotOperation.MaximumWidth, screenshotOperation.MaximumHeight, ScreenshotType.Png, this.screenshotCallbackDelegate, screenshotOperation);
						}
						else
						{
							this.ScreenshotCallback(null, screenshotOperation);
						}
					}
					else if (screenshotOperation.IsComplete)
					{
						screenshotOperation.IsInUse = false;
						try
						{
							if (screenshotOperation != null && screenshotOperation.Callback != null)
							{
								screenshotOperation.Callback(screenshotOperation.FrameNumber, screenshotOperation.Data);
							}
						}
						catch
						{
						}
					}
				}
			}
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00003594 File Offset: 0x00001794
		private void ScreenshotCallback(byte[] data, object state)
		{
			ScreenshotManager.ScreenshotOperation screenshotOperation = state as ScreenshotManager.ScreenshotOperation;
			if (screenshotOperation != null)
			{
				screenshotOperation.Data = data;
				screenshotOperation.IsComplete = true;
			}
		}

		// Token: 0x06000037 RID: 55 RVA: 0x000035B9 File Offset: 0x000017B9
		public void TakeScreenshot(object source, int frameNumber, int maximumWidth, int maximumHeight, Action<int, byte[]> callback)
		{
			ScreenshotManager.ScreenshotOperation screenshotOperation = this.GetScreenshotOperation();
			screenshotOperation.FrameNumber = frameNumber;
			screenshotOperation.MaximumWidth = maximumWidth;
			screenshotOperation.MaximumHeight = maximumHeight;
			screenshotOperation.Source = source;
			screenshotOperation.Callback = callback;
		}

		// Token: 0x04000027 RID: 39
		private Action<byte[], object> screenshotCallbackDelegate;

		// Token: 0x04000028 RID: 40
		private List<ScreenshotManager.ScreenshotOperation> screenshotOperations;

		// Token: 0x04000029 RID: 41
		private ScreenshotRecorder screenshotRecorder;

		// Token: 0x0200003B RID: 59
		private class ScreenshotOperation
		{
			// Token: 0x17000064 RID: 100
			// (get) Token: 0x060001E0 RID: 480 RVA: 0x000090CC File Offset: 0x000072CC
			// (set) Token: 0x060001E1 RID: 481 RVA: 0x000090D4 File Offset: 0x000072D4
			public Action<int, byte[]> Callback { get; set; }

			// Token: 0x17000065 RID: 101
			// (get) Token: 0x060001E2 RID: 482 RVA: 0x000090DD File Offset: 0x000072DD
			// (set) Token: 0x060001E3 RID: 483 RVA: 0x000090E5 File Offset: 0x000072E5
			public byte[] Data { get; set; }

			// Token: 0x17000066 RID: 102
			// (get) Token: 0x060001E4 RID: 484 RVA: 0x000090EE File Offset: 0x000072EE
			// (set) Token: 0x060001E5 RID: 485 RVA: 0x000090F6 File Offset: 0x000072F6
			public int FrameNumber { get; set; }

			// Token: 0x17000067 RID: 103
			// (get) Token: 0x060001E6 RID: 486 RVA: 0x000090FF File Offset: 0x000072FF
			// (set) Token: 0x060001E7 RID: 487 RVA: 0x00009107 File Offset: 0x00007307
			public bool IsAwaiting { get; set; }

			// Token: 0x17000068 RID: 104
			// (get) Token: 0x060001E8 RID: 488 RVA: 0x00009110 File Offset: 0x00007310
			// (set) Token: 0x060001E9 RID: 489 RVA: 0x00009118 File Offset: 0x00007318
			public bool IsComplete { get; set; }

			// Token: 0x17000069 RID: 105
			// (get) Token: 0x060001EA RID: 490 RVA: 0x00009121 File Offset: 0x00007321
			// (set) Token: 0x060001EB RID: 491 RVA: 0x00009129 File Offset: 0x00007329
			public bool IsInUse { get; set; }

			// Token: 0x1700006A RID: 106
			// (get) Token: 0x060001EC RID: 492 RVA: 0x00009132 File Offset: 0x00007332
			// (set) Token: 0x060001ED RID: 493 RVA: 0x0000913A File Offset: 0x0000733A
			public int MaximumHeight { get; set; }

			// Token: 0x1700006B RID: 107
			// (get) Token: 0x060001EE RID: 494 RVA: 0x00009143 File Offset: 0x00007343
			// (set) Token: 0x060001EF RID: 495 RVA: 0x0000914B File Offset: 0x0000734B
			public int MaximumWidth { get; set; }

			// Token: 0x1700006C RID: 108
			// (get) Token: 0x060001F0 RID: 496 RVA: 0x00009154 File Offset: 0x00007354
			// (set) Token: 0x060001F1 RID: 497 RVA: 0x0000915C File Offset: 0x0000735C
			public object Source { get; set; }

			// Token: 0x060001F2 RID: 498 RVA: 0x00009168 File Offset: 0x00007368
			public void Use()
			{
				this.Callback = null;
				this.Data = null;
				this.FrameNumber = 0;
				this.IsAwaiting = true;
				this.IsComplete = false;
				this.IsInUse = true;
				this.MaximumHeight = 0;
				this.MaximumWidth = 0;
				this.Source = null;
			}
		}
	}
}
