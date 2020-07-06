using System.Collections.Generic;
using System.Linq;

namespace SmartThingsTerminal.Scenarios
{
    [ScenarioMetadata(Name: "Schedules", Description: "SmartThings application schedules")]
    [ScenarioCategory("Schedules")]
    class Schedules : Scenario
    {
        public override void Setup()
        {
            Dictionary<string, dynamic> displayItemList = null;
            try
            {
                if (STClient.GetAllSchedules().Items?.Count > 0)
                {
                    displayItemList = STClient.GetAllSchedules().Items
                       .OrderBy(t => t.Name)
                       .Select(t => new KeyValuePair<string, dynamic>(t.Name, t))
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
            ConfigureWindows<Scenario>(displayItemList);
        }
    }
}
