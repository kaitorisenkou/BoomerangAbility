using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace BoomerangAbility {
    [StaticConstructorOnStartup]
    public class BoomerangAbility {
        static BoomerangAbility() {
            Log.Message("[BoomerangAbility] Now active");
            var harmony = new Harmony("kaitorisenkou.BoomerangAbility");
            harmony.Patch(
                AccessTools.Method(typeof(PawnRenderUtility), nameof(PawnRenderUtility.CarryWeaponOpenly), null, null),
                    null,
                    new HarmonyMethod(typeof(BoomerangAbility), nameof(Patch_CarryWeaponOpenly), null),
                    null,
                    null
                    );
            Log.Message("[BoomerangAbility] Harmony patch complete!");

        }
        public static void Patch_CarryWeaponOpenly(ref bool __result, Pawn pawn) {
            if (!__result) return;
            if(Projectile_Boomerang.thrownPawns.ContainsKey(pawn) && Projectile_Boomerang.thrownPawns[pawn]) {
                __result = false;
            }
        }
    }
}
