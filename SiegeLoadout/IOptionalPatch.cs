using HarmonyLib;

namespace SiegeLoadout
{
    public interface IOptionalPatch
    {
        public bool OnSubModuleLoad(Harmony harmony);

        public bool OnBeforeInitialModuleScreenSetAsRoot(Harmony harmony);
    }
}
