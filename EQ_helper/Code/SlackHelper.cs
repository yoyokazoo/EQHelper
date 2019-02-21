using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQ_helper
{
    public static class SlackHelper
    {
        public static string SlackWebHook = "https://hooks.slack.com/services/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";

        public static void SendSlackMessageAsync(string message)
        {
            var webhookUrl = new Uri(SlackWebHook);
            var slackClient = new SlackClient(webhookUrl);
            slackClient.SendMessageAsync(message);
        }
    }
}
