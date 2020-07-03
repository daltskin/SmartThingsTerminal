using SmartThingsNet.Model;

namespace SmartThingsTerminal.Scenarios
{
    [ScenarioMetadata(Name: "Rooms", Description: "SmartThings rooms")]
    [ScenarioCategory("Rooms")]
    class Rooms : Scenario
    {
        public override void Setup()
        {
            ConfigureWindows<Room>();
        }
    }
}
