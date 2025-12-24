using System.Reflection;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace SiegeLoadout
{
    public class HeroExtended
    {
        [SaveableProperty(1)]
        public Hero Hero { get; set; }


        [SaveableField(3)]
        private Equipment? _equipment;

        public Equipment SiegeEquipment
        {
            get
            {
                if (_equipment == null)
                {
                    _equipment = new Equipment(Equipment.EquipmentType.Battle);
                }
                return _equipment;
            }
            set
            {
                _equipment = value;
            }
        }

        public Equipment WithSiegeEquipment()
        {
            if (!Main.Settings!.EnableSiegeLoadouts)
            {
                return Hero.BattleEquipment;
            }

            if (!Main.Settings!.EnforceFullSet)
            {
                var mergedEquipment = new Equipment(Hero.BattleEquipment);
                if (this.SiegeEquipment != null && !this.SiegeEquipment.IsEmpty())
                {
                    if (!Main.Settings.UseBattleLoadoutMountAndHarness)
                    {
                        mergedEquipment[EquipmentIndex.Horse] = this.SiegeEquipment[EquipmentIndex.Horse];
                        mergedEquipment[EquipmentIndex.HorseHarness] = this.SiegeEquipment[EquipmentIndex.HorseHarness];
                    }
                    if (this.SiegeEquipment.HasWeapon())
                    {
                        if (Main.Settings.UseBattleLoadoutPieces)
                        {
                            var weap0 = this.SiegeEquipment[EquipmentIndex.Weapon0];
                            if (!weap0.IsEqualTo(default(EquipmentElement)) && !weap0.IsEmpty && !weap0.IsInvalid())
                            {
                                mergedEquipment[EquipmentIndex.Weapon0] = weap0;
                            }
                            var weap1 = this.SiegeEquipment[EquipmentIndex.Weapon1];
                            if (!weap1.IsEqualTo(default(EquipmentElement)) && !weap1.IsEmpty && !weap1.IsInvalid())
                            {
                                mergedEquipment[EquipmentIndex.Weapon1] = weap1;
                            }
                            var weap2 = this.SiegeEquipment[EquipmentIndex.Weapon2];
                            if (!weap2.IsEqualTo(default(EquipmentElement)) && !weap2.IsEmpty && !weap2.IsInvalid())
                            {
                                mergedEquipment[EquipmentIndex.Weapon2] = weap2;
                            }
                            var weap3 = this.SiegeEquipment[EquipmentIndex.Weapon3];
                            if (!weap3.IsEqualTo(default(EquipmentElement)) && !weap3.IsEmpty && !weap3.IsInvalid())
                            {
                                mergedEquipment[EquipmentIndex.Weapon3] = weap3;
                            }
                            var weapX = this.SiegeEquipment[EquipmentIndex.ExtraWeaponSlot];
                            if (!weapX.IsEqualTo(default(EquipmentElement)) && !weapX.IsEmpty && !weapX.IsInvalid())
                            {
                                mergedEquipment[EquipmentIndex.ExtraWeaponSlot] = weapX;
                            }
                        }
                        else
                        {
                            mergedEquipment[EquipmentIndex.Weapon0] = this.SiegeEquipment[EquipmentIndex.Weapon0];
                            mergedEquipment[EquipmentIndex.Weapon1] = this.SiegeEquipment[EquipmentIndex.Weapon1];
                            mergedEquipment[EquipmentIndex.Weapon2] = this.SiegeEquipment[EquipmentIndex.Weapon2];
                            mergedEquipment[EquipmentIndex.Weapon3] = this.SiegeEquipment[EquipmentIndex.Weapon3];
                            mergedEquipment[EquipmentIndex.ExtraWeaponSlot] = this.SiegeEquipment[EquipmentIndex.ExtraWeaponSlot];
                        }
                    }
                    var head = this.SiegeEquipment[EquipmentIndex.Head];
                    if (!head.IsEqualTo(default(EquipmentElement)) && !head.IsEmpty && !head.IsInvalid())
                    {
                        mergedEquipment[EquipmentIndex.Head] = head;
                    }
                    var body = this.SiegeEquipment[EquipmentIndex.Body];
                    if (!body.IsEqualTo(default(EquipmentElement)) && !body.IsEmpty && !body.IsInvalid())
                    {
                        mergedEquipment[EquipmentIndex.Body] = body;
                    }
                    var leg = this.SiegeEquipment[EquipmentIndex.Leg];
                    if (!leg.IsEqualTo(default(EquipmentElement)) && !leg.IsEmpty && !leg.IsInvalid())
                    {
                        mergedEquipment[EquipmentIndex.Leg] = leg;
                    }
                    var gloves = this.SiegeEquipment[EquipmentIndex.Gloves];
                    if (!gloves.IsEqualTo(default(EquipmentElement)) && !gloves.IsEmpty && !gloves.IsInvalid())
                    {
                        mergedEquipment[EquipmentIndex.Gloves] = gloves;
                    }
                    var cape = this.SiegeEquipment[EquipmentIndex.Cape];
                    if (!cape.IsEqualTo(default(EquipmentElement)) && !cape.IsEmpty && !cape.IsInvalid())
                    {
                        mergedEquipment[EquipmentIndex.Cape] = cape;
                    }
                }

                return mergedEquipment;
            }
            return SiegeEquipment;
        }
    }

}
