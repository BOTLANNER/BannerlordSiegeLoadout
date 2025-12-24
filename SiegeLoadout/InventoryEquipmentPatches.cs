using System;
using System.Collections.Generic;
using System.Reflection;

using HarmonyLib;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;

using static TaleWorlds.CampaignSystem.Inventory.InventoryLogic;
using static TaleWorlds.CampaignSystem.ViewModelCollection.Inventory.SPInventoryVM;

namespace SiegeLoadout
{
    [HarmonyPatch]
    public static class InventoryEquipmentPatches
    {
        private static readonly Type? PartyEquipmentType;
        private static readonly PropertyInfo? CharacterEquipmentsProp;

        static Dictionary<CharacterObject, Equipment[]>? GetCharacterEquipments(object instance)
        {
            return CharacterEquipmentsProp?.GetValue(instance) as Dictionary<CharacterObject, Equipment[]>;
        }

        static InventoryEquipmentPatches()
        {
            PartyEquipmentType = AccessTools.TypeByName("TaleWorlds.CampaignSystem.Inventory.InventoryLogic+PartyEquipment");
            if (PartyEquipmentType != null)
            {
                CharacterEquipmentsProp = AccessTools.Property(PartyEquipmentType, "CharacterEquipments");

                if (CharacterEquipmentsProp == null)
                {
                    return;
                }

                var InitializeCopyFromMethod = AccessTools.Method(PartyEquipmentType, nameof(InitializeCopyFrom));
                if (InitializeCopyFromMethod != null)
                {
                    (Main.Harmony ??= new Harmony(Main.HarmonyDomain)).Patch(InitializeCopyFromMethod, prefix: new HarmonyMethod(typeof(InventoryEquipmentPatches), nameof(InitializeCopyFrom)));
                }
                var ResetEquipmentMethod = AccessTools.Method(PartyEquipmentType, nameof(ResetEquipment));
                if (ResetEquipmentMethod != null)
                {
                    (Main.Harmony ??= new Harmony(Main.HarmonyDomain)).Patch(ResetEquipmentMethod, prefix: new HarmonyMethod(typeof(InventoryEquipmentPatches), nameof(ResetEquipment)));
                }
            }
        }

        public static void PatchLate()
        {
            Debug.Print("Patch late ensures static constructor is run at least after calling this", 0, Debug.DebugColor.Purple, 17592186044416L);
        }

        public static bool InitializeCopyFrom(MobileParty party, ref object __instance)
        {
            if (CharacterEquipmentsProp == null)
            {
                return true;
            }

            var CharacterEquipments = new Dictionary<CharacterObject, Equipment[]>();
            CharacterEquipmentsProp.SetValue(__instance, CharacterEquipments);
            for (int i = 0; i < party.MemberRoster.Count; i++)
            {
                CharacterObject character = party.MemberRoster.GetElementCopyAtIndex(i).Character;
                if (character.IsHero)
                {
                    var extended = character.HeroObject?.AsExtended(extendIfNotFound: false);
                    if (extended == null)
                    {
                        CharacterEquipments.Add(character, new Equipment[3]
                        {
                            new Equipment(character.FirstBattleEquipment),
                            new Equipment(character.FirstCivilianEquipment),
                            new Equipment(character.FirstStealthEquipment)
                        });
                    }
                    else
                    {
                        CharacterEquipments.Add(character, new Equipment[4]
                        {
                            new Equipment(character.FirstBattleEquipment),
                            new Equipment(character.FirstCivilianEquipment),
                            new Equipment(character.FirstStealthEquipment),
                            new Equipment(extended.SiegeEquipment)
                        });
                    }
                }
            }

            return false;
        }

        public static bool ResetEquipment(MobileParty ownerParty, ref object __instance)
        {
            var CharacterEquipments = GetCharacterEquipments(__instance);
            if (CharacterEquipments == null)
            {
                return true;
            }

            foreach (KeyValuePair<CharacterObject, Equipment[]> characterEquipment in CharacterEquipments)
            {
                Equipment[] value = characterEquipment.Value;
                var extended = characterEquipment.Key.HeroObject.AsExtended(extendIfNotFound: false);
                var hadSiege = value.Length > 3;
                var hasSiege = extended != null;
                if (!hadSiege && hasSiege)
                {
                    // There was no siege equipment and might be now. Clear current.
                    extended!.SiegeEquipment.FillFrom(new Equipment(Equipment.EquipmentType.Battle));
                }

                for (int i = 0; i < value.Length; i++)
                {
                    Equipment equipment = value[i];
                    if (equipment.IsBattle && hadSiege && i == 3)
                    {
                        //var extended = characterEquipment.Key.HeroObject.AsExtended(extendIfNotFound: false);
                        if (extended != null)
                        {
                            extended.SiegeEquipment.FillFrom(equipment);
                        }
                        else
                        {
                            characterEquipment.Key.FirstBattleEquipment.FillFrom(equipment);
                        }
                    }
                    else if (equipment.IsBattle)
                    {
                        characterEquipment.Key.FirstBattleEquipment.FillFrom(equipment);
                    }
                    else if (equipment.IsCivilian)
                    {
                        characterEquipment.Key.FirstCivilianEquipment.FillFrom(equipment);
                    }
                    else if (equipment.IsStealth)
                    {
                        characterEquipment.Key.FirstStealthEquipment.FillFrom(equipment);
                    }
                    else
                    {
                        Debug.FailedAssert("Equipment type cannot be found!", "InventoryEquipmentPatches.cs", "ResetEquipment", 113);
                    }
                }
            }
            return false;
        }

        [HarmonyPatch(typeof(SPInventoryVM), "get_ActiveEquipment")]
        [HarmonyPrefix]
        public static bool ActiveEquipment(ref Equipment? __result, ref SPInventoryVM __instance, ref CharacterObject ____currentCharacter)
        {

            if (__instance.GetPropertyValue(nameof(InventoryExtensionVM.Mixin)) is WeakReference<InventoryExtensionVM> weakReference && weakReference.TryGetTarget(out var mixin))
            {
                if (__instance.EquipmentMode == (int) EquipmentModes.Battle && !mixin.IsInBattleSet)
                {
                    __result = ____currentCharacter.GetSiegeEquipment(false);
                    return false;
                }
            }


            return true;
            //if (__instance.EquipmentMode != (int) EquipmentModes.Battle)
            //{
            //    __result = ____currentCharacter.FirstCivilianEquipment;
            //    return false;
            //}
            //__result = ____currentCharacter.FirstBattleEquipment;

            //return false;
        }

        [HarmonyPatch(typeof(SPInventoryVM), "UnequipEquipment")]
        [HarmonyPrefix]
        private static bool UnequipEquipment(ref SPInventoryVM __instance, SPItemVM itemVM, ref CharacterObject ____currentCharacter, ref InventoryLogic ____inventoryLogic)
        {

            if (__instance.GetPropertyValue(nameof(InventoryExtensionVM.Mixin)) is WeakReference<InventoryExtensionVM> weakReference && weakReference.TryGetTarget(out var mixin))
            {
                if (__instance.EquipmentMode != (int) EquipmentModes.Battle || mixin.IsInBattleSet)
                {
                    return true;
                }

                if (itemVM == null || String.IsNullOrEmpty(itemVM.StringId))
                {
                    return true;
                }

                var transferCommand = TransferCommandExtended.Transfer(1, InventoryLogic.InventorySide.BattleEquipment, InventoryLogic.InventorySide.PlayerInventory, itemVM.ItemRosterElement, itemVM.ItemType, itemVM.ItemType, ____currentCharacter, true);
                ____inventoryLogic.AddTransferCommand(transferCommand);

                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(SPInventoryVM), nameof(SellItem))]
        [HarmonyPrefix]

        private static bool SellItem(ref SPInventoryVM __instance, SPItemVM item, ref CharacterObject ____currentCharacter, ref InventoryLogic ____inventoryLogic, ref Stack<SPItemVM> ____equipAfterTransferStack)
        {
            if (__instance.GetPropertyValue(nameof(InventoryExtensionVM.Mixin)) is WeakReference<InventoryExtensionVM> weakReference && weakReference.TryGetTarget(out var mixin))
            {

                if (__instance.EquipmentMode != (int) EquipmentModes.Battle || mixin.IsInBattleSet)
                {
                    return true;
                }

                if (item == null || String.IsNullOrEmpty(item.StringId))
                {
                    return true;
                }

                InventoryLogic.InventorySide inventorySide = item.InventorySide;
                int itemCount = item.ItemCount;
                if (inventorySide == InventoryLogic.InventorySide.OtherInventory)
                {
                    inventorySide = InventoryLogic.InventorySide.PlayerInventory;
                    ItemRosterElement? nullable = ____inventoryLogic.FindItemFromSide(InventoryLogic.InventorySide.PlayerInventory, item.ItemRosterElement.EquipmentElement);
                    if (nullable.HasValue)
                    {
                        itemCount = nullable.Value.Amount;
                    }
                }
                var transferCommand = TransferCommandExtended.Transfer(MathF.Min(__instance.TransactionCount, itemCount), inventorySide, InventoryLogic.InventorySide.OtherInventory, item.ItemRosterElement, item.ItemType, __instance.TargetEquipmentType, ____currentCharacter, true);
                ____inventoryLogic.AddTransferCommand(transferCommand);

                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(SPInventoryVM), nameof(BuyItem))]
        [HarmonyPrefix]

        private static bool BuyItem(ref SPInventoryVM __instance, SPItemVM item, ref CharacterObject ____currentCharacter, ref InventoryLogic ____inventoryLogic, ref Stack<SPItemVM> ____equipAfterTransferStack)
        {
            if (__instance.GetPropertyValue(nameof(InventoryExtensionVM.Mixin)) is WeakReference<InventoryExtensionVM> weakReference && weakReference.TryGetTarget(out var mixin))
            {
                if (__instance.EquipmentMode != (int) EquipmentModes.Battle || mixin.IsInBattleSet)
                {
                    return true;
                }

                if (item == null || String.IsNullOrEmpty(item.StringId))
                {
                    return true;
                }

                if (__instance.TargetEquipmentType != EquipmentIndex.None && item.ItemType != __instance.TargetEquipmentType && (__instance.TargetEquipmentType < EquipmentIndex.WeaponItemBeginSlot || __instance.TargetEquipmentType > EquipmentIndex.ExtraWeaponSlot || item.ItemType < EquipmentIndex.WeaponItemBeginSlot || item.ItemType > EquipmentIndex.ExtraWeaponSlot))
                {
                    return true;
                }

                if (__instance.TargetEquipmentType == EquipmentIndex.None)
                {
                    __instance.TargetEquipmentType = item.ItemType;
                    if (item.ItemType >= EquipmentIndex.WeaponItemBeginSlot && item.ItemType <= EquipmentIndex.ExtraWeaponSlot)
                    {
                        //__instance.TargetEquipmentType = __instance.ActiveEquipment.GetWeaponPickUpSlotIndex(itemVM.ItemRosterElement.EquipmentElement, false);
                        __instance.TargetEquipmentType = ____currentCharacter.GetSiegeEquipment(false)!.GetWeaponPickUpSlotIndex(item.ItemRosterElement.EquipmentElement, false);
                    }
                }
                int itemCount = item.ItemCount;
                if (item.InventorySide == InventoryLogic.InventorySide.PlayerInventory)
                {
                    ItemRosterElement? nullable = ____inventoryLogic.FindItemFromSide(InventoryLogic.InventorySide.OtherInventory, item.ItemRosterElement.EquipmentElement);
                    if (nullable.HasValue)
                    {
                        itemCount = nullable.Value.Amount;
                    }
                }
                var transferCommand = TransferCommandExtended.Transfer(MathF.Min(__instance.TransactionCount, itemCount), InventoryLogic.InventorySide.OtherInventory, InventoryLogic.InventorySide.PlayerInventory, item.ItemRosterElement, item.ItemType, __instance.TargetEquipmentType, ____currentCharacter, true);
                ____inventoryLogic.AddTransferCommand(transferCommand);
                if (__instance.EquipAfterBuy)
                {
                    ____equipAfterTransferStack.Push(item);
                }

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
                if (__instance.EquipmentMode == (int) EquipmentModes.Battle && !mixin.IsInBattleSet)
                {
                    __instance.CurrentCharacterArmArmor = ____currentCharacter.GetArmArmorSum(LoadoutSlot.Siege);
                    __instance.CurrentCharacterBodyArmor = ____currentCharacter.GetBodyArmorSum(LoadoutSlot.Siege);
                    __instance.CurrentCharacterHeadArmor = ____currentCharacter.GetHeadArmorSum(LoadoutSlot.Siege);
                    __instance.CurrentCharacterLegArmor = ____currentCharacter.GetLegArmorSum(LoadoutSlot.Siege);
                    __instance.CurrentCharacterHorseArmor = ____currentCharacter.GetHorseArmorSum(LoadoutSlot.Siege);
                    return false;
                }

            }
            return true;
        }

        [HarmonyPatch(typeof(SPInventoryVM), "EquipEquipment")]
        [HarmonyPrefix]
        private static bool EquipEquipment(ref SPInventoryVM __instance, SPItemVM itemVM, ref CharacterObject ____currentCharacter, ref InventoryLogic ____inventoryLogic)
        {
            if (__instance.GetPropertyValue(nameof(InventoryExtensionVM.Mixin)) is WeakReference<InventoryExtensionVM> weakReference && weakReference.TryGetTarget(out var mixin))
            {
                if (__instance.EquipmentMode != (int) EquipmentModes.Battle || mixin.IsInBattleSet)
                {
                    return true;
                }

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
                sPItemVM.RefreshWith(itemVM, InventoryLogic.InventorySide.BattleEquipment);
                if (!__instance.IsItemEquipmentPossible(sPItemVM))
                {
                    return false;
                }
                SPItemVM itemFromIndex = mixin.GetItemFromIndex(__instance.TargetEquipmentType);
                if (itemFromIndex != null && itemFromIndex.ItemRosterElement.EquipmentElement.Item == sPItemVM.ItemRosterElement.EquipmentElement.Item && itemFromIndex.ItemRosterElement.EquipmentElement.ItemModifier == sPItemVM.ItemRosterElement.EquipmentElement.ItemModifier)
                {
                    return false;
                }
                bool flag = (itemFromIndex == null || itemFromIndex.ItemType == EquipmentIndex.None ? false : itemVM.InventorySide == InventoryLogic.InventorySide.BattleEquipment);
                if (!flag)
                {
                    EquipmentIndex equipmentIndex = EquipmentIndex.None;
                    if (itemVM.ItemRosterElement.EquipmentElement.Item.Type == ItemObject.ItemTypeEnum.Shield && itemVM.InventorySide != InventoryLogic.InventorySide.BattleEquipment)
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
                var transferCommand = TransferCommandExtended.Transfer(1, itemVM.InventorySide, InventoryLogic.InventorySide.BattleEquipment, sPItemVM.ItemRosterElement, sPItemVM.ItemType, __instance.TargetEquipmentType, ____currentCharacter, __instance.EquipmentMode == (int) EquipmentModes.Battle && !mixin.IsInBattleSet);
                transferCommands.Add(transferCommand);
                if (flag)
                {
                    var transferCommand1 = TransferCommandExtended.Transfer(1, InventoryLogic.InventorySide.PlayerInventory, InventoryLogic.InventorySide.BattleEquipment, itemFromIndex.ItemRosterElement, EquipmentIndex.None, sPItemVM.ItemType, ____currentCharacter, __instance.EquipmentMode == (int) EquipmentModes.Battle && !mixin.IsInBattleSet);
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
                return extended?.SiegeEquipment ?? (fallbackToBattle ? characterObject.HeroObject.BattleEquipment : new Equipment(Equipment.EquipmentType.Battle));
            }
            return characterObject.GetAllEquipments().FirstOrDefaultQ<Equipment>((Equipment e) => !e.IsCivilian);
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

        public static void AddTransferCommand(this InventoryLogic inventoryLogic, TransferCommandExtended command)
        {
            inventoryLogic.ProcessTransferCommand(command);
        }

        public static void AddTransferCommands(this InventoryLogic inventoryLogic, IEnumerable<TransferCommandExtended> commands)
        {
            foreach (var command in commands)
            {
                inventoryLogic.ProcessTransferCommand(command);
            }
        }

        private static void ProcessTransferCommand(this InventoryLogic inventoryLogic, TransferCommandExtended command)
        {
            inventoryLogic.OnAfterTransfer(inventoryLogic.TransferItem(ref command, ref command.Original));
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

        public static void RecordTransaction(this InventoryLogic inventoryLogic, EquipmentElement elementToTransfer, bool isSelling, int price)
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


        private static bool DoesTransferItemExistExtended(this InventoryLogic inventoryLogic, ref TransferCommandExtended transferCommandExtended)
        {
            if (transferCommandExtended.Original.FromSide == InventorySide.OtherInventory || transferCommandExtended.Original.FromSide == InventorySide.PlayerInventory)
            {
                return inventoryLogic.DoesTransferItemExist(ref transferCommandExtended.Original);
            }
            if (transferCommandExtended.Original.FromSide == InventoryLogic.InventorySide.None)
            {
                return false;
            }
            if (transferCommandExtended.Original.FromSide == InventorySide.BattleEquipment)
            {
                if (transferCommandExtended.FromSideEquipment[(int) transferCommandExtended.Original.FromEquipmentIndex].Item != null)
                {
                    return transferCommandExtended.Original.ElementToTransfer.EquipmentElement.IsEqualTo(transferCommandExtended.FromSideEquipment[(int) transferCommandExtended.Original.FromEquipmentIndex]);
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
            if (IsEquipmentSide(fromSide))
            {
                return true;
            }
            return fromSide == InventoryLogic.InventorySide.PlayerInventory;
        }

        public static bool IsEquipmentSide(InventoryLogic.InventorySide side)
        {
            if (side == InventoryLogic.InventorySide.CivilianEquipment || side == InventoryLogic.InventorySide.BattleEquipment)
            {
                return true;
            }
            return side == InventoryLogic.InventorySide.StealthEquipment;
        }

        public static bool IsBuy(this InventoryLogic inventoryLogic, InventoryLogic.InventorySide fromSide, InventoryLogic.InventorySide toSide)
        {
            if (fromSide != InventoryLogic.InventorySide.OtherInventory)
            {
                return false;
            }
            if (IsEquipmentSide(toSide))
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
            object str;
            List<TransferCommandResult> transferCommandResults = new List<TransferCommandResult>();
            Object[] fromSide = new Object[4];
            ItemObject item = transferCommand.ElementToTransfer.EquipmentElement.Item;
            if (item != null)
            {
                str = item.Name.ToString();
            }
            else
            {
                str = null;
            }
            if (str == null)
            {
                str = "null";
            }
            fromSide[0] = str;
            fromSide[1] = transferCommand.FromSide;
            fromSide[2] = transferCommand.ToSide;
            fromSide[3] = transferCommand.Amount;
            Debug.Print(String.Format("TransferItem Name: {0} | From: {1} To: {2} | Amount: {3}", fromSide), 0, Debug.DebugColor.White, 17592186044416L);
            if (transferCommand.ElementToTransfer.EquipmentElement.Item != null && TransferIsMovementValid(ref transferCommand) && inventoryLogic.DoesTransferItemExistExtended(ref extended))
            {
                int num = 0;
                bool amount = false;
                if (!InventoryLogic.IsEquipmentSide(transferCommand.FromSide) && transferCommand.FromSide != InventoryLogic.InventorySide.None)
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
                    if (InventoryLogic.IsEquipmentSide(transferCommand.ToSide) && extended.ToSideEquipment[(int) transferCommand.ToEquipmentIndex].Item != null)
                    {
                        TransferCommand transferCommand1 = TransferCommand.Transfer(1, transferCommand.ToSide, InventoryLogic.InventorySide.PlayerInventory, new ItemRosterElement(extended.ToSideEquipment[(int) transferCommand.ToEquipmentIndex], 1), transferCommand.ToEquipmentIndex, EquipmentIndex.None, transferCommand.Character);
                        transferCommandResults.AddRange(inventoryLogic.TransferItem(ref extended, ref transferCommand1));
                    }
                    EquipmentElement equipmentElement = transferCommand.ElementToTransfer.EquipmentElement;
                    int itemPrice = inventoryLogic.GetItemPrice(equipmentElement, flag1);
                    if (flag1 | flag)
                    {
                        inventoryLogic.RecordTransaction(equipmentElement, flag, itemPrice);
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
                    if (InventoryLogic.IsEquipmentSide(transferCommand.FromSide))
                    {
                        ItemRosterElement itemRosterElement = new ItemRosterElement(extended.FromSideEquipment[(int) transferCommand.FromEquipmentIndex], transferCommand.Amount);
                        itemRosterElement.Amount = itemRosterElement.Amount - 1;
                        extended.FromSideEquipment[(int) transferCommand.FromEquipmentIndex] = itemRosterElement.EquipmentElement;
                    }
                    else if (transferCommand.FromSide == InventoryLogic.InventorySide.PlayerInventory || transferCommand.FromSide == InventoryLogic.InventorySide.OtherInventory)
                    {
                        ItemRoster itemRosters1 = inventoryLogic._rosters()[(int) transferCommand.FromSide];
                        elementToTransfer = transferCommand.ElementToTransfer;
                        itemRosters1.AddToCounts(elementToTransfer.EquipmentElement, -1);
                    }
                    if (InventoryLogic.IsEquipmentSide(transferCommand.ToSide))
                    {
                        ItemRosterElement elementToTransfer1 = transferCommand.ElementToTransfer;
                        elementToTransfer1.Amount = 1;
                        extended.ToSideEquipment[(int) transferCommand.ToEquipmentIndex] = elementToTransfer1.EquipmentElement;
                    }
                    else if (transferCommand.ToSide == InventoryLogic.InventorySide.PlayerInventory || transferCommand.ToSide == InventoryLogic.InventorySide.OtherInventory)
                    {
                        ItemRoster itemRosters2 = inventoryLogic._rosters()[(int) transferCommand.ToSide];
                        elementToTransfer = transferCommand.ElementToTransfer;
                        itemRosters2.AddToCounts(elementToTransfer.EquipmentElement, 1);
                    }
                }
                if (InventoryLogic.IsEquipmentSide(transferCommand.FromSide))
                {
                    ItemRosterElement itemRosterElement1 = new ItemRosterElement(extended.FromSideEquipment[(int) transferCommand.FromEquipmentIndex], transferCommand.Amount);
                    itemRosterElement1.Amount = itemRosterElement1.Amount - 1;
                    transferCommandResults.Add(new TransferCommandResultExtended(transferCommand.FromSide, itemRosterElement1, -transferCommand.Amount, itemRosterElement1.Amount, transferCommand.FromEquipmentIndex, transferCommand.Character, extended.IsSiegeEquipment));
                }
                else if (transferCommand.FromSide == InventoryLogic.InventorySide.PlayerInventory || transferCommand.FromSide == InventoryLogic.InventorySide.OtherInventory)
                {
                    if (!amount)
                    {
                        ItemRoster itemRosters3 = inventoryLogic._rosters()[(int) transferCommand.FromSide];
                        elementToTransfer = transferCommand.ElementToTransfer;
                        int num2 = itemRosters3.FindIndexOfElement(elementToTransfer.EquipmentElement);
                        ItemRosterElement elementCopyAtIndex1 = inventoryLogic._rosters()[(int) transferCommand.FromSide].GetElementCopyAtIndex(num2);
                        transferCommandResults.Add(new TransferCommandResultExtended(transferCommand.FromSide, elementCopyAtIndex1, -transferCommand.Amount, elementCopyAtIndex1.Amount, transferCommand.FromEquipmentIndex, transferCommand.Character, extended.IsSiegeEquipment));
                    }
                    else
                    {
                        InventoryLogic.InventorySide inventorySide = transferCommand.FromSide;
                        elementToTransfer = transferCommand.ElementToTransfer;
                        transferCommandResults.Add(new TransferCommandResultExtended(inventorySide, new ItemRosterElement(elementToTransfer.EquipmentElement, 0), -transferCommand.Amount, 0, transferCommand.FromEquipmentIndex, transferCommand.Character, extended.IsSiegeEquipment));
                    }
                }
                if (InventoryLogic.IsEquipmentSide(transferCommand.ToSide))
                {
                    ItemRosterElement elementToTransfer2 = transferCommand.ElementToTransfer;
                    elementToTransfer2.Amount = 1;
                    transferCommandResults.Add(new TransferCommandResultExtended(transferCommand.ToSide, elementToTransfer2, 1, 1, transferCommand.ToEquipmentIndex, transferCommand.Character, extended.IsSiegeEquipment));
                }
                else if (transferCommand.ToSide == InventoryLogic.InventorySide.PlayerInventory || transferCommand.ToSide == InventoryLogic.InventorySide.OtherInventory)
                {
                    ItemRoster itemRosters4 = inventoryLogic._rosters()[(int) transferCommand.ToSide];
                    elementToTransfer = transferCommand.ElementToTransfer;
                    int num3 = itemRosters4.FindIndexOfElement(elementToTransfer.EquipmentElement);
                    ItemRosterElement elementCopyAtIndex2 = inventoryLogic._rosters()[(int) transferCommand.ToSide].GetElementCopyAtIndex(num3);
                    transferCommandResults.Add(new TransferCommandResultExtended(transferCommand.ToSide, elementCopyAtIndex2, transferCommand.Amount, elementCopyAtIndex2.Amount, transferCommand.ToEquipmentIndex, transferCommand.Character, extended.IsSiegeEquipment));
                }
                inventoryLogic.HandleDonationOnTransferItem(transferCommand.ElementToTransfer, transferCommand.Amount, flag1, flag);
                inventoryLogic.SetTransactionDebt(inventoryLogic.TransactionDebt + num);
            }
            return transferCommandResults;
        }
    }

}
