using System.Collections.Generic;

using TaleWorlds.SaveSystem;

namespace SiegeLoadout
{
    internal sealed class CustomSaveableTypeDefiner : SaveableTypeDefiner
    {
        public const int SaveBaseId_b0tlanner0 = 300_711_200;
        public const int SaveBaseId = SaveBaseId_b0tlanner0 + 0;

        public CustomSaveableTypeDefiner() : base(SaveBaseId) { }

        protected override void DefineClassTypes()
        {
            base.DefineClassTypes();
            AddClassDefinition(typeof(HeroEquipmentExtended), 1);
            AddClassDefinition(typeof(HeroExtended), 2);
        }

        protected override void DefineContainerDefinitions()
        {
            base.DefineContainerDefinitions();
            ConstructContainerDefinition(typeof(List<HeroExtended>));
        }
    }

}
