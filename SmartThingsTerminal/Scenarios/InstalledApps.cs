using SmartThingsNet.Model;

namespace SmartThingsTerminal.Scenarios
{
    [ScenarioMetadata(Name: "InstalledApps", Description: "SmartThings installed applications")]
    [ScenarioCategory("Installed Apps")]
    class InstalledApps : Scenario
    {
        public override void Setup()
        {
            ConfigureWindows<InstalledApp>();
        }
    }
}
