using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using Microsoft.NETCore.App;
using Newtonsoft.Json;
using System.Net.Http;
//using System.Runtime.Serialization.Primitives;

namespace EQ_helper
{
    public class SlackClient
    {
        private readonly Uri _webhookUrl;
        private readonly HttpClient _httpClient = new HttpClient();

        public SlackClient(Uri webhookUrl)
        {
            _webhookUrl = webhookUrl;
        }

        public async Task<HttpResponseMessage> SendMessageAsync(string message,
            string channel = null, string username = null)
        {
            var payload = new
            {
                text = message,
                channel,
                username,
            };
            var serializedPayload = JsonConvert.SerializeObject(payload);
            var response = await _httpClient.PostAsync(_webhookUrl,
                new StringContent(serializedPayload, Encoding.UTF8, "application/json"));

            return response;
        }
    }
}
