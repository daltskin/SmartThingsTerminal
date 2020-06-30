using SmartThingsNet.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace SmartThingsTerminal.Scenarios
{
    [ScenarioMetadata(Name: "Rules", Description: "SmartThings rules")]
    [ScenarioCategory("Rules")]
    class Rules : Scenario
    {
        Dictionary<string, Rule> _viewRules = new Dictionary<string, Rule>();

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

            LeftPane = new Window("Automations")
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
                if (STClient.GetAllRules().Items?.Count > 0)
                {
                    _viewRules = STClient.GetAllRules().Items
                       .OrderBy(t => t.Name)
                       .Select(t => new KeyValuePair<string, Rule>(t.Name, t))
                       .ToDictionary(t => t.Key, t => t.Value);
                }
            }
            catch (System.Exception exp)
            {
                SetErrorView($"No data returned from API:{Environment.NewLine}{exp.Message}");
            }

            ClassListView = new ListView(_viewRules?.Keys.ToList())
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(), // for status bar
                AllowsMarking = false,
                ColorScheme = Colors.TopLevel,
            };

            if (_viewRules.Keys.Count > 0)
            {
                ClassListView.SelectedItemChanged += (args) =>
                {
                    ClearClass(CurrentView);
                    var selectedItem = _viewRules.Values.ToArray()[ClassListView.SelectedItem];
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

            if (_viewRules.Count > 0)
            {
                CurrentView = CreateJsonView(_viewRules?.FirstOrDefault().Value?.ToJson());
            }

            DisplayErrorView();
        }

        private void Quit()
        {
            Application.RequestStop();
        }
    }
}
