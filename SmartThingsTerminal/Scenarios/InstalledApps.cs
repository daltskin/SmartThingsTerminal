using SmartThingsNet.Model;
using System.Collections.Generic;
using System.Linq;

namespace SmartThingsTerminal.Scenarios
{
    [ScenarioMetadata(Name: "InstalledApps", Description: "SmartThings installed applications")]
    [ScenarioCategory("Installed Apps")]
    class InstalledApps : Scenario
    {
        public override void Setup()
        {
            Dictionary<string, dynamic> displayItemList = null;

            try
            {
                if (STClient.GetAllInstalledApps().Items?.Count > 0)
                {
                    displayItemList = STClient.GetAllInstalledApps().Items
                        .OrderBy(t => t.DisplayName)
                        .Select(t => new KeyValuePair<string, dynamic>(t.DisplayName, t))
                        .ToDictionary(t => t.Key, t => t.Value);
                }
            }
            catch (SmartThingsNet.Client.ApiException exp)
            {
                SetErrorView($"Error calling API: {exp.Source} {exp.ErrorCode} {exp.Message}");
            }
            catch (System.Exception exp)
            {
                SetErrorView($"Unknown error calling API: {exp.Message}");
            }

            ConfigureWindows<InstalledApp>(displayItemList);
        }
    }
}
