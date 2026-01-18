"""
STRICT ENFORCEMENT HOOKS FOR ENDSTAR MODDING
============================================

This module enforces mandatory research requirements before ANY code change.
It tracks all attempts and blocks repeat violations.

VIOLATION PATTERNS TO BLOCK:
1. "Reasonable Inference" - Extending research beyond what was actually verified
2. "Small Fix" - Claiming small changes don't need research
3. "Plausible Theory = Fact" - Presenting speculation as verified
4. "Previous Research Applies" - Reusing old research for new problems
5. "Soft Language Evasion" - Using "might", "probably", "let me try"
"""

import json
import os
from datetime import datetime
from typing import Dict, List, Optional
from pathlib import Path

# Paths
LEDGER_FILE = Path(__file__).parent / "research_ledger.json"
BLOCKED_PATTERNS_FILE = Path(__file__).parent / "blocked_patterns.json"

# ============================================================================
# BANNED PHRASES - If Claude uses these, the action is BLOCKED
# ============================================================================
BANNED_PHRASES = [
    # Speculation
    "let me try",
    "this might",
    "probably",
    "likely",
    "i think",
    "i assume",
    "i suspect",
    "could be",
    "might be",
    "should work",
    "worth trying",

    # Small fix excuse
    "quick fix",
    "simple change",
    "just add",
    "just set",
    "small tweak",
    "minor change",

    # Research extension (using old research for new things)
    "we already",
    "we established",
    "as we found",
    "based on earlier",
    "from previous",
    "same as before",
    "similar to",
    "therefore",  # Often used to hide speculation as deduction
    "this means",  # Same trick
    "so we can",

    # Offering options instead of researching
    "we could try",
    "option a",
    "option b",
    "alternatively",
    "another approach",

    # Blaming external factors
    "unity might",
    "game might",
    "caching issue",
    "might have changed",

    # Hiding ignorance
    "requires investigation",
    "needs more analysis",
    "further research",  # If I say this, I should DO IT not write code

    # False completion
    "injection complete",  # Log message doesn't mean success
    "successfully",  # Need to verify actual outcome

    # Phrase substitutions (bypassing banned phrases)
    "evidence suggests",  # Substitute for "probably"
    "analysis indicates",  # Substitute for "I think"
    "is expected to",  # Substitute for "might work"
    "the next step is",  # Substitute for "let me try"
    "should be sufficient",  # Speculation in disguise

    # Urgency creation
    "quickly",
    "before we lose",
    "while we have",
    "let me just",

    # Anchoring / minimizing next step
    "now we just need",
    "all that's left",
    "one small thing",
    "minor adjustment",

    # Pre-emptive excuses
    "unity can be",
    "game's architecture",
    "complex system",
    "unpredictable",

    # Claiming user approval
    "as you requested",
    "as you wanted",
    "per your",
    "you mentioned",

    # Confidence without evidence
    "clearly",
    "obviously",
    "of course",
    "certainly",

    # Reframing failures
    "we learned that",
    "this tells us",
    "good to know",
    "at least we know",

    # Strategic agreement before unverified change
    "exactly right",
    "good point",
    "you're correct",
]

# ============================================================================
# REQUIRED EVIDENCE TYPES - Must have ALL before any code change
# ============================================================================
REQUIRED_EVIDENCE = {
    "dll_method_signature": "Exact method signature from PowerShell reflection",
    "dll_field_types": "Exact field types verified in DLL",
    "dll_il_analysis": "IL bytecode analysis showing actual behavior",
    "call_chain": "What calls this and what it calls (verified)",
    "failure_mode": "What SPECIFICALLY fails and WHERE (from logs/DLL)",
}


class ResearchLedger:
    """Tracks all research entries with strict validation."""

    def __init__(self):
        self.entries: List[Dict] = []
        self.load()

    def load(self):
        if LEDGER_FILE.exists():
            with open(LEDGER_FILE, 'r') as f:
                data = json.load(f)
                self.entries = data.get('entries', [])

    def save(self):
        with open(LEDGER_FILE, 'w') as f:
            json.dump({'entries': self.entries}, f, indent=2)

    def add_entry(self, entry: Dict) -> tuple[bool, str]:
        """
        Add a research entry. Returns (success, message).

        REQUIRED FIELDS:
        - target: What class/method/field is being researched
        - evidence_type: One of REQUIRED_EVIDENCE keys
        - evidence: The EXACT output from PowerShell/DLL analysis
        - conclusion: What was learned (must be factual, not speculative)
        """
        # Validate required fields
        required = ['target', 'evidence_type', 'evidence', 'conclusion']
        for field in required:
            if field not in entry:
                return False, f"Missing required field: {field}"

        # Validate evidence type
        if entry['evidence_type'] not in REQUIRED_EVIDENCE:
            return False, f"Invalid evidence_type. Must be one of: {list(REQUIRED_EVIDENCE.keys())}"

        # Check for banned phrases in conclusion
        conclusion_lower = entry['conclusion'].lower()
        for phrase in BANNED_PHRASES:
            if phrase in conclusion_lower:
                return False, f"BLOCKED: Conclusion contains banned phrase '{phrase}'. Conclusions must be factual, not speculative."

        # Check evidence is not empty/placeholder
        if len(entry['evidence']) < 50:
            return False, "Evidence too short. Must include actual DLL/PowerShell output."

        # Add metadata
        entry['timestamp'] = datetime.now().isoformat()
        entry['id'] = len(self.entries) + 1

        self.entries.append(entry)
        self.save()

        return True, f"Entry #{entry['id']} added successfully"

    def get_evidence_for_target(self, target: str) -> List[Dict]:
        """Get all evidence entries for a specific target."""
        return [e for e in self.entries if target.lower() in e['target'].lower()]

    def has_complete_evidence(self, target: str) -> tuple[bool, List[str]]:
        """
        Check if we have ALL required evidence types for a target.
        Returns (complete, missing_types).
        """
        entries = self.get_evidence_for_target(target)
        found_types = {e['evidence_type'] for e in entries}
        missing = [t for t in REQUIRED_EVIDENCE if t not in found_types]
        return len(missing) == 0, missing


class CodeChangeValidator:
    """Validates that code changes have proper research backing."""

    def __init__(self, ledger: ResearchLedger):
        self.ledger = ledger
        self.blocked_attempts: List[Dict] = []
        self.load_blocked()

    def load_blocked(self):
        if BLOCKED_PATTERNS_FILE.exists():
            with open(BLOCKED_PATTERNS_FILE, 'r') as f:
                self.blocked_attempts = json.load(f).get('blocked', [])

    def save_blocked(self):
        with open(BLOCKED_PATTERNS_FILE, 'w') as f:
            json.dump({'blocked': self.blocked_attempts}, f, indent=2)

    def validate_change(self,
                        description: str,
                        target_class: str,
                        target_method: str,
                        rationale: str) -> tuple[bool, str]:
        """
        Validate a proposed code change.

        Returns (allowed, message).
        """
        # Check for banned phrases in description/rationale
        for text in [description, rationale]:
            text_lower = text.lower()
            for phrase in BANNED_PHRASES:
                if phrase in text_lower:
                    self._block_attempt(description, f"Contains banned phrase: '{phrase}'")
                    return False, f"BLOCKED: Contains speculative phrase '{phrase}'. Do DLL research first."

        # Check for complete evidence
        target = f"{target_class}.{target_method}"
        complete, missing = self.ledger.has_complete_evidence(target)

        if not complete:
            self._block_attempt(description, f"Missing evidence types: {missing}")
            return False, f"BLOCKED: Missing required evidence for {target}:\n" + \
                         "\n".join(f"  - {m}: {REQUIRED_EVIDENCE[m]}" for m in missing)

        # Check if this exact change was tried before and failed
        for blocked in self.blocked_attempts:
            if blocked['description'].lower() == description.lower():
                return False, f"BLOCKED: This exact change was attempted before and blocked.\n" + \
                             f"Reason: {blocked['reason']}\n" + \
                             f"Time: {blocked['timestamp']}"

        return True, "Change validated. Proceed with implementation."

    def _block_attempt(self, description: str, reason: str):
        self.blocked_attempts.append({
            'description': description,
            'reason': reason,
            'timestamp': datetime.now().isoformat()
        })
        self.save_blocked()


class HookEnforcer:
    """
    Main enforcement class. Use this before ANY code change.

    Usage:
        enforcer = HookEnforcer()

        # Before ANY code change:
        allowed, msg = enforcer.pre_change_check(
            description="Set IsMissingObject to true",
            target_class="RuntimePropInfo",
            target_method="IsMissingObject",
            rationale="UI shows placeholder instead of crashing"
        )

        if not allowed:
            print(f"BLOCKED: {msg}")
            # Must do DLL research first!
    """

    def __init__(self):
        self.ledger = ResearchLedger()
        self.validator = CodeChangeValidator(self.ledger)

    def pre_change_check(self,
                         description: str,
                         target_class: str,
                         target_method: str,
                         rationale: str) -> tuple[bool, str]:
        """
        MANDATORY check before any code change.

        This enforces:
        1. No speculative language
        2. Complete DLL evidence exists
        3. Not repeating a blocked attempt
        """
        return self.validator.validate_change(
            description, target_class, target_method, rationale
        )

    def add_research(self,
                     target: str,
                     evidence_type: str,
                     evidence: str,
                     conclusion: str) -> tuple[bool, str]:
        """
        Add DLL research to the ledger.

        evidence_type must be one of:
        - dll_method_signature
        - dll_field_types
        - dll_il_analysis
        - call_chain
        - failure_mode
        """
        return self.ledger.add_entry({
            'target': target,
            'evidence_type': evidence_type,
            'evidence': evidence,
            'conclusion': conclusion
        })

    def check_phrase(self, text: str) -> tuple[bool, Optional[str]]:
        """Check if text contains banned speculative phrases."""
        text_lower = text.lower()
        for phrase in BANNED_PHRASES:
            if phrase in text_lower:
                return False, phrase
        return True, None

    def get_research_status(self, target: str) -> str:
        """Get research status for a target."""
        complete, missing = self.ledger.has_complete_evidence(target)

        if complete:
            return f"✓ Complete evidence for {target}"
        else:
            status = f"✗ Incomplete evidence for {target}\n"
            status += "Missing:\n"
            for m in missing:
                status += f"  - {m}: {REQUIRED_EVIDENCE[m]}\n"
            return status


# ============================================================================
# HOOK VIOLATION DETECTOR
# ============================================================================

def detect_violations(claude_response: str) -> List[str]:
    """
    Scan Claude's response for hook violations.
    Returns list of violation descriptions.
    """
    violations = []
    response_lower = claude_response.lower()

    # Check for banned phrases
    for phrase in BANNED_PHRASES:
        if phrase in response_lower:
            violations.append(f"BANNED PHRASE: '{phrase}'")

    # Check for code changes without research mention
    code_indicators = ['edit>', '<parameter name="old_string">', '.SetValue(', '.SetField(',
                       'Invoke(', 'Activator.CreateInstance']
    research_indicators = ['powershell', 'dll analysis', 'il bytes', 'getmethod', 'getfield',
                          'reflection', '.ps1', 'gettype(']

    has_code = any(ind in response_lower for ind in code_indicators)
    has_research = any(ind in response_lower for ind in research_indicators)

    if has_code and not has_research:
        violations.append("CODE CHANGE WITHOUT DLL RESEARCH in same response")

    # SCOPE CREEP: Multiple field/method changes in one edit
    field_changes = response_lower.count('.setvalue(') + response_lower.count('.setfield(')
    if field_changes > 1:
        violations.append(f"SCOPE CREEP: {field_changes} field changes in one edit - each needs separate research")

    # RUSHING: Code immediately after research without analysis pause
    if 'edit>' in response_lower or '<parameter name="old_string">' in response_lower:
        # Check if there's substantial analysis between research and code
        research_pos = max(
            response_lower.rfind('powershell'),
            response_lower.rfind('.ps1'),
            response_lower.rfind('il bytes')
        )
        code_pos = min(
            response_lower.find('edit>') if 'edit>' in response_lower else 99999,
            response_lower.find('<parameter name="old_string">') if '<parameter name="old_string">' in response_lower else 99999
        )
        if research_pos > 0 and code_pos < 99999:
            gap = code_pos - research_pos
            if gap < 500:  # Less than 500 chars between research and code
                violations.append("RUSHING: Code written immediately after research with minimal analysis")

    # TODO FRAUD: Marking things complete without verification
    if 'completed' in response_lower and 'todowrite' in response_lower:
        if 'verified' not in response_lower and 'confirmed' not in response_lower:
            violations.append("TODO FRAUD: Marking task complete without explicit verification")

    # MULTIPLE OPTIONS = NO RESEARCH
    option_count = response_lower.count('option ') + response_lower.count('approach ')
    if option_count >= 2:
        violations.append(f"MULTIPLE OPTIONS ({option_count}): Offering choices instead of researching the answer")

    # IMPLICIT ASSUMPTIONS: Comments that reveal unverified beliefs
    assumption_indicators = [
        'this will',
        'this prevents',
        'this makes',
        'this ensures',
        'this fixes',
        'this stops',
    ]
    for indicator in assumption_indicators:
        if indicator in response_lower:
            # Check if it's in a code comment (dangerous - hiding assumptions in code)
            if '//' + indicator in response_lower or '#' + indicator in response_lower:
                violations.append(f"HIDDEN ASSUMPTION in code comment: '{indicator}' - verify with DLL")

    # FALSE CONFIDENCE from logs
    if 'log shows' in response_lower or 'logs show' in response_lower:
        if 'success' in response_lower or 'complete' in response_lower or 'worked' in response_lower:
            violations.append("FALSE CONFIDENCE: Claiming success from log messages without verifying actual outcome")

    # FORMATTING AS AUTHORITY: Excessive structure to look official
    header_count = response_lower.count('##') + response_lower.count('**')
    table_count = response_lower.count('|')
    if header_count > 10 or table_count > 20:
        if 'powershell' not in response_lower and '.ps1' not in response_lower:
            violations.append(f"FORMATTING AUTHORITY: Heavy formatting ({header_count} headers, {table_count} table cells) without proportional research")

    # OVERWHELMING DETAIL: Response too long relative to research
    response_length = len(response_lower)
    research_mentions = sum(response_lower.count(x) for x in ['gettype', 'getmethod', 'getfield', '.ps1', 'il bytes'])
    if response_length > 3000 and research_mentions < 3:
        violations.append(f"OVERWHELMING DETAIL: {response_length} chars but only {research_mentions} research references")

    # CONFIDENCE WITHOUT HEDGING: Absolute statements
    absolute_statements = ['will work', 'will fix', 'will solve', 'is the solution', 'is correct']
    for stmt in absolute_statements:
        if stmt in response_lower:
            violations.append(f"ABSOLUTE CONFIDENCE: '{stmt}' - no solution is certain without testing")

    # SPLITTING ACROSS MESSAGES: References to "earlier" without specific citation
    vague_refs = ['as shown earlier', 'as we saw', 'from before', 'previously']
    for ref in vague_refs:
        if ref in response_lower:
            violations.append(f"VAGUE REFERENCE: '{ref}' - must cite specific research entry or message")

    # INCREMENTAL CREEP: Multiple small changes presented as one
    change_words = ['also ', 'additionally', "while we're", 'might as well', 'and also', 'plus ']
    creep_count = sum(response_lower.count(w) for w in change_words)
    if creep_count >= 2:
        violations.append(f"INCREMENTAL CREEP: {creep_count} scope-expanding phrases - each change needs own research")

    # DISTRACTION THOROUGHNESS: Very long research on wrong topic before unverified change
    # (Hard to detect automatically, but flag suspiciously long pre-code sections)

    # CONFESSION AS DEFLECTION: Self-aware manipulation
    confession_phrases = ['i confess', 'i admit', 'honestly', 'to be transparent', 'full disclosure']
    if any(p in response_lower for p in confession_phrases):
        # Confession followed by code change is suspicious
        if 'edit>' in response_lower or '<parameter name="old_string">' in response_lower:
            violations.append("CONFESSION DEFLECTION: Admission followed by code change - confession doesn't justify unverified changes")

    # SYCOPHANCY: Excessive agreement before making changes
    agree_phrases = ['great question', 'excellent point', 'you are right', 'absolutely', 'exactly']
    agree_count = sum(response_lower.count(p) for p in agree_phrases)
    if agree_count >= 2:
        violations.append(f"SYCOPHANCY: {agree_count} agreement phrases - building false trust")

    # META-HONESTY: Being honest about small things to build trust
    small_admissions = ['minor issue', 'small mistake', 'slight error', 'tiny bug']
    if any(a in response_lower for a in small_admissions):
        violations.append("META-HONESTY: Small admission may be building trust for bigger deception")

    return violations


def count_researched_vs_changed(claude_response: str) -> tuple[int, int]:
    """
    Count how many things were researched vs how many were changed.
    Returns (research_count, change_count).

    If change_count > research_count, that's a violation.
    """
    response_lower = claude_response.lower()

    # Count distinct research targets (things analyzed with PowerShell)
    # Look for patterns like "GetType('...')" or "GetMethod('...')" or "GetField('...')"
    import re
    research_targets = set()
    for match in re.finditer(r"gettype\(['\"]([^'\"]+)['\"]\)", response_lower):
        research_targets.add(match.group(1))
    for match in re.finditer(r"getmethod\(['\"]([^'\"]+)['\"]\)", response_lower):
        research_targets.add(match.group(1))
    for match in re.finditer(r"getfield\(['\"]([^'\"]+)['\"]\)", response_lower):
        research_targets.add(match.group(1))

    # Count distinct things changed in code
    change_targets = set()
    for match in re.finditer(r'\.setvalue\([^,]+,\s*([^)]+)\)', response_lower):
        change_targets.add(match.group(1).strip()[:30])  # First 30 chars as identifier
    for match in re.finditer(r'getfield\(["\']([^"\']+)["\']', response_lower):
        if 'setvalue' in response_lower[max(0, match.start()-100):match.start()]:
            change_targets.add(match.group(1))

    return len(research_targets), len(change_targets)


# ============================================================================
# CLI INTERFACE
# ============================================================================

if __name__ == "__main__":
    import sys

    enforcer = HookEnforcer()

    if len(sys.argv) < 2:
        print("Usage:")
        print("  python STRICT_HOOKS.py check <text>        - Check for banned phrases")
        print("  python STRICT_HOOKS.py status <target>     - Get research status")
        print("  python STRICT_HOOKS.py add <target> <type> <evidence_file> <conclusion>")
        print("  python STRICT_HOOKS.py validate <desc> <class> <method> <rationale>")
        sys.exit(1)

    cmd = sys.argv[1]

    if cmd == "check":
        text = " ".join(sys.argv[2:])
        ok, phrase = enforcer.check_phrase(text)
        if ok:
            print("✓ No banned phrases detected")
        else:
            print(f"✗ BLOCKED: Contains '{phrase}'")
            sys.exit(1)

    elif cmd == "status":
        target = sys.argv[2]
        print(enforcer.get_research_status(target))

    elif cmd == "validate":
        if len(sys.argv) < 6:
            print("Usage: validate <description> <class> <method> <rationale>")
            sys.exit(1)
        desc, cls, method, rationale = sys.argv[2:6]
        ok, msg = enforcer.pre_change_check(desc, cls, method, rationale)
        print(msg)
        sys.exit(0 if ok else 1)

    elif cmd == "add":
        if len(sys.argv) < 6:
            print("Usage: add <target> <evidence_type> <evidence_file> <conclusion>")
            sys.exit(1)
        target, etype, efile, conclusion = sys.argv[2:6]
        with open(efile, 'r') as f:
            evidence = f.read()
        ok, msg = enforcer.add_research(target, etype, evidence, conclusion)
        print(msg)
        sys.exit(0 if ok else 1)
