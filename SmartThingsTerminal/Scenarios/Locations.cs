using SmartThingsNet.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace SmartThingsTerminal.Scenarios
{
    [ScenarioMetadata(Name: "Locations", Description: "SmartThings locations")]
    [ScenarioCategory("Locations")]
    class Locations : Scenario
    {
        Dictionary<string, Location> _viewLocations = new Dictionary<string, Location>();

        public override void Init(Toplevel top, ColorScheme colorScheme, SmartThingsClient SmartThingsTerminalent)
        {
            Application.Init();

            Top = top;
            if (Top == null)
            {
                Top = Application.Top;
            }

            STClient = SmartThingsTerminalent;
        }

        public override void Setup()
        {
            var statusBar = new StatusBar(new StatusItem[] {
                //new StatusItem(Key.ControlR, "~CTRL-R~ Refresh Data", () => RefreshScreen()),
                new StatusItem(Key.ControlQ, "~CTRL-Q~ Back/Quit", () => Quit())
            });

            LeftPane = new Window("Locations")
            {
                X = 0,
                Y = 0, // for menu
                Width = 40,
                Height = Dim.Fill(),
                CanFocus = false,
                ColorScheme = Colors.TopLevel,
            };

            try
            {
                if (STClient.GetAllLocations().Items?.Count > 0)
                {
                    _viewLocations = STClient.GetAllLocations().Items
                        .OrderBy(t => t.Name)
                        .Select(t => new KeyValuePair<string, Location>(t.Name, t))
                        .ToDictionary(t => t.Key, t => t.Value);
                }
                else
                {
                    SetErrorView($"You have no locations configured");
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

            ClassListView = new ListView(_viewLocations?.Keys?.ToList())
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(), // for status bar
                AllowsMarking = false,
                ColorScheme = Colors.TopLevel,
            };

            if (_viewLocations?.Keys?.Count > 0)
            {
                ClassListView.SelectedItemChanged += (args) =>
                {
                    ClearClass(CurrentView);
                    var selectedItem = _viewLocations.Values.ToArray()[ClassListView.SelectedItem];
                    string json = selectedItem.ToJson();
                    CurrentView = CreateJsonView(json);
                };
            }
            LeftPane.Add(ClassListView);

            HostPane = new FrameView("")
            {
                X = Pos.Right(LeftPane),
                //Y = Pos.Bottom(_settingsPane),
                Width = Dim.Fill(),
                Height = Dim.Fill(1), // + 1 for status bar
                ColorScheme = Colors.Dialog,
            };

            Top.Add(LeftPane, HostPane);
            Top.Add(statusBar);

            if (_viewLocations?.Count > 0)
            {
                CurrentView = CreateJsonView(_viewLocations?.FirstOrDefault().Value?.ToJson());
            }

            DisplayErrorView();
        }

        private void Quit()
        {
            Application.RequestStop();
        }
    }
}
