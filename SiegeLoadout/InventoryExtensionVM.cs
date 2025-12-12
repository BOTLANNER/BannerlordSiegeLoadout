using System;
using System.Reflection;

using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;

using HarmonyLib;

using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace SiegeLoadout
{
    [ViewModelMixin(refreshMethodName: "RefreshValues", true)]

    public class InventoryExtensionVM : BaseViewModelMixin<SPInventoryVM>

    {
        private bool _isInBattleSet = true;

        private bool _isSiegeFilterHighlightEnabled;

        private HintViewModel? _siegeOutfitHint;


        private readonly MethodInfo updateRightCharacter;
        private readonly MethodInfo getItemFromIndex;

        public InventoryExtensionVM(SPInventoryVM vm) : base(vm)
        {
            updateRightCharacter = AccessTools.Method(typeof(SPInventoryVM), nameof(UpdateRightCharacter));
            getItemFromIndex = AccessTools.Method(typeof(SPInventoryVM), nameof(GetItemFromIndex));
        }

        [DataSourceProperty]
        public WeakReference<InventoryExtensionVM> Mixin => new(this);


        [DataSourceProperty]
        public bool IsInBattleSet
        {
            get
            {
                return this._isInBattleSet;
            }
            set
            {
                if (value != this._isInBattleSet)
                {
                    this._isInBattleSet = value;
                    ViewModel?.OnPropertyChangedWithValue(value, "IsInBattleSet");

                    UpdateRightCharacter();
                    Game.Current.EventManager.TriggerEvent<InventoryEquipmentTypeChangedEvent>(new InventoryEquipmentTypeChangedEvent(value));
                }
            }
        }

        [DataSourceProperty]
        public bool IsSiegeFilterHighlightEnabled
        {
            get
            {
                return this._isSiegeFilterHighlightEnabled;
            }
            set
            {
                if (value != this._isSiegeFilterHighlightEnabled)
                {
                    this._isSiegeFilterHighlightEnabled = value;
                    ViewModel?.OnPropertyChangedWithValue(value, "IsSiegeFilterHighlightEnabled");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel? SiegeOutfitHint
        {
            get
            {
                return this._siegeOutfitHint;
            }
            set
            {
                if (value != this._siegeOutfitHint)
                {
                    this._siegeOutfitHint = value;
                    ViewModel?.OnPropertyChangedWithValue(value, "SiegeOutfitHint");
                }
            }
        }

        public override void OnRefresh()
        {
            base.OnRefresh();
            this.SiegeOutfitHint = new HintViewModel(new TextObject("{=siege_loadout_h_str_inventory_siege_outfit}Siege Outfit", null), null);
        }

        public override void OnFinalize()
        {
            this._isInBattleSet = true;

            base.OnFinalize();
        }

        private void UpdateRightCharacter()
        {
            updateRightCharacter.Invoke(ViewModel, null);
        }

        internal SPItemVM GetItemFromIndex(EquipmentIndex itemType)
        {
            return (SPItemVM) getItemFromIndex.Invoke(ViewModel, new object[] { itemType });
        }
    }

}
