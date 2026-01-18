using System;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x0200046B RID: 1131
	public class Visuals
	{
		// Token: 0x06001C75 RID: 7285 RVA: 0x0007DB64 File Offset: 0x0007BD64
		internal Visuals(DynamicVisuals visuals)
		{
			this.visuals = visuals;
		}

		// Token: 0x06001C76 RID: 7286 RVA: 0x0007DB73 File Offset: 0x0007BD73
		public void SetLocalPosition(Context instigator, string transformId, global::UnityEngine.Vector3 position)
		{
			this.visuals.SetPositionData(instigator, transformId, position, null);
		}

		// Token: 0x06001C77 RID: 7287 RVA: 0x0007DB84 File Offset: 0x0007BD84
		public void SetLocalPosition(Context instigator, string transformId, global::UnityEngine.Vector3 position, string callbackName)
		{
			this.visuals.SetPositionData(instigator, transformId, position, callbackName);
		}

		// Token: 0x06001C78 RID: 7288 RVA: 0x0007DB96 File Offset: 0x0007BD96
		public void SetLocalPosition(Context instigator, string transformId, global::UnityEngine.Vector3 position, float duration)
		{
			this.visuals.SetPositionData(instigator, transformId, position, duration, null);
		}

		// Token: 0x06001C79 RID: 7289 RVA: 0x0007DBA9 File Offset: 0x0007BDA9
		public void SetLocalPosition(Context instigator, string transformId, global::UnityEngine.Vector3 position, float duration, string callbackName)
		{
			this.visuals.SetPositionData(instigator, transformId, position, duration, callbackName);
		}

		// Token: 0x06001C7A RID: 7290 RVA: 0x0007DBBD File Offset: 0x0007BDBD
		public void SetLocalPositionFromTo(Context instigator, string transformId, global::UnityEngine.Vector3 positionOne, global::UnityEngine.Vector3 positionTwo, float duration)
		{
			this.visuals.SetPositionData(instigator, transformId, positionOne, positionTwo, duration, null);
		}

		// Token: 0x06001C7B RID: 7291 RVA: 0x0007DBD2 File Offset: 0x0007BDD2
		public void SetLocalPositionFromTo(Context instigator, string transformId, global::UnityEngine.Vector3 positionOne, global::UnityEngine.Vector3 positionTwo, float duration, string callbackName)
		{
			this.visuals.SetPositionData(instigator, transformId, positionOne, positionTwo, duration, callbackName);
		}

		// Token: 0x06001C7C RID: 7292 RVA: 0x0007DBE8 File Offset: 0x0007BDE8
		public void SetLocalRotation(Context instigator, string transformId, global::UnityEngine.Vector3 rotation)
		{
			this.visuals.SetRotationData(transformId, rotation, null, null);
		}

		// Token: 0x06001C7D RID: 7293 RVA: 0x0007DBF9 File Offset: 0x0007BDF9
		public void SetLocalRotation(Context instigator, string transformId, global::UnityEngine.Vector3 rotation, string callbackName)
		{
			this.visuals.SetRotationData(transformId, rotation, instigator, callbackName);
		}

		// Token: 0x06001C7E RID: 7294 RVA: 0x0007DC0B File Offset: 0x0007BE0B
		public void SetLocalRotation(Context instigator, string transformId, global::UnityEngine.Vector3 rotation, float duration)
		{
			this.visuals.SetRotationData(transformId, rotation, duration, null, null);
		}

		// Token: 0x06001C7F RID: 7295 RVA: 0x0007DC1E File Offset: 0x0007BE1E
		public void SetLocalRotation(Context instigator, string transformId, global::UnityEngine.Vector3 rotation, float duration, string callbackName)
		{
			this.visuals.SetRotationData(transformId, rotation, duration, instigator, callbackName);
		}

		// Token: 0x06001C80 RID: 7296 RVA: 0x0007DC32 File Offset: 0x0007BE32
		public void SetLocalRotationFromTo(Context instigator, string transformId, global::UnityEngine.Vector3 positionOne, global::UnityEngine.Vector3 positionTwo, float duration)
		{
			this.visuals.SetRotationData(transformId, positionOne, positionTwo, duration, null, null, false);
		}

		// Token: 0x06001C81 RID: 7297 RVA: 0x0007DC48 File Offset: 0x0007BE48
		public void SetLocalRotationFromTo(Context instigator, string transformId, global::UnityEngine.Vector3 positionOne, global::UnityEngine.Vector3 positionTwo, float duration, string callbackName)
		{
			this.visuals.SetRotationData(transformId, positionOne, positionTwo, duration, instigator, callbackName, false);
		}

		// Token: 0x06001C82 RID: 7298 RVA: 0x0007DC5F File Offset: 0x0007BE5F
		public void SetContinousRotation(Context instigator, string transformId, global::UnityEngine.Vector3 rotationRate)
		{
			this.visuals.SetContinuousRotationData(transformId, rotationRate, instigator);
		}

		// Token: 0x06001C83 RID: 7299 RVA: 0x0007DC6F File Offset: 0x0007BE6F
		public void StopContinousRotation(Context instigator, string transformId)
		{
			this.visuals.SetContinuousRotationData(transformId, global::UnityEngine.Vector3.zero, instigator);
		}

		// Token: 0x06001C84 RID: 7300 RVA: 0x0007DC83 File Offset: 0x0007BE83
		public void SetEmissiveColor(Context instigator, string transformId, global::UnityEngine.Color emissiveColor)
		{
			this.visuals.SetEmissionColor(transformId, emissiveColor);
		}

		// Token: 0x06001C85 RID: 7301 RVA: 0x0007DC92 File Offset: 0x0007BE92
		public void SetAlbedoColor(Context instigator, string transformId, global::UnityEngine.Color albedoColor)
		{
			this.visuals.SetAlbedoColor(transformId, albedoColor);
		}

		// Token: 0x06001C86 RID: 7302 RVA: 0x0007DCA1 File Offset: 0x0007BEA1
		public void SetEnabled(Context instigator, string transformId, bool enabled)
		{
			this.visuals.SetEnabled(transformId, enabled);
		}

		// Token: 0x040015CD RID: 5581
		private DynamicVisuals visuals;
	}
}
