using Utility;

namespace PatchViewerModule.ViewModels
{
    public abstract class ViewModelBase(IEventAggregator ea) : BaseUtility
    {
        protected IEventAggregator _ea = ea;
    }
}
