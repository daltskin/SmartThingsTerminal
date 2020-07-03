using SmartThingsNet.Model;

namespace SmartThingsTerminal.Scenarios
{
    [ScenarioMetadata(Name: "Locations", Description: "SmartThings locations")]
    [ScenarioCategory("Locations")]
    class Locations : Scenario
    {
        public override void Setup()
        {
            ConfigureWindows<Location>();
        }
    }
}
