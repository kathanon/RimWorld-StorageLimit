using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageLimit {
    [HarmonyPatch(typeof(ITab_Storage))]
    public static class Patches_ITabStorage {
        [HarmonyPrefix]
        [HarmonyPatch("FillTab")]
        public static void FillTab_Pre(ITab_Storage __instance) 
            => Patches_ListingTreeThing.Current = __instance.GetStoreSettings().Limits();

        [HarmonyPostfix]
        [HarmonyPatch("FillTab")]
        public static void FillTab_Post(ITab_Storage __instance) 
            => Patches_ListingTreeThing.Current = null;

        public static StorageSettings GetStoreSettings(this ITab_Storage tab) 
            => Traverse.Create(tab).Property<IStoreSettingsParent>("SelStoreSettingsParent").Value.GetStoreSettings();
    }
}
