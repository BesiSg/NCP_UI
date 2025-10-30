using Utility;

namespace BitBucketHandler
{
    public abstract class BitBucket<BitBucketStorage>(string token, BitBucketStorage dataset) : BaseUtility
    {
        //string token = "BBDC-ODY0OTA2MDI3NzE5OqsVe462P4amY13co6pHfMUQbKUq";
        protected BitBucketStorage _dataset = dataset;
        protected string HTTPaccesstoken = token;
        protected const string _baseURL = "http://bb.eu.besi.corp/rest/api/1.0";
        protected const string limitposURL = "&limit=1000";
    }

}
