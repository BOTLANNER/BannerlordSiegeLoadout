using System;
using System.Collections.Generic;
using System.Reflection;

using HarmonyLib;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;

using static TaleWorlds.CampaignSystem.Inventory.InventoryLogic;

namespace SiegeLoadout
{
    [HarmonyPatch]
    public static class InventoryEquipmentPatches
    {
        [HarmonyPatch(typeof(SPInventoryVM), "get_ActiveEquipment")]
        [HarmonyPrefix]
        public static bool ActiveEquipment(ref Equipment? __result, ref SPInventoryVM __instance, ref CharacterObject ____currentCharacter)
        {

            if (__instance.GetPropertyValue(nameof(InventoryExtensionVM.Mixin)) is WeakReference<InventoryExtensionVM> weakReference && weakReference.TryGetTarget(out var mixin))
            {
                if (__instance.IsInWarSet && !mixin.IsInBattleSet)
                {
                    __result = ____currentCharacter.GetSiegeEquipment(false);
                    return false;
                }
            }


            if (!__instance.IsInWarSet)
            {
                __result = ____currentCharacter.FirstCivilianEquipment;
                return false;
            }
            __result = ____currentCharacter.FirstBattleEquipment;

            return false;
        }

        [HarmonyPatch(typeof(SPInventoryVM), "UnequipEquipment")]
        [HarmonyPrefix]
        private static bool UnequipEquipment(ref SPInventoryVM __instance, SPItemVM itemVM, ref CharacterObject ____currentCharacter, ref InventoryLogic ____inventoryLogic)
        {

            if (__instance.GetPropertyValue(nameof(InventoryExtensionVM.Mixin)) is WeakReference<InventoryExtensionVM> weakReference && weakReference.TryGetTarget(out var mixin))
            {
                if (itemVM == null || String.IsNullOrEmpty(itemVM.StringId))
                {
                    return false;
                }
                var transferCommand = TransferCommandExtended.Transfer(1, InventoryLogic.InventorySide.Equipment, InventoryLogic.InventorySide.PlayerInventory, itemVM.ItemRosterElement, itemVM.ItemType, itemVM.ItemType, ____currentCharacter, !__instance.IsInWarSet, !mixin.IsInBattleSet);
                ____inventoryLogic.AddTransferCommand(transferCommand);

                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(SPInventoryVM), "UpdateCharacterArmorValues")]
        [HarmonyPrefix]
        private static bool UpdateCharacterArmorValues(ref SPInventoryVM __instance, ref CharacterObject ____currentCharacter)
        {
            if (__instance.GetPropertyValue(nameof(InventoryExtensionVM.Mixin)) is WeakReference<InventoryExtensionVM> weakReference && weakReference.TryGetTarget(out var mixin))
            {
                __instance.CurrentCharacterArmArmor = ____currentCharacter.GetArmArmorSum(!__instance.IsInWarSet ? LoadoutSlot.Civilian : mixin.IsInBattleSet ? LoadoutSlot.Battle : LoadoutSlot.Siege);
                __instance.CurrentCharacterBodyArmor = ____currentCharacter.GetBodyArmorSum(!__instance.IsInWarSet ? LoadoutSlot.Civilian : mixin.IsInBattleSet ? LoadoutSlot.Battle : LoadoutSlot.Siege);
                __instance.CurrentCharacterHeadArmor = ____currentCharacter.GetHeadArmorSum(!__instance.IsInWarSet ? LoadoutSlot.Civilian : mixin.IsInBattleSet ? LoadoutSlot.Battle : LoadoutSlot.Siege);
                __instance.CurrentCharacterLegArmor = ____currentCharacter.GetLegArmorSum(!__instance.IsInWarSet ? LoadoutSlot.Civilian : mixin.IsInBattleSet ? LoadoutSlot.Battle : LoadoutSlot.Siege);
                __instance.CurrentCharacterHorseArmor = ____currentCharacter.GetHorseArmorSum(!__instance.IsInWarSet ? LoadoutSlot.Civilian : mixin.IsInBattleSet ? LoadoutSlot.Battle : LoadoutSlot.Siege);

                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(SPInventoryVM), "EquipEquipment")]
        [HarmonyPrefix]
        private static bool EquipEquipment(ref SPInventoryVM __instance, SPItemVM itemVM, ref CharacterObject ____currentCharacter, ref InventoryLogic ____inventoryLogic)
        {
            if (__instance.GetPropertyValue(nameof(InventoryExtensionVM.Mixin)) is WeakReference<InventoryExtensionVM> weakReference && weakReference.TryGetTarget(out var mixin))
            {
                EquipmentElement equipmentElement;
                ItemObject.ItemTypeEnum? nullable;
                ItemObject.ItemTypeEnum? nullable1;
                ItemObject.ItemTypeEnum? nullable2;
                ItemObject.ItemTypeEnum? nullable3;
                bool valueOrDefault;
                if (itemVM == null || String.IsNullOrEmpty(itemVM.StringId))
                {
                    return false;
                }
                SPItemVM sPItemVM = new SPItemVM();
                sPItemVM.RefreshWith(itemVM, InventoryLogic.InventorySide.Equipment);
                if (!__instance.IsItemEquipmentPossible(sPItemVM))
                {
                    return false;
                }
                SPItemVM itemFromIndex = mixin.GetItemFromIndex(__instance.TargetEquipmentType);
                if (itemFromIndex != null && itemFromIndex.ItemRosterElement.EquipmentElement.Item == sPItemVM.ItemRosterElement.EquipmentElement.Item && itemFromIndex.ItemRosterElement.EquipmentElement.ItemModifier == sPItemVM.ItemRosterElement.EquipmentElement.ItemModifier)
                {
                    return false;
                }
                bool flag = (itemFromIndex == null || itemFromIndex.ItemType == EquipmentIndex.None ? false : itemVM.InventorySide == InventoryLogic.InventorySide.Equipment);
                if (!flag)
                {
                    EquipmentIndex equipmentIndex = EquipmentIndex.None;
                    if (itemVM.ItemRosterElement.EquipmentElement.Item.Type == ItemObject.ItemTypeEnum.Shield && itemVM.InventorySide != InventoryLogic.InventorySide.Equipment)
                    {
                        EquipmentIndex equipmentIndex1 = EquipmentIndex.WeaponItemBeginSlot;
                        while (equipmentIndex1 <= EquipmentIndex.NumAllWeaponSlots)
                        {
                            SPItemVM itemFromIndex1 = mixin.GetItemFromIndex(equipmentIndex1);
                            if (itemFromIndex1 != null)
                            {
                                equipmentElement = itemFromIndex1.ItemRosterElement.EquipmentElement;
                                ItemObject item = equipmentElement.Item;
                                if (item != null)
                                {
                                    nullable3 = new ItemObject.ItemTypeEnum?(item.Type);
                                }
                                else
                                {
                                    nullable1 = null;
                                    nullable3 = nullable1;
                                }
                                nullable = nullable3;
                                ItemObject.ItemTypeEnum itemTypeEnum = ItemObject.ItemTypeEnum.Shield;
                                valueOrDefault = nullable.GetValueOrDefault() == itemTypeEnum & nullable.HasValue;
                            }
                            else
                            {
                                valueOrDefault = false;
                            }
                            if (!valueOrDefault)
                            {
                                equipmentIndex1 += (int) EquipmentIndex.Weapon1;
                            }
                            else
                            {
                                equipmentIndex = equipmentIndex1;
                                break;
                            }
                        }
                    }
                    if (itemVM != null)
                    {
                        equipmentElement = itemVM.ItemRosterElement.EquipmentElement;
                        ItemObject itemObject = equipmentElement.Item;
                        if (itemObject != null)
                        {
                            nullable2 = new ItemObject.ItemTypeEnum?(itemObject.Type);
                        }
                        else
                        {
                            nullable1 = null;
                            nullable2 = nullable1;
                        }
                        nullable = nullable2;
                        if (nullable.GetValueOrDefault() == ItemObject.ItemTypeEnum.Shield & nullable.HasValue && equipmentIndex != EquipmentIndex.None)
                        {
                            __instance.TargetEquipmentType = equipmentIndex;
                        }
                    }
                }
                List<TransferCommandExtended> transferCommands = new List<TransferCommandExtended>();
                var transferCommand = TransferCommandExtended.Transfer(1, itemVM.InventorySide, InventoryLogic.InventorySide.Equipment, sPItemVM.ItemRosterElement, sPItemVM.ItemType, __instance.TargetEquipmentType, ____currentCharacter, !__instance.IsInWarSet, !mixin.IsInBattleSet);
                transferCommands.Add(transferCommand);
                if (flag)
                {
                    var transferCommand1 = TransferCommandExtended.Transfer(1, InventoryLogic.InventorySide.PlayerInventory, InventoryLogic.InventorySide.Equipment, itemFromIndex.ItemRosterElement, EquipmentIndex.None, sPItemVM.ItemType, ____currentCharacter, !__instance.IsInWarSet, !mixin.IsInBattleSet);
                    transferCommands.Add(transferCommand1);
                }
                ____inventoryLogic.AddTransferCommands(transferCommands);

                return false;
            }
            return true;
        }

        public static Equipment? GetSiegeEquipment(this CharacterObject characterObject, bool fallbackToBattle = false)
        {
            if (characterObject.IsHero)
            {
                var extended = characterObject.HeroObject.AsExtended(extendIfNotFound: true);
                return extended?.SiegeEquipment ?? (fallbackToBattle ? characterObject.HeroObject.BattleEquipment : new Equipment(false));
            }
            return characterObject.AllEquipments.FirstOrDefaultQ<Equipment>((Equipment e) => !e.IsCivilian);
        }

        public static float GetArmArmorSum(this CharacterObject characterObject, LoadoutSlot loadoutSlot)
        {
            switch (loadoutSlot)
            {
                case LoadoutSlot.Civilian:
                    return characterObject.FirstCivilianEquipment.GetArmArmorSum();
                case LoadoutSlot.Siege:
                    return characterObject.GetSiegeEquipment()?.GetArmArmorSum() ?? characterObject.FirstBattleEquipment.GetArmArmorSum();
                case LoadoutSlot.Battle:
                default:
                    return characterObject.FirstBattleEquipment.GetArmArmorSum();
            }
        }

        public static float GetBodyArmorSum(this CharacterObject characterObject, LoadoutSlot loadoutSlot)
        {
            switch (loadoutSlot)
            {
                case LoadoutSlot.Civilian:
                    return characterObject.FirstCivilianEquipment.GetHumanBodyArmorSum();
                case LoadoutSlot.Siege:
                    return characterObject.GetSiegeEquipment()?.GetHumanBodyArmorSum() ?? characterObject.FirstBattleEquipment.GetHumanBodyArmorSum();
                case LoadoutSlot.Battle:
                default:
                    return characterObject.FirstBattleEquipment.GetHumanBodyArmorSum();
            }
        }

        public static float GetHeadArmorSum(this CharacterObject characterObject, LoadoutSlot loadoutSlot)
        {
            switch (loadoutSlot)
            {
                case LoadoutSlot.Civilian:
                    return characterObject.FirstCivilianEquipment.GetHeadArmorSum();
                case LoadoutSlot.Siege:
                    return characterObject.GetSiegeEquipment()?.GetHeadArmorSum() ?? characterObject.FirstBattleEquipment.GetHeadArmorSum();
                case LoadoutSlot.Battle:
                default:
                    return characterObject.FirstBattleEquipment.GetHeadArmorSum();
            }
        }

        public static float GetLegArmorSum(this CharacterObject characterObject, LoadoutSlot loadoutSlot)
        {
            switch (loadoutSlot)
            {
                case LoadoutSlot.Civilian:
                    return characterObject.FirstCivilianEquipment.GetLegArmorSum();
                case LoadoutSlot.Siege:
                    return characterObject.GetSiegeEquipment()?.GetLegArmorSum() ?? characterObject.FirstBattleEquipment.GetLegArmorSum();
                case LoadoutSlot.Battle:
                default:
                    return characterObject.FirstBattleEquipment.GetLegArmorSum();
            }
        }

        public static float GetHorseArmorSum(this CharacterObject characterObject, LoadoutSlot loadoutSlot)
        {
            switch (loadoutSlot)
            {
                case LoadoutSlot.Civilian:
                    return characterObject.FirstCivilianEquipment.GetHorseArmorSum();
                case LoadoutSlot.Siege:
                    return characterObject.GetSiegeEquipment()?.GetHorseArmorSum() ?? characterObject.FirstBattleEquipment.GetHorseArmorSum();
                case LoadoutSlot.Battle:
                default:
                    return characterObject.FirstBattleEquipment.GetHorseArmorSum();
            }
        }

        public static void AddTransferCommand(this InventoryLogic inventoryLogic,TransferCommandExtended command)
        {
            inventoryLogic.ProcessTransferCommand(command);
        }

        public static void AddTransferCommands(this InventoryLogic inventoryLogic,IEnumerable<TransferCommandExtended> commands)
        {
            foreach (var command in commands)
            {
                inventoryLogic.ProcessTransferCommand(command);
            }
        }

        private static void ProcessTransferCommand(this InventoryLogic inventoryLogic, TransferCommandExtended command)
        {
            inventoryLogic.OnAfterTransfer(inventoryLogic.TransferItem(ref command,ref command.Original));
        }

        private static MethodInfo onAfterTransfer = AccessTools.Method(typeof(InventoryLogic), "OnAfterTransfer");

        private static void OnAfterTransfer(this InventoryLogic inventoryLogic, List<TransferCommandResult> resultList)
        {
            onAfterTransfer.Invoke(inventoryLogic, new object[] { resultList });
        }

        
        private static FieldInfo _rostersField = AccessTools.Field(typeof(InventoryLogic), "_rosters");
        private static ItemRoster[] _rosters(this InventoryLogic inventoryLogic)
        {
            return (ItemRoster[]) _rostersField.GetValue(inventoryLogic);
        }
        private static FieldInfo _transactionHistoryField = AccessTools.Field(typeof(InventoryLogic), "_transactionHistory");
        private static MethodInfo recordTransaction = AccessTools.Method(_transactionHistoryField.FieldType, "RecordTransaction");

        public static void RecordTransaction(this InventoryLogic inventoryLogic, ItemRosterElement elementToTransfer, bool isSelling, int price)
        {
            var _transactionHistory = _transactionHistoryField.GetValue(inventoryLogic);
            recordTransaction.Invoke(_transactionHistory, new object[] { elementToTransfer, isSelling, price });
        }


        private static MethodInfo transferIsMovementValid = AccessTools.Method(typeof(InventoryLogic), "TransferIsMovementValid");
        public static bool TransferIsMovementValid(ref TransferCommand transferCommand)
        {
            var args = new object[] { transferCommand };
            var result = (bool) transferIsMovementValid.Invoke(null, args);
            transferCommand = (TransferCommand) args[0];

            return result;
        }

        private static MethodInfo doesTransferItemExist = AccessTools.Method(typeof(InventoryLogic), "DoesTransferItemExist");
        public static bool DoesTransferItemExist(this InventoryLogic inventoryLogic, ref TransferCommand transferCommand)
        {
            var args = new object[] { transferCommand };
            var result = (bool) doesTransferItemExist.Invoke(inventoryLogic, args);
            transferCommand = (TransferCommand) args[0];

            return result;
        }


        private static bool DoesTransferItemExistExtended(this InventoryLogic inventoryLogic,ref TransferCommandExtended transferCommandExtended)
        {
            if (transferCommandExtended.Original.FromSide == InventorySide.OtherInventory || transferCommandExtended.Original.FromSide == InventorySide.PlayerInventory)
            {
                return inventoryLogic.DoesTransferItemExist(ref transferCommandExtended.Original);
            }
            if (transferCommandExtended.Original.FromSide == InventorySide.Equipment)
            {
                if (transferCommandExtended.CharacterEquipment[(int) transferCommandExtended.Original.FromEquipmentIndex].Item != null)
                {
                    return transferCommandExtended.Original.ElementToTransfer.EquipmentElement.IsEqualTo(transferCommandExtended.CharacterEquipment[(int) transferCommandExtended.Original.FromEquipmentIndex]);
                }
                return false;
            }
            return false;
        }

        public static bool IsSell(this InventoryLogic inventoryLogic, InventoryLogic.InventorySide fromSide, InventoryLogic.InventorySide toSide)
        {
            if (toSide != InventoryLogic.InventorySide.OtherInventory)
            {
                return false;
            }
            if (fromSide == InventoryLogic.InventorySide.Equipment)
            {
                return true;
            }
            return fromSide == InventoryLogic.InventorySide.PlayerInventory;
        }

        public static bool IsBuy(this InventoryLogic inventoryLogic, InventoryLogic.InventorySide fromSide, InventoryLogic.InventorySide toSide)
        {
            if (fromSide != InventoryLogic.InventorySide.OtherInventory)
            {
                return false;
            }
            if (toSide == InventoryLogic.InventorySide.Equipment)
            {
                return true;
            }
            return toSide == InventoryLogic.InventorySide.PlayerInventory;
        }



        private static MethodInfo handleDonationOnTransferItem = AccessTools.Method(typeof(InventoryLogic), "HandleDonationOnTransferItem");
        public static void HandleDonationOnTransferItem(this InventoryLogic inventoryLogic, ItemRosterElement rosterElement, int amount, bool isBuying, bool isSelling)
        {
            handleDonationOnTransferItem.Invoke(inventoryLogic, new object[] { rosterElement, amount, isBuying, isSelling });
        }


        private static MethodInfo TransactionDebt = AccessTools.Method(typeof(InventoryLogic), "set_TransactionDebt");
        public static void SetTransactionDebt(this InventoryLogic inventoryLogic, int value)
        {
            TransactionDebt.Invoke(inventoryLogic, new object[] { value });
        }


        private static List<TransferCommandResult> TransferItem(this InventoryLogic inventoryLogic, ref TransferCommandExtended extended, ref TransferCommand transferCommand)
        {
            ItemRosterElement elementToTransfer;
            List<TransferCommandResult> transferCommandResults = new List<TransferCommandResult>();
            Object[] str = new Object[4];
            EquipmentElement equipmentElement = transferCommand.ElementToTransfer.EquipmentElement;
            str[0] = equipmentElement.Item.Name.ToString();
            str[1] = transferCommand.FromSide;
            str[2] = transferCommand.ToSide;
            str[3] = transferCommand.Amount;
            Debug.Print(String.Format("TransferItem Name: {0} | From: {1} To: {2} | Amount: {3}", str), 0, Debug.DebugColor.White, 17592186044416L);
            if (transferCommand.ElementToTransfer.EquipmentElement.Item != null && TransferIsMovementValid(ref transferCommand) && inventoryLogic.DoesTransferItemExistExtended(ref extended))
            {
                int num = 0;
                bool amount = false;
                if (transferCommand.FromSide != InventoryLogic.InventorySide.Equipment && transferCommand.FromSide != InventoryLogic.InventorySide.None)
                {
                    ItemRoster itemRosters = inventoryLogic._rosters()[(int) transferCommand.FromSide];
                    elementToTransfer = transferCommand.ElementToTransfer;
                    int num1 = itemRosters.FindIndexOfElement(elementToTransfer.EquipmentElement);
                    ItemRosterElement elementCopyAtIndex = inventoryLogic._rosters()[(int) transferCommand.FromSide].GetElementCopyAtIndex(num1);
                    amount = transferCommand.Amount == elementCopyAtIndex.Amount;
                }
                bool flag = inventoryLogic.IsSell(transferCommand.FromSide, transferCommand.ToSide);
                bool flag1 = inventoryLogic.IsBuy(transferCommand.FromSide, transferCommand.ToSide);
                for (int i = 0; i < transferCommand.Amount; i++)
                {
                    if (transferCommand.ToSide == InventoryLogic.InventorySide.Equipment && extended.CharacterEquipment[(int) transferCommand.ToEquipmentIndex].Item != null)
                    {
                        TransferCommand transferCommand1 = TransferCommand.Transfer(1, InventoryLogic.InventorySide.Equipment, InventoryLogic.InventorySide.PlayerInventory, new ItemRosterElement(extended.CharacterEquipment[(int) transferCommand.ToEquipmentIndex], 1), transferCommand.ToEquipmentIndex, EquipmentIndex.None, transferCommand.Character, transferCommand.IsCivilianEquipment);
                        transferCommandResults.AddRange(inventoryLogic.TransferItem(ref extended,ref transferCommand1));
                    }
                    ItemRosterElement itemRosterElement = transferCommand.ElementToTransfer;
                    int itemPrice = inventoryLogic.GetItemPrice(transferCommand.ElementToTransfer, flag1);
                    if (flag1 | flag)
                    {
                        inventoryLogic.RecordTransaction(transferCommand.ElementToTransfer, flag, itemPrice);
                    }
                    if (inventoryLogic.IsTrading)
                    {
                        if (flag1)
                        {
                            num += itemPrice;
                        }
                        else if (flag)
                        {
                            num -= itemPrice;
                        }
                    }
                    if (transferCommand.FromSide == InventoryLogic.InventorySide.Equipment)
                    {
                        ItemRosterElement itemRosterElement1 = new ItemRosterElement(extended.CharacterEquipment[(int) transferCommand.FromEquipmentIndex], transferCommand.Amount);
                        itemRosterElement1.Amount = itemRosterElement1.Amount - 1;

                        extended.CharacterEquipment[(int) transferCommand.FromEquipmentIndex] = itemRosterElement1.EquipmentElement;
                    }
                    else if (transferCommand.FromSide == InventoryLogic.InventorySide.PlayerInventory || transferCommand.FromSide == InventoryLogic.InventorySide.OtherInventory)
                    {
                        ItemRoster itemRosters1 = inventoryLogic._rosters()[(int) transferCommand.FromSide];
                        elementToTransfer = transferCommand.ElementToTransfer;
                        itemRosters1.AddToCounts(elementToTransfer.EquipmentElement, -1);
                    }
                    if (transferCommand.ToSide == InventoryLogic.InventorySide.Equipment)
                    {
                        ItemRosterElement elementToTransfer1 = transferCommand.ElementToTransfer;
                        elementToTransfer1.Amount = 1;
                        extended.CharacterEquipment[(int) transferCommand.ToEquipmentIndex] = elementToTransfer1.EquipmentElement;
                    }
                    else if (transferCommand.ToSide == InventoryLogic.InventorySide.PlayerInventory || transferCommand.ToSide == InventoryLogic.InventorySide.OtherInventory)
                    {
                        ItemRoster itemRosters2 = inventoryLogic._rosters()[(int) transferCommand.ToSide];
                        elementToTransfer = transferCommand.ElementToTransfer;
                        itemRosters2.AddToCounts(elementToTransfer.EquipmentElement, 1);
                    }
                }
                if (transferCommand.FromSide == InventoryLogic.InventorySide.Equipment)
                {
                    ItemRosterElement itemRosterElement2 = new ItemRosterElement(extended.CharacterEquipment[(int) transferCommand.FromEquipmentIndex], transferCommand.Amount);
                    itemRosterElement2.Amount = itemRosterElement2.Amount - 1;

                    transferCommandResults.Add(new TransferCommandResultExtended(transferCommand.FromSide, itemRosterElement2, -transferCommand.Amount, itemRosterElement2.Amount, transferCommand.FromEquipmentIndex, transferCommand.Character, transferCommand.IsCivilianEquipment, extended.IsSiegeEquipment));
                }
                else if (transferCommand.FromSide == InventoryLogic.InventorySide.PlayerInventory || transferCommand.FromSide == InventoryLogic.InventorySide.OtherInventory)
                {
                    if (!amount)
                    {
                        ItemRoster itemRosters3 = inventoryLogic._rosters()[(int) transferCommand.FromSide];
                        elementToTransfer = transferCommand.ElementToTransfer;
                        int num2 = itemRosters3.FindIndexOfElement(elementToTransfer.EquipmentElement);
                        ItemRosterElement elementCopyAtIndex1 = inventoryLogic._rosters()[(int) transferCommand.FromSide].GetElementCopyAtIndex(num2);
                        transferCommandResults.Add(new TransferCommandResultExtended(transferCommand.FromSide, elementCopyAtIndex1, -transferCommand.Amount, elementCopyAtIndex1.Amount, transferCommand.FromEquipmentIndex, transferCommand.Character, transferCommand.IsCivilianEquipment, extended.IsSiegeEquipment));
                    }
                    else
                    {
                        InventoryLogic.InventorySide fromSide = transferCommand.FromSide;
                        elementToTransfer = transferCommand.ElementToTransfer;
                        transferCommandResults.Add(new TransferCommandResultExtended(fromSide, new ItemRosterElement(elementToTransfer.EquipmentElement, 0), -transferCommand.Amount, 0, transferCommand.FromEquipmentIndex, transferCommand.Character, transferCommand.IsCivilianEquipment, extended.IsSiegeEquipment));
                    }
                }
                if (transferCommand.ToSide == InventoryLogic.InventorySide.Equipment)
                {
                    ItemRosterElement elementToTransfer2 = transferCommand.ElementToTransfer;
                    elementToTransfer2.Amount = 1;
                    transferCommandResults.Add(new TransferCommandResultExtended(transferCommand.ToSide, elementToTransfer2, 1, 1, transferCommand.ToEquipmentIndex, transferCommand.Character, transferCommand.IsCivilianEquipment, extended.IsSiegeEquipment));
                }
                else if (transferCommand.ToSide == InventoryLogic.InventorySide.PlayerInventory || transferCommand.ToSide == InventoryLogic.InventorySide.OtherInventory)
                {
                    ItemRoster itemRosters4 = inventoryLogic._rosters()[(int) transferCommand.ToSide];
                    elementToTransfer = transferCommand.ElementToTransfer;
                    int num3 = itemRosters4.FindIndexOfElement(elementToTransfer.EquipmentElement);
                    ItemRosterElement elementCopyAtIndex2 = inventoryLogic._rosters()[(int) transferCommand.ToSide].GetElementCopyAtIndex(num3);
                    transferCommandResults.Add(new TransferCommandResultExtended(transferCommand.ToSide, elementCopyAtIndex2, transferCommand.Amount, elementCopyAtIndex2.Amount, transferCommand.ToEquipmentIndex, transferCommand.Character, transferCommand.IsCivilianEquipment, extended.IsSiegeEquipment));
                }
                inventoryLogic.HandleDonationOnTransferItem(transferCommand.ElementToTransfer, transferCommand.Amount, flag1, flag);
                inventoryLogic.SetTransactionDebt(inventoryLogic.TransactionDebt + num);
            }
            return transferCommandResults;
        }
    }

}
