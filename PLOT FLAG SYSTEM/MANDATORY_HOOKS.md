# MANDATORY HOOKS - CANNOT BE SKIPPED

## CRITICAL: READ THIS BEFORE EVERY RESPONSE

---

## HOOK A: BEFORE ANY CODE CHANGE

**I MUST answer these questions IN MY RESPONSE before writing/modifying code:**

1. **What class/method am I using?**
   - Answer: [must list specific class and method names]

2. **Have I verified this exists in the DLL?**
   - If YES: Quote the exact signature from ENDSTAR_ASSEMBLY_REFERENCE.md
   - If NO: STOP. Run DLL analysis first.

3. **What are the exact parameter types and return types?**
   - Answer: [must list exact types from DLL analysis]

4. **When is this called in the game lifecycle?**
   - Answer: [must explain based on verified information]

**IF I CANNOT ANSWER ALL 4, I MUST STOP AND ANALYZE DLLs FIRST.**

---

## HOOK B: DLL ANALYSIS REQUIREMENTS

**Before I can claim I "know" something about Endstar, I MUST have:**

1. Run PowerShell reflection on the specific DLL
2. Documented the findings in ENDSTAR_ASSEMBLY_REFERENCE.md
3. Updated ENDSTAR_FLOW_MAP.md if I learned new flow info

**"I think" or "probably" = I DON'T KNOW = MUST ANALYZE FIRST**

---

## HOOK C: AFTER ANY FAILURE

**When code fails, I MUST:**

1. STOP trying different approaches blindly
2. Analyze the EXACT error message
3. Go back to DLLs and find the ACTUAL types/signatures involved
4. Document what I learned in ENDSTAR_ASSEMBLY_REFERENCE.md
5. Only then try again with verified knowledge

**DO NOT: Try 4 different approaches hoping one works**
**DO: Find the actual answer in the DLLs**

## HOOK D: AFTER EACH TEST

**After every test run, I MUST:**

1. Read the full error message carefully
2. Identify which type/method/field caused the error
3. Run DLL analysis to verify the actual types
4. Update documentation before attempting fix

## HOOK E: USE REAL ASSETS ONLY

**I MUST NEVER create stub/prototype objects with missing data.**

When injecting game objects (props, items, etc.):
1. Clone a REAL existing object that has ALL required fields
2. Only modify the fields that need to be different (ID, name)
3. Keep ALL other data intact from the original
4. NEVER create empty/placeholder objects
5. NEVER assume optional fields - if real objects have it, my objects need it

**Stubs will ALWAYS crash the game. Real clones work.**

---

## CURRENT STATUS CHECKLIST

Before I can work on prop injection, I must verify:

- [ ] How is Prop ScriptableObject created? (need to find factory method or constructor in DLL)
- [ ] What fields are required vs optional?
- [ ] How does the game instantiate Props from asset bundles?
- [ ] What is the exact InjectProp method signature in PropLibrary?
- [ ] What does StageManager.InjectProp do internally?

**I CANNOT proceed until these are checked off with DLL evidence.**

---

## ENFORCEMENT

If I skip these hooks, the user should call me out with:
"You skipped the hooks. Go back to DLLs."

I will then STOP and follow the hooks properly.
