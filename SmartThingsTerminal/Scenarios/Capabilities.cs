using SmartThingsNet.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;
using TextField = Terminal.Gui.TextField;

namespace SmartThingsTerminal.Scenarios
{
    [ScenarioMetadata(Name: "Capabilities", Description: "SmartThings capabilities")]
    [ScenarioCategory("Capabilities")]
    class Capabilities : Scenario
    {
        private FrameView _capabilitySummaryFrame;
        private TextView _capabilityPresentationJsonView;
        private ListView _componentList;
        private FrameView _componentFrame;

        public override void Setup()
        {
            ConfigureLeftPane(GetName());
            ConfigureSettingsPane();

            Dictionary<string, dynamic> dataItemList = null;
            Dictionary<string, string> displayItemList = null;
            try
            {
                if (STClient.GetAllCapabilities().Items?.Count > 0)
                {
                    dataItemList = STClient.GetAllCapabilities().Items
                        .OrderBy(t => t.Id)
                        .Select(t => new KeyValuePair<string, dynamic>(t.Id, t))
                        .ToDictionary(t => t.Key, t => t.Value);

                    displayItemList = STClient.GetAllCapabilities().Items
                            .OrderBy(o => o.Id)
                            .Select(t => new KeyValuePair<string, string>(t.Id, t.Id))
                            .ToDictionary(t => t.Key, t => t.Value);
                }
            }
            catch (SmartThingsNet.Client.ApiException exp)
            {
                ShowErrorMessage($"Error {exp.ErrorCode}{Environment.NewLine}{exp.Message}");
            }
            catch (Exception exp)
            {
                ShowErrorMessage($"Error {exp.Message}");
            }
            ConfigureWindows<CapabilitySummary>(displayItemList, dataItemList, false, false);

            if (ClassListView != null)
            {
                ClassListView.Enter += (args) =>
                {
                    if (_componentFrame != null)
                    {
                        HostPane.Remove(_componentFrame);
                        _componentFrame = null;
                    }
                    if (_componentList != null)
                    {
                        HostPane.Remove(_componentList);
                        _componentList = null;
                    }
                    if (_capabilityPresentationJsonView != null)
                    {
                        HostPane.Remove(_capabilityPresentationJsonView);
                        _capabilityPresentationJsonView = null;
                    }
                };
            }
        }

        public override void ConfigureSettingsPane()
        {
            SettingsPane = new FrameView("Settings")
            {
                X = Pos.Right(LeftPane),
                Y = 1, // for menu
                Width = Dim.Fill(),
                Height = 8,
                CanFocus = false,
                ColorScheme = Colors.TopLevel,
            };

            _capabilitySummaryFrame = new FrameView("Capability Details")
            {
                X = 0,
                Y = 0,
                Height = 6,
                Width = Dim.Fill(),
            };
            SettingsPane.Add(_capabilitySummaryFrame);
            ConfigureHostPane("");
        }

        public override void UpdateSettings<T>(object selectedItem)
        {
            CapabilitySummary capability = (CapabilitySummary)selectedItem;

            _capabilitySummaryFrame.RemoveAll();

            var labelId = new Label("Id:") { X = 0, Y = 0 };
            _capabilitySummaryFrame.Add(labelId);
            var deviceId = new TextField($"{capability.Id}") { X = Pos.Right(labelId) + 1, Y = 0, Width = 40 };
            deviceId.ColorScheme = Colors.Base;
            _capabilitySummaryFrame.Add(deviceId);

            var labelDeviceLabel = new Label("Status:") { X = 0, Y = 1 };
            _capabilitySummaryFrame.Add(labelDeviceLabel);
            var deviceLabel = new TextField($"{capability?.Status}") { X = Pos.Right(labelDeviceLabel) + 1, Y = 1, Width = 40 };
            deviceLabel.ColorScheme = Colors.Base;
            _capabilitySummaryFrame.Add(deviceLabel);

            var labelType = new Label("Version:") { X = 0, Y = 2 };
            _capabilitySummaryFrame.Add(labelType);
            var deviceType = new TextField($"{capability.Version}") { X = Pos.Right(labelType) + 1, Y = 2, Width = 40 };
            deviceType.ColorScheme = Colors.Base;
            _capabilitySummaryFrame.Add(deviceType);
        }

        private void ConfigureCapabilityPane(CapabilitySummary selectedCapability)
        {
            _componentFrame = new FrameView()
            {
                X = 0,
                Y = 0,
                Height = Dim.Fill(),
                Width = Dim.Fill(),
                Title = $"Capability Presentation",
                ColorScheme = Colors.TopLevel
            };

            _capabilityPresentationJsonView = new TextView()
            {
                Y = 0,
                X = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ReadOnly = true,
                ColorScheme = Colors.Dialog,
            };
            GetCapabilityPresentation(selectedCapability);
            _componentFrame.Add(_capabilityPresentationJsonView);
            HostPane.Add(_componentFrame);

            HostPane.SetFocus(_capabilityPresentationJsonView);

            HostPane.ColorScheme = Colors.TopLevel;
        }

        public override void ConfigureStatusBar()
        {
            StatusBar = new StatusBar(new StatusItem[] {
                new StatusItem(Key.F1, "~F1~ Capability Presentation", () => ToggleCapability()),
                new StatusItem(Key.F5, "~F5~ Refresh Data", () => RefreshScreen()),
                new StatusItem(Key.Home, "~Home~ Back", () => Quit())
            });
        }

        private void GetCapabilityPresentation(CapabilitySummary selectedCapability)
        {
            try
            {
                var capbility = STClient.GetCapability(selectedCapability.Id, selectedCapability.Version);
                if (capbility != null)
                {
                    _capabilityPresentationJsonView.Text = FormatJson(capbility.ToJson());
                }
            }
            catch (SmartThingsNet.Client.ApiException exp)
            {
                _capabilityPresentationJsonView.Text = $"Error: {exp.ErrorCode}";
            }
            catch (Exception exp)
            {
                ShowErrorMessage($"Error {exp.Message}");
            }
        }

        private void ToggleCapability()
        {
            if (SelectedItem != null)
            {
                if (_componentFrame != null)
                {
                    HostPane.Remove(_componentFrame);
                    HostPane.Remove(_componentList);
                    HostPane.Remove(_capabilityPresentationJsonView);
                    _componentFrame = null;
                    _componentList = null;
                    _capabilityPresentationJsonView = null;
                    LeftPane.SetFocus(ClassListView);
                }
                else
                {
                    CapabilitySummary selectedCapability = (CapabilitySummary)SelectedItem;
                    ConfigureCapabilityPane(selectedCapability);
                }
            }
        }
    }
}
