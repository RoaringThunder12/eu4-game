using System;
using System.Reflection;
using Harmony;
using RimWorld;
using Verse;
using UnityEngine;

namespace yayoCombat
{
    
    public class yyUtil
    {
        /*
        public enum RangeCategory : byte
        {
            Touch,
            Short,
            Medium,
            Long
        }

        static public float AdjustedAccuracy(RangeCategory cat, Thing equipment)
        {
            StatDef stat = null;
            switch (cat)
            {
                case RangeCategory.Touch:
                    stat = StatDefOf.AccuracyTouch;
                    break;
                case RangeCategory.Short:
                    stat = StatDefOf.AccuracyShort;
                    break;
                case RangeCategory.Medium:
                    stat = StatDefOf.AccuracyMedium;
                    break;
                case RangeCategory.Long:
                    stat = StatDefOf.AccuracyLong;
                    break;
            }
            return equipment.GetStatValue(stat, true);

        }

        static public float GetEquipFactor(Thing equipment, float dist)
        {
            float value;
            if (dist <= 3f)
            {
                value = AdjustedAccuracy(RangeCategory.Touch, equipment);
            }
            else if (dist <= 12f)
            {
                value = Mathf.Lerp(AdjustedAccuracy(RangeCategory.Touch, equipment), AdjustedAccuracy(RangeCategory.Short, equipment), (dist - 3f) / 9f);
            }
            else if (dist <= 25f)
            {
                value = Mathf.Lerp(AdjustedAccuracy(RangeCategory.Short, equipment), AdjustedAccuracy(RangeCategory.Medium, equipment), (dist - 12f) / 13f);
            }
            else if (dist <= 40f)
            {
                value = Mathf.Lerp(AdjustedAccuracy(RangeCategory.Medium, equipment), AdjustedAccuracy(RangeCategory.Long, equipment), (dist - 25f) / 15f);
            }
            else
            {
                value = AdjustedAccuracy(RangeCategory.Long, equipment);
            }
            return Mathf.Clamp(value, 0.01f, 1f);
        }
        */


    }
    

}
