using Newtonsoft.Json;
using RestSharp.Serialization.Json;
using SmartThingsNet.Model;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace SmartThingsTerminal.Scenarios
{
    [ScenarioMetadata(Name: "Scenes", Description: "SmartThings scenes")]
    [ScenarioCategory("Scenes")]
    class Scenes : Scenario
    {
        public override void Setup()
        {
            Dictionary<string, dynamic> displayItemList = null;

            try
            {
                if (STClient.GetAllScenes().Items?.Count > 0)
                {
                    displayItemList = STClient.GetAllScenes().Items
                        .OrderBy(t => t.SceneName)
                        .Select(t => new KeyValuePair<string, dynamic>(t.SceneName, t))
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
            ConfigureWindows<SceneSummary>(displayItemList);
        }

        public override void ConfigureStatusBar()
        {
            StatusBar = new StatusBar(new StatusItem[] {
                new StatusItem(Key.F2, "~F2~ Run Scene", () => RunScene()),
                new StatusItem(Key.F5, "~F5~ Refresh Data", () => RefreshScreen()),
                new StatusItem(Key.Home, "~Home~ Back", () => Quit())
            });
        }

        public void RunScene()
        {
            if (SelectedItem != null)
            {
                var response = STClient.RunScene(((SceneSummary)SelectedItem).SceneId);
                ShowStatusBarMessage($"Execution: {response.Status}");
            }
        }
    }
}
