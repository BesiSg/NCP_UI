using Utility.Lib.BitBucketRepositories;

namespace Utility.EventAggregator
{
    public class BranchSelectedChanged : PubSubEvent<Branch>
    {
    }
}
