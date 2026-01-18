# PROPS.DLL - COMPLETE ANALYSIS
Generated: 01/06/2026 17:28:22

## Prop (ScriptableObject)
Namespace: Endless.Props.Assets
Base: Asset

### Fields
- private Boolean applyXOffset
- private Boolean applyZOffset
- public String AssetID
- public String AssetType
- public String AssetVersion
- private String baseTypeId
- private Vector3Int bounds
- private List`1 componentIds
- public String Description
- public String InternalVersion
- private Int32 iconFileInstanceId
- public String Name
- private Boolean openSource
- private AssetReference prefabBundle
- private PropLocationOffset[] propLocationOffsets
- private StringPairDictionary propMetaData
- private Dictionary`2 remapDictionary
- public RevisionMetaData RevisionMetaData
- private AssetReference scriptAsset
- private List`1 transformRemaps
- private List`1 visualAssets

### Properties
- Boolean ApplyXOffset { get: True; set: True }
- Boolean ApplyZOffset { get: True; set: True }
- String BaseTypeId { get: True; set: False }
- Vector3Int Bounds { get: True; set: False }
- IReadOnlyList`1 ComponentIds { get: True; set: False }
- Boolean HasScript { get: True; set: False }
- Int32 IconFileInstanceId { get: True; set: True }
- Boolean OpenSource { get: True; set: False }
- AssetReference PrefabAsset { get: True; set: False }
- IReadOnlyCollection`1 PropLocationOffsets { get: True; set: False }
- AssetReference ScriptAsset { get: True; set: True }
- List`1 VisualAssets { get: True; set: False }

---

## Script
Namespace: Endless.Props.Assets
Base: Asset

### Fields
- public String AssetID
- public String AssetType
- public String AssetVersion
- private String baseTypeId
- private String body
- private List`1 componentIds
- public String Description
- private List`1 enumTypes
- private List`1 eventOrganizationData
- private List`1 events
- private Boolean hasErrors
- public String InternalVersion
- private List`1 inspectorOrganizationData
- private List`1 inspectorValues
- public String Name
- private Boolean openSource
- private List`1 receiverOrganizationData
- private List`1 receivers
- public RevisionMetaData RevisionMetaData
- private List`1 scriptReferences

---

## All Public Types in Props.dll
- Endless.Props.Assets.AudioAsset (Base: Asset)
- Endless.Props.Assets.AudioCategory (Base: Enum)
- Endless.Props.Assets.EndlessPrefabAsset (Base: Asset)
- Endless.Props.Assets.InspectorOrganizationData (Base: Object)
- Endless.Props.Assets.LocationOffsetState (Base: Enum)
- Endless.Props.Assets.Prop (Base: Asset)
- Endless.Props.Assets.PropLocationOffset (Base: Object)
- Endless.Props.Assets.Script (Base: Asset)
- Endless.Props.Assets.ScriptReference (Base: Object)
- Endless.Props.Assets.StringPair (Base: Object)
- Endless.Props.Assets.StringPairDictionary (Base: ValueType)
- Endless.Props.Assets.WireOrganizationData (Base: Object)
- Endless.Props.ColliderInfo (Base: MonoBehaviour)
- Endless.Props.ColliderInfoValidator (Base: ReferenceBaseValidator)
- Endless.Props.Loaders.PropLoader (Base: Object)
- Endless.Props.ProjectileShooterEjectionSettings (Base: Object)
- Endless.Props.ReferenceComponents.BaseTypeReferences (Base: ReferenceBase)
- Endless.Props.ReferenceComponents.BasicLevelGateReferences (Base: BaseTypeReferences)
- Endless.Props.ReferenceComponents.BouncePadReferences (Base: BaseTypeReferences)
- Endless.Props.ReferenceComponents.BoxColliderValidator (Base: ReferenceBaseValidator)
- Endless.Props.ReferenceComponents.CollectionLengthValidator (Base: ReferenceBaseValidator)
- Endless.Props.ReferenceComponents.ComponentReferences (Base: ReferenceBase)
- Endless.Props.ReferenceComponents.DashPackVisualReferences (Base: ComponentReferences)
- Endless.Props.ReferenceComponents.DestroyableReferences (Base: ComponentReferences)
- Endless.Props.ReferenceComponents.DoorReferences (Base: ReferenceBase)
- Endless.Props.ReferenceComponents.DraggablePhysicsCubeReferences (Base: BaseTypeReferences)
- Endless.Props.ReferenceComponents.HittableReferences (Base: ComponentReferences)
- Endless.Props.ReferenceComponents.InstantPickupReferences (Base: BaseTypeReferences)
- Endless.Props.ReferenceComponents.JetpackVisualReferences (Base: ComponentReferences)
- Endless.Props.ReferenceComponents.KeyReferences (Base: ReferenceBase)
- Endless.Props.ReferenceComponents.LockableComponentReferences (Base: ReferenceBase)
- Endless.Props.ReferenceComponents.MeleeWeaponItemReferences (Base: BaseTypeReferences)
- Endless.Props.ReferenceComponents.NotNullObjectValidator (Base: ReferenceBaseValidator)
- Endless.Props.ReferenceComponents.OneHandedRangedWeaponVisualReferences (Base: ComponentReferences)
- Endless.Props.ReferenceComponents.ProjectileShooterReferences (Base: ComponentReferences)
- Endless.Props.ReferenceComponents.ReferenceBase (Base: MonoBehaviour)
- Endless.Props.ReferenceComponents.ReferenceBaseValidator (Base: Validator)
- Endless.Props.ReferenceComponents.ResizableVolumeReferences (Base: ReferenceBase)
- Endless.Props.ReferenceComponents.ResourcePickupReferences (Base: BaseTypeReferences)
- Endless.Props.ReferenceComponents.SensorReferences (Base: ComponentReferences)
- Endless.Props.ReferenceComponents.SentryReferences (Base: BaseTypeReferences)
- Endless.Props.ReferenceComponents.ShieldComponentReferences (Base: ReferenceBase)
- Endless.Props.ReferenceComponents.SimpleInteractableReferences (Base: ComponentReferences)
- Endless.Props.ReferenceComponents.SpawnPointReferences (Base: BaseTypeReferences)
- Endless.Props.ReferenceComponents.SpikeTrapReferences (Base: BaseTypeReferences)
- Endless.Props.ReferenceComponents.StaticPropReferences (Base: BaseTypeReferences)
- Endless.Props.ReferenceComponents.SwordVisualReferences (Base: ComponentReferences)
- Endless.Props.ReferenceComponents.TargeterReferences (Base: ComponentReferences)
- Endless.Props.ReferenceComponents.TextBubbleReferences (Base: BaseTypeReferences)
- Endless.Props.ReferenceComponents.TextReferences (Base: ComponentReferences)
- Endless.Props.ReferenceComponents.TreasureVisualReferences (Base: ComponentReferences)
- Endless.Props.ReferenceComponents.TriggerComponentReferences (Base: ComponentReferences)
- Endless.Props.ReferenceComponents.TwoHandedRangedWeaponVisualReferences (Base: ComponentReferences)
- Endless.Props.Scripting.ClampValue (Base: Object)
- Endless.Props.Scripting.ComponentListEntry (Base: Object)
- Endless.Props.Scripting.EndlessEventInfo (Base: Object)
- Endless.Props.Scripting.EndlessLuaEvent (Base: Object)
- Endless.Props.Scripting.EndlessParameterInfo (Base: Object)
- Endless.Props.Scripting.EnumEntry (Base: Object)
- Endless.Props.Scripting.InspectorScriptValue (Base: Object)
- Endless.Props.Scripting.SDKComponentList (Base: ScriptableObject)
- Endless.Props.Scripting.WiringReceiver (Base: Object)
- Endless.Props.TransformIdentifier (Base: MonoBehaviour)
- Runtime.Props.Validations.ReadWriteSetValidator (Base: Validator)
