using Newtonsoft.Json;
using System.Text;

namespace Email.Sender.ServiceBus.Extensions
{
    public static class MessageBrokerHelperExtension
    {
        private static JsonSerializerSettings jsonSerializerSettings;

        private static JsonSerializerSettings JsonSerializerSettings
        {
            get
            {
                if (jsonSerializerSettings == null)
                {
                    jsonSerializerSettings = new JsonSerializerSettings
                    {
                        //NullValueHandling = NullValueHandling.Ignore
                    };
                }
                return jsonSerializerSettings;
            }
        }

        public static T Deserialize<T>(this byte[] message)
        {
            string content = Encoding.UTF8.GetString(message);
            return JsonConvert.DeserializeObject<T>(content, JsonSerializerSettings);
        }
    }
}