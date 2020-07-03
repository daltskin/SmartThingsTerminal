using SmartThingsNet.Model;
using Terminal.Gui;

namespace SmartThingsTerminal.Scenarios
{
    [ScenarioMetadata(Name: "Scenes", Description: "SmartThings scenes")]
    [ScenarioCategory("Scenes")]
    class Scenes : Scenario
    {
        public override void Setup()
        {
            ConfigureWindows<SceneSummary>();
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
                var statusButton = new Button($"Execution: {response.Status}")
                {
                    X = Pos.Center(),
                    Y = Pos.Bottom(HostPane) + 4,
                    IsDefault = true,
                };
                Top.Add(statusButton);
            }
        }
    }
}
