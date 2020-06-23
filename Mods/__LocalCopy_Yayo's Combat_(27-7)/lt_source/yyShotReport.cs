using System;
using System.Reflection;
using Harmony;
using RimWorld;
using Verse;
using UnityEngine;

namespace yayoCombat
{
    
    internal class yyShotReport : Mod
    {
        
        public yyShotReport(ModContentPack content) : base(content)
        {
            HarmonyInstance.Create("yayo.yyShotReport").PatchAll(Assembly.GetExecutingAssembly());
        }

        [HarmonyPriority(800)]
        [HarmonyPatch(typeof(Verb_LaunchProjectile), "TryCastShot")]
        public class yayoTryCastShot
        {
            public static bool Prefix(ref bool __result, Verb_LaunchProjectile __instance)
            {
                LocalTargetInfo currentTarget = Traverse.Create(__instance).Field("currentTarget").GetValue<LocalTargetInfo>();
                bool canHitNonTargetPawnsNow = Traverse.Create(__instance).Field("canHitNonTargetPawnsNow ").GetValue<bool>();


                if (currentTarget.HasThing && currentTarget.Thing.Map != __instance.caster.Map)
                {
                    __result = false;
                    return false;
                }
                ThingDef projectile = __instance.Projectile;
                
                if (projectile == null)
                {
                    __result = false;
                    return false;
                }
                ShootLine shootLine;
                bool flag = __instance.TryFindShootLineFromTo(__instance.caster.Position, currentTarget, out shootLine);
                if (__instance.verbProps.stopBurstWithoutLos && !flag)
                {
                    __result = false;
                    return false;
                }
                if (__instance.EquipmentSource != null)
                {
                    CompChangeableProjectile comp = __instance.EquipmentSource.GetComp<CompChangeableProjectile>();
                    if (comp != null)
                    {
                        comp.Notify_ProjectileLaunched();
                    }
                }
                Thing launcher = __instance.caster;
                Thing equipment = __instance.EquipmentSource;
                CompMannable compMannable = __instance.caster.TryGetComp<CompMannable>();
                if (compMannable != null && compMannable.ManningPawn != null)
                {
                    launcher = compMannable.ManningPawn;
                    equipment = __instance.caster;
                }
                Vector3 drawPos = __instance.caster.DrawPos;
                Projectile projectile2 = (Projectile)GenSpawn.Spawn(projectile, shootLine.Source, __instance.caster.Map, WipeMode.Vanish);
                

                if (__instance.verbProps.forcedMissRadius > 0.5f)
                {
                    float num = VerbUtility.CalculateAdjustedForcedMiss(__instance.verbProps.forcedMissRadius, currentTarget.Cell - __instance.caster.Position);
                    if (num > 0.5f)
                    {
                        int max = GenRadial.NumCellsInRadius(num);
                        int num2 = Rand.Range(0, max);
                        if (num2 > 0)
                        {
                            IntVec3 c = currentTarget.Cell + GenRadial.RadialPattern[num2];
                            ProjectileHitFlags projectileHitFlags = ProjectileHitFlags.NonTargetWorld;
                            if (Rand.Chance(0.5f))
                            {
                                projectileHitFlags = ProjectileHitFlags.All;
                            }
                            if (!canHitNonTargetPawnsNow)
                            {
                                projectileHitFlags &= ~ProjectileHitFlags.NonTargetPawns;
                            }
                            projectile2.Launch(launcher, drawPos, c, currentTarget, projectileHitFlags, equipment, null);
                            __result = true;
                            return false;
                        }
                    }
                }
                ShotReport shotReport = ShotReport.HitReportFor(__instance.caster, __instance, currentTarget);
                Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
                ThingDef targetCoverDef = (randomCoverToMissInto == null) ? null : randomCoverToMissInto.def;

                // yayo
                bool isEquip = (__instance.EquipmentSource != null) && (__instance.EquipmentSource.def.equipmentType != null);

                float missR = (1f - shotReport.AimOnTargetChance_IgnoringPosture);

                float factorStat = 0.95f;
                float factorSkill = 0.3f;
                if (__instance.CasterIsPawn)
                {
                    if (__instance.CasterPawn.skills != null)
                    {
                        factorSkill = (float)__instance.CasterPawn.skills.GetSkill(SkillDefOf.Shooting).levelInt / 20f;

                    }
                    factorStat = 1f - __instance.caster.GetStatValue(StatDefOf.ShootingAccuracyPawn, true) * factorSkill;
                }
                else
                {
                    factorStat = 1f - factorStat * factorSkill;
                }
                
                


                float factorEquip = 1f - __instance.verbProps.GetHitChanceFactor(__instance.EquipmentSource, (currentTarget.Cell - __instance.caster.Position).LengthHorizontal);

                float factorGas = 1f;

                float factorWeather = 1f;
                if (!__instance.caster.Position.Roofed(__instance.caster.Map) || !currentTarget.Cell.Roofed(__instance.caster.Map))
                {
                    factorWeather = __instance.caster.Map.weatherManager.CurWeatherAccuracyMultiplier;
                }
                else
                {
                    factorWeather = 1f;
                }

                float factorAir = factorGas * factorWeather;

                

                if (isEquip)
                {
                    missR *= ((0.25f * factorStat) + 0.55f * factorEquip * factorStat + 0.2f) * factorAir + (1f - factorAir);
                }
                missR = missR * 0.95f + 0.05f;
                Mathf.Clamp(missR, 0.05f, 0.95f);

                /*
                Log.Error("factorStat " + factorStat.ToString());
                Log.Error("factorEquip " + factorEquip.ToString());
                Log.Error("factorAir " + factorAir.ToString());
                Log.Error("missR " + missR.ToString());
                */

                if (Rand.Chance(missR))
                {
                    shootLine.ChangeDestToMissWild(shotReport.AimOnTargetChance_StandardTarget);
                    ProjectileHitFlags projectileHitFlags2 = ProjectileHitFlags.NonTargetWorld;
                    if (Rand.Chance(0.5f) && canHitNonTargetPawnsNow)
                    {
                        projectileHitFlags2 |= ProjectileHitFlags.NonTargetPawns;
                    }
                    projectile2.Launch(launcher, drawPos, shootLine.Dest, currentTarget, projectileHitFlags2, equipment, targetCoverDef);
                    __result = true;
                    return false;
                }
                
                if (currentTarget.Thing != null && currentTarget.Thing.def.category == ThingCategory.Pawn && !Rand.Chance(shotReport.PassCoverChance))
                {
                    shootLine.ChangeDestToMissWild(shotReport.AimOnTargetChance_StandardTarget);
                    ProjectileHitFlags projectileHitFlags3 = ProjectileHitFlags.NonTargetWorld;
                    if (canHitNonTargetPawnsNow)
                    {
                        projectileHitFlags3 |= ProjectileHitFlags.NonTargetPawns;
                    }
                    projectile2.Launch(launcher, drawPos, shootLine.Dest, currentTarget, projectileHitFlags3, equipment, targetCoverDef);
                    __result = true;
                    return false;
                }
                ProjectileHitFlags projectileHitFlags4 = ProjectileHitFlags.IntendedTarget;
                if (canHitNonTargetPawnsNow)
                {
                    projectileHitFlags4 |= ProjectileHitFlags.NonTargetPawns;
                }
                if (!currentTarget.HasThing || currentTarget.Thing.def.Fillage == FillCategory.Full)
                {
                    projectileHitFlags4 |= ProjectileHitFlags.NonTargetWorld;
                }
                if (currentTarget.Thing != null)
                {
                    projectile2.Launch(launcher, drawPos, currentTarget, currentTarget, projectileHitFlags4, equipment, targetCoverDef);
                }
                else
                {
                    projectile2.Launch(launcher, drawPos, shootLine.Dest, currentTarget, projectileHitFlags4, equipment, targetCoverDef);
                }
                __result = true;
                return false;
            }
        }


        

    }
    
    




}
