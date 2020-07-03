using SmartThingsNet.Model;

namespace SmartThingsTerminal.Scenarios
{
    [ScenarioMetadata(Name: "Rules", Description: "SmartThings rules")]
    [ScenarioCategory("Rules")]
    class Rules : Scenario
    {
        public override void Setup()
        {
            ConfigureWindows<Rule>();
        }
    }
}