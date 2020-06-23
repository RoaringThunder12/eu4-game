using System;
using System.Reflection;
using Harmony;
using RimWorld;
using Verse;
using UnityEngine;

namespace yayoCombat
{
    [HarmonyPatch(typeof(Projectile))]
    [HarmonyPatch("StartingTicksToImpact", PropertyMethod.Getter)]
    static class yayoStartingTicksToImpact
    {
        public static bool Prefix(Projectile __instance, ref int __result)
        {
            Vector3 origin = Traverse.Create(__instance).Field("origin").GetValue<Vector3>();
            Vector3 destination = Traverse.Create(__instance).Field("destination").GetValue<Vector3>();

            int num = Mathf.RoundToInt((origin - destination).magnitude / (__instance.def.projectile.speed * 3f / 100f));
            if (num < 1)
            {
                num = 1;
            }
            __result = num;
            return false;
        }
    }
    

}
