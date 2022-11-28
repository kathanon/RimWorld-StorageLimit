using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace StorageLimit {
    [HarmonyPatch(typeof(Listing_TreeThingFilter))]
    public static class Patches_ListingTreeThing {
        private const float HintMargin = 8f;

        private static float width;
        private static float space;
        private static Rect row;
        private static bool show;
        private static TreeNode_ThingCategory treeNode = null;

        public static StorageLimits Current;

        [HarmonyPrefix]
        [HarmonyPatch("DoCategory")]
        public static void DoCategory_Pre(int indentLevel, TreeNode_ThingCategory node, 
                                          Listing_TreeThingFilter __instance,
                                          float ___curY, float ___lineHeight, 
                                          float ___nestIndentWidth, Rect ___visibleRect) {
            Prepare(indentLevel, ___nestIndentWidth, ___lineHeight, ___curY, ___visibleRect, __instance);
            treeNode = node;
        }

        [HarmonyPrefix]
        [HarmonyPatch("DoThingDef")]
        public static void DoThingDef_Pre(ThingDef tDef, int nestLevel, Listing_TreeThingFilter __instance,
                                          float ___curY, float ___lineHeight, float ___nestIndentWidth,
                                          Rect ___visibleRect) {
            if (tDef.uiIcon.Valid()) nestLevel++;
            Prepare(nestLevel, ___nestIndentWidth, ___lineHeight, ___curY, ___visibleRect, __instance);
            treeNode = null;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Listing_Tree), "LabelLeft")]
        public static void LabelLeft_Pre(string label, float widthOffset) 
            => space = Mathf.Max(0f, row.width - Text.CalcSize(label).x) - widthOffset - row.height - HintMargin;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Listing_Lines), "EndLine")]
        public static void EndLine_Post(ref float ___curY, Listing_Lines __instance) {
            if (treeNode != null && __instance is Listing_TreeThingFilter listing) {
                if (show && listing.AllowanceStateOf(treeNode) != MultiCheckboxState.Off) {
                    Current.DoUIFor(treeNode.catDef, row, width, space, ref ___curY);
                }
                show = false;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("DoThingDef")]
        public static void DoThingDef_Post(ThingDef tDef, ref float ___curY, ThingFilter ___filter) {
            if (show && ___filter.Allows(tDef)) {
                Current.DoUIFor(tDef, row, width, space, ref ___curY);
            }
            show = false;
        }

        private static void Prepare(int indent, float indentWidth, float height, float curY, Rect visible, 
                                    Listing_TreeThingFilter view) {
            if (Current != null) {
                width = Traverse.Create(view).Property<float>("ColumnWidth").Value;
                row = new Rect(0f, curY, width, height);
                row.xMin += indent * indentWidth + 18f;
                show = visible.Overlaps(row);
            } else {
                show = false;
            }
        }
    }
}
