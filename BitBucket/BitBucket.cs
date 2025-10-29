using Utility;

namespace BitBucket
{
    public abstract class BitBucket(string token) : BaseUtility
    {
        //string token = "BBDC-ODY0OTA2MDI3NzE5OqsVe462P4amY13co6pHfMUQbKUq";
        protected string HTTPaccesstoken = token;
        protected const string _baseURL = "http://bb.eu.besi.corp/rest/api/1.0";
        protected const string limitposURL = "&limit=1000";
    }

}
