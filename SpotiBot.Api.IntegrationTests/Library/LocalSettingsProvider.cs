using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.IO;

namespace SpotiBot.Api.IntegrationTests.Library
{
    /// <summary>
    /// Reads values from the local settings json file and adds them to the configuration.
    /// </summary>
    public class LocalSettingsProvider : ConfigurationProvider
    {
        private readonly string _solutionDirectoryName;
        private readonly string _projectDirectoryName;
        private readonly string _localSettingsFileName;

        public LocalSettingsProvider(string solutionDirectoryName, string projectDirectoryName, string localSettingsFileName)
        {
            _solutionDirectoryName = solutionDirectoryName;
            _projectDirectoryName = projectDirectoryName;
            _localSettingsFileName = localSettingsFileName;
        }

        public override void Load()
        {
            var solutionDirectory = GetSolutionDirectory(_solutionDirectoryName);

            // TODO: this won't work when running integrationtests in a pipeline, since local settings aren't available there.
            var localSettingPath = Path.Combine(solutionDirectory.FullName, _projectDirectoryName, _localSettingsFileName);

            // Read from the local settings file.
            var json = File.ReadAllText(localSettingPath);
            var localSettings = JsonConvert.DeserializeObject<LocalSettings>(json);

            Data = localSettings.Values;
        }

        private static DirectoryInfo GetSolutionDirectory(string solutionDirectoryName)
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            var directoryInfo = new DirectoryInfo(currentDirectory);

            // Keep searching in parent folders until the solution folder is found.
            while (directoryInfo.Name != solutionDirectoryName)
                directoryInfo = directoryInfo.Parent;

            return directoryInfo;
        }
    }
}
