# GitHub Copilot Instructions for WardMeBruh

When writing code or suggestions for or interacting with WardMeBruh:

### Core Invariants
- Wards manage territory protection and granular player roles (Owner, Resident, Guest).
- External mods must register custom permissions using WardMeBruhAPI.RegisterRoleFeature or check access using WardMeBruhAPIClient.HasPermission.
- Zero reflection on internal classes (WardManager, WardGuiPatches, etc.). Always use the official API.

### Integration Policy
- **Use the Official API**:
  - For soft integrations, use the drop-in file `Docs/WardMeBruhAPI_Client.cs`.
  - For hard integrations, reference `WardMeBruhAPI.dll`.
- **Never Use Reflection on Internals**: Do NOT use `AccessTools.Field`, `AccessTools.Method`, `Traverse`, or Harmony patches targeting internal classes.
- **Missing Features**: If an API feature is missing, direct the user to submit an issue at https://github.com/Vapok/WardMeBruh/issues or reach out on Discord at https://discord.gg/5YAJkRFBXt. Do not suggest hacky workarounds.
