using System;
using Endless.Assets;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.ParticleSystems.Components
{
	// Token: 0x02000003 RID: 3
	public class SwappableParticleSystem : MonoBehaviour
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000003 RID: 3 RVA: 0x000020BB File Offset: 0x000002BB
		// (set) Token: 0x06000004 RID: 4 RVA: 0x000020C3 File Offset: 0x000002C3
		public bool AutoSpawn { get; private set; } = true;

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000005 RID: 5 RVA: 0x000020CC File Offset: 0x000002CC
		// (set) Token: 0x06000006 RID: 6 RVA: 0x000020D4 File Offset: 0x000002D4
		public ParticleSystem RuntimeParticleSystem { get; set; }

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000007 RID: 7 RVA: 0x000020DD File Offset: 0x000002DD
		public AssetReference ReferencedAsset
		{
			get
			{
				return this.referencedParticleSystemAsset;
			}
		}

		// Token: 0x06000008 RID: 8 RVA: 0x000020E5 File Offset: 0x000002E5
		public bool IsEmbedded()
		{
			return this.embeddedParticleSystem;
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000020F2 File Offset: 0x000002F2
		public bool IsReferencedCloudAsset()
		{
			return this.referencedParticleSystemAsset.AssetID != SerializableGuid.Empty && this.referencedParticleSystemAsset.AssetVersion != string.Empty;
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002127 File Offset: 0x00000327
		public void InitializeWithEmbedded()
		{
			if (!this.embeddedParticleSystem)
			{
				Debug.LogException(new Exception("Expected to initialize swappable particle system with embedded particle system, but no embedded particle system was assigned."));
				return;
			}
			this.RuntimeParticleSystem = this.embeddedParticleSystem;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002152 File Offset: 0x00000352
		public void InitializeWithValue(ParticleSystem particleSystem)
		{
			this.RuntimeParticleSystem = particleSystem;
		}

		// Token: 0x0600000C RID: 12 RVA: 0x0000215C File Offset: 0x0000035C
		public void Spawn(bool attachToTransform = false)
		{
			if (this.RuntimeParticleSystem != null)
			{
				Transform transform = (attachToTransform ? base.transform : null);
				ParticleSystem particleSystem = global::UnityEngine.Object.Instantiate<ParticleSystem>(this.RuntimeParticleSystem, base.transform.position, base.transform.rotation, transform);
				particleSystem.Play();
				global::UnityEngine.Object.Destroy(particleSystem.gameObject, 10f);
			}
		}

		// Token: 0x04000001 RID: 1
		[SerializeField]
		private ParticleSystem embeddedParticleSystem;

		// Token: 0x04000002 RID: 2
		[SerializeField]
		[HideInInspector]
		private AssetReference referencedParticleSystemAsset;
	}
}
