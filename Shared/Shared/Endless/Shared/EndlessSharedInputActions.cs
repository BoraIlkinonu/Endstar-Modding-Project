using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Endless.Shared
{
	// Token: 0x02000063 RID: 99
	public class EndlessSharedInputActions : IInputActionCollection2, IInputActionCollection, IEnumerable<InputAction>, IEnumerable, IDisposable
	{
		// Token: 0x1700006A RID: 106
		// (get) Token: 0x06000300 RID: 768 RVA: 0x0000E9EF File Offset: 0x0000CBEF
		public InputActionAsset asset { get; }

		// Token: 0x06000301 RID: 769 RVA: 0x0000E9F8 File Offset: 0x0000CBF8
		public EndlessSharedInputActions()
		{
			this.asset = InputActionAsset.FromJson("{\n    \"name\": \"EndlessSharedInputActions\",\n    \"maps\": [\n        {\n            \"name\": \"Player\",\n            \"id\": \"d2ba8fbf-edd0-41ec-8e88-0a219637a43e\",\n            \"actions\": [\n                {\n                    \"name\": \"Back\",\n                    \"type\": \"Button\",\n                    \"id\": \"c636514f-82da-4e5b-a2c3-5151da8ea140\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"PrimaryFingerPosition\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"00a6f9d3-8597-4c9f-b31f-26332c61b262\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"SecondaryFingerPosition\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"d2e946be-d0f8-4e6b-90af-626ad723ba92\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"SecondaryTouchContact\",\n                    \"type\": \"Button\",\n                    \"id\": \"05226047-99af-45c2-ad3b-a6557a9bea6f\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"Press\",\n                    \"initialStateCheck\": false\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"\",\n                    \"id\": \"0a653cfc-ec8c-482c-8aa8-d223e7ebeb9d\",\n                    \"path\": \"<Keyboard>/escape\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Back\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"659e2279-ba23-4e46-875e-bd0420aee84f\",\n                    \"path\": \"<Touchscreen>/touch0/position\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Touch\",\n                    \"action\": \"PrimaryFingerPosition\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"a0a2b49e-8ef1-46ee-8202-0938fa452e37\",\n                    \"path\": \"<Touchscreen>/touch1/position\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Touch\",\n                    \"action\": \"SecondaryFingerPosition\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"6494b105-47c1-451b-91ee-3e72c77bd654\",\n                    \"path\": \"<Touchscreen>/touch1/press\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Touch\",\n                    \"action\": \"SecondaryTouchContact\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                }\n            ]\n        },\n        {\n            \"name\": \"UI\",\n            \"id\": \"316b6803-4c95-4df4-a12c-97bd9ca4ec48\",\n            \"actions\": [\n                {\n                    \"name\": \"Navigate\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"eade4651-b411-47f6-99ab-b7211a086b46\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Submit\",\n                    \"type\": \"Button\",\n                    \"id\": \"13470a91-9ec3-4ba8-b620-58d490c164d5\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Cancel\",\n                    \"type\": \"Button\",\n                    \"id\": \"04306deb-6f0f-466f-b59b-261b231b6840\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Point\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"491d06dc-ad52-4e2c-aab6-a32cb2ceb86c\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"Click\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"afea1469-ecd1-4a16-b04c-7a4d8963babd\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"ScrollWheel\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"4b82f306-744f-41e1-8fed-8961818ee249\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"MiddleClick\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"50b6b82e-f009-44e5-9f1f-a425a6fa80de\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"RightClick\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"d3ced0ba-466c-427d-8f79-54643eb266d3\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"TrackedDevicePosition\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"41d0aebc-1148-4ed4-b930-436893454769\",\n                    \"expectedControlType\": \"Vector3\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"TrackedDeviceOrientation\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"70e80fe3-e729-4809-b338-48c0d31cc741\",\n                    \"expectedControlType\": \"Quaternion\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"SelectNext\",\n                    \"type\": \"Button\",\n                    \"id\": \"1ba6d50b-49c2-4fed-b3de-1fbcc18374d4\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"SelectNextUI\",\n                    \"type\": \"Button\",\n                    \"id\": \"78306bab-8cdd-4e93-b3cc-764a507bf08f\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"ToggleDebugWindow\",\n                    \"type\": \"Button\",\n                    \"id\": \"36d97ffe-96a8-4ceb-9093-6dacadd0d3d5\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"Gamepad\",\n                    \"id\": \"809f371f-c5e2-4e7a-83a1-d867598f40dd\",\n                    \"path\": \"2DVector\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"up\",\n                    \"id\": \"14a5d6e8-4aaf-4119-a9ef-34b8c2c548bf\",\n                    \"path\": \"<Gamepad>/leftStick/up\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Gamepad\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"up\",\n                    \"id\": \"9144cbe6-05e1-4687-a6d7-24f99d23dd81\",\n                    \"path\": \"<Gamepad>/rightStick/up\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Gamepad\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"down\",\n                    \"id\": \"2db08d65-c5fb-421b-983f-c71163608d67\",\n                    \"path\": \"<Gamepad>/leftStick/down\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Gamepad\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"down\",\n                    \"id\": \"58748904-2ea9-4a80-8579-b500e6a76df8\",\n                    \"path\": \"<Gamepad>/rightStick/down\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Gamepad\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"left\",\n                    \"id\": \"8ba04515-75aa-45de-966d-393d9bbd1c14\",\n                    \"path\": \"<Gamepad>/leftStick/left\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Gamepad\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"left\",\n                    \"id\": \"712e721c-bdfb-4b23-a86c-a0d9fcfea921\",\n                    \"path\": \"<Gamepad>/rightStick/left\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Gamepad\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"right\",\n                    \"id\": \"fcd248ae-a788-4676-a12e-f4d81205600b\",\n                    \"path\": \"<Gamepad>/leftStick/right\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Gamepad\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"right\",\n                    \"id\": \"1f04d9bc-c50b-41a1-bfcc-afb75475ec20\",\n                    \"path\": \"<Gamepad>/rightStick/right\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Gamepad\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"fb8277d4-c5cd-4663-9dc7-ee3f0b506d90\",\n                    \"path\": \"<Gamepad>/dpad\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Gamepad\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"Joystick\",\n                    \"id\": \"e25d9774-381c-4a61-b47c-7b6b299ad9f9\",\n                    \"path\": \"2DVector\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"up\",\n                    \"id\": \"3db53b26-6601-41be-9887-63ac74e79d19\",\n                    \"path\": \"<Joystick>/stick/up\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Joystick\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"down\",\n                    \"id\": \"0cb3e13e-3d90-4178-8ae6-d9c5501d653f\",\n                    \"path\": \"<Joystick>/stick/down\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Joystick\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"left\",\n                    \"id\": \"0392d399-f6dd-4c82-8062-c1e9c0d34835\",\n                    \"path\": \"<Joystick>/stick/left\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Joystick\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"right\",\n                    \"id\": \"942a66d9-d42f-43d6-8d70-ecb4ba5363bc\",\n                    \"path\": \"<Joystick>/stick/right\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Joystick\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"Keyboard\",\n                    \"id\": \"ff527021-f211-4c02-933e-5976594c46ed\",\n                    \"path\": \"2DVector\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"up\",\n                    \"id\": \"563fbfdd-0f09-408d-aa75-8642c4f08ef0\",\n                    \"path\": \"<Keyboard>/w\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"up\",\n                    \"id\": \"eb480147-c587-4a33-85ed-eb0ab9942c43\",\n                    \"path\": \"<Keyboard>/upArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"down\",\n                    \"id\": \"2bf42165-60bc-42ca-8072-8c13ab40239b\",\n                    \"path\": \"<Keyboard>/s\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"down\",\n                    \"id\": \"85d264ad-e0a0-4565-b7ff-1a37edde51ac\",\n                    \"path\": \"<Keyboard>/downArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"left\",\n                    \"id\": \"74214943-c580-44e4-98eb-ad7eebe17902\",\n                    \"path\": \"<Keyboard>/a\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"left\",\n                    \"id\": \"cea9b045-a000-445b-95b8-0c171af70a3b\",\n                    \"path\": \"<Keyboard>/leftArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"right\",\n                    \"id\": \"8607c725-d935-4808-84b1-8354e29bab63\",\n                    \"path\": \"<Keyboard>/d\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"right\",\n                    \"id\": \"4cda81dc-9edd-4e03-9d7c-a71a14345d0b\",\n                    \"path\": \"<Keyboard>/rightArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"9e92bb26-7e3b-4ec4-b06b-3c8f8e498ddc\",\n                    \"path\": \"*/{Submit}\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse;Gamepad;Touch;Joystick;XR\",\n                    \"action\": \"Submit\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"82627dcc-3b13-4ba9-841d-e4b746d6553e\",\n                    \"path\": \"*/{Cancel}\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse;Gamepad;Touch;Joystick;XR\",\n                    \"action\": \"Cancel\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\"[...string is too long...]");
			this.m_Player = this.asset.FindActionMap("Player", true);
			this.m_Player_Back = this.m_Player.FindAction("Back", true);
			this.m_Player_PrimaryFingerPosition = this.m_Player.FindAction("PrimaryFingerPosition", true);
			this.m_Player_SecondaryFingerPosition = this.m_Player.FindAction("SecondaryFingerPosition", true);
			this.m_Player_SecondaryTouchContact = this.m_Player.FindAction("SecondaryTouchContact", true);
			this.m_UI = this.asset.FindActionMap("UI", true);
			this.m_UI_Navigate = this.m_UI.FindAction("Navigate", true);
			this.m_UI_Submit = this.m_UI.FindAction("Submit", true);
			this.m_UI_Cancel = this.m_UI.FindAction("Cancel", true);
			this.m_UI_Point = this.m_UI.FindAction("Point", true);
			this.m_UI_Click = this.m_UI.FindAction("Click", true);
			this.m_UI_ScrollWheel = this.m_UI.FindAction("ScrollWheel", true);
			this.m_UI_MiddleClick = this.m_UI.FindAction("MiddleClick", true);
			this.m_UI_RightClick = this.m_UI.FindAction("RightClick", true);
			this.m_UI_TrackedDevicePosition = this.m_UI.FindAction("TrackedDevicePosition", true);
			this.m_UI_TrackedDeviceOrientation = this.m_UI.FindAction("TrackedDeviceOrientation", true);
			this.m_UI_SelectNext = this.m_UI.FindAction("SelectNext", true);
			this.m_UI_SelectNextUI = this.m_UI.FindAction("SelectNextUI", true);
			this.m_UI_ToggleDebugWindow = this.m_UI.FindAction("ToggleDebugWindow", true);
		}

		// Token: 0x06000302 RID: 770 RVA: 0x0000EC0C File Offset: 0x0000CE0C
		~EndlessSharedInputActions()
		{
		}

		// Token: 0x06000303 RID: 771 RVA: 0x0000EC34 File Offset: 0x0000CE34
		public void Dispose()
		{
			global::UnityEngine.Object.Destroy(this.asset);
		}

		// Token: 0x1700006B RID: 107
		// (get) Token: 0x06000304 RID: 772 RVA: 0x0000EC41 File Offset: 0x0000CE41
		// (set) Token: 0x06000305 RID: 773 RVA: 0x0000EC4E File Offset: 0x0000CE4E
		public InputBinding? bindingMask
		{
			get
			{
				return this.asset.bindingMask;
			}
			set
			{
				this.asset.bindingMask = value;
			}
		}

		// Token: 0x1700006C RID: 108
		// (get) Token: 0x06000306 RID: 774 RVA: 0x0000EC5C File Offset: 0x0000CE5C
		// (set) Token: 0x06000307 RID: 775 RVA: 0x0000EC69 File Offset: 0x0000CE69
		public ReadOnlyArray<InputDevice>? devices
		{
			get
			{
				return this.asset.devices;
			}
			set
			{
				this.asset.devices = value;
			}
		}

		// Token: 0x1700006D RID: 109
		// (get) Token: 0x06000308 RID: 776 RVA: 0x0000EC77 File Offset: 0x0000CE77
		public ReadOnlyArray<InputControlScheme> controlSchemes
		{
			get
			{
				return this.asset.controlSchemes;
			}
		}

		// Token: 0x06000309 RID: 777 RVA: 0x0000EC84 File Offset: 0x0000CE84
		public bool Contains(InputAction action)
		{
			return this.asset.Contains(action);
		}

		// Token: 0x0600030A RID: 778 RVA: 0x0000EC92 File Offset: 0x0000CE92
		public IEnumerator<InputAction> GetEnumerator()
		{
			return this.asset.GetEnumerator();
		}

		// Token: 0x0600030B RID: 779 RVA: 0x0000EC9F File Offset: 0x0000CE9F
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x0600030C RID: 780 RVA: 0x0000ECA7 File Offset: 0x0000CEA7
		public void Enable()
		{
			this.asset.Enable();
		}

		// Token: 0x0600030D RID: 781 RVA: 0x0000ECB4 File Offset: 0x0000CEB4
		public void Disable()
		{
			this.asset.Disable();
		}

		// Token: 0x1700006E RID: 110
		// (get) Token: 0x0600030E RID: 782 RVA: 0x0000ECC1 File Offset: 0x0000CEC1
		public IEnumerable<InputBinding> bindings
		{
			get
			{
				return this.asset.bindings;
			}
		}

		// Token: 0x0600030F RID: 783 RVA: 0x0000ECCE File Offset: 0x0000CECE
		public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
		{
			return this.asset.FindAction(actionNameOrId, throwIfNotFound);
		}

		// Token: 0x06000310 RID: 784 RVA: 0x0000ECDD File Offset: 0x0000CEDD
		public int FindBinding(InputBinding bindingMask, out InputAction action)
		{
			return this.asset.FindBinding(bindingMask, out action);
		}

		// Token: 0x1700006F RID: 111
		// (get) Token: 0x06000311 RID: 785 RVA: 0x0000ECEC File Offset: 0x0000CEEC
		public EndlessSharedInputActions.PlayerActions Player
		{
			get
			{
				return new EndlessSharedInputActions.PlayerActions(this);
			}
		}

		// Token: 0x17000070 RID: 112
		// (get) Token: 0x06000312 RID: 786 RVA: 0x0000ECF4 File Offset: 0x0000CEF4
		public EndlessSharedInputActions.UIActions UI
		{
			get
			{
				return new EndlessSharedInputActions.UIActions(this);
			}
		}

		// Token: 0x17000071 RID: 113
		// (get) Token: 0x06000313 RID: 787 RVA: 0x0000ECFC File Offset: 0x0000CEFC
		public InputControlScheme KeyboardMouseScheme
		{
			get
			{
				if (this.m_KeyboardMouseSchemeIndex == -1)
				{
					this.m_KeyboardMouseSchemeIndex = this.asset.FindControlSchemeIndex("Keyboard&Mouse");
				}
				return this.asset.controlSchemes[this.m_KeyboardMouseSchemeIndex];
			}
		}

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x06000314 RID: 788 RVA: 0x0000ED44 File Offset: 0x0000CF44
		public InputControlScheme GamepadScheme
		{
			get
			{
				if (this.m_GamepadSchemeIndex == -1)
				{
					this.m_GamepadSchemeIndex = this.asset.FindControlSchemeIndex("Gamepad");
				}
				return this.asset.controlSchemes[this.m_GamepadSchemeIndex];
			}
		}

		// Token: 0x17000073 RID: 115
		// (get) Token: 0x06000315 RID: 789 RVA: 0x0000ED8C File Offset: 0x0000CF8C
		public InputControlScheme TouchScheme
		{
			get
			{
				if (this.m_TouchSchemeIndex == -1)
				{
					this.m_TouchSchemeIndex = this.asset.FindControlSchemeIndex("Touch");
				}
				return this.asset.controlSchemes[this.m_TouchSchemeIndex];
			}
		}

		// Token: 0x17000074 RID: 116
		// (get) Token: 0x06000316 RID: 790 RVA: 0x0000EDD4 File Offset: 0x0000CFD4
		public InputControlScheme JoystickScheme
		{
			get
			{
				if (this.m_JoystickSchemeIndex == -1)
				{
					this.m_JoystickSchemeIndex = this.asset.FindControlSchemeIndex("Joystick");
				}
				return this.asset.controlSchemes[this.m_JoystickSchemeIndex];
			}
		}

		// Token: 0x17000075 RID: 117
		// (get) Token: 0x06000317 RID: 791 RVA: 0x0000EE1C File Offset: 0x0000D01C
		public InputControlScheme XRScheme
		{
			get
			{
				if (this.m_XRSchemeIndex == -1)
				{
					this.m_XRSchemeIndex = this.asset.FindControlSchemeIndex("XR");
				}
				return this.asset.controlSchemes[this.m_XRSchemeIndex];
			}
		}

		// Token: 0x0400017D RID: 381
		private readonly InputActionMap m_Player;

		// Token: 0x0400017E RID: 382
		private List<EndlessSharedInputActions.IPlayerActions> m_PlayerActionsCallbackInterfaces = new List<EndlessSharedInputActions.IPlayerActions>();

		// Token: 0x0400017F RID: 383
		private readonly InputAction m_Player_Back;

		// Token: 0x04000180 RID: 384
		private readonly InputAction m_Player_PrimaryFingerPosition;

		// Token: 0x04000181 RID: 385
		private readonly InputAction m_Player_SecondaryFingerPosition;

		// Token: 0x04000182 RID: 386
		private readonly InputAction m_Player_SecondaryTouchContact;

		// Token: 0x04000183 RID: 387
		private readonly InputActionMap m_UI;

		// Token: 0x04000184 RID: 388
		private List<EndlessSharedInputActions.IUIActions> m_UIActionsCallbackInterfaces = new List<EndlessSharedInputActions.IUIActions>();

		// Token: 0x04000185 RID: 389
		private readonly InputAction m_UI_Navigate;

		// Token: 0x04000186 RID: 390
		private readonly InputAction m_UI_Submit;

		// Token: 0x04000187 RID: 391
		private readonly InputAction m_UI_Cancel;

		// Token: 0x04000188 RID: 392
		private readonly InputAction m_UI_Point;

		// Token: 0x04000189 RID: 393
		private readonly InputAction m_UI_Click;

		// Token: 0x0400018A RID: 394
		private readonly InputAction m_UI_ScrollWheel;

		// Token: 0x0400018B RID: 395
		private readonly InputAction m_UI_MiddleClick;

		// Token: 0x0400018C RID: 396
		private readonly InputAction m_UI_RightClick;

		// Token: 0x0400018D RID: 397
		private readonly InputAction m_UI_TrackedDevicePosition;

		// Token: 0x0400018E RID: 398
		private readonly InputAction m_UI_TrackedDeviceOrientation;

		// Token: 0x0400018F RID: 399
		private readonly InputAction m_UI_SelectNext;

		// Token: 0x04000190 RID: 400
		private readonly InputAction m_UI_SelectNextUI;

		// Token: 0x04000191 RID: 401
		private readonly InputAction m_UI_ToggleDebugWindow;

		// Token: 0x04000192 RID: 402
		private int m_KeyboardMouseSchemeIndex = -1;

		// Token: 0x04000193 RID: 403
		private int m_GamepadSchemeIndex = -1;

		// Token: 0x04000194 RID: 404
		private int m_TouchSchemeIndex = -1;

		// Token: 0x04000195 RID: 405
		private int m_JoystickSchemeIndex = -1;

		// Token: 0x04000196 RID: 406
		private int m_XRSchemeIndex = -1;

		// Token: 0x02000064 RID: 100
		public struct PlayerActions
		{
			// Token: 0x06000318 RID: 792 RVA: 0x0000EE61 File Offset: 0x0000D061
			public PlayerActions(EndlessSharedInputActions wrapper)
			{
				this.m_Wrapper = wrapper;
			}

			// Token: 0x17000076 RID: 118
			// (get) Token: 0x06000319 RID: 793 RVA: 0x0000EE6A File Offset: 0x0000D06A
			public InputAction Back
			{
				get
				{
					return this.m_Wrapper.m_Player_Back;
				}
			}

			// Token: 0x17000077 RID: 119
			// (get) Token: 0x0600031A RID: 794 RVA: 0x0000EE77 File Offset: 0x0000D077
			public InputAction PrimaryFingerPosition
			{
				get
				{
					return this.m_Wrapper.m_Player_PrimaryFingerPosition;
				}
			}

			// Token: 0x17000078 RID: 120
			// (get) Token: 0x0600031B RID: 795 RVA: 0x0000EE84 File Offset: 0x0000D084
			public InputAction SecondaryFingerPosition
			{
				get
				{
					return this.m_Wrapper.m_Player_SecondaryFingerPosition;
				}
			}

			// Token: 0x17000079 RID: 121
			// (get) Token: 0x0600031C RID: 796 RVA: 0x0000EE91 File Offset: 0x0000D091
			public InputAction SecondaryTouchContact
			{
				get
				{
					return this.m_Wrapper.m_Player_SecondaryTouchContact;
				}
			}

			// Token: 0x0600031D RID: 797 RVA: 0x0000EE9E File Offset: 0x0000D09E
			public InputActionMap Get()
			{
				return this.m_Wrapper.m_Player;
			}

			// Token: 0x0600031E RID: 798 RVA: 0x0000EEAB File Offset: 0x0000D0AB
			public void Enable()
			{
				this.Get().Enable();
			}

			// Token: 0x0600031F RID: 799 RVA: 0x0000EEB8 File Offset: 0x0000D0B8
			public void Disable()
			{
				this.Get().Disable();
			}

			// Token: 0x1700007A RID: 122
			// (get) Token: 0x06000320 RID: 800 RVA: 0x0000EEC5 File Offset: 0x0000D0C5
			public bool enabled
			{
				get
				{
					return this.Get().enabled;
				}
			}

			// Token: 0x06000321 RID: 801 RVA: 0x0000EED2 File Offset: 0x0000D0D2
			public static implicit operator InputActionMap(EndlessSharedInputActions.PlayerActions set)
			{
				return set.Get();
			}

			// Token: 0x06000322 RID: 802 RVA: 0x0000EEDC File Offset: 0x0000D0DC
			public void AddCallbacks(EndlessSharedInputActions.IPlayerActions instance)
			{
				if (instance == null || this.m_Wrapper.m_PlayerActionsCallbackInterfaces.Contains(instance))
				{
					return;
				}
				this.m_Wrapper.m_PlayerActionsCallbackInterfaces.Add(instance);
				this.Back.started += instance.OnBack;
				this.Back.performed += instance.OnBack;
				this.Back.canceled += instance.OnBack;
				this.PrimaryFingerPosition.started += instance.OnPrimaryFingerPosition;
				this.PrimaryFingerPosition.performed += instance.OnPrimaryFingerPosition;
				this.PrimaryFingerPosition.canceled += instance.OnPrimaryFingerPosition;
				this.SecondaryFingerPosition.started += instance.OnSecondaryFingerPosition;
				this.SecondaryFingerPosition.performed += instance.OnSecondaryFingerPosition;
				this.SecondaryFingerPosition.canceled += instance.OnSecondaryFingerPosition;
				this.SecondaryTouchContact.started += instance.OnSecondaryTouchContact;
				this.SecondaryTouchContact.performed += instance.OnSecondaryTouchContact;
				this.SecondaryTouchContact.canceled += instance.OnSecondaryTouchContact;
			}

			// Token: 0x06000323 RID: 803 RVA: 0x0000F034 File Offset: 0x0000D234
			private void UnregisterCallbacks(EndlessSharedInputActions.IPlayerActions instance)
			{
				this.Back.started -= instance.OnBack;
				this.Back.performed -= instance.OnBack;
				this.Back.canceled -= instance.OnBack;
				this.PrimaryFingerPosition.started -= instance.OnPrimaryFingerPosition;
				this.PrimaryFingerPosition.performed -= instance.OnPrimaryFingerPosition;
				this.PrimaryFingerPosition.canceled -= instance.OnPrimaryFingerPosition;
				this.SecondaryFingerPosition.started -= instance.OnSecondaryFingerPosition;
				this.SecondaryFingerPosition.performed -= instance.OnSecondaryFingerPosition;
				this.SecondaryFingerPosition.canceled -= instance.OnSecondaryFingerPosition;
				this.SecondaryTouchContact.started -= instance.OnSecondaryTouchContact;
				this.SecondaryTouchContact.performed -= instance.OnSecondaryTouchContact;
				this.SecondaryTouchContact.canceled -= instance.OnSecondaryTouchContact;
			}

			// Token: 0x06000324 RID: 804 RVA: 0x0000F161 File Offset: 0x0000D361
			public void RemoveCallbacks(EndlessSharedInputActions.IPlayerActions instance)
			{
				if (this.m_Wrapper.m_PlayerActionsCallbackInterfaces.Remove(instance))
				{
					this.UnregisterCallbacks(instance);
				}
			}

			// Token: 0x06000325 RID: 805 RVA: 0x0000F180 File Offset: 0x0000D380
			public void SetCallbacks(EndlessSharedInputActions.IPlayerActions instance)
			{
				foreach (EndlessSharedInputActions.IPlayerActions playerActions in this.m_Wrapper.m_PlayerActionsCallbackInterfaces)
				{
					this.UnregisterCallbacks(playerActions);
				}
				this.m_Wrapper.m_PlayerActionsCallbackInterfaces.Clear();
				this.AddCallbacks(instance);
			}

			// Token: 0x04000197 RID: 407
			private EndlessSharedInputActions m_Wrapper;
		}

		// Token: 0x02000065 RID: 101
		public struct UIActions
		{
			// Token: 0x06000326 RID: 806 RVA: 0x0000F1F0 File Offset: 0x0000D3F0
			public UIActions(EndlessSharedInputActions wrapper)
			{
				this.m_Wrapper = wrapper;
			}

			// Token: 0x1700007B RID: 123
			// (get) Token: 0x06000327 RID: 807 RVA: 0x0000F1F9 File Offset: 0x0000D3F9
			public InputAction Navigate
			{
				get
				{
					return this.m_Wrapper.m_UI_Navigate;
				}
			}

			// Token: 0x1700007C RID: 124
			// (get) Token: 0x06000328 RID: 808 RVA: 0x0000F206 File Offset: 0x0000D406
			public InputAction Submit
			{
				get
				{
					return this.m_Wrapper.m_UI_Submit;
				}
			}

			// Token: 0x1700007D RID: 125
			// (get) Token: 0x06000329 RID: 809 RVA: 0x0000F213 File Offset: 0x0000D413
			public InputAction Cancel
			{
				get
				{
					return this.m_Wrapper.m_UI_Cancel;
				}
			}

			// Token: 0x1700007E RID: 126
			// (get) Token: 0x0600032A RID: 810 RVA: 0x0000F220 File Offset: 0x0000D420
			public InputAction Point
			{
				get
				{
					return this.m_Wrapper.m_UI_Point;
				}
			}

			// Token: 0x1700007F RID: 127
			// (get) Token: 0x0600032B RID: 811 RVA: 0x0000F22D File Offset: 0x0000D42D
			public InputAction Click
			{
				get
				{
					return this.m_Wrapper.m_UI_Click;
				}
			}

			// Token: 0x17000080 RID: 128
			// (get) Token: 0x0600032C RID: 812 RVA: 0x0000F23A File Offset: 0x0000D43A
			public InputAction ScrollWheel
			{
				get
				{
					return this.m_Wrapper.m_UI_ScrollWheel;
				}
			}

			// Token: 0x17000081 RID: 129
			// (get) Token: 0x0600032D RID: 813 RVA: 0x0000F247 File Offset: 0x0000D447
			public InputAction MiddleClick
			{
				get
				{
					return this.m_Wrapper.m_UI_MiddleClick;
				}
			}

			// Token: 0x17000082 RID: 130
			// (get) Token: 0x0600032E RID: 814 RVA: 0x0000F254 File Offset: 0x0000D454
			public InputAction RightClick
			{
				get
				{
					return this.m_Wrapper.m_UI_RightClick;
				}
			}

			// Token: 0x17000083 RID: 131
			// (get) Token: 0x0600032F RID: 815 RVA: 0x0000F261 File Offset: 0x0000D461
			public InputAction TrackedDevicePosition
			{
				get
				{
					return this.m_Wrapper.m_UI_TrackedDevicePosition;
				}
			}

			// Token: 0x17000084 RID: 132
			// (get) Token: 0x06000330 RID: 816 RVA: 0x0000F26E File Offset: 0x0000D46E
			public InputAction TrackedDeviceOrientation
			{
				get
				{
					return this.m_Wrapper.m_UI_TrackedDeviceOrientation;
				}
			}

			// Token: 0x17000085 RID: 133
			// (get) Token: 0x06000331 RID: 817 RVA: 0x0000F27B File Offset: 0x0000D47B
			public InputAction SelectNext
			{
				get
				{
					return this.m_Wrapper.m_UI_SelectNext;
				}
			}

			// Token: 0x17000086 RID: 134
			// (get) Token: 0x06000332 RID: 818 RVA: 0x0000F288 File Offset: 0x0000D488
			public InputAction SelectNextUI
			{
				get
				{
					return this.m_Wrapper.m_UI_SelectNextUI;
				}
			}

			// Token: 0x17000087 RID: 135
			// (get) Token: 0x06000333 RID: 819 RVA: 0x0000F295 File Offset: 0x0000D495
			public InputAction ToggleDebugWindow
			{
				get
				{
					return this.m_Wrapper.m_UI_ToggleDebugWindow;
				}
			}

			// Token: 0x06000334 RID: 820 RVA: 0x0000F2A2 File Offset: 0x0000D4A2
			public InputActionMap Get()
			{
				return this.m_Wrapper.m_UI;
			}

			// Token: 0x06000335 RID: 821 RVA: 0x0000F2AF File Offset: 0x0000D4AF
			public void Enable()
			{
				this.Get().Enable();
			}

			// Token: 0x06000336 RID: 822 RVA: 0x0000F2BC File Offset: 0x0000D4BC
			public void Disable()
			{
				this.Get().Disable();
			}

			// Token: 0x17000088 RID: 136
			// (get) Token: 0x06000337 RID: 823 RVA: 0x0000F2C9 File Offset: 0x0000D4C9
			public bool enabled
			{
				get
				{
					return this.Get().enabled;
				}
			}

			// Token: 0x06000338 RID: 824 RVA: 0x0000F2D6 File Offset: 0x0000D4D6
			public static implicit operator InputActionMap(EndlessSharedInputActions.UIActions set)
			{
				return set.Get();
			}

			// Token: 0x06000339 RID: 825 RVA: 0x0000F2E0 File Offset: 0x0000D4E0
			public void AddCallbacks(EndlessSharedInputActions.IUIActions instance)
			{
				if (instance == null || this.m_Wrapper.m_UIActionsCallbackInterfaces.Contains(instance))
				{
					return;
				}
				this.m_Wrapper.m_UIActionsCallbackInterfaces.Add(instance);
				this.Navigate.started += instance.OnNavigate;
				this.Navigate.performed += instance.OnNavigate;
				this.Navigate.canceled += instance.OnNavigate;
				this.Submit.started += instance.OnSubmit;
				this.Submit.performed += instance.OnSubmit;
				this.Submit.canceled += instance.OnSubmit;
				this.Cancel.started += instance.OnCancel;
				this.Cancel.performed += instance.OnCancel;
				this.Cancel.canceled += instance.OnCancel;
				this.Point.started += instance.OnPoint;
				this.Point.performed += instance.OnPoint;
				this.Point.canceled += instance.OnPoint;
				this.Click.started += instance.OnClick;
				this.Click.performed += instance.OnClick;
				this.Click.canceled += instance.OnClick;
				this.ScrollWheel.started += instance.OnScrollWheel;
				this.ScrollWheel.performed += instance.OnScrollWheel;
				this.ScrollWheel.canceled += instance.OnScrollWheel;
				this.MiddleClick.started += instance.OnMiddleClick;
				this.MiddleClick.performed += instance.OnMiddleClick;
				this.MiddleClick.canceled += instance.OnMiddleClick;
				this.RightClick.started += instance.OnRightClick;
				this.RightClick.performed += instance.OnRightClick;
				this.RightClick.canceled += instance.OnRightClick;
				this.TrackedDevicePosition.started += instance.OnTrackedDevicePosition;
				this.TrackedDevicePosition.performed += instance.OnTrackedDevicePosition;
				this.TrackedDevicePosition.canceled += instance.OnTrackedDevicePosition;
				this.TrackedDeviceOrientation.started += instance.OnTrackedDeviceOrientation;
				this.TrackedDeviceOrientation.performed += instance.OnTrackedDeviceOrientation;
				this.TrackedDeviceOrientation.canceled += instance.OnTrackedDeviceOrientation;
				this.SelectNext.started += instance.OnSelectNext;
				this.SelectNext.performed += instance.OnSelectNext;
				this.SelectNext.canceled += instance.OnSelectNext;
				this.SelectNextUI.started += instance.OnSelectNextUI;
				this.SelectNextUI.performed += instance.OnSelectNextUI;
				this.SelectNextUI.canceled += instance.OnSelectNextUI;
				this.ToggleDebugWindow.started += instance.OnToggleDebugWindow;
				this.ToggleDebugWindow.performed += instance.OnToggleDebugWindow;
				this.ToggleDebugWindow.canceled += instance.OnToggleDebugWindow;
			}

			// Token: 0x0600033A RID: 826 RVA: 0x0000F6C0 File Offset: 0x0000D8C0
			private void UnregisterCallbacks(EndlessSharedInputActions.IUIActions instance)
			{
				this.Navigate.started -= instance.OnNavigate;
				this.Navigate.performed -= instance.OnNavigate;
				this.Navigate.canceled -= instance.OnNavigate;
				this.Submit.started -= instance.OnSubmit;
				this.Submit.performed -= instance.OnSubmit;
				this.Submit.canceled -= instance.OnSubmit;
				this.Cancel.started -= instance.OnCancel;
				this.Cancel.performed -= instance.OnCancel;
				this.Cancel.canceled -= instance.OnCancel;
				this.Point.started -= instance.OnPoint;
				this.Point.performed -= instance.OnPoint;
				this.Point.canceled -= instance.OnPoint;
				this.Click.started -= instance.OnClick;
				this.Click.performed -= instance.OnClick;
				this.Click.canceled -= instance.OnClick;
				this.ScrollWheel.started -= instance.OnScrollWheel;
				this.ScrollWheel.performed -= instance.OnScrollWheel;
				this.ScrollWheel.canceled -= instance.OnScrollWheel;
				this.MiddleClick.started -= instance.OnMiddleClick;
				this.MiddleClick.performed -= instance.OnMiddleClick;
				this.MiddleClick.canceled -= instance.OnMiddleClick;
				this.RightClick.started -= instance.OnRightClick;
				this.RightClick.performed -= instance.OnRightClick;
				this.RightClick.canceled -= instance.OnRightClick;
				this.TrackedDevicePosition.started -= instance.OnTrackedDevicePosition;
				this.TrackedDevicePosition.performed -= instance.OnTrackedDevicePosition;
				this.TrackedDevicePosition.canceled -= instance.OnTrackedDevicePosition;
				this.TrackedDeviceOrientation.started -= instance.OnTrackedDeviceOrientation;
				this.TrackedDeviceOrientation.performed -= instance.OnTrackedDeviceOrientation;
				this.TrackedDeviceOrientation.canceled -= instance.OnTrackedDeviceOrientation;
				this.SelectNext.started -= instance.OnSelectNext;
				this.SelectNext.performed -= instance.OnSelectNext;
				this.SelectNext.canceled -= instance.OnSelectNext;
				this.SelectNextUI.started -= instance.OnSelectNextUI;
				this.SelectNextUI.performed -= instance.OnSelectNextUI;
				this.SelectNextUI.canceled -= instance.OnSelectNextUI;
				this.ToggleDebugWindow.started -= instance.OnToggleDebugWindow;
				this.ToggleDebugWindow.performed -= instance.OnToggleDebugWindow;
				this.ToggleDebugWindow.canceled -= instance.OnToggleDebugWindow;
			}

			// Token: 0x0600033B RID: 827 RVA: 0x0000FA75 File Offset: 0x0000DC75
			public void RemoveCallbacks(EndlessSharedInputActions.IUIActions instance)
			{
				if (this.m_Wrapper.m_UIActionsCallbackInterfaces.Remove(instance))
				{
					this.UnregisterCallbacks(instance);
				}
			}

			// Token: 0x0600033C RID: 828 RVA: 0x0000FA94 File Offset: 0x0000DC94
			public void SetCallbacks(EndlessSharedInputActions.IUIActions instance)
			{
				foreach (EndlessSharedInputActions.IUIActions iuiactions in this.m_Wrapper.m_UIActionsCallbackInterfaces)
				{
					this.UnregisterCallbacks(iuiactions);
				}
				this.m_Wrapper.m_UIActionsCallbackInterfaces.Clear();
				this.AddCallbacks(instance);
			}

			// Token: 0x04000198 RID: 408
			private EndlessSharedInputActions m_Wrapper;
		}

		// Token: 0x02000066 RID: 102
		public interface IPlayerActions
		{
			// Token: 0x0600033D RID: 829
			void OnBack(InputAction.CallbackContext context);

			// Token: 0x0600033E RID: 830
			void OnPrimaryFingerPosition(InputAction.CallbackContext context);

			// Token: 0x0600033F RID: 831
			void OnSecondaryFingerPosition(InputAction.CallbackContext context);

			// Token: 0x06000340 RID: 832
			void OnSecondaryTouchContact(InputAction.CallbackContext context);
		}

		// Token: 0x02000067 RID: 103
		public interface IUIActions
		{
			// Token: 0x06000341 RID: 833
			void OnNavigate(InputAction.CallbackContext context);

			// Token: 0x06000342 RID: 834
			void OnSubmit(InputAction.CallbackContext context);

			// Token: 0x06000343 RID: 835
			void OnCancel(InputAction.CallbackContext context);

			// Token: 0x06000344 RID: 836
			void OnPoint(InputAction.CallbackContext context);

			// Token: 0x06000345 RID: 837
			void OnClick(InputAction.CallbackContext context);

			// Token: 0x06000346 RID: 838
			void OnScrollWheel(InputAction.CallbackContext context);

			// Token: 0x06000347 RID: 839
			void OnMiddleClick(InputAction.CallbackContext context);

			// Token: 0x06000348 RID: 840
			void OnRightClick(InputAction.CallbackContext context);

			// Token: 0x06000349 RID: 841
			void OnTrackedDevicePosition(InputAction.CallbackContext context);

			// Token: 0x0600034A RID: 842
			void OnTrackedDeviceOrientation(InputAction.CallbackContext context);

			// Token: 0x0600034B RID: 843
			void OnSelectNext(InputAction.CallbackContext context);

			// Token: 0x0600034C RID: 844
			void OnSelectNextUI(InputAction.CallbackContext context);

			// Token: 0x0600034D RID: 845
			void OnToggleDebugWindow(InputAction.CallbackContext context);
		}
	}
}
