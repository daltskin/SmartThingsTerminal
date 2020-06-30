using SmartThingsNet.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace SmartThingsTerminal.Scenarios
{
    [ScenarioMetadata(Name: "Rooms", Description: "SmartThings rooms")]
    [ScenarioCategory("Rooms")]
    class Rooms : Scenario
    {
        Dictionary<string, Room> _viewRooms = new Dictionary<string, Room>();

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

            LeftPane = new Window("Rooms")
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
                if (STClient.GetAllRooms().Items?.Count > 0)
                {
                    _viewRooms = STClient.GetAllRooms().Items
                        .OrderBy(t => t.Name)
                        .Select(t => new KeyValuePair<string, Room>(t.Name, t))
                        .ToDictionary(t => t.Key, t => t.Value);
                }
            }
            catch (System.Exception exp)
            {
                SetErrorView($"No data returned from API:{Environment.NewLine}{exp.Message}");
            }

            ClassListView = new ListView(_viewRooms.Keys?.ToList())
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(), // for status bar
                AllowsMarking = false,
                ColorScheme = Colors.TopLevel,
            };

            if (_viewRooms.Keys.Count > 0)
            {
                ClassListView.SelectedItemChanged += (args) =>
                {
                    ClearClass(CurrentView);
                    var selectedItem = _viewRooms.Values.ToArray()[ClassListView.SelectedItem];
                    CurrentView = CreateJsonView(selectedItem.ToJson());
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

            if (_viewRooms.Count > 0)
            {
                CurrentView = CreateJsonView(_viewRooms?.FirstOrDefault().Value?.ToJson());
            }

            DisplayErrorView();
        }

        private void Quit()
        {
            Application.RequestStop();
        }
    }
}
