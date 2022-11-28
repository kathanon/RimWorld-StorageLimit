using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace StorageLimit {
    [HarmonyPatch]
    public static class Patches_Haul_Misc {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GridsUtility), nameof(GridsUtility.GetItemStackSpaceLeftFor))]
        public static void GetItemStackSpaceLeftFor(ref int __result, IntVec3 c, Map map, ThingDef itemDef) {
            ISlotGroup slots = map.haulDestinationManager.SlotGroupAt(c);
            slots = slots?.StorageGroup ?? slots;
            if (slots != null) {
                var space = slots.Settings.Limits().SpaceToLimit(itemDef, slots);
                __result = Mathf.Min(space, __result);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HaulAIUtility), nameof(HaulAIUtility.HaulToCellStorageJob))]
        public static void HaulToCellStorageJob(ref Job __result) {
            if (__result != null && __result.count <= 0) {
                __result = null;
                JobFailReason.Is(HaulAIUtility.NoEmptyPlaceLowerTrans);
            }
        }
    }
}
