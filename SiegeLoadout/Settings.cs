using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
#if MCM_v5
using MCM.Abstractions.Base.Global;
#else
using MCM.Abstractions.Settings.Base.Global;
#endif

namespace SiegeLoadout
{
    public class Settings : AttributeGlobalSettings<Settings>
    {
        public override string Id => $"{Main.Name}_v1";
        public override string DisplayName => Main.DisplayName;
        public override string FolderName => Main.Name;
        public override string FormatType => "json";

        private const string EnableSiegeLoadouts_Hint = "Enables using Siege Loadouts. Otherwise will always use Battle Loadout. [ Default: ON ]";

        [SettingPropertyBool("Enable Siege Loadouts", HintText = EnableSiegeLoadouts_Hint, RequireRestart = false, Order = 0, IsToggle = true)]
        [SettingPropertyGroup("General Settings", GroupOrder = 0)]
        public bool EnableSiegeLoadouts { get; set; } = true;

        private const string UseSiegeArmorInTournaments_Hint = "Use armor from Siege loadout in Tournaments. Otherwise will use armor from Battle loadout. [ Default: OFF ]";

        [SettingPropertyBool("Use Siege Armor In Tournaments", HintText = UseSiegeArmorInTournaments_Hint, RequireRestart = false, Order = 1, IsToggle = false)]
        [SettingPropertyGroup("General Settings")]
        public bool UseSiegeArmorInTournaments { get; set; } = false;

        private const string UseSiegeLoadoutInHideouts_Hint = "Use Siege Loadout in Hideout missions. Otherwise will use Battle Loadout. [ Default: OFF ]";

        [SettingPropertyBool("Use Siege Loadout In Hideouts", HintText = UseSiegeLoadoutInHideouts_Hint, RequireRestart = false, Order = 2, IsToggle = false)]
        [SettingPropertyGroup("General Settings")]
        public bool UseSiegeLoadoutInHideouts { get; set; } = false;

        private const string UseSiegeLoadoutInSallyOut_Hint = "Use Siege Loadout in Sally out during Siege. Otherwise will use Battle Loadout. [ Default: ON ]";

        [SettingPropertyBool("Use Siege Loadout In Sally Out", HintText = UseSiegeLoadoutInSallyOut_Hint, RequireRestart = false, Order = 3, IsToggle = false)]
        [SettingPropertyGroup("General Settings")]
        public bool UseSiegeLoadoutInSallyOut { get; set; } = true;

        private const string UseSiegeLoadoutInSiegeOutside_Hint = "Use Siege Loadout in outside battle during Siege. Otherwise will use Battle Loadout. [ Default: OFF ]";

        [SettingPropertyBool("Use Siege Loadout In Siege Outside", HintText = UseSiegeLoadoutInSiegeOutside_Hint, RequireRestart = false, Order = 4, IsToggle = false)]
        [SettingPropertyGroup("General Settings")]
        public bool UseSiegeLoadoutInSiegeOutside { get; set; } = false;

        private const string EnforceFullSet_Hint = "Use full Siege Loadout. Otherwise will use Battle Loadout with populated Siege Loadout pieces. [ Default: OFF ]";
        
        [SettingPropertyBool("Enforce Full Set", HintText = EnforceFullSet_Hint, RequireRestart = false, Order = 5, IsToggle = false)]
        [SettingPropertyGroup("Outfit Parts", GroupOrder = 1)]
        public bool EnforceFullSet { get; set; } = false;

        private const string UseBattleLoadoutMountAndHarness_Hint = "When not enforcing full set, uses the mount and harness pieces from battle loadout. Otherwise will still enforce mount and harness pieces from Siege Loadout. [ Default: OFF ]";
        
        [SettingPropertyBool("Use Battle Loadout Mount and Harness", HintText = UseBattleLoadoutMountAndHarness_Hint, RequireRestart = false, Order = 6, IsToggle = false)]
        [SettingPropertyGroup("Outfit Parts")]
        public bool UseBattleLoadoutMountAndHarness { get; set; } = false;
    }

}
