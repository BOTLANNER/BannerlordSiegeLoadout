using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs;

using TaleWorlds.CampaignSystem.ComponentInterfaces;

namespace SiegeLoadout
{
    [PrefabExtension("Inventory", "//ButtonWidget[@Id='SwapPlacesButtonWidget']")]

    public class InventoryExtensionAbove : PrefabExtensionInsertAsSiblingPatch

    {
        private readonly XmlDocument document;

        public override string Id => "InventoryExtend";

        public bool ShowButtons => (!Main.Settings?.ButtonsBelowMountSwap ?? false);
        public override InsertType Type => InsertType.Append;

        public InventoryExtensionAbove()
        {
            document = InventoryExtendDocument.Document;
        }

        public override XmlDocument GetPrefabExtension()
        {
            if (!ShowButtons)
            {
                return EmptyDocument.Empty;
            }
            return document;
        }
    }
    [PrefabExtension("Inventory", "//ListPanel[@Id='WarningsAndSwapPlacesPanel']")]

    public class InventoryExtensionBelow : PrefabExtensionInsertAsSiblingPatch

    {
        private readonly XmlDocument document;

        public override string Id => "InventoryExtend";

        public bool ShowButtons => (Main.Settings?.ButtonsBelowMountSwap ?? true);

        public override InsertType Type => InsertType.Prepend;

        public InventoryExtensionBelow()
        {
            document = InventoryExtendDocument.Document;
        }

        public override XmlDocument GetPrefabExtension()
        {
            if (!ShowButtons)
            {
                return EmptyDocument.Empty;
            }
            return document;
        }
    }

    static class EmptyDocument
    {
        internal static XmlDocument Empty;

        static EmptyDocument()
        {
            Empty = new XmlDocument();
            Empty.LoadXml("<Constants></Constants>");
        }
    }
    static class InventoryExtendDocument
    {
        internal static XmlDocument Document = ResourcePrefab.Load("SiegeLoadout.InventoryExtend.xml");
    }

}
