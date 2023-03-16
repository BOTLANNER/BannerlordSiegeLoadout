using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace SiegeLoadout
{
    [HarmonyPatch(typeof(Hero))]
    public static class HeroPatches
    {
        [HarmonyPatch(nameof(CheckInvalidEquipmentsAndReplaceIfNeeded))]
        [HarmonyPrefix]
        public static void CheckInvalidEquipmentsAndReplaceIfNeeded(ref Hero __instance)
        {
            EquipmentElement item;
            bool flag;
            var extendedHero = __instance.AsExtended();
            if (extendedHero == null)
            {
                return;
            }

            for (int i = 0; i < 12; i++)
            {
                if (extendedHero.SiegeEquipment[i].Item == DefaultItems.Trash)
                {
                    extendedHero.HandleInvalidItem(i);
                }
                else if (extendedHero.SiegeEquipment[i].Item != null)
                {
                    if (!extendedHero.SiegeEquipment[i].Item.IsReady)
                    {
                        if (MBObjectManager.Instance.GetObject(extendedHero.SiegeEquipment[i].Item.Id) == extendedHero.SiegeEquipment[i].Item)
                        {
                            MBObjectManager instance = MBObjectManager.Instance;
                            item = extendedHero.SiegeEquipment[i];
                            instance.UnregisterObject(item.Item);
                        }
                        extendedHero.HandleInvalidItem(i);
                        MobileParty partyBelongedTo = __instance.PartyBelongedTo;
                        if (partyBelongedTo != null)
                        {
                            partyBelongedTo.ItemRoster.AddToCounts(DefaultItems.Trash, 1);
                        }
                        else
                        {
                        }
                    }
                    item = extendedHero.SiegeEquipment[i];
                    ItemModifier itemModifier = item.ItemModifier;
                    if (itemModifier != null)
                    {
                        flag = !itemModifier.IsReady;
                    }
                    else
                    {
                        flag = false;
                    }
                    if (flag)
                    {
                        extendedHero.HandleInvalidModifier(i);
                    }
                }
            }
        }


        [HarmonyPatch(nameof(ResetEquipments))]
        [HarmonyPostfix]
        public static void ResetEquipments(ref Hero __instance)
        {
            var extended = __instance.AsExtended();
            if (extended != null)
            {
                extended.SiegeEquipment = __instance.Template.FirstBattleEquipment.Clone(false);
            }
        }

        private static void HandleInvalidItem(this HeroExtended extendedHero, int i)
        {
            if (extendedHero.Hero.IsHumanPlayerCharacter)
            {
                extendedHero.SiegeEquipment[i] = EquipmentElement.Invalid;
                return;
            }

            List<Equipment> equipment = (
                from t in extendedHero.Hero.CharacterObject.BattleEquipments
                where !t.IsEmpty()
                select t).ToList<Equipment>().ToList<Equipment>();
            EquipmentElement item = equipment[extendedHero.Hero.RandomInt(equipment.Count)][i];
            if (item.IsEmpty || !item.Item.IsReady)
            {
                item = EquipmentElement.Invalid;
            }
            EquipmentElement equipmentElement = extendedHero.SiegeEquipment[i];

            extendedHero.SiegeEquipment[i] = item;
        }

        private static void HandleInvalidModifier(this HeroExtended extendedHero, int i)
        {
            EquipmentElement item;

            Equipment siegeEquipment = extendedHero.SiegeEquipment;
            item = extendedHero.SiegeEquipment[i];
            siegeEquipment[i] = new EquipmentElement(item.Item, null, null, false);
        }

    }

}
