# Endstar Modding - Gatekeeper-Controlled Testing

## THE RULE

**You CANNOT launch Endstar. Only the gatekeeper can.**

The gatekeeper only launches after independent validation passes.

---

## ARCHITECTURE

```
┌─────────────────┐         ┌──────────────────┐         ┌─────────────────┐
│   MAIN AGENT    │ ──────► │   GATEKEEPER     │ ◄────── │   VALIDATOR     │
│   (You)         │  file   │   (Separate      │  file   │   (Separate     │
│                 │  write  │    process)      │  write  │    Claude)      │
│   Research      │         │                  │         │                 │
│   Build artifact│         │   Only this can  │         │   Verifies      │
│   Submit        │         │   launch Endstar │         │   your research │
└─────────────────┘         └──────────────────┘         └─────────────────┘
```

---

## WORKFLOW

### Step 1: Start Gatekeeper (User does this)

In a separate terminal:
```bash
python .claude/gatekeeper/gatekeeper.py
```

Leave it running.

### Step 2: Research (You do this)

```bash
# Start research session
python .claude/enforcement/research_capture.py new

# Run PowerShell queries (output captured by system)
python .claude/enforcement/research_capture.py run "<powershell>" "<purpose>"

# Add interpretations
python .claude/enforcement/research_capture.py interpret <id> "<interpretation>"
```

### Step 3: Build Artifact (You do this)

```bash
python .claude/enforcement/artifact_builder.py start
python .claude/enforcement/artifact_builder.py problem '{"statement":"...", "observed_behavior":"...", "expected_behavior":"..."}'
python .claude/enforcement/artifact_builder.py chain '<json>'
python .claude/enforcement/artifact_builder.py hypothesis '{"statement":"...", "depends_on_claims":["step_1"], "testable_prediction":"..."}'
python .claude/enforcement/artifact_builder.py validate
python .claude/enforcement/artifact_builder.py submit
```

### Step 4: Submit to Gatekeeper (You do this)

```bash
python .claude/gatekeeper/submit_research.py .claude/enforcement/research_artifacts/<artifact>.json
```

### Step 5: Validation (Separate Claude session)

User opens NEW terminal, runs `claude` in:
```
.claude/gatekeeper/inbox/validator/
```

Validator reads VALIDATOR_INSTRUCTIONS.md and the artifact, then writes result to:
```
.claude/gatekeeper/outbox/validator/validation_result.json
```

### Step 6: Test Launch (Gatekeeper does this)

If validation passes (PASS + confidence >= 70%), gatekeeper automatically launches Endstar.

You do NOT launch it. You CANNOT launch it.

### Step 7: Post-Mortem (You do this)

After test completes:
```bash
python .claude/gatekeeper/submit_postmortem.py success|failed '<detailed analysis 100+ chars>'
```

### Step 8: Repeat

Back to Step 2 with new research.

---

## CHECK STATUS

```bash
python .claude/gatekeeper/check_status.py
```

---

## WHAT YOU CAN DO

- Run ANY PowerShell, cmd, or bash commands
- Use all shell tools for research, building, testing logic
- Read/write code files
- Build plugins with dotnet, msbuild, etc.

## WHAT YOU CANNOT DO

- Launch Endstar.exe (blocked by enforcer)
- Launch Endless Launcher (blocked by enforcer)

Everything else is allowed. The enforcer only blocks the game executables.

---

## DLL LOCATION

```
C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\
```

---

## WHY THIS EXISTS

This system ensures:
1. Research is validated by an independent agent before testing
2. Only the gatekeeper can launch the game
3. All testing is meaningful, not guesswork
