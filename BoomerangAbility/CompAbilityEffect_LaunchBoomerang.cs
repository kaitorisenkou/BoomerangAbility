using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Runtime.Remoting.Lifetime;

namespace BoomerangAbility {
    public class CompAbilityEffect_LaunchBoomerang : CompAbilityEffect {
        public new CompProperties_AbilityLaunchBoomerang Props {
            get {
                return (CompProperties_AbilityLaunchBoomerang)this.props;
            }
        }

        IEnumerable<Thing> GetTargets(LocalTargetInfo target) {
            int i = 0;
            if (target.TryGetPawn(out Pawn centerPawn)) {
                yield return centerPawn;
                i++;
            }
            var things = GenRadial.RadialDistinctThingsAround(target.Cell, parent.pawn.Map, Props.targetDetectRadius, false);
            bool end = false;
            foreach (var thing in things) {
                if (thing is Pawn && thing.Faction != null && thing.Faction.HostileTo(parent.pawn.Faction)) {
                    yield return thing;
                    end = true;
                    i++;
                }
                if (i >= Props.targetAmount) yield break;
            }
            if (end) yield break;

            foreach (var thing in things) {
                if (thing is Pawn && thing.Faction == null) {
                    yield return thing;
                    end = true;
                    i++;
                }
                if (i >= Props.targetAmount) yield break;
            }
            if (end) yield break;

            foreach (var thing in things) {
                if (thing is Pawn) {
                    yield return thing;
                    i++;
                }
                if (i >= Props.targetAmount) yield break;
            }
            yield break;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
            base.Apply(target, dest);
            Launch(target);
        }
        private void Launch(LocalTargetInfo target) {
            if (this.Props.projectileDef != null) {
                var pawn = this.parent.pawn;
                var equipment = pawn.equipment.Primary;
                var proj = (Projectile_Boomerang)GenSpawn.Spawn(this.Props.projectileDef, pawn.Position, pawn.Map, WipeMode.Vanish);
                proj.InitAndLaunchBoomerang(GetTargets(target), pawn, equipment);
            }
        }

        public override void DrawEffectPreview(LocalTargetInfo target) {
            foreach (Thing t in this.GetTargets(target)) {
                GenDraw.DrawTargetHighlight(t);
            }
            GenDraw.DrawRadiusRing(target.Cell, Props.targetDetectRadius);
        }

        public override bool GizmoDisabled(out string reason) {
            var equipment = parent.pawn.equipment.Primary;
            if (equipment == null || !equipment.def.IsWeapon) {
                reason = "BoomerangRequireWeapon".Translate();
                return true;
            }
            if (Props.requireMeleeWeapon && !equipment.def.IsMeleeWeapon) {
                reason = "BoomerangRequireMelee".Translate();
                return true;
            }
            return base.GizmoDisabled(out reason);
        }
    }
    public class CompProperties_AbilityLaunchBoomerang : CompProperties_AbilityEffect {
        public bool requireMeleeWeapon = true;
        public int targetAmount = 1;
        public float targetDetectRadius = 5;
        public ThingDef projectileDef;

        public override IEnumerable<string> ConfigErrors(AbilityDef parentDef) {
            foreach(var i in base.ConfigErrors(parentDef)) {
                yield return i;
            }
            if (!typeof(Projectile_Boomerang).IsAssignableFrom(projectileDef.thingClass)) {
                yield return "projectileDef's thing class must be Projectile_Boomerang";
            }
        }
    }
}
