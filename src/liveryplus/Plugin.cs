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
using System.Linq;

namespace liveryplus
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class LiveryOverridePlugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        private static readonly Dictionary<string, AssetBundle> _bundleCache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Material> _matCache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Material> _spinnerMatCache = new(StringComparer.OrdinalIgnoreCase);
        const string spinnerSuffix = "__spinner";
        private static ConfigEntry<bool> testing;
        private void Awake()
        {
            hideFlags = HideFlags.HideAndDontSave;
            Logger = base.Logger;
            testing = Config.Bind("Debug","Output LiveryName to console", false);

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
                    MeshRenderer mr = prefab.GetComponentInChildren<MeshRenderer>(true);
                    if (mr?.sharedMaterial == null)
                    {
                        Logger.LogWarning($"liveryplus Prefab '{prefab.name}' in '{Path.GetFileName(file)}' has no MeshRenderer/material");
                        continue;
                    }
                    //normal mat
                    if (!prefab.name.EndsWith(spinnerSuffix, StringComparison.OrdinalIgnoreCase))
                    {
                        var matInstance = new Material(mr.sharedMaterial) { name = $"{prefab.name} (liveryplus)" };
                        _matCache[prefab.name] = matInstance;
                        Logger.LogInfo($"Loaded liveryplus for '{prefab.name}'");

                    }
                    //spinner
                    else
                    {
                        string baseName = prefab.name[..^spinnerSuffix.Length];
                        var mat = new Material(mr.sharedMaterial) { name = $"{prefab.name} (liveryplus spinner)" };
                        _spinnerMatCache[baseName] = mat;
                        Logger.LogInfo($"Loaded spinner material for '{baseName}'");
                    }
                }
                _bundleCache[Path.GetFileNameWithoutExtension(file)] = bundle;
            }
            new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
        }
        static readonly FieldInfo DamageMatField = typeof(UnitPart).GetField("damageMaterial", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly FieldInfo RenderersField = typeof(DamageMaterial).GetField("renderers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        [HarmonyPatch(typeof(UnitPart), nameof(UnitPart.SetLivery))]
        class SetLivery_Patch
        {
            private static Unit lastunit;
            static void Prefix(UnitPart __instance, LiveryData livery, MaterialCleanup materialCleanup)
            {
                string liveryName = livery.Texture?.name;
                if (string.IsNullOrEmpty(liveryName)) return;

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
                if (!hasOverride) return;

                if (DamageMatField == null) return;
                var dmgMat = DamageMatField.GetValue(__instance);
                if (dmgMat == null) return;
                if (RenderersField == null) return;

                var renderersObj = RenderersField.GetValue(dmgMat);
                if (renderersObj is not IList<Renderer> renderers) return;

                foreach (Renderer r in renderers)
                {
                    r.sharedMaterial = overrideMat;
                    overrideMat.renderQueue = 2450;
                }
                if (_spinnerMatCache.TryGetValue(liveryName, out var spinMat))
                {
                    var radomePart = __instance.parentUnit.partLookup.FirstOrDefault(p => p.name != null && p.name.IndexOf("radome", StringComparison.OrdinalIgnoreCase) >= 0);

                    if (radomePart != null)
                    {
                        Transform spinnerTr = radomePart.transform.Find("spinner");
                        if (spinnerTr != null && spinnerTr.TryGetComponent(out Renderer spRenderer))
                        {
                            spRenderer.sharedMaterial = spinMat;
                            spinMat.renderQueue = 2450;
                        }
                    }
                }
            }
        }
    }
}
