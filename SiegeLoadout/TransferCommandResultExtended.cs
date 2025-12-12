
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
            harmony.Patch(AccessTools.Method(typeof(TransferCommandResult), "get_ResultSideEquipment"), new HarmonyMethod(typeof(TransferCommandResultExtended), nameof(GetResultSideEquipment)));
        }

        static bool GetResultSideEquipment(ref Equipment? __result, ref TransferCommandResult __instance)
        {
            if (__instance is TransferCommandResultExtended resultExtended && resultExtended.IsSiegeEquipment)
            {
                if (resultExtended.ResultSide == InventoryLogic.InventorySide.BattleEquipment)
                {
                    CharacterObject transferCharacter = resultExtended.TransferCharacter;
                    if (transferCharacter != null)
                    {
                        __result = transferCharacter.GetSiegeEquipment();
                        return false;
                    }
                }
            }
            return true;
        }

        public TransferCommandResultExtended() : base()
        {

        }
        public TransferCommandResultExtended(InventoryLogic.InventorySide resultSide, ItemRosterElement effectedItemRosterElement, int effectedNumber, int finalNumber, EquipmentIndex effectedEquipmentIndex, CharacterObject transferCharacter, bool isSiegeEquipment) : base(resultSide, effectedItemRosterElement, effectedNumber, finalNumber, effectedEquipmentIndex, transferCharacter)
        {
            this.IsSiegeEquipment = isSiegeEquipment;
        }
    }

}
