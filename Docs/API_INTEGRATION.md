# WardMeBruh API Integration Guide

This guide details how external mod developers can integrate with WardMeBruh.

Two integration workflows are supported:
1. **Option A: Repackable API Assembly (`WardMeBruhAPI.dll`)** — Compile against the standalone API assembly and merge it using `ILRepack`.
2. **Option B: Zero-Dependency Client Wrapper (`Docs/WardMeBruhAPI_Client.cs`)** — Drop a single standalone C# file into your project with zero external DLL references.

For complete class, struct, and method signatures, consult [WardMeBruhAPI.md](file:///home/vapok/Modding/Valheim/WardMeBruh/Docs/WardMeBruhAPI.md).

---

## Option A: Repackable API Assembly (`WardMeBruhAPI.dll`)

This option is recommended if your mod already uses `ILRepack` to bundle dependencies and you prefer compile-time type safety directly against the API types.

### 1. Download the API Assembly
Download `WardMeBruhAPI-Vapok-<Version>.zip` from [GitHub Releases](https://github.com/Vapok/WardMeBruh/releases). Extract `WardMeBruhAPI.dll`.

### 2. Add Project Reference
Reference `WardMeBruhAPI.dll` in your `.csproj`:

```xml
<ItemGroup>
    <Reference Include="WardMeBruhAPI">
        <HintPath>path\to\WardMeBruhAPI.dll</HintPath>
        <Private>False</Private>
    </Reference>
</ItemGroup>
```

### 3. Configure ILRepack
Add `WardMeBruhAPI.dll` to your `ILRepack` inputs. WardMeBruh dynamically intercepts assembly loading and redirects all API references to the live `WardMeBruh.dll` assembly at runtime.

When WardMeBruh is not installed, the repacked stub methods execute safely and return default values (`false`, `null`, `WardRole.Guest`) without causing runtime exceptions.

### 4. Implementation Example

```csharp
using UnityEngine;
using WardMeBruh.API;
using WardMeBruh.Models;

public class WardModIntegration
{
    public void Awake()
    {
        // Register custom permission under External Mod Integrations
        WardMeBruhAPI.RegisterRoleFeature(
            id: "com.author.mymod.teleport",
            displayName: "Ward Teleport Pad",
            category: WardCapabilityCategory.External,
            description: "Allows using custom teleport pads inside this ward.",
            defaultRole: WardRole.Resident
        );
    }

    public bool CanUseTeleport(Player player)
    {
        if (!WardMeBruhAPI.IsLoaded())
        {
            return true; // WardMeBruh not installed; allow default behavior
        }

        return WardMeBruhAPI.HasFeatureAccess(player, "com.author.mymod.teleport");
    }
}
```

---

## Option B: Zero-Dependency Client Wrapper (`WardMeBruhAPI_Client.cs`)

This option is recommended for mods that integrate with WardMeBruh as an optional soft dependency and want to avoid adding extra assembly references or configuring ILRepack.

### 1. Add the Client File
Copy [Docs/WardMeBruhAPI_Client.cs](file:///home/vapok/Modding/Valheim/WardMeBruh/Docs/WardMeBruhAPI_Client.cs) directly into your mod's project source.

### 2. How It Works
The `WardMeBruhAPIClient` class inspects the current `AppDomain` for the loaded `"WardMeBruh"` assembly and binds typed reflection delegates.
- If WardMeBruh is not installed, all methods return safe defaults (`false`, `true` for open access, empty strings) with zero missing-assembly exceptions.
- If WardMeBruh is installed, calls execute directly against the live mod with cached reflection delegates.

### 3. Implementation Example

```csharp
using UnityEngine;
using WardMeBruh.API.Client;

public class SoftWardIntegration
{
    public void Awake()
    {
        // Register custom feature with string-based parameters
        WardMeBruhAPIClient.RegisterRoleFeature(
            id: "com.author.mymod.craftstation",
            displayName: "Special Crafting Station",
            categoryName: "External",
            description: "Allows operating the special crafting station within ward territory.",
            defaultRole: 1 // 0 = Guest, 1 = Resident, 2 = Manager, 3 = Owner
        );
    }

    public bool CheckAccess(Vector3 location, long playerId)
    {
        if (!WardMeBruhAPIClient.IsAvailable)
        {
            return true; // WardMeBruh not installed; unconstrained access
        }

        return WardMeBruhAPIClient.HasFeatureAccess(location, "com.author.mymod.craftstation", playerId);
    }
}
```

---

## Comparison Summary

| Characteristic | Option A (`WardMeBruhAPI.dll`) | Option B (`WardMeBruhAPI_Client.cs`) |
| :--- | :--- | :--- |
| **Dependencies** | Requires `WardMeBruhAPI.dll` at compile time | Zero external dependencies |
| **Build Tooling** | Requires `ILRepack` | Standard compilation (drop-in `.cs`) |
| **Runtime Mechanism** | Dynamic assembly redirection | Cached reflection delegates via `WardMeBruhAPIClient` |
| **Missing Mod Safety** | Executes repacked dummy stubs returning defaults | Returns safe defaults via null-checked delegates |
| **Recommended Use** | Hard or repacked integrations | Soft, optional integrations |
