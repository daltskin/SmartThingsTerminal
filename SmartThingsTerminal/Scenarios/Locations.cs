using Newtonsoft.Json;
using SmartThingsNet.Model;
using System;
using System.Collections.Generic;
using System.IO;
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
            Dictionary<string, dynamic> dataItemList = null;
            Dictionary<string, string> displayItemList = null;
            try
            {
                if (STClient.GetAllLocations().Items?.Count > 0)
                {
                    // Get the full location details of each location into a dictionary
                    List<Location> allLocationDetails = new List<Location>();
                    foreach (var location in STClient.GetAllLocations().Items)
                    {
                        var locationDetails = STClient.GetLocationDetails(location.LocationId.ToString());
                        if (locationDetails != null)
                        {
                            allLocationDetails.Add(locationDetails);
                        }
                    }

                    dataItemList = allLocationDetails
                        .OrderBy(t => t.Name)
                        .Select(t => new KeyValuePair<string, dynamic>(t.LocationId.ToString(), t))
                        .ToDictionary(t => t.Key, t => t.Value);

                    displayItemList = allLocationDetails
                        .OrderBy(t => t.Name)
                        .Select(t => new KeyValuePair<string, string>(t.LocationId.ToString(), t.Name))
                        .ToDictionary(t => t.Key, t => t.Value);
                }
            }
            catch (SmartThingsNet.Client.ApiException exp)
            {
                ShowErrorMessage($"Error calling API: {exp.Source} {exp.ErrorCode} {exp.Message}");
            }
            catch (System.Exception exp)
            {
                ShowErrorMessage($"Unknown error calling API: {exp.Message}");
            }

            ConfigureWindows<Location>(displayItemList, dataItemList);
        }


        public override void ConfigureStatusBar()
        {
            StatusBar = new StatusBar(new StatusItem[] {
                new StatusItem(Key.F3, "~F3~ Edit", () => EnableEditMode()),
                new StatusItem(Key.F4, "~F4~ Save", () => SaveItem(false)),
                new StatusItem(Key.F5, "~F5~ Refresh Data", () => RefreshScreen()),
                new StatusItem(Key.F6, "~F6~ Copy Location", () => SaveItem(true)),
                new StatusItem(Key.F7, "~F7~ Delete Location", () => DeleteItem()),
                new StatusItem(Key.F12, "~F12~ Back", () => Quit())
            });
        }

        public override bool SaveItem(bool copyCurrent = false)
        {
            var json = JsonView?.Text.ToString();

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var location = JsonConvert.DeserializeObject<Location>(json);

                    if (copyCurrent)
                    {
                        var createLocationRequest = new CreateLocationRequest(
                            name: location.Name += "-copy",
                            countryCode: location.CountryCode ?? "USA",
                            latitude: location.Latitude,
                            longitude: location.Longitude,
                            regionRadius: location.RegionRadius,
                            temperatureScale: location.TemperatureScale,
                            locale: location.Locale,
                            additionalProperties: location.AdditionalProperties);

                        STClient.CreateLocation(createLocationRequest);
                    }
                    else
                    {
                        UpdateLocationRequest locationRequest = new UpdateLocationRequest(
                            name: location.Name,
                            latitude: location.Latitude,
                            longitude: location.Longitude,
                            regionRadius: location.RegionRadius,
                            temperatureScale: location.TemperatureScale,
                            locale: location.Locale,
                            additionalProperties: location.AdditionalProperties);

                        STClient.UpdateLocation(location.LocationId.ToString(), locationRequest);
                    }

                    RefreshScreen();
                }
                catch (SmartThingsNet.Client.ApiException exp)
                {
                    ShowErrorMessage($"Error: {exp.ErrorCode} {Environment.NewLine} {exp.Message}");
                }
                catch (Exception exp)
                {
                    ShowErrorMessage($"Error updating: {exp}");
                }
            }
            return true;
        }

        public override void DeleteItem()
        {
            if (SelectedItem != null)
            {
                Location currentItem = (Location)SelectedItem;
                try
                {
                    STClient.DeleteLocation(currentItem.LocationId.ToString());
                    base.DeleteItem();
                    RefreshScreen();
                }
                catch (Exception exp)
                {
                    ShowErrorMessage($"Error deleting: {exp.Message}");
                }
            }
        }

        public override void GetDirectoriesAndFileView(string currentDirectory)
        {
            var files = Directory.GetFiles(currentDirectory, "*.json").Select(t => t.Substring(t.LastIndexOf(@"\") + 1));

            var directoryList = new ListView(files.ToList());
            directoryList.Width = Dim.Fill();
            directoryList.Height = Dim.Fill();

            directoryList.OpenSelectedItem += (args) =>
            {
                string selectedDirectory = ((ListViewItemEventArgs)args).Value.ToString();
                ImportLocation($"{currentDirectory}//{selectedDirectory}");
            };

            FilePicker.Add(directoryList);
            directoryList.SetFocus();
        }

        private void ImportLocation(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                var location = JsonConvert.DeserializeObject<Location>(json);

                var createLocationRequest = new CreateLocationRequest(
                    name: location.Name,
                    countryCode: location.CountryCode ?? "USA",
                    latitude: location.Latitude,
                    longitude: location.Longitude,
                    regionRadius: location.RegionRadius,
                    temperatureScale: location.TemperatureScale,
                    locale: location.Locale,
                    additionalProperties: location.AdditionalProperties);

                STClient.CreateLocation(createLocationRequest);
                ShowMessage($"Location added!");
            }
            catch (SmartThingsNet.Client.ApiException exp)
            {
                ShowErrorMessage($"Error {exp.ErrorCode}{Environment.NewLine}{exp.Message}");
            }
            catch (Exception exp)
            {
                ShowErrorMessage($"Error {exp.Message}");
            }
            finally
            {
                ImportItem();
            }
        }
    }
}
