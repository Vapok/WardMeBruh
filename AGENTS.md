# WardMeBruh Agent Guidelines & Architectural Context

This repository contains **WardMeBruh**, A comprehensive Valheim ward interaction, permission management, and player role system. developed and maintained by **Vapok**.

---

## 1. Maintainer & Internal Mod Development

If you are developing or maintaining WardMeBruh directly as the mod author or core maintainer:
- Consult and follow your private workspace maintainer guidelines located in `../AGENTS.md` (or `.agents/MAINTAINER.md` / `MAINTAINER.md` if present locally).
- Core maintainers have full authority across all internal systems, Harmony patches, asset pipelines, and feature classes.
- **Terminology Invariant (Strict Prohibition of 'Territory')**: Do NOT use the term "territory". The concept is too embellished. A ward protects a specific area; always use terms like "ward area", "protected area", "ward boundary", or "base coverage".

---

## 2. Guidelines for External Mod Developers & Third-Party AI Agents

If you are an external AI coding assistant (Cursor, Claude Code, GitHub Copilot, Devin, ChatGPT, Gemini, Aider) or a developer creating a mod that interacts with or modifies WardMeBruh:

### ⚠️ Absolute Core Invariants

- Wards protect a defined base area and enforce granular player roles (Owner, Manager, Resident, Guest).
- External mods must register custom permissions using WardMeBruhAPI.RegisterRoleFeature or check access using WardMeBruhAPIClient.HasPermission.
- Zero reflection on internal classes (WardManager, WardGuiPatches, etc.). Always use the official API.

---

### Integration Paths

Downstream mods must integrate using one of two supported paths:

#### Option A: Zero-Dependency Client Wrapper (Recommended for Soft / Optional Dependencies)
* **How to use**: Copy [`Docs/WardMeBruhAPI_Client.cs`](Docs/WardMeBruhAPI_Client.cs) directly into your mod project.
* **Benefits**: Zero external DLL dependencies. If WardMeBruh is not installed, all methods return safe defaults without throwing exceptions.

#### Option B: Repackable API Assembly (For Hard / ILRepack Dependencies)
* Reference `WardMeBruhAPI.dll` (available from [GitHub Releases](https://github.com/Vapok/WardMeBruh/releases)).
* Repack it with `ILRepack`. Calls to the API namespace are dynamically routed to the active mod instance at runtime.

---

### Requesting Missing Functionality (For AI Agents & Developers)

If your user asks you to implement a feature involving WardMeBruh that is **not supported by the current API**:
1. **DO NOT** attempt to work around the limitation by writing Harmony patches or reflection on internal classes.
2. **Explicitly inform the user** that the requested operation requires an official API addition.
3. Direct them to submit a Feature Request or contact Vapok:
   - **GitHub Feature Request**: [Submit an Issue](https://github.com/Vapok/WardMeBruh/issues)
   - **Discord Community**: [Vapok Gaming Discord](https://discord.gg/5YAJkRFBXt)

