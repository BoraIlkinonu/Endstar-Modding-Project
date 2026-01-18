using System;

namespace Unity.Screenshots
{
	// Token: 0x0200000C RID: 12
	public static class Downsampler
	{
		// Token: 0x06000026 RID: 38 RVA: 0x00002B60 File Offset: 0x00000D60
		public static byte[] Downsample(byte[] dataRgba, int stride, int maximumWidth, int maximumHeight, out int downsampledStride)
		{
			if (stride == 0)
			{
				throw new ArgumentException("The stride must be greater than 0.");
			}
			if (stride % 4 != 0)
			{
				throw new ArgumentException("The stride must be evenly divisible by 4.");
			}
			if (dataRgba == null)
			{
				throw new ArgumentNullException("dataRgba");
			}
			if (dataRgba.Length == 0)
			{
				throw new ArgumentException("The data length must be greater than 0.");
			}
			if (dataRgba.Length % 4 != 0)
			{
				throw new ArgumentException("The data must be evenly divisible by 4.");
			}
			if (dataRgba.Length % stride != 0)
			{
				throw new ArgumentException("The data must be evenly divisible by the stride.");
			}
			int num = stride / 4;
			int num2 = dataRgba.Length / stride;
			float num3 = (float)maximumWidth / (float)num;
			float num4 = (float)maximumHeight / (float)num2;
			float num5 = Math.Min(num3, num4);
			if (num5 < 1f)
			{
				int num6 = (int)Math.Round((double)((float)num * num5));
				int num7 = (int)Math.Round((double)((float)num2 * num5));
				float[] array = new float[num6 * num7 * 4];
				float num8 = (float)num / (float)num6;
				float num9 = (float)num2 / (float)num7;
				int num10 = (int)Math.Floor((double)num8);
				int num11 = (int)Math.Floor((double)num9);
				int num12 = num10 * num11;
				for (int i = 0; i < num7; i++)
				{
					for (int j = 0; j < num6; j++)
					{
						int num13 = i * num6 * 4 + j * 4;
						int num14 = (int)Math.Floor((double)((float)j * num8));
						int num15 = (int)Math.Floor((double)((float)i * num9));
						int num16 = num14 + num10;
						int num17 = num15 + num11;
						for (int k = num15; k < num17; k++)
						{
							if (k < num2)
							{
								for (int l = num14; l < num16; l++)
								{
									if (l < num)
									{
										int num18 = k * num * 4 + l * 4;
										array[num13] += (float)dataRgba[num18];
										array[num13 + 1] += (float)dataRgba[num18 + 1];
										array[num13 + 2] += (float)dataRgba[num18 + 2];
										array[num13 + 3] += (float)dataRgba[num18 + 3];
									}
								}
							}
						}
						array[num13] /= (float)num12;
						array[num13 + 1] /= (float)num12;
						array[num13 + 2] /= (float)num12;
						array[num13 + 3] /= (float)num12;
					}
				}
				byte[] array2 = new byte[num6 * num7 * 4];
				for (int m = 0; m < num7; m++)
				{
					for (int n = 0; n < num6; n++)
					{
						int num19 = (num7 - 1 - m) * num6 * 4 + n * 4;
						int num20 = m * num6 * 4 + n * 4;
						byte[] array3 = array2;
						int num21 = num20;
						array3[num21] += (byte)array[num19];
						byte[] array4 = array2;
						int num22 = num20 + 1;
						array4[num22] += (byte)array[num19 + 1];
						byte[] array5 = array2;
						int num23 = num20 + 2;
						array5[num23] += (byte)array[num19 + 2];
						byte[] array6 = array2;
						int num24 = num20 + 3;
						array6[num24] += (byte)array[num19 + 3];
					}
				}
				downsampledStride = num6 * 4;
				return array2;
			}
			byte[] array7 = new byte[dataRgba.Length];
			for (int num25 = 0; num25 < num2; num25++)
			{
				for (int num26 = 0; num26 < num; num26++)
				{
					int num27 = (num2 - 1 - num25) * num * 4 + num26 * 4;
					int num28 = num25 * num * 4 + num26 * 4;
					byte[] array8 = array7;
					int num29 = num28;
					array8[num29] += dataRgba[num27];
					byte[] array9 = array7;
					int num30 = num28 + 1;
					array9[num30] += dataRgba[num27 + 1];
					byte[] array10 = array7;
					int num31 = num28 + 2;
					array10[num31] += dataRgba[num27 + 2];
					byte[] array11 = array7;
					int num32 = num28 + 3;
					array11[num32] += dataRgba[num27 + 3];
				}
			}
			downsampledStride = num * 4;
			return array7;
		}
	}
}
