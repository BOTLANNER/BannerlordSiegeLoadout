
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.Core;

namespace SiegeLoadout
{
    public struct TransferCommandExtended
    {
        public TransferCommand Original;
        public bool IsSiegeEquipment;


        public Equipment? FromSideEquipment
        {
            get
            {
                if (!this.IsSiegeEquipment)
                {
                    return Original.FromSideEquipment;
                }

                CharacterObject characterObject = Original.Character;
                if (characterObject != null)
                {
                    return characterObject.GetSiegeEquipment(false);
                }
                return null;
            }
        }

        public Equipment? ToSideEquipment
        {
            get
            {
                if (!this.IsSiegeEquipment)
                {
                    return Original.ToSideEquipment;
                }

                CharacterObject characterObject = Original.Character;
                if (characterObject != null)
                {
                    return characterObject.GetSiegeEquipment(false);
                }
                return null;
            }
        }



        public static TransferCommandExtended Transfer(int amount, InventoryLogic.InventorySide fromSide, InventoryLogic.InventorySide toSide, ItemRosterElement elementToTransfer, EquipmentIndex fromEquipmentIndex, EquipmentIndex toEquipmentIndex, CharacterObject character, bool siegeEquipment)
        {
            return new TransferCommandExtended
            {
                Original = TransferCommand.Transfer(amount, fromSide, toSide, elementToTransfer, fromEquipmentIndex, toEquipmentIndex, character),
                IsSiegeEquipment = siegeEquipment
            };
        }
    }

}
