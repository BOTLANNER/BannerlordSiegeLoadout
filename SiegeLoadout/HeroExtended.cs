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
                        mergedEquipment[EquipmentIndex.Weapon0] = this.SiegeEquipment[EquipmentIndex.Weapon0];
                        mergedEquipment[EquipmentIndex.Weapon1] = this.SiegeEquipment[EquipmentIndex.Weapon1];
                        mergedEquipment[EquipmentIndex.Weapon2] = this.SiegeEquipment[EquipmentIndex.Weapon2];
                        mergedEquipment[EquipmentIndex.Weapon3] = this.SiegeEquipment[EquipmentIndex.Weapon3];
                        mergedEquipment[EquipmentIndex.ExtraWeaponSlot] = this.SiegeEquipment[EquipmentIndex.ExtraWeaponSlot];
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
