using System.Collections.Generic;

namespace SpotiBot.Api.IntegrationTests.Library
{
    /// <summary>
    /// Azure functions use a file called local.settings.json to store app settings in.
    /// <a href="https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local#local-settings-file">See documentation.</a>
    /// </summary>
    public class LocalSettings
    {
        public bool IsEncrypted { get; set; }
        public Dictionary<string, string> Values { get; set; }
    }
}
