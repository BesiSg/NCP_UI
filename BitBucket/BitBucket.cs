using BitBucket;
using System.Net.Http;
using Utility;

namespace BitBucketHandler
{
    public abstract class BitBucket<BitBucketStorage>(string token, BitBucketStorage? dataset, HttpClient client) : BaseUtility
    {
        //string token = "BBDC-ODY0OTA2MDI3NzE5OqsVe462P4amY13co6pHfMUQbKUq";
        protected BitBucketStorage? _dataset = dataset;
        protected string HTTPaccesstoken = token;
        protected const string _baseURL = "http://bb.eu.besi.corp/rest/api/1.0";
        protected const string _limitposURL = "&limit=1000";
        public BitBucketStorage Data => _dataset;
        protected BitBucketRestAPI _bitBucket = new BitBucketRestAPI(_baseURL, _limitposURL, token, client);
    }

}
