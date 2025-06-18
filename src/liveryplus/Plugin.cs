using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;
using static System.Collections.Specialized.BitVector32;

namespace liveryplus
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class LiveryOverridePlugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;

        static readonly FieldInfo DamageMatField = typeof(UnitPart).GetField("damageMaterial", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly FieldInfo RenderersField = typeof(DamageMaterial).GetField("renderers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private static readonly Dictionary<string, AssetBundle> _bundleCache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Material> _matCache = new(StringComparer.OrdinalIgnoreCase);
        private static ConfigEntry<bool> testing;

        private void Awake()
        {
            hideFlags = HideFlags.HideAndDontSave;
            Logger = base.Logger;
            testing = Config.Bind(
                section: "Debug",
                key:     "Output LiveryName to console",
                defaultValue: false
            );
            string dllFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Logger.LogInfo($"Searching for .liveryplus files in: {dllFolder}");

            foreach (string file in Directory.GetFiles(dllFolder, "*.liveryplus", SearchOption.TopDirectoryOnly))
            {
                var bundle = AssetBundle.LoadFromFile(file);
                if (!bundle)
                {
                    Logger.LogError($"Failed to load liveryplus: {file}");
                    continue;
                }

                foreach (GameObject prefab in bundle.LoadAllAssets<GameObject>())
                {
                    var mr = prefab.GetComponentInChildren<MeshRenderer>(true);
                    if (mr?.sharedMaterial == null)
                    {
                        Logger.LogWarning($"liveryplus Prefab '{prefab.name}' in '{Path.GetFileName(file)}' has no MeshRenderer/material");
                        continue;
                    }

                    var matInstance = new Material(mr.sharedMaterial) { name = $"{prefab.name} (liveryplus)" };
                    _matCache[prefab.name] = matInstance;
                    Logger.LogInfo($"Loaded liveryplus for '{prefab.name}'");
                    break;
                }
                _bundleCache[Path.GetFileNameWithoutExtension(file)] = bundle;
            }
            new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
        }

        [HarmonyPatch(typeof(UnitPart), nameof(UnitPart.SetLivery))]
        class SetLivery_Patch
        {
            private static Unit lastunit;
            static void Prefix(UnitPart __instance, LiveryData livery, MaterialCleanup materialCleanup)
            {
                string liveryName = livery.Texture?.name;
                if (string.IsNullOrEmpty(liveryName))
                    return;

                var hasOverride = _matCache.TryGetValue(liveryName, out var overrideMat);
                if (__instance.parentUnit != lastunit) //despamming
                {
                    lastunit = __instance.parentUnit;
                    if (testing.Value)
                    {
                        if (hasOverride) Logger.LogWarning($"Loaded liveryplus: {liveryName}");
                        else Logger.LogDebug($"no liveryplus found for {liveryName}");
                    }
                }
                if (!hasOverride)
                    return;

                if (DamageMatField == null) return;
                var dmgMat = DamageMatField.GetValue(__instance);
                if (dmgMat == null) return;

                if (RenderersField == null) return;
                var renderersObj = RenderersField.GetValue(dmgMat);
                if (renderersObj is not IList<Renderer> renderers) return;

                foreach (Renderer r in renderers)
                {
                    r.sharedMaterial = overrideMat;
                    overrideMat.renderQueue = 2500;
                }
            }
        }
    }
}
