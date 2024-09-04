using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SiegeLoadout
{
    internal sealed class SiegeLoadoutBehavior : CampaignBehaviorBase
    {
        private HeroEquipmentExtended _heroEquipmentExtended = new();
        private bool HasLoaded { get; set; }

        public static bool UseSiegeLoadouts
        {
            get
            {
                var mapEvent = MobileParty.MainParty?.MapEvent ?? MapEvent.PlayerMapEvent;
                if (mapEvent != null)
                {
                    switch (mapEvent.EventType)
                    {
                        case TaleWorlds.CampaignSystem.MapEvents.MapEvent.BattleTypes.Siege:
                            return Main.Settings!.EnableSiegeLoadouts;
                        case TaleWorlds.CampaignSystem.MapEvents.MapEvent.BattleTypes.Hideout:
                            return Main.Settings!.EnableSiegeLoadouts && Main.Settings!.UseSiegeLoadoutInHideouts;
                        case TaleWorlds.CampaignSystem.MapEvents.MapEvent.BattleTypes.SallyOut:
                            return Main.Settings!.EnableSiegeLoadouts && Main.Settings!.UseSiegeLoadoutInSallyOut;
                        case TaleWorlds.CampaignSystem.MapEvents.MapEvent.BattleTypes.SiegeOutside:
                            return Main.Settings!.EnableSiegeLoadouts && Main.Settings!.UseSiegeLoadoutInSiegeOutside;
                        //case TaleWorlds.CampaignSystem.MapEvents.MapEvent.BattleTypes.None:
                        //case TaleWorlds.CampaignSystem.MapEvents.MapEvent.BattleTypes.FieldBattle:
                        //case TaleWorlds.CampaignSystem.MapEvents.MapEvent.BattleTypes.Raid:
                        //case TaleWorlds.CampaignSystem.MapEvents.MapEvent.BattleTypes.IsForcingVolunteers:
                        //case TaleWorlds.CampaignSystem.MapEvents.MapEvent.BattleTypes.IsForcingSupplies:
                        //case TaleWorlds.CampaignSystem.MapEvents.MapEvent.BattleTypes.AlleyFight:
                        default:
                            return false;
                    }
                }
                return false;
            }
        }

        public override void RegisterEvents()
        {
            try
            {
                CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnNewGameCreated));
                CampaignEvents.OnGameEarlyLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnGameEarlyLoaded));
            }
            catch (Exception e)
            {
                Debug.PrintError(e.Message, e.StackTrace);
                Debug.WriteDebugLineOnScreen(e.ToString());
                Debug.SetCrashReportCustomString(e.Message);
                Debug.SetCrashReportCustomStack(e.StackTrace);
            }
        }

        private void OnNewGameCreated(CampaignGameStarter starter)
        {
            try
            {
                OnLoad();
            }
            catch (Exception e) { Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString()); Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); }

        }

        /* OnGameEarlyLoaded is only present so that we can still initialize when adding the mod to a save
         * that didn't previously have it enabled (so-called "vanilla save"). This is because SyncData does
         * not even get called during game loading for behaviors that were not previously not part of the save.
         */
        private void OnGameEarlyLoaded(CampaignGameStarter starter)
        {
            try
            {
                if (!HasLoaded) // if SyncData were to be called, it would've been by now
                {
                    OnLoad();
                }
            }
            catch (Exception e) { Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString()); Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); }
        }

        private void OnLoad()
        {
            _heroEquipmentExtended ??= new HeroEquipmentExtended();
            HeroEquipmentExtended.Instance = _heroEquipmentExtended;

            HasLoaded = true;
        }

        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                if (dataStore.IsSaving)
                {
                    _heroEquipmentExtended = HeroEquipmentExtended.Instance ?? new HeroEquipmentExtended();
                }
                dataStore.SyncData("SiegeLoadout_HeroEquipmentExtended", ref _heroEquipmentExtended);
                _heroEquipmentExtended ??= new HeroEquipmentExtended();

                if (!dataStore.IsSaving)
                {
                    OnLoad();
                }
            }
            catch (Exception e)
            {
                Debug.PrintError(e.Message, e.StackTrace);
                Debug.WriteDebugLineOnScreen(e.ToString());
                Debug.SetCrashReportCustomString(e.Message);
                Debug.SetCrashReportCustomStack(e.StackTrace);
            }
        }
    }

}
