using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace StorageLimit {
    public class StorageLimits : IExposable {
        private Dictionary<ThingDef, Limit> thingLimits = 
            new Dictionary<ThingDef, Limit>();
        private Dictionary<ThingCategoryDef, Limit> catLimits = 
            new Dictionary<ThingCategoryDef, Limit>();

        public void ExposeData() {
            Scribe_Collections.Look(ref thingLimits, "things",     LookMode.Def, LookMode.Deep);
            Scribe_Collections.Look(ref catLimits,   "categories", LookMode.Def, LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit) {
                foreach ((var thing, var limits) in thingLimits) {
                    limits.StackSize = thing.stackLimit;
                }
            }
        }

        public void Prune() {
            thingLimits.RemoveAll(p => p.Value.IsDefault);
            catLimits  .RemoveAll(p => p.Value.IsDefault);
        }

        public bool HasData => thingLimits.Count > 0 || catLimits.Count > 0;

        public Limit this[ThingDef t]         => For(t, thingLimits, t.stackLimit);
        public Limit this[ThingCategoryDef t] => For(t, catLimits);

        private Limit For<T>(T key, Dictionary<T, Limit> limits, int stackSize = 0) {
            if (!limits.ContainsKey(key)) {
                var lim = new Limit();
                limits[key] = lim;
                if (stackSize > 0) lim.StackSize = stackSize;
            }
            return limits[key];
        }

        private Limit TryGet(ThingDef t)         => thingLimits.GetWithFallback(t);
        private Limit TryGet(ThingCategoryDef t) => catLimits  .GetWithFallback(t);

        /*
        public bool AllowedToAccept(Thing t, IStoreSettingsParent owner) 
            => AllowedToAccept(t.def, owner, t.stackCount);

        public bool AllowedToAccept(ThingDef t, IStoreSettingsParent owner, int count = 1) => true;
        */

        public bool Active(ThingDef t)         => thingLimits.TryGetValue(t, out var lim) && lim.Active;
        public bool Active(ThingCategoryDef t) => catLimits  .TryGetValue(t, out var lim) && lim.Active;

        public void DoUIFor(ThingDef t,         Rect row, float width, float space, ref float curY) 
            => DoUIFor(TryGet(t), row, width, space, ref curY, () => this[t]);
        public void DoUIFor(ThingCategoryDef t, Rect row, float width, float space, ref float curY) 
            => DoUIFor(TryGet(t), row, width, space, ref curY, () => this[t]);

        private void DoUIFor(Limit lim, Rect row, float width, float space, ref float curY, Func<Limit> get) {
            bool active = lim?.Active ?? false;
            if (Mouse.IsOver(row) || active) {
                float iconOffset = row.height + 26f;
                Rect icon = new Rect(row.xMax - iconOffset, row.y, row.height, row.height);
                TooltipHandler.TipRegion(icon, active ? lim.Tooltip : "Set a maximum amount for this");
                if (Widgets.ButtonImage(icon, Textures.LimitIcon,
                                        active ? Color.white : Color.grey,
                                        active ? GenUI.MouseoverColor : Color.white)) {
                    (lim ?? get()).Toggle();
                }
                if (active) {
                    if (Mouse.IsOver(icon)) {
                        lim.Open = true;
                    }

                    var hint = lim.SettingHint;
                    Text.Font = GameFont.Tiny;
                    space -= iconOffset;
                    if (Text.CalcSize(hint).x < space) {
                        Text.Anchor = TextAnchor.MiddleRight;
                        GUI.color = Color.gray;
                        icon.x -= space;
                        icon.width = space;
                        Widgets.Label(icon, hint);
                        GUI.color = Color.white;
                        GenUI.ResetLabelAlign();
                    }
                    Text.Font = GameFont.Small;
                }
            }
            if (active && lim.Open) {
                lim.DoControls(width, row.y, ref curY);
            }
        }

        public int SpaceToLimit(ThingDef t, ISlotGroup slots) {
            var counter = new ItemCounter(t);
            foreach (var item in slots.HeldThings) { 
                counter.Add(item);
            }
            int stackSize = t.stackLimit;
            counter.Compact(stackSize);
            var forThing = counter[t];
            int stackAdjust = (stackSize - forThing.items % stackSize) % stackSize;
            int space = TryGet(t)?.SpaceLeft(forThing, stackSize) ?? int.MaxValue;
            foreach (var cat in counter.Categories) {
                var lim = TryGet(cat);
                if (lim != null) {
                    space = Mathf.Min(space, lim.SpaceLeft(counter[cat], stackSize, stackAdjust));
                }
            }
            return space;
        }

        private class ItemCounter {
            private readonly Dictionary<object, int> indices = new Dictionary<object, int>();
            private readonly int[] sums;

            public ItemCounter() {}

            public ItemCounter(ThingDef t) {
                int n = 0;
                foreach (var item in GetKeysFor(t)) {
                    indices[item] = n;
                    n += 2;
                }
                sums = new int[n];
            }

            public void Compact(int stackSize) {
                int compacted = (sums[1] + stackSize - 1) / stackSize;
                int removed = sums[0] - compacted;
                if (removed > 0) {
                    for (int i = 0; i < sums.Length; i += 2) {
                        sums[i] -= removed;
                    }
                }
            }

            public (int stacks, int items) this[object key] {
                get {
                    if (indices.TryGetValue(key, out int i)) {
                        return (sums[i], sums[i+1]);
                    }
                    return (0, 0);
                }
            }

            public void Add(Thing t) {
                foreach (var item in GetKeysFor(t.def)) {
                    if (indices.TryGetValue(item, out int i)) {
                        sums[i]++;
                        sums[i + 1] += t.stackCount;
                    }
                }
            }

            public IEnumerable<ThingCategoryDef> Categories
                => indices.Keys.OfType<ThingCategoryDef>();

            private IEnumerable<object> GetKeysFor(ThingDef t) {
                yield return t;
                foreach (var immediate in t.thingCategories) {
                    yield return immediate;
                    foreach (var parent in immediate.Parents) {
                        yield return parent;
                    }
                }
            }
        }
    }
}
