using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs;

using TaleWorlds.CampaignSystem.ComponentInterfaces;

namespace SiegeLoadout
{
    [PrefabExtension("Inventory", "/Prefab/Window/InventoryScreenWidget/Children/CharacterTableauWidget/Children/ButtonWidget")]

    public class InventoryExtension : PrefabExtensionInsertAsSiblingPatch

    {
        private readonly XmlDocument document;

        public override string Id => "InventoryExtend";

        public override InsertType Type => InsertType.Append;

        public InventoryExtension()
        {
            document = ResourcePrefab.Load("SiegeLoadout.InventoryExtend.xml");
        }

        public override XmlDocument GetPrefabExtension()
        {
            return document;
        }
    }

}
