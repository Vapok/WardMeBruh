using System;
using System.Reflection;
using UnityEngine;

namespace WardMeBruh.API.Client
{
    public static class WardMeBruhAPIClient
    {
        private static bool _initialized;
        private static bool _isAvailable;
        private static Type _apiType;

        private static MethodInfo _isLoadedMethod;
        private static MethodInfo _registerRoleFeatureMethod;
        private static MethodInfo _registerRoleTemplateMethod;
        private static MethodInfo _hasFeatureAccessPointMethod;
        private static MethodInfo _hasFeatureAccessPlayerMethod;
        private static MethodInfo _isInsideWardMethod;
        private static MethodInfo _isWardShieldActiveMethod;
        private static MethodInfo _getWardRadiusMethod;
        private static MethodInfo _getWardNameMethod;
        private static MethodInfo _getPlayerRoleNameMethod;

        public static bool IsAvailable
        {
            get
            {
                EnsureInitialized();
                return _isAvailable;
            }
        }

        private static void EnsureInitialized()
        {
            if (_initialized)
                return;

            _initialized = true;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                if (assemblies[i].GetName().Name == "WardMeBruh")
                {
                    _apiType = assemblies[i].GetType("WardMeBruh.API.WardMeBruhAPI");
                    if (_apiType != null)
                    {
                        _isAvailable = true;
                        _isLoadedMethod = _apiType.GetMethod("IsLoaded", BindingFlags.Public | BindingFlags.Static);
                        _registerRoleFeatureMethod = _apiType.GetMethod("RegisterRoleFeature", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(int), typeof(int) }, null);
                        _hasFeatureAccessPointMethod = _apiType.GetMethod("HasFeatureAccess", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Vector3), typeof(string), typeof(long) }, null);
                        _hasFeatureAccessPlayerMethod = _apiType.GetMethod("HasFeatureAccess", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Player), typeof(string) }, null);
                        _isInsideWardMethod = _apiType.GetMethod("IsInsideWard", BindingFlags.Public | BindingFlags.Static);
                        _isWardShieldActiveMethod = _apiType.GetMethod("IsWardShieldActive", BindingFlags.Public | BindingFlags.Static);
                        _getWardRadiusMethod = _apiType.GetMethod("GetWardRadius", BindingFlags.Public | BindingFlags.Static);
                        _getWardNameMethod = _apiType.GetMethod("GetWardName", BindingFlags.Public | BindingFlags.Static);
                        _getPlayerRoleNameMethod = _apiType.GetMethod("GetPlayerRoleName", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Vector3), typeof(long) }, null);
                    }
                    break;
                }
            }
        }

        public static void RegisterRoleFeature(string id, string displayName, string categoryName, string description, int defaultRole = 1, int sortOrder = 100)
        {
            EnsureInitialized();
            if (_registerRoleFeatureMethod != null)
            {
                _registerRoleFeatureMethod.Invoke(null, new object[] { id, displayName, categoryName, description, defaultRole, sortOrder });
            }
        }

        public static bool HasFeatureAccess(Vector3 point, string featureId, long playerId = 0L)
        {
            EnsureInitialized();
            if (_hasFeatureAccessPointMethod != null)
            {
                return (bool)_hasFeatureAccessPointMethod.Invoke(null, new object[] { point, featureId, playerId });
            }
            return true;
        }

        public static bool HasFeatureAccess(Player player, string featureId)
        {
            EnsureInitialized();
            if (_hasFeatureAccessPlayerMethod != null && player != null)
            {
                return (bool)_hasFeatureAccessPlayerMethod.Invoke(null, new object[] { player, featureId });
            }
            return true;
        }

        public static bool IsInsideWard(Vector3 point)
        {
            EnsureInitialized();
            if (_isInsideWardMethod != null)
            {
                return (bool)_isInsideWardMethod.Invoke(null, new object[] { point });
            }
            return false;
        }

        public static bool IsWardShieldActive(Vector3 point)
        {
            EnsureInitialized();
            if (_isWardShieldActiveMethod != null)
            {
                return (bool)_isWardShieldActiveMethod.Invoke(null, new object[] { point });
            }
            return false;
        }

        public static float GetWardRadius(Vector3 point)
        {
            EnsureInitialized();
            if (_getWardRadiusMethod != null)
            {
                return (float)_getWardRadiusMethod.Invoke(null, new object[] { point });
            }
            return 32.0f;
        }

        public static string GetWardName(Vector3 point)
        {
            EnsureInitialized();
            if (_getWardNameMethod != null)
            {
                return (string)_getWardNameMethod.Invoke(null, new object[] { point });
            }
            return string.Empty;
        }

        public static string GetPlayerRoleName(Vector3 point, long playerId = 0L)
        {
            EnsureInitialized();
            if (_getPlayerRoleNameMethod != null)
            {
                return (string)_getPlayerRoleNameMethod.Invoke(null, new object[] { point, playerId });
            }
            return "None";
        }
    }

    public static class WardPermissionIds
    {
        public const string CoreDomeAccess = "core.dome_access";
        public const string CoreBenefits = "core.benefits";
        public const string CoreBuilding = "core.building";
        public const string CoreFarming = "core.farming";
        public const string CoreHarvest = "core.harvest";
        public const string CoreViewGui = "core.view_gui";
        public const string CorePower = "core.power";
        public const string CoreRename = "core.rename";

        public const string AccessDoors = "access.doors";
        public const string AccessChests = "access.chests";
        public const string AccessPortals = "access.portals";

        public const string DefensesDomeSize = "defenses.dome_size";
        public const string DefensesCombat = "defenses.combat";
        public const string DefensesCamouflage = "defenses.camouflage";
        public const string DefensesBiome = "defenses.biome";
        public const string DefensesTames = "defenses.tames";

        public const string UpkeepRepair = "upkeep.repair";
        public const string UpkeepAutoFuel = "upkeep.autofuel";
        public const string UpkeepAutoDoors = "upkeep.autodoors";
        public const string UpkeepPerks = "upkeep.perks";
        public const string UpkeepWaterDrain = "upkeep.waterdrain";
        public const string UpkeepSecurity = "upkeep.security";

        public const string PersonnelRoster = "personnel.roster";
        public const string PersonnelGovernance = "personnel.governance";
    }
}
