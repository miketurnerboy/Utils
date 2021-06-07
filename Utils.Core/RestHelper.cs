using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Utils.Core
{
    public static class RestHelper
    {
        public async static Task<IRestResponse> Client(string baseUrl, string resource, Method method, DataFormat dataFormat, object requestBody, Dictionary<string, string> headersValues, List<Parameter> parameters = null)
        {
            IRestClient client = new RestClient(baseUrl);
            RestRequest request = new(resource, method);
            request.RequestFormat = dataFormat;
            if (headersValues != null)
                request.AddHeaders(headersValues);
            if (parameters != null)
                request.AddOrUpdateParameters(parameters);
            request.AddJsonBody(requestBody);
            IRestResponse response = await client.ExecuteAsync(request);
            return response;
        }
    }
}
