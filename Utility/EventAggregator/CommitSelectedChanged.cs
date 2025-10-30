using Utility.Lib.BitBucketRepositories;

namespace Utility.EventAggregator
{
    public class CommitSelectedChanged : PubSubEvent<Commit>
    {
    }
}
