# Bug Log

This document tracks bugs found during development and their fixes.

## Template

```
### BUG-XXX: [Title]
**Date:** YYYY-MM-DD
**Severity:** Critical/High/Medium/Low
**Status:** Fixed/Open/Won't Fix

**Description:**
[What happens]

**Reproduction:**
1. Step 1
2. Step 2

**Root Cause:**
[Why it happens]

**Fix:**
[How it was fixed]

**Regression Test:**
[Test added to prevent recurrence]
```

---

## Known Issues (Pre-Launch)

### BUG-001: Unity Project Must Be Created Manually
**Date:** 2024-01-XX
**Severity:** Low
**Status:** By Design

**Description:**
Unity projects cannot be created programmatically and must be created via Unity Hub.

**Workaround:**
Follow the setup instructions in README.md to create the project in Unity Hub.

---

## Fixed Issues

(None yet - document bugs here as they're discovered during testing)

---

## Testing Notes

### Game Over Detection
- The game over detector uses brute-force checking (all pieces, all rotations, all positions)
- This is intentionally not optimized for reliability over performance
- Performance is acceptable for 9x9 grid with typical piece counts

### Save System
- Atomic writes prevent save corruption on crash
- Backup system provides recovery from corrupted saves
- Checksum validation detects tampering

### Piece Rotation
- All rotations are precomputed at initialization
- No runtime float math in rotation logic
- Unique rotations are deduplicated (e.g., square piece has only 1 rotation)
