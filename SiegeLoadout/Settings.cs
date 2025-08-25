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

        private const string EnableSiegeLoadouts_Hint = "{=NtYhm6pRNT}Enables using Siege Loadouts. Otherwise will always use Battle Loadout. [ Default: ON ]";

        [SettingPropertyBool("{=4WFRIq4Ebk}Enable Siege Loadouts", HintText = EnableSiegeLoadouts_Hint, RequireRestart = false, Order = 0, IsToggle = true)]
        [SettingPropertyGroup("{=v8RBUW5awV}General Settings", GroupOrder = 0)]
        public bool EnableSiegeLoadouts { get; set; } = true;

        private const string UseSiegeArmorInTournaments_Hint = "{=NkiKKZ3wju}Use armor from Siege loadout in Tournaments. Otherwise will use armor from Battle loadout. [ Default: OFF ]";

        [SettingPropertyBool("{=tBh20PkZa0}Use Siege Armor In Tournaments", HintText = UseSiegeArmorInTournaments_Hint, RequireRestart = false, Order = 1, IsToggle = false)]
        [SettingPropertyGroup("{=v8RBUW5awV}General Settings")]
        public bool UseSiegeArmorInTournaments { get; set; } = false;

        private const string UseSiegeLoadoutInHideouts_Hint = "{=hqfjT9bhyn}Use Siege Loadout in Hideout missions. Otherwise will use Battle Loadout. [ Default: OFF ]";

        [SettingPropertyBool("{=Hj4y4yjr22}Use Siege Loadout In Hideouts", HintText = UseSiegeLoadoutInHideouts_Hint, RequireRestart = false, Order = 2, IsToggle = false)]
        [SettingPropertyGroup("{=v8RBUW5awV}General Settings")]
        public bool UseSiegeLoadoutInHideouts { get; set; } = false;

        private const string UseSiegeLoadoutInSallyOut_Hint = "{=KHifb2IZDy}Use Siege Loadout in Sally out during Siege. Otherwise will use Battle Loadout. [ Default: ON ]";

        [SettingPropertyBool("{=PEAAATMzjJ}Use Siege Loadout In Sally Out", HintText = UseSiegeLoadoutInSallyOut_Hint, RequireRestart = false, Order = 3, IsToggle = false)]
        [SettingPropertyGroup("{=v8RBUW5awV}General Settings")]
        public bool UseSiegeLoadoutInSallyOut { get; set; } = true;

        private const string UseSiegeLoadoutInSiegeOutside_Hint = "{=qTtFcul4Ja}Use Siege Loadout in outside battle during Siege. Otherwise will use Battle Loadout. [ Default: OFF ]";

        [SettingPropertyBool("{=RppZ4sXtGV}Use Siege Loadout In Siege Outside", HintText = UseSiegeLoadoutInSiegeOutside_Hint, RequireRestart = false, Order = 4, IsToggle = false)]
        [SettingPropertyGroup("{=v8RBUW5awV}General Settings")]
        public bool UseSiegeLoadoutInSiegeOutside { get; set; } = false;

        private const string EnforceFullSet_Hint = "{=FhNb3mLbFK}Use full Siege Loadout. Otherwise will use Battle Loadout with populated Siege Loadout pieces. [ Default: OFF ]";

        [SettingPropertyBool("{=40fWbiFKQR}Enforce Full Set", HintText = EnforceFullSet_Hint, RequireRestart = false, Order = 5, IsToggle = false)]
        [SettingPropertyGroup("{=p97qWrYgJ7}Outfit Parts", GroupOrder = 1)]
        public bool EnforceFullSet { get; set; } = false;

        private const string UseBattleLoadoutMountAndHarness_Hint = "{=N9sHgd8os6}When not enforcing full set, uses the mount and harness pieces from battle loadout. Otherwise will still enforce mount and harness pieces from Siege Loadout. [ Default: OFF ]";

        [SettingPropertyBool("{=iOC7cCbuKi}Use Battle Loadout Mount and Harness", HintText = UseBattleLoadoutMountAndHarness_Hint, RequireRestart = false, Order = 6, IsToggle = false)]
        [SettingPropertyGroup("{=p97qWrYgJ7}Outfit Parts")]
        public bool UseBattleLoadoutMountAndHarness { get; set; } = false;

        private const string ButtonsBelowMountSwap_Hint = "{=inQ6N0oOmd}When OFF, loadout buttons on inventory screen will be above mount swap buttons and hints near the center. When ON, loadout buttons will be below the mount swap buttons and above the Cancel,Reset, Done button bar. [ Default: OFF ]";

        [SettingPropertyBool("{=jZ6DneHlBK}Buttons below mount swap", HintText = ButtonsBelowMountSwap_Hint, RequireRestart = false, Order = 7, IsToggle = false)]
        [SettingPropertyGroup("{=GMVfyevmyT}Button Layout", GroupOrder = 2)]
        public bool ButtonsBelowMountSwap { get; set; } = false;
    }

}
