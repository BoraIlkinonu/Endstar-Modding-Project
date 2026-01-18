using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing
{
	// Token: 0x020004E3 RID: 1251
	public class InspectorTestObject : MonoBehaviour
	{
		// Token: 0x040017B7 RID: 6071
		[SerializeField]
		private NpcInstanceReference test_npcInstanceReference;

		// Token: 0x040017B8 RID: 6072
		[SerializeField]
		private InventoryLibraryReference test_inventoryLibraryReference;

		// Token: 0x040017B9 RID: 6073
		[SerializeField]
		private InspectorTestObject.TestEnum test_enum = InspectorTestObject.TestEnum.A;

		// Token: 0x040017BA RID: 6074
		[SerializeField]
		private InspectorTestObject.TestEnumFlags test_enumFlags = InspectorTestObject.TestEnumFlags.A;

		// Token: 0x040017BB RID: 6075
		[SerializeField]
		private float test_single;

		// Token: 0x040017BC RID: 6076
		[SerializeField]
		private float[] test_single_array = new float[0];

		// Token: 0x040017BD RID: 6077
		[SerializeField]
		private List<float> test_single_list = new List<float>();

		// Token: 0x040017BE RID: 6078
		[SerializeField]
		private int test_int32;

		// Token: 0x040017BF RID: 6079
		[SerializeField]
		private int[] test_int32_array = new int[0];

		// Token: 0x040017C0 RID: 6080
		[SerializeField]
		private List<int> test_int32_list = new List<int>();

		// Token: 0x040017C1 RID: 6081
		[SerializeField]
		private bool test_boolean;

		// Token: 0x040017C2 RID: 6082
		[SerializeField]
		private bool[] test_boolean_array = new bool[0];

		// Token: 0x040017C3 RID: 6083
		[SerializeField]
		private List<bool> test_boolean_list = new List<bool>();

		// Token: 0x040017C4 RID: 6084
		[SerializeField]
		private string test_string = string.Empty;

		// Token: 0x040017C5 RID: 6085
		[SerializeField]
		private string[] test_string_array = new string[0];

		// Token: 0x040017C6 RID: 6086
		[SerializeField]
		private List<string> test_string_list = new List<string>();

		// Token: 0x040017C7 RID: 6087
		[SerializeField]
		private Vector2 test_vector2 = Vector2.zero;

		// Token: 0x040017C8 RID: 6088
		[SerializeField]
		private Vector2[] test_vector2_array = new Vector2[0];

		// Token: 0x040017C9 RID: 6089
		[SerializeField]
		private List<Vector2> test_vector2_list = new List<Vector2>();

		// Token: 0x040017CA RID: 6090
		[SerializeField]
		private Vector3 test_vector3 = Vector3.zero;

		// Token: 0x040017CB RID: 6091
		[SerializeField]
		private Vector3[] test_vector3_array = new Vector3[0];

		// Token: 0x040017CC RID: 6092
		[SerializeField]
		private List<Vector3> test_vector3_list = new List<Vector3>();

		// Token: 0x040017CD RID: 6093
		[SerializeField]
		private Vector4 test_vector4 = Vector4.zero;

		// Token: 0x040017CE RID: 6094
		[SerializeField]
		private Vector4[] test_vector4_array = new Vector4[0];

		// Token: 0x040017CF RID: 6095
		[SerializeField]
		private List<Vector4> test_vector4_list = new List<Vector4>();

		// Token: 0x040017D0 RID: 6096
		[SerializeField]
		private Vector2Int test_vector2Int = Vector2Int.zero;

		// Token: 0x040017D1 RID: 6097
		[SerializeField]
		private Vector2Int[] test_vector2Int_array = new Vector2Int[0];

		// Token: 0x040017D2 RID: 6098
		[SerializeField]
		private List<Vector2Int> test_vector2Int_list = new List<Vector2Int>();

		// Token: 0x040017D3 RID: 6099
		[SerializeField]
		private Vector3Int test_vector3Int = Vector3Int.zero;

		// Token: 0x040017D4 RID: 6100
		[SerializeField]
		private Vector3Int[] test_vector3Int_array = new Vector3Int[0];

		// Token: 0x040017D5 RID: 6101
		[SerializeField]
		private List<Vector3Int> test_vector3Int_list = new List<Vector3Int>();

		// Token: 0x040017D6 RID: 6102
		[SerializeField]
		private Quaternion test_quaternion = Quaternion.identity;

		// Token: 0x040017D7 RID: 6103
		[SerializeField]
		private Quaternion[] test_quaternion_array = new Quaternion[0];

		// Token: 0x040017D8 RID: 6104
		[SerializeField]
		private List<Quaternion> test_quaternion_list = new List<Quaternion>();

		// Token: 0x040017D9 RID: 6105
		[SerializeField]
		private LevelDestination test_levelDestination = new LevelDestination();

		// Token: 0x040017DA RID: 6106
		[SerializeField]
		private LevelDestination[] test_levelDestination_array = new LevelDestination[0];

		// Token: 0x040017DB RID: 6107
		[SerializeField]
		private List<LevelDestination> test_levelDestination_list = new List<LevelDestination>();

		// Token: 0x020004E4 RID: 1252
		private enum TestEnum
		{
			// Token: 0x040017DD RID: 6109
			None,
			// Token: 0x040017DE RID: 6110
			A,
			// Token: 0x040017DF RID: 6111
			B,
			// Token: 0x040017E0 RID: 6112
			C = 4,
			// Token: 0x040017E1 RID: 6113
			D = 8,
			// Token: 0x040017E2 RID: 6114
			E = 16,
			// Token: 0x040017E3 RID: 6115
			F = 32,
			// Token: 0x040017E4 RID: 6116
			G = 64
		}

		// Token: 0x020004E5 RID: 1253
		[Flags]
		private enum TestEnumFlags
		{
			// Token: 0x040017E6 RID: 6118
			None = 0,
			// Token: 0x040017E7 RID: 6119
			A = 1,
			// Token: 0x040017E8 RID: 6120
			B = 2,
			// Token: 0x040017E9 RID: 6121
			C = 4,
			// Token: 0x040017EA RID: 6122
			D = 8,
			// Token: 0x040017EB RID: 6123
			E = 16,
			// Token: 0x040017EC RID: 6124
			F = 32,
			// Token: 0x040017ED RID: 6125
			G = 64
		}
	}
}
