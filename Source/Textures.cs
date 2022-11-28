using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace StorageLimit {
    [StaticConstructorOnStartup]
    public static class Textures {
        private const string Prefix = Strings.ID + "/";

        public static readonly Texture2D LimitIcon = ContentFinder<Texture2D>.Get(Prefix + "LimitIcon");

        public static bool Valid(this Texture2D tex) => tex != null && tex != BaseContent.BadTex;
    }
}
