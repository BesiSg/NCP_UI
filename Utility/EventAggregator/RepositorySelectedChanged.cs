using Utility.Lib.BitBucketRepositories;

namespace Utility.EventAggregator
{
    public class RepositorySelectedChanged : PubSubEvent<Repository>
    {
    }
}
