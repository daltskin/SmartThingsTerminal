using Newtonsoft.Json;
using SmartThingsNet.Model;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace SmartThingsTerminal.Scenarios
{
    [ScenarioMetadata(Name: "Locations", Description: "SmartThings locations")]
    [ScenarioCategory("Locations")]
    class Locations : Scenario
    {
        public override void Setup()
        {
            Dictionary<string, dynamic> displayItemList = null;

            try
            {
                if (STClient.GetAllLocations().Items?.Count > 0)
                {
                    displayItemList = STClient.GetAllLocations().Items
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

            ConfigureWindows<Location>(displayItemList);
        }


        public override void ConfigureStatusBar()
        {
            StatusBar = new StatusBar(new StatusItem[] {
                new StatusItem(Key.F3, "~F3~ Edit", () => EnableEditMode()),
                new StatusItem(Key.F4, "~F4~ Save", () => SaveItem()),
                new StatusItem(Key.F5, "~F5~ Refresh Data", () => RefreshScreen()),
                new StatusItem(Key.Home, "~Home~ Back", () => Quit())
            });
        }

        public override bool SaveItem()
        {
            var json = JsonView?.Text.ToString();

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var location = JsonConvert.DeserializeObject<Location>(json);

                    UpdateLocationRequest locationRequest = new UpdateLocationRequest(
                        location.Name,
                        location.Latitude,
                        location.Longitude,
                        location.RegionRadius,
                        location.TemperatureScale,
                        location.Locale,
                        location.AdditionalProperties);

                    STClient.UpdateLocation(location.LocationId.ToString(), locationRequest);
                    RefreshScreen();
                }
                catch (System.Exception exp)
                {
                    ShowStatusBarMessage($"Error updating: {exp}");
                }
            }
            return true;
        }
    }
}
