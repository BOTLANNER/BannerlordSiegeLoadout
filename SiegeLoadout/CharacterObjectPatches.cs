using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;

namespace SiegeLoadout
{
    [HarmonyPatch]
    public static class CharacterObjectPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterObject), "AfterRegister")]

        public static void AfterRegister(ref CharacterObject __instance)
        {
            var extended = __instance.HeroObject.AsExtended();
            if (extended != null && extended.SiegeEquipment != null)
            {
                extended.SiegeEquipment.SyncEquipments = true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterObject), "get_AllEquipments")]

        public static bool AllEquipments(ref CharacterObject __instance, ref MBReadOnlyList<Equipment> __result)
        {
            if (!__instance.IsHero)
            {
                //__result = __instance.AllEquipments;
                return true;
            }

            List<Equipment> equipment = new List<Equipment>()
            {
                __instance.HeroObject.BattleEquipment

            };
            var extended = __instance.HeroObject.AsExtended();
            if (extended != null)
            {
                equipment.Add(extended.SiegeEquipment);
            }
            equipment.Add(__instance.HeroObject.CivilianEquipment);
            __result = equipment.GetReadOnlyList();
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterObject), "get_BattleEquipments")]
        public static bool BattleEquipments(ref CharacterObject __instance, ref IEnumerable<Equipment> __result)
        {
            if (__instance.IsHero)
            {
                List<Equipment> equipment = new List<Equipment>()
                {
                    __instance.HeroObject.BattleEquipment
                };
                var extended = __instance.HeroObject.AsExtended();
                if (extended != null)
                {
                    equipment.Add(extended.SiegeEquipment);
                }
                __result = equipment.AsEnumerable();
                return false;
            }
            __result = __instance.AllEquipments.WhereQ<Equipment>((Equipment e) => !e.IsCivilian);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterObject), "get_Equipment")]
        public static bool Equipment(ref CharacterObject __instance, ref Equipment __result)
        {
            if (!__instance.IsHero)
            {
                return true;
            }
            if (SiegeLoadoutBehavior.UseSiegeLoadouts)
            {

                var extended = __instance.HeroObject.AsExtended();
                if (extended != null)
                {
                    __result = extended.WithSiegeEquipment();
                    return false;
                } 
            }
            __result = __instance.HeroObject.BattleEquipment;
            return false;
        }
    }

}
