namespace SmartThingsTerminal.Scenarios
{
    [ScenarioMetadata(Name: "Schedules", Description: "SmartThings application schedules")]
    [ScenarioCategory("Schedules")]
    class Schedules : Scenario
    {
        public override void Setup()
        {
            ConfigureWindows<Scenario>();
        }
    }
}
