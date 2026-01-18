using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing;

public class InspectorTestObject : MonoBehaviour
{
	private enum TestEnum
	{
		None = 0,
		A = 1,
		B = 2,
		C = 4,
		D = 8,
		E = 0x10,
		F = 0x20,
		G = 0x40
	}

	[Flags]
	private enum TestEnumFlags
	{
		None = 0,
		A = 1,
		B = 2,
		C = 4,
		D = 8,
		E = 0x10,
		F = 0x20,
		G = 0x40
	}

	[SerializeField]
	private NpcInstanceReference test_npcInstanceReference;

	[SerializeField]
	private InventoryLibraryReference test_inventoryLibraryReference;

	[SerializeField]
	private TestEnum test_enum = TestEnum.A;

	[SerializeField]
	private TestEnumFlags test_enumFlags = TestEnumFlags.A;

	[SerializeField]
	private float test_single;

	[SerializeField]
	private float[] test_single_array = new float[0];

	[SerializeField]
	private List<float> test_single_list = new List<float>();

	[SerializeField]
	private int test_int32;

	[SerializeField]
	private int[] test_int32_array = new int[0];

	[SerializeField]
	private List<int> test_int32_list = new List<int>();

	[SerializeField]
	private bool test_boolean;

	[SerializeField]
	private bool[] test_boolean_array = new bool[0];

	[SerializeField]
	private List<bool> test_boolean_list = new List<bool>();

	[SerializeField]
	private string test_string = string.Empty;

	[SerializeField]
	private string[] test_string_array = new string[0];

	[SerializeField]
	private List<string> test_string_list = new List<string>();

	[SerializeField]
	private Vector2 test_vector2 = Vector2.zero;

	[SerializeField]
	private Vector2[] test_vector2_array = new Vector2[0];

	[SerializeField]
	private List<Vector2> test_vector2_list = new List<Vector2>();

	[SerializeField]
	private Vector3 test_vector3 = Vector3.zero;

	[SerializeField]
	private Vector3[] test_vector3_array = new Vector3[0];

	[SerializeField]
	private List<Vector3> test_vector3_list = new List<Vector3>();

	[SerializeField]
	private Vector4 test_vector4 = Vector4.zero;

	[SerializeField]
	private Vector4[] test_vector4_array = new Vector4[0];

	[SerializeField]
	private List<Vector4> test_vector4_list = new List<Vector4>();

	[SerializeField]
	private Vector2Int test_vector2Int = Vector2Int.zero;

	[SerializeField]
	private Vector2Int[] test_vector2Int_array = new Vector2Int[0];

	[SerializeField]
	private List<Vector2Int> test_vector2Int_list = new List<Vector2Int>();

	[SerializeField]
	private Vector3Int test_vector3Int = Vector3Int.zero;

	[SerializeField]
	private Vector3Int[] test_vector3Int_array = new Vector3Int[0];

	[SerializeField]
	private List<Vector3Int> test_vector3Int_list = new List<Vector3Int>();

	[SerializeField]
	private Quaternion test_quaternion = Quaternion.identity;

	[SerializeField]
	private Quaternion[] test_quaternion_array = new Quaternion[0];

	[SerializeField]
	private List<Quaternion> test_quaternion_list = new List<Quaternion>();

	[SerializeField]
	private LevelDestination test_levelDestination = new LevelDestination();

	[SerializeField]
	private LevelDestination[] test_levelDestination_array = new LevelDestination[0];

	[SerializeField]
	private List<LevelDestination> test_levelDestination_list = new List<LevelDestination>();
}
