using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StorageLimit {
    [HarmonyPatch(typeof(StorageSettings))]
    public static class Patches_StorageSettings {
        private static Dictionary<StorageSettings,StorageLimits> limits = 
            new Dictionary<StorageSettings,StorageLimits>();

        public static StorageLimits Limits(this StorageSettings settings) {
            if (settings == null) return null;
            if (!limits.ContainsKey(settings)) {
                limits[settings] = new StorageLimits();
            }
            return limits[settings];
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(StorageSettings.ExposeData))]
        public static void ExposeData(StorageSettings __instance) {
            var limits = Limits(__instance);

            if (Scribe.mode == LoadSaveMode.Saving) {
                limits.Prune();
                if (!limits.HasData) return;
            }

            if (Scribe.EnterNode(Strings.ID)) {
                limits.ExposeData();
                Scribe.ExitNode();
            }
        }
/*
        [HarmonyPostfix]
        [HarmonyPatch(nameof(StorageSettings.AllowedToAccept), typeof(Thing))]
        public static void AllowedToAccept(Thing t, ref bool __result, StorageSettings __instance) {
            if (__result) {
                __result = Limits(__instance).AllowedToAccept(t, __instance.owner);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(StorageSettings.AllowedToAccept), typeof(ThingDef))]
        public static void AllowedToAccept_Def(ThingDef t, ref bool __result, StorageSettings __instance) {
            if (__result) {
                __result = Limits(__instance).AllowedToAccept(t, __instance.owner);
            }
        }
*/
    }
}
