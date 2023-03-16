
using System;

using HarmonyLib;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace SiegeLoadout
{
    [HarmonyPatch(typeof(DefaultTournamentModel))]
    public static class DefaultTournamentModelPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GetParticipantArmor))]

        public static bool GetParticipantArmor(CharacterObject participant, ref Equipment __result,ref DefaultTournamentModel __instance)
        {
            if (CampaignMission.Current == null || CampaignMission.Current.Mode == MissionMode.Tournament || Settlement.CurrentSettlement == null)
            {
                if (participant.IsHero && Settings.Instance!.EnableSiegeLoadouts && Settings.Instance!.UseSiegeArmorInTournaments)
                {
                    var extended = participant.HeroObject.AsExtended();
                    if (extended != null)
                    {
                        __result = extended.WithSiegeEquipment();
                        return false;
                    }
                }
                __result = participant.RandomBattleEquipment;
                return false;
            }
            __result = (Game.Current.ObjectManager.GetObject<CharacterObject>(String.Concat("gear_practice_dummy_", Settlement.CurrentSettlement.MapFaction.Culture.StringId)) ?? Game.Current.ObjectManager.GetObject<CharacterObject>("gear_practice_dummy_empire")).RandomBattleEquipment;
            return false;
        }
    }

}
