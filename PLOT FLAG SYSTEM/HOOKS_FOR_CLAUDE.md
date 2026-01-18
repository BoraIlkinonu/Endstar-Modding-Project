# MANDATORY HOOKS FOR CLAUDE - READ BEFORE ANY ACTION

## HOOK 1: DLL VERIFICATION BEFORE TESTING

**BEFORE writing or modifying ANY plugin code, you MUST:**

1. Identify which classes/methods you plan to use
2. Go to `ENDSTAR_ASSEMBLY_REFERENCE.md` and verify:
   - The exact class name and namespace
   - The exact method signatures (parameters, return types)
   - The exact field names and types
   - Whether it's static, instance, public, private
3. If the information is NOT in the reference, you MUST run PowerShell analysis FIRST
4. Document any new findings in the reference file

**NEVER assume:**
- Method signatures
- Field names
- How Unity lifecycle works in this specific game
- When/where things get initialized
- What scene names mean

---

## HOOK 2: SYSTEMATIC ASSEMBLY READING

**You MUST read these assemblies systematically:**

### Priority 1 - Core Game Systems
- [ ] Gameplay.dll - StageManager, PropLibrary, RuntimePropInfo
- [ ] Creator.dll - PropTool, CreatorManager, UI classes
- [ ] Props.dll - Prop, EndlessProp, prop definitions

### Priority 2 - Asset Systems
- [ ] Assets.dll - AssetReference, asset loading
- [ ] Addressables related assemblies

### For EACH class, document:
- Full namespace and class name
- Base class / interfaces
- ALL fields (name, type, accessibility)
- ALL properties (name, type, getter/setter)
- ALL methods (name, parameters, return type)
- Events and delegates
- Static vs instance members

---

## HOOK 3: DYNAMIC FLOW MAPPING

**Maintain `ENDSTAR_FLOW_MAP.md` with:**

1. **Initialization Flow** - What loads when, in what order
2. **Scene Flow** - What scenes exist, what triggers transitions
3. **Prop System Flow** - How props get loaded, stored, displayed
4. **UI Flow** - How UI reads data, what triggers updates
5. **Event Flow** - What events exist, when they fire, what listens

**UPDATE this map every time you:**
- Discover new information from DLLs
- See runtime logs that reveal behavior
- Find connections between systems

---

## HOOK 4: ASSUMPTION CHECKLIST

Before ANY code change, answer these:

- [ ] Have I verified this class/method exists in the DLLs?
- [ ] Have I verified the exact method signature?
- [ ] Have I verified when this gets called in the game's lifecycle?
- [ ] Have I checked if there are prerequisites (other systems initialized first)?
- [ ] Have I updated the flow map with my understanding?

If ANY answer is NO, STOP and investigate first.

---

## VIOLATION CONSEQUENCES

If you skip these hooks:
- Your code WILL fail
- You WILL waste the user's time
- You WILL be called out for not learning

FOLLOW THE HOOKS.
