using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace StorageLimit {
    public class Limit : IExposable {
        public const float ControlHeight = 22f;
        public const float ControlGap    =  1f;
        public const float LineGap       =  2f;
        public const float ButtonMargin  = 12f;
        public const float SideMargin    =  8f;
        private const string Max = "Max:";

        public bool Active;

        private bool inStacks = true;
        private int max = 1;
        //private int min;
        //private StoragePriority newPrio;
        //private bool setBelow;

        public bool Open;
        public int StackSize = 75;

        private string maxBuffer;

        public void ExposeData() {
            Scribe_Values.Look(ref Active,   "active");
            Scribe_Values.Look(ref inStacks, "inStacks", true);
            Scribe_Values.Look(ref max,      "max");
            //Scribe_Values.Look(ref min,      "min");
            //Scribe_Values.Look(ref newPrio,  "newPrio");
            //Scribe_Values.Look(ref setBelow, "setBelow");
        }

        public void Toggle() => Active = !Active;

        public bool IsDefault => !Active && inStacks && max == 1;

        public string Tooltip => $"Stores a maximum of {max} {Type} of this";

        public string SettingHint => inStacks ? $"({max}S)" : $"({max})";

        public string Type => inStacks ? "stacks" : "items";

        public void DoControls(float width, float topY, ref float curY) {
            var row = new Rect(SideMargin, curY, width - 2 * SideMargin, ControlHeight);

            var rect = row;
            Widgets.Label(rect, Max);
            rect.xMin += Text.CalcSize(Max).x + 2 * ControlGap;
            Dropdown(ref rect, HorizontalJustification.Right, 
                     inStacks ? 0 : 1, 
                     i => inStacks = i == 0, 
                     "Stacks", "Items");
            IntEntry(rect, 1, ref max, ref maxBuffer);
            curY += ControlHeight + LineGap;

            var mouseArea = new Rect(0f, topY, width, curY - topY);
            if (!Mouse.IsOver(mouseArea)) Open = false;
        }

        public void IntEntry(Rect rect, int min, ref int value, ref string editBuffer) {
            int stack = Mathf.Min(StackSize, 100);
            var buttonWidth = Mathf.Round(Mathf.Min(
                (int) rect.width / 3,
                Mathf.Max(
                    Text.CalcSize((-stack).ToStringCached()).x + ButtonMargin, 
                    (int) rect.width / 4)));
            int multiplier = 1;
            if (KeyBindingDefOf.ModifierIncrement_10x .IsDownEvent) multiplier = 10;
            if (KeyBindingDefOf.ModifierIncrement_100x.IsDownEvent) multiplier = stack;

            if (Widgets.ButtonText(new Rect(rect.xMin, rect.yMin, buttonWidth, rect.height), (-1 * multiplier).ToStringCached())) {
                value -= multiplier;
                editBuffer = value.ToStringCached();
                SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
            }

            if (Widgets.ButtonText(new Rect(rect.xMax - buttonWidth, rect.yMin, buttonWidth, rect.height), "+" + multiplier.ToStringCached())) {
                if (multiplier > 1 && value == 1) {
                    value = multiplier;
                } else {
                    value += multiplier;
                }
                editBuffer = value.ToStringCached();
                SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
            }

            var step = buttonWidth + ControlGap;
            var textRect = new Rect(rect.xMin + step, rect.yMin, rect.width - 2 * step, rect.height);
            var old = Text.textFieldStyles[1].alignment;
            Text.textFieldStyles[1].alignment = TextAnchor.MiddleCenter;
            Widgets.TextFieldNumeric(textRect, ref value, ref editBuffer, min);
            Text.textFieldStyles[1].alignment = old;
        }

        private void Dropdown(ref Rect rect, HorizontalJustification align, int sel, Action<int> set, params string[] options) {
            float width = options.Max(s => Text.CalcSize(s).x) + ButtonMargin;
            bool left = align == HorizontalJustification.Left;
            var button = left ? rect.LeftPartPixels(width) : rect.RightPartPixels(width);
            var setLocal = set;
            if (Widgets.ButtonText(button, options[sel])) {
                if (options.Length == 2) { 
                    set(1 - sel);
                } else {
                    var opts = options.Select((s, i) => new FloatMenuOption(s, () => setLocal(i)));
                    Find.WindowStack.Add(new FloatMenu(opts.ToList()));
                }
            }
            if (left) {
                rect.xMin += width + ControlGap;
            } else {
                rect.xMax -= width + ControlGap;
            }
        }

        public int SpaceLeft((int stacks, int items) used, int stackSize, int stackAdjust = 0) 
            => inStacks ? (max - used.stacks) * stackSize + stackAdjust : max - used.items;
    }

}
