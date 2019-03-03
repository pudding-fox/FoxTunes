using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class AsyncCollectionLoaderBehaviour : StandardBehaviour
    {
        public override void InitializeComponent(ICore core)
        {
            CollectionLoader<MetaDataItem>.Instance = AsyncCollectionLoader<MetaDataItem>.Instance;
            CollectionLoader<LibraryHierarchyNode>.Instance = AsyncCollectionLoader<LibraryHierarchyNode>.Instance;
            base.InitializeComponent(core);
        }
    }
}
