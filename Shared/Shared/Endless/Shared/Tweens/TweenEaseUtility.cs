using System;
using UnityEngine;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000B4 RID: 180
	public static class TweenEaseUtility
	{
		// Token: 0x060004F0 RID: 1264 RVA: 0x00015A78 File Offset: 0x00013C78
		public static float Interpolate(TweenEase tweenEase, float start, float end, float interpolation)
		{
			if (tweenEase == TweenEase.Immediate)
			{
				return end;
			}
			if (interpolation <= 0f)
			{
				return start;
			}
			if (interpolation >= 1f)
			{
				return end;
			}
			float num;
			switch (tweenEase)
			{
			case TweenEase.Linear:
				num = TweenEaseUtility.Linear(start, end, interpolation);
				break;
			case TweenEase.Spring:
				num = TweenEaseUtility.Spring(start, end, interpolation);
				break;
			case TweenEase.EaseInQuad:
				num = TweenEaseUtility.EaseInQuad(start, end, interpolation);
				break;
			case TweenEase.EaseOutQuad:
				num = TweenEaseUtility.EaseOutQuad(start, end, interpolation);
				break;
			case TweenEase.EaseInOutQuad:
				num = TweenEaseUtility.EaseInOutQuad(start, end, interpolation);
				break;
			case TweenEase.EaseInCubic:
				num = TweenEaseUtility.EaseInCubic(start, end, interpolation);
				break;
			case TweenEase.EaseOutCubic:
				num = TweenEaseUtility.EaseOutCubic(start, end, interpolation);
				break;
			case TweenEase.EaseInOutCubic:
				num = TweenEaseUtility.EaseInOutCubic(start, end, interpolation);
				break;
			case TweenEase.EaseInQuart:
				num = TweenEaseUtility.EaseInQuart(start, end, interpolation);
				break;
			case TweenEase.EaseOutQuart:
				num = TweenEaseUtility.EaseOutQuart(start, end, interpolation);
				break;
			case TweenEase.EaseInOutQuart:
				num = TweenEaseUtility.EaseInOutQuart(start, end, interpolation);
				break;
			case TweenEase.EaseInQuint:
				num = TweenEaseUtility.EaseInQuint(start, end, interpolation);
				break;
			case TweenEase.EaseOutQuint:
				num = TweenEaseUtility.EaseOutQuint(start, end, interpolation);
				break;
			case TweenEase.EaseInOutQuint:
				num = TweenEaseUtility.EaseInOutQuint(start, end, interpolation);
				break;
			case TweenEase.EaseInSine:
				num = TweenEaseUtility.EaseInSine(start, end, interpolation);
				break;
			case TweenEase.EaseOutSine:
				num = TweenEaseUtility.EaseOutSine(start, end, interpolation);
				break;
			case TweenEase.EaseInOutSine:
				num = TweenEaseUtility.EaseInOutSine(start, end, interpolation);
				break;
			case TweenEase.EaseInExpo:
				num = TweenEaseUtility.EaseInExpo(start, end, interpolation);
				break;
			case TweenEase.EaseOutExpo:
				num = TweenEaseUtility.EaseOutExpo(start, end, interpolation);
				break;
			case TweenEase.EaseInOutExpo:
				num = TweenEaseUtility.EaseInOutExpo(start, end, interpolation);
				break;
			case TweenEase.EaseInCirc:
				num = TweenEaseUtility.EaseInCirc(start, end, interpolation);
				break;
			case TweenEase.EaseOutCirc:
				num = TweenEaseUtility.EaseOutCirc(start, end, interpolation);
				break;
			case TweenEase.EaseInOutCirc:
				num = TweenEaseUtility.EaseInOutCirc(start, end, interpolation);
				break;
			case TweenEase.EaseInBounce:
				num = TweenEaseUtility.EaseInBounce(start, end, interpolation);
				break;
			case TweenEase.EaseOutBounce:
				num = TweenEaseUtility.EaseOutBounce(start, end, interpolation);
				break;
			case TweenEase.EaseInOutBounce:
				num = TweenEaseUtility.EaseInOutBounce(start, end, interpolation);
				break;
			case TweenEase.EaseInBack:
				num = TweenEaseUtility.EaseInBack(start, end, interpolation);
				break;
			case TweenEase.EaseOutBack:
				num = TweenEaseUtility.EaseOutBack(start, end, interpolation);
				break;
			case TweenEase.EaseInOutBack:
				num = TweenEaseUtility.EaseInOutBack(start, end, interpolation);
				break;
			case TweenEase.EaseInElastic:
				num = TweenEaseUtility.EaseInElastic(start, end, interpolation);
				break;
			case TweenEase.EaseOutElastic:
				num = TweenEaseUtility.EaseOutElastic(start, end, interpolation);
				break;
			case TweenEase.EaseInOutElastic:
				num = TweenEaseUtility.EaseInOutElastic(start, end, interpolation);
				break;
			default:
				num = TweenEaseUtility.Linear(start, end, interpolation);
				break;
			}
			return num;
		}

		// Token: 0x060004F1 RID: 1265 RVA: 0x00015CD4 File Offset: 0x00013ED4
		public static float Linear(float start, float end, float interpolation)
		{
			return Mathf.Lerp(start, end, interpolation);
		}

		// Token: 0x060004F2 RID: 1266 RVA: 0x00015CE0 File Offset: 0x00013EE0
		public static float Spring(float start, float end, float interpolation)
		{
			interpolation = Mathf.Clamp01(interpolation);
			interpolation = (Mathf.Sin(interpolation * 3.1415927f * (0.2f + 2.5f * interpolation * interpolation * interpolation)) * Mathf.Pow(1f - interpolation, 2.2f) + interpolation) * (1f + 1.2f * (1f - interpolation));
			return start + (end - start) * interpolation;
		}

		// Token: 0x060004F3 RID: 1267 RVA: 0x00015D44 File Offset: 0x00013F44
		public static float EaseInQuad(float start, float end, float interpolation)
		{
			end -= start;
			return end * interpolation * interpolation + start;
		}

		// Token: 0x060004F4 RID: 1268 RVA: 0x00015D52 File Offset: 0x00013F52
		public static float EaseOutQuad(float start, float end, float interpolation)
		{
			end -= start;
			return -end * interpolation * (interpolation - 2f) + start;
		}

		// Token: 0x060004F5 RID: 1269 RVA: 0x00015D68 File Offset: 0x00013F68
		public static float EaseInOutQuad(float start, float end, float interpolation)
		{
			interpolation /= 0.5f;
			end -= start;
			if (interpolation < 1f)
			{
				return end / 2f * interpolation * interpolation + start;
			}
			interpolation -= 1f;
			return -end / 2f * (interpolation * (interpolation - 2f) - 1f) + start;
		}

		// Token: 0x060004F6 RID: 1270 RVA: 0x00015DBC File Offset: 0x00013FBC
		public static float EaseInCubic(float start, float end, float interpolation)
		{
			end -= start;
			return end * interpolation * interpolation * interpolation + start;
		}

		// Token: 0x060004F7 RID: 1271 RVA: 0x00015DCC File Offset: 0x00013FCC
		public static float EaseOutCubic(float start, float end, float interpolation)
		{
			interpolation -= 1f;
			end -= start;
			return end * (interpolation * interpolation * interpolation + 1f) + start;
		}

		// Token: 0x060004F8 RID: 1272 RVA: 0x00015DEC File Offset: 0x00013FEC
		public static float EaseInOutCubic(float start, float end, float interpolation)
		{
			interpolation /= 0.5f;
			end -= start;
			if (interpolation < 1f)
			{
				return end / 2f * interpolation * interpolation * interpolation + start;
			}
			interpolation -= 2f;
			return end / 2f * (interpolation * interpolation * interpolation + 2f) + start;
		}

		// Token: 0x060004F9 RID: 1273 RVA: 0x00015E3D File Offset: 0x0001403D
		public static float EaseInQuart(float start, float end, float interpolation)
		{
			end -= start;
			return end * interpolation * interpolation * interpolation * interpolation + start;
		}

		// Token: 0x060004FA RID: 1274 RVA: 0x00015E4F File Offset: 0x0001404F
		public static float EaseOutQuart(float start, float end, float interpolation)
		{
			interpolation -= 1f;
			end -= start;
			return -end * (interpolation * interpolation * interpolation * interpolation - 1f) + start;
		}

		// Token: 0x060004FB RID: 1275 RVA: 0x00015E74 File Offset: 0x00014074
		public static float EaseInOutQuart(float start, float end, float interpolation)
		{
			interpolation /= 0.5f;
			end -= start;
			if (interpolation < 1f)
			{
				return end / 2f * interpolation * interpolation * interpolation * interpolation + start;
			}
			interpolation -= 2f;
			return -end / 2f * (interpolation * interpolation * interpolation * interpolation - 2f) + start;
		}

		// Token: 0x060004FC RID: 1276 RVA: 0x00015ECA File Offset: 0x000140CA
		public static float EaseInQuint(float start, float end, float interpolation)
		{
			end -= start;
			return end * interpolation * interpolation * interpolation * interpolation * interpolation + start;
		}

		// Token: 0x060004FD RID: 1277 RVA: 0x00015EDE File Offset: 0x000140DE
		public static float EaseOutQuint(float start, float end, float interpolation)
		{
			interpolation -= 1f;
			end -= start;
			return end * (interpolation * interpolation * interpolation * interpolation * interpolation + 1f) + start;
		}

		// Token: 0x060004FE RID: 1278 RVA: 0x00015F04 File Offset: 0x00014104
		public static float EaseInOutQuint(float start, float end, float interpolation)
		{
			interpolation /= 0.5f;
			end -= start;
			if (interpolation < 1f)
			{
				return end / 2f * interpolation * interpolation * interpolation * interpolation * interpolation + start;
			}
			interpolation -= 2f;
			return end / 2f * (interpolation * interpolation * interpolation * interpolation * interpolation + 2f) + start;
		}

		// Token: 0x060004FF RID: 1279 RVA: 0x00015F5D File Offset: 0x0001415D
		public static float EaseInSine(float start, float end, float interpolation)
		{
			end -= start;
			return -end * Mathf.Cos(interpolation / 1f * 1.5707964f) + end + start;
		}

		// Token: 0x06000500 RID: 1280 RVA: 0x00015F7D File Offset: 0x0001417D
		public static float EaseOutSine(float start, float end, float interpolation)
		{
			end -= start;
			return end * Mathf.Sin(interpolation / 1f * 1.5707964f) + start;
		}

		// Token: 0x06000501 RID: 1281 RVA: 0x00015F9A File Offset: 0x0001419A
		public static float EaseInOutSine(float start, float end, float interpolation)
		{
			end -= start;
			return -end / 2f * (Mathf.Cos(3.1415927f * interpolation / 1f) - 1f) + start;
		}

		// Token: 0x06000502 RID: 1282 RVA: 0x00015FC4 File Offset: 0x000141C4
		public static float EaseInExpo(float start, float end, float interpolation)
		{
			end -= start;
			if (interpolation == 0f)
			{
				return start;
			}
			return end * Mathf.Pow(2f, 10f * (interpolation - 1f)) + start;
		}

		// Token: 0x06000503 RID: 1283 RVA: 0x00015FF0 File Offset: 0x000141F0
		public static float EaseOutExpo(float start, float end, float interpolation)
		{
			end -= start;
			if (Mathf.Approximately(interpolation, 1f))
			{
				return end + start;
			}
			return end * (-Mathf.Pow(2f, -10f * interpolation) + 1f) + start;
		}

		// Token: 0x06000504 RID: 1284 RVA: 0x00016024 File Offset: 0x00014224
		public static float EaseInOutExpo(float start, float end, float interpolation)
		{
			interpolation /= 0.5f;
			end -= start;
			if (interpolation < 1f)
			{
				return end / 2f * Mathf.Pow(2f, 10f * (interpolation - 1f)) + start;
			}
			interpolation -= 1f;
			return end / 2f * (-Mathf.Pow(2f, -10f * interpolation) + 2f) + start;
		}

		// Token: 0x06000505 RID: 1285 RVA: 0x00016094 File Offset: 0x00014294
		public static float EaseInCirc(float start, float end, float interpolation)
		{
			end -= start;
			return -end * (Mathf.Sqrt(1f - interpolation * interpolation) - 1f) + start;
		}

		// Token: 0x06000506 RID: 1286 RVA: 0x000160B4 File Offset: 0x000142B4
		public static float EaseOutCirc(float start, float end, float interpolation)
		{
			interpolation -= 1f;
			end -= start;
			return end * Mathf.Sqrt(1f - interpolation * interpolation) + start;
		}

		// Token: 0x06000507 RID: 1287 RVA: 0x000160D8 File Offset: 0x000142D8
		public static float EaseInOutCirc(float start, float end, float interpolation)
		{
			interpolation /= 0.5f;
			end -= start;
			if (interpolation < 1f)
			{
				return -end / 2f * (Mathf.Sqrt(1f - interpolation * interpolation) - 1f) + start;
			}
			interpolation -= 2f;
			return end / 2f * (Mathf.Sqrt(1f - interpolation * interpolation) + 1f) + start;
		}

		// Token: 0x06000508 RID: 1288 RVA: 0x00016142 File Offset: 0x00014342
		public static float EaseInBounce(float start, float end, float interpolation)
		{
			end -= start;
			return end - TweenEaseUtility.EaseOutBounce(0f, end, 1f - interpolation) + start;
		}

		// Token: 0x06000509 RID: 1289 RVA: 0x00016160 File Offset: 0x00014360
		public static float EaseOutBounce(float start, float end, float interpolation)
		{
			interpolation /= 1f;
			end -= start;
			if (interpolation < 0.36363637f)
			{
				return end * (7.5625f * interpolation * interpolation) + start;
			}
			if (interpolation < 0.72727275f)
			{
				interpolation -= 0.54545456f;
				return end * (7.5625f * interpolation * interpolation + 0.75f) + start;
			}
			if ((double)interpolation < 0.9090909090909091)
			{
				interpolation -= 0.8181818f;
				return end * (7.5625f * interpolation * interpolation + 0.9375f) + start;
			}
			interpolation -= 0.95454544f;
			return end * (7.5625f * interpolation * interpolation + 0.984375f) + start;
		}

		// Token: 0x0600050A RID: 1290 RVA: 0x000161FC File Offset: 0x000143FC
		public static float EaseInOutBounce(float start, float end, float interpolation)
		{
			end -= start;
			if (interpolation < 0.5f)
			{
				return TweenEaseUtility.EaseInBounce(0f, end, interpolation * 2f) * 0.5f + start;
			}
			return TweenEaseUtility.EaseOutBounce(0f, end, interpolation * 2f - 1f) * 0.5f + end * 0.5f + start;
		}

		// Token: 0x0600050B RID: 1291 RVA: 0x00016259 File Offset: 0x00014459
		public static float EaseInBack(float start, float end, float interpolation)
		{
			end -= start;
			interpolation /= 1f;
			return end * interpolation * interpolation * (2.70158f * interpolation - 1.70158f) + start;
		}

		// Token: 0x0600050C RID: 1292 RVA: 0x0001627E File Offset: 0x0001447E
		public static float EaseOutBack(float start, float end, float interpolation)
		{
			end -= start;
			interpolation = interpolation / 1f - 1f;
			return end * (interpolation * interpolation * (2.70158f * interpolation + 1.70158f) + 1f) + start;
		}

		// Token: 0x0600050D RID: 1293 RVA: 0x000162B0 File Offset: 0x000144B0
		public static float EaseInOutBack(float start, float end, float interpolation)
		{
			float num = 1.70158f;
			end -= start;
			interpolation /= 0.5f;
			if (interpolation < 1f)
			{
				num *= 1.525f;
				return end / 2f * (interpolation * interpolation * ((num + 1f) * interpolation - num)) + start;
			}
			interpolation -= 2f;
			num *= 1.525f;
			return end / 2f * (interpolation * interpolation * ((num + 1f) * interpolation + num) + 2f) + start;
		}

		// Token: 0x0600050E RID: 1294 RVA: 0x0001632C File Offset: 0x0001452C
		public static float EaseInElastic(float start, float end, float interpolation)
		{
			end -= start;
			float num = 0f;
			if (interpolation == 0f)
			{
				return start;
			}
			interpolation /= 1f;
			if (Mathf.Approximately(interpolation, 1f))
			{
				return start + end;
			}
			float num2;
			if (num == 0f || num < Mathf.Abs(end))
			{
				num = end;
				num2 = 0.075f;
			}
			else
			{
				num2 = 0.047746483f * Mathf.Asin(end / num);
			}
			interpolation -= 1f;
			return -(num * Mathf.Pow(2f, 10f * interpolation) * Mathf.Sin((interpolation * 1f - num2) * 6.2831855f / 0.3f)) + start;
		}

		// Token: 0x0600050F RID: 1295 RVA: 0x000163D4 File Offset: 0x000145D4
		public static float EaseOutElastic(float start, float end, float interpolation)
		{
			end -= start;
			float num = 0f;
			if (interpolation == 0f)
			{
				return start;
			}
			interpolation /= 1f;
			if (Mathf.Approximately(interpolation, 1f))
			{
				return start + end;
			}
			float num2;
			if (num == 0f || num < Mathf.Abs(end))
			{
				num = end;
				num2 = 0.075f;
			}
			else
			{
				num2 = 0.047746483f * Mathf.Asin(end / num);
			}
			return num * Mathf.Pow(2f, -10f * interpolation) * Mathf.Sin((interpolation * 1f - num2) * 6.2831855f / 0.3f) + end + start;
		}

		// Token: 0x06000510 RID: 1296 RVA: 0x00016474 File Offset: 0x00014674
		public static float EaseInOutElastic(float start, float end, float interpolation)
		{
			end -= start;
			float num = 0f;
			if (interpolation == 0f)
			{
				return start;
			}
			interpolation /= 0.5f;
			if (Mathf.Approximately(interpolation, 2f))
			{
				return start + end;
			}
			float num2;
			if (num == 0f || num < Mathf.Abs(end))
			{
				num = end;
				num2 = 0.075f;
			}
			else
			{
				num2 = 0.047746483f * Mathf.Asin(end / num);
			}
			if (interpolation < 1f)
			{
				interpolation -= 1f;
				return -0.5f * (num * Mathf.Pow(2f, 10f * interpolation) * Mathf.Sin((interpolation * 1f - num2) * 6.2831855f / 0.3f)) + start;
			}
			interpolation -= 1f;
			return num * Mathf.Pow(2f, -10f * interpolation) * Mathf.Sin((interpolation * 1f - num2) * 6.2831855f / 0.3f) * 0.5f + end + start;
		}
	}
}
