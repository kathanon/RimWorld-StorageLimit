using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageLimit {
    [HarmonyPatch(typeof(StoreUtility))]
    public static class Patches_StoreUtility {
    }
}
