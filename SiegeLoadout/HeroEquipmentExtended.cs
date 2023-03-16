using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace SiegeLoadout
{
    public class HeroEquipmentExtended
    {
        private List<HeroExtended>? _list;

        [SaveableProperty(1)]
        public List<HeroExtended> ExtendedHeroesList 
        {
            get
            {
                if (_list == null)
                {
                    _list = new List<HeroExtended>();
                }
                return _list;
            }
            set
            {
                _list = value;
            }
        }

        private static HeroEquipmentExtended? _instance = null;
        public static HeroEquipmentExtended? Instance
        {
            get
            {
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        public HeroEquipmentExtended() 
        {
        }

        internal HeroExtended Extend(Hero hero, Equipment? siegeEquipment = null)
        {
            var extended = ExtendedHeroesList.FirstOrDefault(h => h.Hero == hero);
            if (extended == null)
            {
                extended = new HeroExtended
                {
                    Hero = hero
                };
                ExtendedHeroesList.Add(extended);
            }
            if (siegeEquipment != null)
            {
                extended.SiegeEquipment = siegeEquipment;
            }

            return extended;
        }

        internal HeroExtended AsExtended(Hero hero, bool extendIfNotFound = false)
        {
            if (extendIfNotFound)
            {
                return Extend(hero);
            }

            var extended = ExtendedHeroesList.FirstOrDefault(h => h.Hero == hero);
            return extended;
        }

        internal bool IsExtended(Hero hero)
        {
            return AsExtended(hero) != default;
        }
    }

    public static class HeroExtensions
    {
        public static HeroExtended? AsExtended(this Hero hero, bool extendIfNotFound = false)
        {
            return HeroEquipmentExtended.Instance?.AsExtended(hero,extendIfNotFound);
        }

        public static HeroExtended? Extend(this Hero hero, Equipment? siegeEquipment = null)
        {
            return HeroEquipmentExtended.Instance?.Extend(hero,siegeEquipment);
        }

        public static bool IsExtended(this Hero hero)
        {
            return HeroEquipmentExtended.Instance?.AsExtended(hero) != default;
        }
    }

}
