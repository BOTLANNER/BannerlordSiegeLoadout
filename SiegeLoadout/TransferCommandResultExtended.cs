
using HarmonyLib;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.Core;

namespace SiegeLoadout
{
    public class TransferCommandResultExtended : TransferCommandResult
    {
        public bool IsSiegeEquipment
        {
            get;
            private set;
        }

        static TransferCommandResultExtended()
        {
            var harmony = new Harmony($"{Main.HarmonyDomain}.{nameof(TransferCommand).ToLower()}");
            harmony.Patch(AccessTools.Method(typeof(TransferCommandResult), "get_TransferEquipment"), new HarmonyMethod(typeof(TransferCommandResultExtended), nameof(GetTransferEquipment)));
        }

        static bool GetTransferEquipment(ref Equipment? __result, ref TransferCommandResult __instance)
        {
            if (__instance is TransferCommandResultExtended resultExtended)
            {
                if (!resultExtended.IsCivilianEquipment)
                {
                    CharacterObject transferCharacter = resultExtended.TransferCharacter;
                    if (transferCharacter != null)
                    {
                        if (resultExtended.IsSiegeEquipment)
                        {
                            __result = transferCharacter.GetSiegeEquipment();
                        }
                        else
                        {
                            __result = transferCharacter.FirstBattleEquipment;
                        }
                        return false;
                    }
                    __result = null;
                    return false;
                }
                CharacterObject characterObject = resultExtended.TransferCharacter;
                if (characterObject != null)
                {
                    __result = characterObject.FirstCivilianEquipment;
                    return false;
                }
                __result = null;
                return false;
            }
            return true;
        }

        public TransferCommandResultExtended() : base()
        {

        }
        public TransferCommandResultExtended(InventoryLogic.InventorySide resultSide, ItemRosterElement effectedItemRosterElement, int effectedNumber, int finalNumber, EquipmentIndex effectedEquipmentIndex, CharacterObject transferCharacter, bool isCivilianEquipment, bool isSiegeEquipment) : base(resultSide, effectedItemRosterElement, effectedNumber, finalNumber, effectedEquipmentIndex, transferCharacter, isCivilianEquipment)
        {
            this.IsSiegeEquipment = isSiegeEquipment;
        }
    }

}
