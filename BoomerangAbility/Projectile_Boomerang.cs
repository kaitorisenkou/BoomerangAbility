using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace BoomerangAbility {
    public class Projectile_Boomerang : Projectile {
        public static Dictionary<Pawn, bool> thrownPawns = new Dictionary<Pawn, bool>();

        List<Thing> multipleTargets = null;
        int targetIndex = 0;
        Thing equipment = null;
        IEnumerable<DamageInfo> dinfos = null;
        Vector3 angle = Vector3.zero;

        public void InitAndLaunchBoomerang(IEnumerable<Thing> targets, Pawn pawn, Thing equipment) {
            thrownPawns[pawn] = true;
            multipleTargets = targets.ToList();
            targetIndex = 0;
            this.equipment = equipment;
            angle = (multipleTargets[0].Position - pawn.Position).ToVector3();
            dinfos = GetDamageInfos(pawn,equipment.TryGetComp<CompEquippable>());
            Launch(pawn, pawn.DrawPos, multipleTargets[targetIndex], multipleTargets[targetIndex], ProjectileHitFlags.IntendedTarget, false, equipment, null);
        }

        protected IEnumerable<DamageInfo> GetDamageInfos(Pawn pawn, CompEquippable equipment) {
            bool instigatorGuilty = (pawn == null || !pawn.Drafted);
            equipment.parent.TryGetQuality(out QualityCategory quality);
            foreach (var tool in equipment.Tools) {
                foreach(var capa in tool.capacities) {
                    var damageDef = capa.Maneuvers.FirstOrDefault().verb?.meleeDamageDef;
                    if (damageDef != null) {
                        var dinfo = new DamageInfo(damageDef, tool.power, Mathf.Max(tool.armorPenetration, tool.power * 0.015f), instigator: launcher, category: DamageInfo.SourceCategory.ThingOrUnknown, instigatorGuilty: instigatorGuilty);
                        dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
                        dinfo.SetWeaponBodyPartGroup(tool.linkedBodyPartsGroup);
                        dinfo.SetWeaponHediff(damageDef.hediff);
                        dinfo.SetTool(tool);
                        dinfo.SetWeaponQuality(quality);
#if DEBUG
                        Log.Message("[Boomerang]"+tool.label+", "+ damageDef);
#endif
                        yield return dinfo;
                    }
                }
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false) {
            if (targetIndex < multipleTargets.Count) {
                //base.Impact(hitThing, blockedByShield);
                Map map = base.Map;
                IntVec3 position = base.Position;
                BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(this.launcher, hitThing, this.intendedTarget.Thing, this.equipmentDef, this.def, this.targetCoverDef);
                Find.BattleLog.Add(battleLogEntry_RangedImpact);
                if (hitThing != null && !hitThing.Destroyed) {
                    DamageInfo dinfo = dinfos.RandomElementByWeight(t => t.Amount);
                    dinfo.SetAngle(angle);
                    hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
                }
            } else {
                thrownPawns[launcher as Pawn] = false;
                this.Destroy(DestroyMode.Vanish);
                return;
            }

            targetIndex++;
            if (targetIndex < multipleTargets.Count) {
                angle = (multipleTargets[targetIndex].Position - multipleTargets[targetIndex - 1].Position).ToVector3();
                Launch(launcher, multipleTargets[targetIndex - 1].Position.ToVector3(), multipleTargets[targetIndex], multipleTargets[targetIndex], ProjectileHitFlags.IntendedTarget, false, equipment, null);
            } else {
                Launch(launcher, multipleTargets[targetIndex - 1].Position.ToVector3(), launcher, launcher, ProjectileHitFlags.IntendedTarget, false, equipment, null);
            }
        }

        public override Material DrawMat {
            get {
                if (equipment != null) {
                    return equipmentDef.graphic.MatSingleFor(equipment);
                }
                return base.DrawMat;
            }
        }
    }
}
