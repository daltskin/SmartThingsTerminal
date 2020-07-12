using SmartThingsNet.Model;
using System;
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
            Dictionary<string, dynamic> dataItemList = null;
            Dictionary<string, string> displayItemList = null;

            try
            {
                if (STClient.GetAllInstalledApps().Items?.Count > 0)
                {
                    dataItemList = STClient.GetAllInstalledApps().Items
                        .OrderBy(t => t.DisplayName)
                        .Select(t => new KeyValuePair<string, dynamic>(t.InstalledAppId.ToString(), t))
                        .ToDictionary(t => t.Key, t => t.Value);

                    displayItemList = STClient.GetAllInstalledApps().Items
                        .OrderBy(o => o.DisplayName)
                        .Select(t => new KeyValuePair<string, string>(t.InstalledAppId.ToString(), t.DisplayName))
                        .ToDictionary(t => t.Key, t => t.Value);
                }
            }
            catch (SmartThingsNet.Client.ApiException exp)
            {
                ShowErrorMessage($"Error {exp.ErrorCode}{Environment.NewLine}{exp.Message}");
            }
            catch (Exception exp)
            {
                ShowErrorMessage($"Error {exp.Message}");
            }

            ConfigureWindows<InstalledApp>(displayItemList, dataItemList);
        }
    }
}
