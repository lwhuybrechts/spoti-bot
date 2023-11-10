using Microsoft.Extensions.Configuration;

namespace SpotiBot.Api.IntegrationTests.Library
{
    public static class ConfigurationBuilderExtensions
    {
        /// <summary>
        /// Azure functions use a file called local.settings.json to store app settings in.
        /// To run integrationtests we also need to add these settings to the configuration.
        /// <a href="https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local#local-settings-file">See documentation.</a>
        /// </summary>
        public static void AddLocalSettings(this IConfigurationBuilder configurationBuilder,
            string solutionDirectoryName, string projectDirectoryName, string localSettingsFileName)
        {
            // Add the local settings source to the configuration builder.
            configurationBuilder.Add(new LocalSettingsSource(solutionDirectoryName, projectDirectoryName, localSettingsFileName));
        }

        private class LocalSettingsSource : IConfigurationSource
        {
            private readonly string _solutionDirectoryName;
            private readonly string _projectDirectoryName;
            private readonly string _localSettingsFileName;

            public LocalSettingsSource(string solutionDirectoryName, string projectDirectoryName, string localSettingsFileName)
            {
                _solutionDirectoryName = solutionDirectoryName;
                _projectDirectoryName = projectDirectoryName;
                _localSettingsFileName = localSettingsFileName;
            }

            /// <summary>
            /// When the configuration builder is built, provides the local settings.
            /// </summary>
            public IConfigurationProvider Build(IConfigurationBuilder builder)
            {
                return new LocalSettingsProvider(_solutionDirectoryName, _projectDirectoryName, _localSettingsFileName);
            }
        }
    }
}
