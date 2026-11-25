<a name='assembly'></a>
# WardMeBruh

## Contents

- [ABAPIClient](#T-AdventureBackpacks-API-Client-ABAPIClient 'AdventureBackpacks.API.Client.ABAPIClient')
- [WardMeBruhAPI](#T-WardMeBruh-API-WardMeBruhAPI 'WardMeBruh.API.WardMeBruhAPI')
  - [GetAllPermissionIds()](#M-WardMeBruh-API-WardMeBruhAPI-GetAllPermissionIds 'WardMeBruh.API.WardMeBruhAPI.GetAllPermissionIds')
  - [GetAttunedMask(point)](#M-WardMeBruh-API-WardMeBruhAPI-GetAttunedMask-UnityEngine-Vector3- 'WardMeBruh.API.WardMeBruhAPI.GetAttunedMask(UnityEngine.Vector3)')
  - [GetAttunedMask(wardZdo)](#M-WardMeBruh-API-WardMeBruhAPI-GetAttunedMask-ZDO- 'WardMeBruh.API.WardMeBruhAPI.GetAttunedMask(ZDO)')
  - [GetCoveringWard(point)](#M-WardMeBruh-API-WardMeBruhAPI-GetCoveringWard-UnityEngine-Vector3- 'WardMeBruh.API.WardMeBruhAPI.GetCoveringWard(UnityEngine.Vector3)')
  - [GetPlayerRole(point,playerId)](#M-WardMeBruh-API-WardMeBruhAPI-GetPlayerRole-UnityEngine-Vector3,System-Int64- 'WardMeBruh.API.WardMeBruhAPI.GetPlayerRole(UnityEngine.Vector3,System.Int64)')
  - [GetPlayerRole(ward,playerId)](#M-WardMeBruh-API-WardMeBruhAPI-GetPlayerRole-PrivateArea,System-Int64- 'WardMeBruh.API.WardMeBruhAPI.GetPlayerRole(PrivateArea,System.Int64)')
  - [GetPlayerRoleName(point,playerId)](#M-WardMeBruh-API-WardMeBruhAPI-GetPlayerRoleName-UnityEngine-Vector3,System-Int64- 'WardMeBruh.API.WardMeBruhAPI.GetPlayerRoleName(UnityEngine.Vector3,System.Int64)')
  - [GetPlayerRoleName(ward,playerId)](#M-WardMeBruh-API-WardMeBruhAPI-GetPlayerRoleName-PrivateArea,System-Int64- 'WardMeBruh.API.WardMeBruhAPI.GetPlayerRoleName(PrivateArea,System.Int64)')
  - [GetWardName(point)](#M-WardMeBruh-API-WardMeBruhAPI-GetWardName-UnityEngine-Vector3- 'WardMeBruh.API.WardMeBruhAPI.GetWardName(UnityEngine.Vector3)')
  - [GetWardRadius(point)](#M-WardMeBruh-API-WardMeBruhAPI-GetWardRadius-UnityEngine-Vector3- 'WardMeBruh.API.WardMeBruhAPI.GetWardRadius(UnityEngine.Vector3)')
  - [HasFeatureAccess(point,featureId,playerId)](#M-WardMeBruh-API-WardMeBruhAPI-HasFeatureAccess-UnityEngine-Vector3,System-String,System-Int64- 'WardMeBruh.API.WardMeBruhAPI.HasFeatureAccess(UnityEngine.Vector3,System.String,System.Int64)')
  - [HasFeatureAccess(ward,featureId,playerId)](#M-WardMeBruh-API-WardMeBruhAPI-HasFeatureAccess-PrivateArea,System-String,System-Int64- 'WardMeBruh.API.WardMeBruhAPI.HasFeatureAccess(PrivateArea,System.String,System.Int64)')
  - [HasFeatureAccess(player,featureId)](#M-WardMeBruh-API-WardMeBruhAPI-HasFeatureAccess-Player,System-String- 'WardMeBruh.API.WardMeBruhAPI.HasFeatureAccess(Player,System.String)')
  - [IsExternalFeatureUnlocked(point,externalFeatureId,playerId)](#M-WardMeBruh-API-WardMeBruhAPI-IsExternalFeatureUnlocked-UnityEngine-Vector3,System-String,System-Int64- 'WardMeBruh.API.WardMeBruhAPI.IsExternalFeatureUnlocked(UnityEngine.Vector3,System.String,System.Int64)')
  - [IsExternalFeatureUnlocked(wardZdo,externalFeatureId,playerId)](#M-WardMeBruh-API-WardMeBruhAPI-IsExternalFeatureUnlocked-ZDO,System-String,System-Int64- 'WardMeBruh.API.WardMeBruhAPI.IsExternalFeatureUnlocked(ZDO,System.String,System.Int64)')
  - [IsFeatureUnlocked(point,feature,playerId)](#M-WardMeBruh-API-WardMeBruhAPI-IsFeatureUnlocked-UnityEngine-Vector3,WardMeBruh-Models-WardFeature,System-Int64- 'WardMeBruh.API.WardMeBruhAPI.IsFeatureUnlocked(UnityEngine.Vector3,WardMeBruh.Models.WardFeature,System.Int64)')
  - [IsFeatureUnlocked(wardZdo,feature,playerId)](#M-WardMeBruh-API-WardMeBruhAPI-IsFeatureUnlocked-ZDO,WardMeBruh-Models-WardFeature,System-Int64- 'WardMeBruh.API.WardMeBruhAPI.IsFeatureUnlocked(ZDO,WardMeBruh.Models.WardFeature,System.Int64)')
  - [IsInsideWard(point)](#M-WardMeBruh-API-WardMeBruhAPI-IsInsideWard-UnityEngine-Vector3- 'WardMeBruh.API.WardMeBruhAPI.IsInsideWard(UnityEngine.Vector3)')
  - [IsLoaded()](#M-WardMeBruh-API-WardMeBruhAPI-IsLoaded 'WardMeBruh.API.WardMeBruhAPI.IsLoaded')
  - [IsProgressionEnabled()](#M-WardMeBruh-API-WardMeBruhAPI-IsProgressionEnabled 'WardMeBruh.API.WardMeBruhAPI.IsProgressionEnabled')
  - [IsRuneAttuned(point,rune)](#M-WardMeBruh-API-WardMeBruhAPI-IsRuneAttuned-UnityEngine-Vector3,WardMeBruh-Models-WardRune- 'WardMeBruh.API.WardMeBruhAPI.IsRuneAttuned(UnityEngine.Vector3,WardMeBruh.Models.WardRune)')
  - [IsRuneAttuned(wardZdo,rune)](#M-WardMeBruh-API-WardMeBruhAPI-IsRuneAttuned-ZDO,WardMeBruh-Models-WardRune- 'WardMeBruh.API.WardMeBruhAPI.IsRuneAttuned(ZDO,WardMeBruh.Models.WardRune)')
  - [IsWardShieldActive(point)](#M-WardMeBruh-API-WardMeBruhAPI-IsWardShieldActive-UnityEngine-Vector3- 'WardMeBruh.API.WardMeBruhAPI.IsWardShieldActive(UnityEngine.Vector3)')
  - [RegisterRoleFeature(id,displayName,category,description,defaultRole,sortOrder)](#M-WardMeBruh-API-WardMeBruhAPI-RegisterRoleFeature-System-String,System-String,WardMeBruh-Models-WardCapabilityCategory,System-String,WardMeBruh-Models-WardRole,System-Int32- 'WardMeBruh.API.WardMeBruhAPI.RegisterRoleFeature(System.String,System.String,WardMeBruh.Models.WardCapabilityCategory,System.String,WardMeBruh.Models.WardRole,System.Int32)')
  - [RegisterRoleFeature(id,displayName,category,description,requiredRune,defaultRole,sortOrder)](#M-WardMeBruh-API-WardMeBruhAPI-RegisterRoleFeature-System-String,System-String,WardMeBruh-Models-WardCapabilityCategory,System-String,WardMeBruh-Models-WardRune,WardMeBruh-Models-WardRole,System-Int32- 'WardMeBruh.API.WardMeBruhAPI.RegisterRoleFeature(System.String,System.String,WardMeBruh.Models.WardCapabilityCategory,System.String,WardMeBruh.Models.WardRune,WardMeBruh.Models.WardRole,System.Int32)')
  - [RegisterRoleFeature(id,displayName,categoryName,description,defaultRole,sortOrder)](#M-WardMeBruh-API-WardMeBruhAPI-RegisterRoleFeature-System-String,System-String,System-String,System-String,WardMeBruh-Models-WardRole,System-Int32- 'WardMeBruh.API.WardMeBruhAPI.RegisterRoleFeature(System.String,System.String,System.String,System.String,WardMeBruh.Models.WardRole,System.Int32)')
  - [RegisterRoleFeature(id,displayName,description,defaultRole,sortOrder)](#M-WardMeBruh-API-WardMeBruhAPI-RegisterRoleFeature-System-String,System-String,System-String,WardMeBruh-Models-WardRole,System-Int32- 'WardMeBruh.API.WardMeBruhAPI.RegisterRoleFeature(System.String,System.String,System.String,WardMeBruh.Models.WardRole,System.Int32)')
  - [RegisterRoleTemplate(name,level,colorHex,defaultPermissions)](#M-WardMeBruh-API-WardMeBruhAPI-RegisterRoleTemplate-System-String,System-Int32,System-String,System-Collections-Generic-IEnumerable{System-String}- 'WardMeBruh.API.WardMeBruhAPI.RegisterRoleTemplate(System.String,System.Int32,System.String,System.Collections.Generic.IEnumerable{System.String})')
- [WardPermissionIds](#T-WardMeBruh-API-WardPermissionIds 'WardMeBruh.API.WardPermissionIds')
  - [AccessChests](#F-WardMeBruh-API-WardPermissionIds-AccessChests 'WardMeBruh.API.WardPermissionIds.AccessChests')
  - [AccessDoors](#F-WardMeBruh-API-WardPermissionIds-AccessDoors 'WardMeBruh.API.WardPermissionIds.AccessDoors')
  - [AccessPortals](#F-WardMeBruh-API-WardPermissionIds-AccessPortals 'WardMeBruh.API.WardPermissionIds.AccessPortals')
  - [CoreAttuneWard](#F-WardMeBruh-API-WardPermissionIds-CoreAttuneWard 'WardMeBruh.API.WardPermissionIds.CoreAttuneWard')
  - [CoreBenefits](#F-WardMeBruh-API-WardPermissionIds-CoreBenefits 'WardMeBruh.API.WardPermissionIds.CoreBenefits')
  - [CoreBuilding](#F-WardMeBruh-API-WardPermissionIds-CoreBuilding 'WardMeBruh.API.WardPermissionIds.CoreBuilding')
  - [CoreDomeAccess](#F-WardMeBruh-API-WardPermissionIds-CoreDomeAccess 'WardMeBruh.API.WardPermissionIds.CoreDomeAccess')
  - [CoreExtendWard](#F-WardMeBruh-API-WardPermissionIds-CoreExtendWard 'WardMeBruh.API.WardPermissionIds.CoreExtendWard')
  - [CoreFarming](#F-WardMeBruh-API-WardPermissionIds-CoreFarming 'WardMeBruh.API.WardPermissionIds.CoreFarming')
  - [CoreHarvest](#F-WardMeBruh-API-WardPermissionIds-CoreHarvest 'WardMeBruh.API.WardPermissionIds.CoreHarvest')
  - [CorePower](#F-WardMeBruh-API-WardPermissionIds-CorePower 'WardMeBruh.API.WardPermissionIds.CorePower')
  - [CoreRename](#F-WardMeBruh-API-WardPermissionIds-CoreRename 'WardMeBruh.API.WardPermissionIds.CoreRename')
  - [CoreSubWard](#F-WardMeBruh-API-WardPermissionIds-CoreSubWard 'WardMeBruh.API.WardPermissionIds.CoreSubWard')
  - [CoreViewGui](#F-WardMeBruh-API-WardPermissionIds-CoreViewGui 'WardMeBruh.API.WardPermissionIds.CoreViewGui')
  - [DefensesBiome](#F-WardMeBruh-API-WardPermissionIds-DefensesBiome 'WardMeBruh.API.WardPermissionIds.DefensesBiome')
  - [DefensesCamouflage](#F-WardMeBruh-API-WardPermissionIds-DefensesCamouflage 'WardMeBruh.API.WardPermissionIds.DefensesCamouflage')
  - [DefensesCombat](#F-WardMeBruh-API-WardPermissionIds-DefensesCombat 'WardMeBruh.API.WardPermissionIds.DefensesCombat')
  - [DefensesDomeSize](#F-WardMeBruh-API-WardPermissionIds-DefensesDomeSize 'WardMeBruh.API.WardPermissionIds.DefensesDomeSize')
  - [DefensesTames](#F-WardMeBruh-API-WardPermissionIds-DefensesTames 'WardMeBruh.API.WardPermissionIds.DefensesTames')
  - [PersonnelGovernance](#F-WardMeBruh-API-WardPermissionIds-PersonnelGovernance 'WardMeBruh.API.WardPermissionIds.PersonnelGovernance')
  - [PersonnelRoster](#F-WardMeBruh-API-WardPermissionIds-PersonnelRoster 'WardMeBruh.API.WardPermissionIds.PersonnelRoster')
  - [UpkeepAutoDoors](#F-WardMeBruh-API-WardPermissionIds-UpkeepAutoDoors 'WardMeBruh.API.WardPermissionIds.UpkeepAutoDoors')
  - [UpkeepAutoFuel](#F-WardMeBruh-API-WardPermissionIds-UpkeepAutoFuel 'WardMeBruh.API.WardPermissionIds.UpkeepAutoFuel')
  - [UpkeepPerks](#F-WardMeBruh-API-WardPermissionIds-UpkeepPerks 'WardMeBruh.API.WardPermissionIds.UpkeepPerks')
  - [UpkeepRepair](#F-WardMeBruh-API-WardPermissionIds-UpkeepRepair 'WardMeBruh.API.WardPermissionIds.UpkeepRepair')
  - [UpkeepSecurity](#F-WardMeBruh-API-WardPermissionIds-UpkeepSecurity 'WardMeBruh.API.WardPermissionIds.UpkeepSecurity')
  - [UpkeepWaterDrain](#F-WardMeBruh-API-WardPermissionIds-UpkeepWaterDrain 'WardMeBruh.API.WardPermissionIds.UpkeepWaterDrain')

<a name='T-AdventureBackpacks-API-Client-ABAPIClient'></a>
## ABAPIClient `type`

##### Namespace

AdventureBackpacks.API.Client

##### Summary

Lightweight, zero-dependency client wrapper for AdventureBackpacks API.
Drop this file into your mod project to interact with AdventureBackpacks without adding assembly references or repacking DLLs.

<a name='T-WardMeBruh-API-WardMeBruhAPI'></a>
## WardMeBruhAPI `type`

##### Namespace

WardMeBruh.API

##### Summary

Public developer API for WardMeBruh. Provides programmatic registration for external role features, custom role templates, and ward status/permission queries.

<a name='M-WardMeBruh-API-WardMeBruhAPI-GetAllPermissionIds'></a>
### GetAllPermissionIds() `method`

##### Summary

Retrieves all registered permission IDs currently tracked by WardMeBruh, including built-in and external mod permissions.

##### Returns

Collection of permission ID strings.

##### Parameters

This method has no parameters.

<a name='M-WardMeBruh-API-WardMeBruhAPI-GetAttunedMask-UnityEngine-Vector3-'></a>
### GetAttunedMask(point) `method`

##### Summary

Retrieves the bitmask of all attuned runes on the ward covering the specified world position.

##### Returns

Bitmask integer where bit index corresponds to (1 << (int)WardRune).

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| point | [UnityEngine.Vector3](#T-UnityEngine-Vector3 'UnityEngine.Vector3') | World position to check. |

<a name='M-WardMeBruh-API-WardMeBruhAPI-GetAttunedMask-ZDO-'></a>
### GetAttunedMask(wardZdo) `method`

##### Summary

Retrieves the bitmask of all attuned runes on the specified ward ZDO.

##### Returns

Bitmask integer where bit index corresponds to (1 << (int)WardRune).

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| wardZdo | [ZDO](#T-ZDO 'ZDO') | Ward network data object. |

<a name='M-WardMeBruh-API-WardMeBruhAPI-GetCoveringWard-UnityEngine-Vector3-'></a>
### GetCoveringWard(point) `method`

##### Summary

Retrieves the active PrivateArea component covering the specified position if currently loaded.

##### Returns

PrivateArea instance if loaded within scene; otherwise null.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| point | [UnityEngine.Vector3](#T-UnityEngine-Vector3 'UnityEngine.Vector3') | World position to check. |

<a name='M-WardMeBruh-API-WardMeBruhAPI-GetPlayerRole-UnityEngine-Vector3,System-Int64-'></a>
### GetPlayerRole(point,playerId) `method`

##### Summary

Retrieves the assigned WardRole enum of a player at a specific world position.

##### Returns

Assigned WardRole enum, or WardRole.Guest if outside any ward.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| point | [UnityEngine.Vector3](#T-UnityEngine-Vector3 'UnityEngine.Vector3') | World position to check. |
| playerId | [System.Int64](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int64 'System.Int64') | Player ID to evaluate, or 0 to query the local player. |

<a name='M-WardMeBruh-API-WardMeBruhAPI-GetPlayerRole-PrivateArea,System-Int64-'></a>
### GetPlayerRole(ward,playerId) `method`

##### Summary

Retrieves the assigned WardRole enum of a player for a specific ward.

##### Returns

Assigned WardRole enum, or WardRole.Guest if invalid.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ward | [PrivateArea](#T-PrivateArea 'PrivateArea') | PrivateArea component instance. |
| playerId | [System.Int64](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int64 'System.Int64') | Player ID to evaluate, or 0 to query the local player. |

<a name='M-WardMeBruh-API-WardMeBruhAPI-GetPlayerRoleName-UnityEngine-Vector3,System-Int64-'></a>
### GetPlayerRoleName(point,playerId) `method`

##### Summary

Retrieves the display name of the player's role at a specific world position.

##### Returns

Role name string, or "None" if outside any ward.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| point | [UnityEngine.Vector3](#T-UnityEngine-Vector3 'UnityEngine.Vector3') | World position to check. |
| playerId | [System.Int64](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int64 'System.Int64') | Player ID to evaluate, or 0 to query the local player. |

<a name='M-WardMeBruh-API-WardMeBruhAPI-GetPlayerRoleName-PrivateArea,System-Int64-'></a>
### GetPlayerRoleName(ward,playerId) `method`

##### Summary

Retrieves the display name of the player's role for a specific ward.

##### Returns

Role name string, or "None" if invalid.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ward | [PrivateArea](#T-PrivateArea 'PrivateArea') | PrivateArea component instance. |
| playerId | [System.Int64](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int64 'System.Int64') | Player ID to evaluate, or 0 to query the local player. |

<a name='M-WardMeBruh-API-WardMeBruhAPI-GetWardName-UnityEngine-Vector3-'></a>
### GetWardName(point) `method`

##### Summary

Retrieves the custom name assigned to the ward covering the specified position.

##### Returns

Custom ward name string, or empty string if not found.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| point | [UnityEngine.Vector3](#T-UnityEngine-Vector3 'UnityEngine.Vector3') | World position to check. |

<a name='M-WardMeBruh-API-WardMeBruhAPI-GetWardRadius-UnityEngine-Vector3-'></a>
### GetWardRadius(point) `method`

##### Summary

Retrieves the protection radius in meters of the ward covering the specified position.

##### Returns

Ward radius in meters (defaults to 32.0m if not found).

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| point | [UnityEngine.Vector3](#T-UnityEngine-Vector3 'UnityEngine.Vector3') | World position to check. |

<a name='M-WardMeBruh-API-WardMeBruhAPI-HasFeatureAccess-UnityEngine-Vector3,System-String,System-Int64-'></a>
### HasFeatureAccess(point,featureId,playerId) `method`

##### Summary

Checks whether a player has access to a specific feature permission at the given world position.

##### Returns

True if access is granted or if outside any ward territory; otherwise false.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| point | [UnityEngine.Vector3](#T-UnityEngine-Vector3 'UnityEngine.Vector3') | World position to check. |
| featureId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Permission ID registered with WardMeBruh. |
| playerId | [System.Int64](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int64 'System.Int64') | Player ID to evaluate, or 0 to query the local player. |

<a name='M-WardMeBruh-API-WardMeBruhAPI-HasFeatureAccess-PrivateArea,System-String,System-Int64-'></a>
### HasFeatureAccess(ward,featureId,playerId) `method`

##### Summary

Checks whether a player has access to a specific feature permission within a specific ward component.

##### Returns

True if access is granted; otherwise false.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ward | [PrivateArea](#T-PrivateArea 'PrivateArea') | PrivateArea component instance. |
| featureId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Permission ID registered with WardMeBruh. |
| playerId | [System.Int64](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int64 'System.Int64') | Player ID to evaluate, or 0 to query the local player. |

<a name='M-WardMeBruh-API-WardMeBruhAPI-HasFeatureAccess-Player,System-String-'></a>
### HasFeatureAccess(player,featureId) `method`

##### Summary

Checks whether a player has access to a specific feature permission at their current location.

##### Returns

True if access is granted; otherwise false.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| player | [Player](#T-Player 'Player') | Player instance to evaluate. |
| featureId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Permission ID registered with WardMeBruh. |

<a name='M-WardMeBruh-API-WardMeBruhAPI-IsExternalFeatureUnlocked-UnityEngine-Vector3,System-String,System-Int64-'></a>
### IsExternalFeatureUnlocked(point,externalFeatureId,playerId) `method`

##### Summary

Checks whether an external 3rd-party mod feature (registered via Path A) is currently unlocked on the ward covering the specified position.

##### Returns

True if feature is unlocked or progression disabled; otherwise false.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| point | [UnityEngine.Vector3](#T-UnityEngine-Vector3 'UnityEngine.Vector3') | World position to check. |
| externalFeatureId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Unique external feature identifier string (e.g. "com.author.mod.feature"). |
| playerId | [System.Int64](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int64 'System.Int64') | Optional player ID for checking admin bypass privileges. |

<a name='M-WardMeBruh-API-WardMeBruhAPI-IsExternalFeatureUnlocked-ZDO,System-String,System-Int64-'></a>
### IsExternalFeatureUnlocked(wardZdo,externalFeatureId,playerId) `method`

##### Summary

Checks whether an external 3rd-party mod feature (registered via Path A) is currently unlocked on the specified ward ZDO.

##### Returns

True if feature is unlocked or progression disabled; otherwise false.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| wardZdo | [ZDO](#T-ZDO 'ZDO') | Ward network data object. |
| externalFeatureId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Unique external feature identifier string (e.g. "com.author.mod.feature"). |
| playerId | [System.Int64](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int64 'System.Int64') | Optional player ID for checking admin bypass privileges. |

<a name='M-WardMeBruh-API-WardMeBruhAPI-IsFeatureUnlocked-UnityEngine-Vector3,WardMeBruh-Models-WardFeature,System-Int64-'></a>
### IsFeatureUnlocked(point,feature,playerId) `method`

##### Summary

Checks whether a specific built-in ward capability feature is currently unlocked on the ward covering the specified position.

##### Returns

True if feature is unlocked; otherwise false.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| point | [UnityEngine.Vector3](#T-UnityEngine-Vector3 'UnityEngine.Vector3') | World position to check. |
| feature | [WardMeBruh.Models.WardFeature](#T-WardMeBruh-Models-WardFeature 'WardMeBruh.Models.WardFeature') | Built-in WardFeature enum token. |
| playerId | [System.Int64](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int64 'System.Int64') | Optional player ID for checking admin bypass privileges. |

<a name='M-WardMeBruh-API-WardMeBruhAPI-IsFeatureUnlocked-ZDO,WardMeBruh-Models-WardFeature,System-Int64-'></a>
### IsFeatureUnlocked(wardZdo,feature,playerId) `method`

##### Summary

Checks whether a specific built-in ward capability feature is currently unlocked on the specified ward ZDO.

##### Returns

True if feature is unlocked; otherwise false.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| wardZdo | [ZDO](#T-ZDO 'ZDO') | Ward network data object. |
| feature | [WardMeBruh.Models.WardFeature](#T-WardMeBruh-Models-WardFeature 'WardMeBruh.Models.WardFeature') | Built-in WardFeature enum token. |
| playerId | [System.Int64](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int64 'System.Int64') | Optional player ID for checking admin bypass privileges. |

<a name='M-WardMeBruh-API-WardMeBruhAPI-IsInsideWard-UnityEngine-Vector3-'></a>
### IsInsideWard(point) `method`

##### Summary

Determines whether the specified position is within an active ward territory or network.

##### Returns

True if inside an active ward boundary; otherwise false.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| point | [UnityEngine.Vector3](#T-UnityEngine-Vector3 'UnityEngine.Vector3') | World position to check. |

<a name='M-WardMeBruh-API-WardMeBruhAPI-IsLoaded'></a>
### IsLoaded() `method`

##### Summary

Notifies whether WardMeBruh is loaded and active in the current game session.

##### Returns

True if WardMeBruh is loaded; otherwise false.

##### Parameters

This method has no parameters.

<a name='M-WardMeBruh-API-WardMeBruhAPI-IsProgressionEnabled'></a>
### IsProgressionEnabled() `method`

##### Summary

Returns whether the Runic Attunement Progression System is currently enabled on the active server or session.

##### Returns

True if progression system is enabled; false if all features are globally unlocked.

##### Parameters

This method has no parameters.

<a name='M-WardMeBruh-API-WardMeBruhAPI-IsRuneAttuned-UnityEngine-Vector3,WardMeBruh-Models-WardRune-'></a>
### IsRuneAttuned(point,rune) `method`

##### Summary

Checks whether a specific Elder Futhark rune is attuned on the ward covering the specified world coordinates.

##### Returns

True if the covering ward has attuned this rune; otherwise false.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| point | [UnityEngine.Vector3](#T-UnityEngine-Vector3 'UnityEngine.Vector3') | World position to check. |
| rune | [WardMeBruh.Models.WardRune](#T-WardMeBruh-Models-WardRune 'WardMeBruh.Models.WardRune') | Rune enum value to verify. |

<a name='M-WardMeBruh-API-WardMeBruhAPI-IsRuneAttuned-ZDO,WardMeBruh-Models-WardRune-'></a>
### IsRuneAttuned(wardZdo,rune) `method`

##### Summary

Checks whether a specific Elder Futhark rune is attuned on the specified ward ZDO.

##### Returns

True if attuned; otherwise false.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| wardZdo | [ZDO](#T-ZDO 'ZDO') | Ward network data object. |
| rune | [WardMeBruh.Models.WardRune](#T-WardMeBruh-Models-WardRune 'WardMeBruh.Models.WardRune') | Rune enum value to verify. |

<a name='M-WardMeBruh-API-WardMeBruhAPI-IsWardShieldActive-UnityEngine-Vector3-'></a>
### IsWardShieldActive(point) `method`

##### Summary

Determines whether the ward covering the specified point has its forcefield dome active.

##### Returns

True if the ward's shield dome is active; otherwise false.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| point | [UnityEngine.Vector3](#T-UnityEngine-Vector3 'UnityEngine.Vector3') | World position to check. |

<a name='M-WardMeBruh-API-WardMeBruhAPI-RegisterRoleFeature-System-String,System-String,WardMeBruh-Models-WardCapabilityCategory,System-String,WardMeBruh-Models-WardRole,System-Int32-'></a>
### RegisterRoleFeature(id,displayName,category,description,defaultRole,sortOrder) `method`

##### Summary

Registers a custom external role feature permission into the WardMeBruh permission matrix.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| id | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Unique identifier for the permission (e.g. "com.author.mod.feature"). |
| displayName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Human-readable label displayed in the Management matrix. |
| category | [WardMeBruh.Models.WardCapabilityCategory](#T-WardMeBruh-Models-WardCapabilityCategory 'WardMeBruh.Models.WardCapabilityCategory') | Capability category under which the feature is grouped. |
| description | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Tooltip explanation of what this permission grants. |
| defaultRole | [WardMeBruh.Models.WardRole](#T-WardMeBruh-Models-WardRole 'WardMeBruh.Models.WardRole') | Default minimum role level required to access this feature. |
| sortOrder | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | Visual sort ordering within the category. |

<a name='M-WardMeBruh-API-WardMeBruhAPI-RegisterRoleFeature-System-String,System-String,WardMeBruh-Models-WardCapabilityCategory,System-String,WardMeBruh-Models-WardRune,WardMeBruh-Models-WardRole,System-Int32-'></a>
### RegisterRoleFeature(id,displayName,category,description,requiredRune,defaultRole,sortOrder) `method`

##### Summary

Registers a custom external role feature permission into the WardMeBruh permission matrix, gated by a specific Runic Attunement progression tier.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| id | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Unique identifier for the permission (e.g. "com.author.mod.feature"). |
| displayName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Human-readable label displayed in the Management matrix. |
| category | [WardMeBruh.Models.WardCapabilityCategory](#T-WardMeBruh-Models-WardCapabilityCategory 'WardMeBruh.Models.WardCapabilityCategory') | Capability category under which the feature is grouped. |
| description | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Tooltip explanation of what this permission grants. |
| requiredRune | [WardMeBruh.Models.WardRune](#T-WardMeBruh-Models-WardRune 'WardMeBruh.Models.WardRune') | Rune tier required to unlock this feature when progression is active. |
| defaultRole | [WardMeBruh.Models.WardRole](#T-WardMeBruh-Models-WardRole 'WardMeBruh.Models.WardRole') | Default minimum role level required to access this feature. |
| sortOrder | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | Visual sort ordering within the category. |

<a name='M-WardMeBruh-API-WardMeBruhAPI-RegisterRoleFeature-System-String,System-String,System-String,System-String,WardMeBruh-Models-WardRole,System-Int32-'></a>
### RegisterRoleFeature(id,displayName,categoryName,description,defaultRole,sortOrder) `method`

##### Summary

Registers a custom external role feature permission using a category string name.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| id | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Unique identifier for the permission (e.g. "com.author.mod.feature"). |
| displayName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Human-readable label displayed in the Management matrix. |
| categoryName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Name of the category to assign or group under. |
| description | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Tooltip explanation of what this permission grants. |
| defaultRole | [WardMeBruh.Models.WardRole](#T-WardMeBruh-Models-WardRole 'WardMeBruh.Models.WardRole') | Default minimum role level required to access this feature. |
| sortOrder | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | Visual sort ordering within the category. |

<a name='M-WardMeBruh-API-WardMeBruhAPI-RegisterRoleFeature-System-String,System-String,System-String,WardMeBruh-Models-WardRole,System-Int32-'></a>
### RegisterRoleFeature(id,displayName,description,defaultRole,sortOrder) `method`

##### Summary

Registers a custom external role feature under the default External Mod Integrations category.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| id | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Unique identifier for the permission (e.g. "com.author.mod.feature"). |
| displayName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Human-readable label displayed in the Management matrix. |
| description | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Tooltip explanation of what this permission grants. |
| defaultRole | [WardMeBruh.Models.WardRole](#T-WardMeBruh-Models-WardRole 'WardMeBruh.Models.WardRole') | Default minimum role level required to access this feature. |
| sortOrder | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | Visual sort ordering within the category. |

<a name='M-WardMeBruh-API-WardMeBruhAPI-RegisterRoleTemplate-System-String,System-Int32,System-String,System-Collections-Generic-IEnumerable{System-String}-'></a>
### RegisterRoleTemplate(name,level,colorHex,defaultPermissions) `method`

##### Summary

Registers a custom default role template into newly created or unconfigured wards.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| name | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Display name of the custom role. |
| level | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | Hierarchy level from 0 (Guest equivalent) to 3 (Admin equivalent). |
| colorHex | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Hex color string (e.g. "#38bdf8") for UI badges. |
| defaultPermissions | [System.Collections.Generic.IEnumerable{System.String}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IEnumerable 'System.Collections.Generic.IEnumerable{System.String}') | Optional list of permission IDs granted to this role by default. |

<a name='T-WardMeBruh-API-WardPermissionIds'></a>
## WardPermissionIds `type`

##### Namespace

WardMeBruh.API

##### Summary

Canonical string identifiers for all built-in WardMeBruh role feature permissions.
Pass these constants to WardMeBruhAPI methods such as HasFeatureAccess.

<a name='F-WardMeBruh-API-WardPermissionIds-AccessChests'></a>
### AccessChests `constants`

##### Summary

Clearance to configure storage container clearances, toggle Fuel Depots, and rename chests. Default role: Manager.

<a name='F-WardMeBruh-API-WardPermissionIds-AccessDoors'></a>
### AccessDoors `constants`

##### Summary

Clearance to configure individual door permissions, auto-close timers, and custom names. Default role: Manager.

<a name='F-WardMeBruh-API-WardPermissionIds-AccessPortals'></a>
### AccessPortals `constants`

##### Summary

Clearance to configure portal transit clearances. Default role: Manager.

<a name='F-WardMeBruh-API-WardPermissionIds-CoreAttuneWard'></a>
### CoreAttuneWard `constants`

##### Summary

Clearance to perform runic attunement ceremonies and upgrade the ward progression rank. Default role: Owner.

<a name='F-WardMeBruh-API-WardPermissionIds-CoreBenefits'></a>
### CoreBenefits `constants`

##### Summary

Clearance to receive territory status, maximum comfort, food decay pause, and death skill protection. Default role: Resident.

<a name='F-WardMeBruh-API-WardPermissionIds-CoreBuilding'></a>
### CoreBuilding `constants`

##### Summary

Clearance to place, repair, and deconstruct build pieces and furniture within the protected perimeter. Default role: Resident.

<a name='F-WardMeBruh-API-WardPermissionIds-CoreDomeAccess'></a>
### CoreDomeAccess `constants`

##### Summary

Clearance to pass through the forcefield dome without repulsion or damage. Default role: Guest.

<a name='F-WardMeBruh-API-WardPermissionIds-CoreExtendWard'></a>
### CoreExtendWard `constants`

##### Summary

Clearance to place an Extension Ward to expand base territory. Default role: Owner.

<a name='F-WardMeBruh-API-WardPermissionIds-CoreFarming'></a>
### CoreFarming `constants`

##### Summary

Clearance to cultivate soil, plant seeds, plant crops, and plant saplings using the Cultivator. Default role: Resident.

<a name='F-WardMeBruh-API-WardPermissionIds-CoreHarvest'></a>
### CoreHarvest `constants`

##### Summary

Clearance to harvest mature crops, berry bushes, mushrooms, and pickable resources. Default role: Resident.

<a name='F-WardMeBruh-API-WardPermissionIds-CorePower'></a>
### CorePower `constants`

##### Summary

Clearance to toggle the ward shield power state on or off. Default role: Manager.

<a name='F-WardMeBruh-API-WardPermissionIds-CoreRename'></a>
### CoreRename `constants`

##### Summary

Clearance to rename the ward settlement. Default role: Manager.

<a name='F-WardMeBruh-API-WardPermissionIds-CoreSubWard'></a>
### CoreSubWard `constants`

##### Summary

Clearance to place a Sub-Ward inside this ward territory. Default role: Manager.

<a name='F-WardMeBruh-API-WardPermissionIds-CoreViewGui'></a>
### CoreViewGui `constants`

##### Summary

Clearance to open and inspect the WardMeBruh Central Hub GUI at a ward stone. Default role: Resident.

<a name='F-WardMeBruh-API-WardPermissionIds-DefensesBiome'></a>
### DefensesBiome `constants`

##### Summary

Clearance to configure Virtual Roof, Ashlands Cinder Shield, and Deep North Snow Shield. Default role: Manager.

<a name='F-WardMeBruh-API-WardPermissionIds-DefensesCamouflage'></a>
### DefensesCamouflage `constants`

##### Summary

Clearance to configure Ghost Base, Raid Suppression, and Mob Spawn Suppression. Default role: Manager.

<a name='F-WardMeBruh-API-WardPermissionIds-DefensesCombat'></a>
### DefensesCombat `constants`

##### Summary

Clearance to adjust pushback force, retaliatory shock damage, and shield visuals. Default role: Manager.

<a name='F-WardMeBruh-API-WardPermissionIds-DefensesDomeSize'></a>
### DefensesDomeSize `constants`

##### Summary

Clearance to adjust the ward dome size slider up to 100m. Default role: Owner.

<a name='F-WardMeBruh-API-WardPermissionIds-DefensesTames'></a>
### DefensesTames `constants`

##### Summary

Clearance to toggle damage immunity for tamed animals and livestock. Default role: Manager.

<a name='F-WardMeBruh-API-WardPermissionIds-PersonnelGovernance'></a>
### PersonnelGovernance `constants`

##### Summary

Clearance to access the Management matrix and create custom roles. Default role: Owner.

<a name='F-WardMeBruh-API-WardPermissionIds-PersonnelRoster'></a>
### PersonnelRoster `constants`

##### Summary

Clearance to promote, demote, and assign players on the personnel roster. Default role: Manager.

<a name='F-WardMeBruh-API-WardPermissionIds-UpkeepAutoDoors'></a>
### UpkeepAutoDoors `constants`

##### Summary

Clearance to toggle automated closing of doors and gates. Default role: Manager.

<a name='F-WardMeBruh-API-WardPermissionIds-UpkeepAutoFuel'></a>
### UpkeepAutoFuel `constants`

##### Summary

Clearance to toggle automated refueling from fuel depots for fireplaces and smelters. Default role: Manager.

<a name='F-WardMeBruh-API-WardPermissionIds-UpkeepPerks'></a>
### UpkeepPerks `constants`

##### Summary

Clearance to toggle sanctuary perks (Max Comfort, Food Decay Freeze, Death Shield, Regen Boost). Default role: Manager.

<a name='F-WardMeBruh-API-WardPermissionIds-UpkeepRepair'></a>
### UpkeepRepair `constants`

##### Summary

Clearance to configure Area Piece Repair and weathering repair thresholds. Default role: Manager.

<a name='F-WardMeBruh-API-WardPermissionIds-UpkeepSecurity'></a>
### UpkeepSecurity `constants`

##### Summary

Clearance to toggle agricultural crop protection, terrain modification locking, and cart/boat locking. Default role: Manager.

<a name='F-WardMeBruh-API-WardPermissionIds-UpkeepWaterDrain'></a>
### UpkeepWaterDrain `constants`

##### Summary

Clearance to toggle underwater seabed water displacement. Default role: Manager.
