using SmartThingsNet.Model;
using System.Collections.Generic;
using System.Linq;

namespace SmartThingsTerminal.Scenarios
{
    [ScenarioMetadata(Name: "Subscriptions", Description: "SmartThings application subscriptions *untested*")]
    [ScenarioCategory("Subscriptions")]
    class Subscriptions : Scenario
    {
        public override void Setup()
        {
            Dictionary<string, dynamic> displayItemList = null;

            try
            {
                if (STClient.GetAllSubscriptions().Items?.Count > 0)
                {
                    displayItemList = STClient.GetAllSubscriptions().Items
                       .OrderBy(t => t.Id)
                       .Select(t => new KeyValuePair<string, dynamic>(t.Id, t))
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
            ConfigureWindows<Subscription>(displayItemList);
        }
    }
}
