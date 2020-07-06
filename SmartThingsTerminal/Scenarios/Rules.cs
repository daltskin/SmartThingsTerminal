using Newtonsoft.Json;
using SmartThingsNet.Model;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace SmartThingsTerminal.Scenarios
{
    [ScenarioMetadata(Name: "Rules", Description: "SmartThings rules")]
    [ScenarioCategory("Rules")]
    class Rules : Scenario
    {
        public override void Setup()
        {
            Dictionary<string, dynamic> displayItemList = null;
            try
            {
                if (STClient.GetAllRules().Items?.Count > 0)
                {
                    displayItemList = STClient.GetAllRules().Items
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
            ConfigureWindows<Rule>(displayItemList);
        }

        public override void ConfigureStatusBar()
        {
            StatusBar = new StatusBar(new StatusItem[] {
                new StatusItem(Key.F3, "~F3~ Edit", () => EnableEditMode()),
                new StatusItem(Key.F4, "~F4~ Save", () => SaveUpdates()),
                new StatusItem(Key.F5, "~F5~ Refresh Data", () => RefreshScreen()),
                new StatusItem(Key.Home, "~Home~ Back", () => Quit())
            });
        }

        public override bool SaveUpdates()
        {
            var json = JsonView?.Text.ToString();

            try
            {
                var rule = JsonConvert.DeserializeObject<Rule>(json);
                string locationId = null;

                RuleRequest ruleRequest = new RuleRequest(rule.Name, new List<Action>(), rule.TimeZoneId);
                // Get the locationId for this rule
                foreach (var location in STClient.GetAllLocations().Items)
                {
                    var locationRules = STClient.GetAllRules(location.LocationId.ToString()).Items.Where(r=>r.Id == rule.Id);
                    locationId = location.LocationId.ToString();
                    break;
                }
                
                var response = STClient.UpdateRule(rule.Id, locationId, ruleRequest);
                RefreshScreen();
            }
            catch (System.Exception exp)
            {
                ShowStatusBarMessage($"Error updating: {exp.Message}");
            }
            return true;
        }
    }
}