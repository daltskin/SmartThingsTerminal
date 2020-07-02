using SmartThingsNet.Model;

namespace SmartThingsTerminal.Scenarios
{
    [ScenarioMetadata(Name: "Subscriptions", Description: "SmartThings application subscriptions *untested*")]
    [ScenarioCategory("Subscriptions")]
    class Subscriptions : Scenario
    {
        public override void Setup()
        {
            ConfigureWindows<Subscription>();

        }
    }
}
