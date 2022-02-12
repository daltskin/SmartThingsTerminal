using SmartThingsNet.Model;
using System;
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
            Dictionary<string, dynamic> dataItemList = null;
            Dictionary<string, string> displayItemList = null;
            try
            {
                if (STClient.GetAllScenes().Items?.Count > 0)
                {
                    dataItemList = STClient.GetAllScenes().Items
                        .OrderBy(t => t.SceneName)
                        .Select(t => new KeyValuePair<string, dynamic>(t.SceneId, t))
                        .ToDictionary(t => t.Key, t => t.Value);

                    displayItemList = STClient.GetAllScenes().Items
                            .OrderBy(o => o.SceneName)
                            .Select(t => new KeyValuePair<string, string>(t.SceneId, t.SceneName))
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
            ConfigureWindows<SceneSummary>(displayItemList, dataItemList);
        }

        public override void ConfigureStatusBar()
        {
            StatusBar = new StatusBar(new StatusItem[] {
                new StatusItem(Key.F2, "~F2~ Run Scene", () => RunScene()),
                new StatusItem(Key.F5, "~F5~ Refresh Data", () => RefreshScreen()),
                new StatusItem(Key.F9, "~F9~ Menu", () => { }),
                new StatusItem(Key.F12, "~F12~ Back", () => Quit())
            });
        }

        public void RunScene()
        {
            if (SelectedItem != null)
            {
                try
                {
                    var response = STClient.RunScene(((SceneSummary)SelectedItem).SceneId);
                    ShowStatusBarMessage($"Execution: {response.Status}");
                }
                catch (SmartThingsNet.Client.ApiException exp)
                {
                    ShowErrorMessage($"Error {exp.ErrorCode}{Environment.NewLine}{exp.Message}");
                }
                catch (Exception exp)
                {
                    ShowErrorMessage($"Error {exp.Message}");
                }
            }
        }
    }
}
