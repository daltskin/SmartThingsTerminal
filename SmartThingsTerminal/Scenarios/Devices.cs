using SmartThingsNet.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace SmartThingsTerminal.Scenarios
{
    [ScenarioMetadata(Name: "Devices", Description: "SmartThings devices")]
    [ScenarioCategory("Devices")]
    class Devices : Scenario
    {
        Dictionary<string, Device> _viewDevices;
        FrameView _deviceDetailsFrame;
        FrameView _deviceLocationFrame;

        public override void Init(Toplevel top, ColorScheme colorScheme, SmartThingsClient smartThingsClient)
        {
            Application.Init();

            Top = top;
            if (Top == null)
            {
                Top = Application.Top;
            }
            STClient = smartThingsClient;
        }

        public override void Setup()
        {
            var statusBar = new StatusBar(new StatusItem[] {
                //new StatusItem(Key.ControlR, "~CTRL-R~ Refresh Data", () => RefreshScreen()),
                new StatusItem(Key.ControlQ, "~CTRL-Q~ Back/Quit", () => Quit())
            });

            LeftPane = new Window("Devices")
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
                if (STClient.GetAllDevices().Items?.Count > 0)
                {
                    _viewDevices = STClient.GetAllDevices().Items
                        .OrderBy(t => t.Name)
                        .Select(t => new KeyValuePair<string, Device>(t.Label, t))
                        .ToDictionary(t => t.Key, t => t.Value);
                }
                else
                {
                    SetErrorView($"You have no devices configured");
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

            ClassListView = new ListView(_viewDevices?.Keys?.ToList())
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(), // for status bar
                AllowsMarking = false,
                ColorScheme = Colors.TopLevel
            };

            if (_viewDevices?.Keys?.Count > 0)
            {
                ClassListView.OpenSelectedItem += (a) =>
                {
                    Top.SetFocus(SettingsPane);
                };
                ClassListView.SelectedItemChanged += (args) =>
                {
                    ClearClass(CurrentView);

                    var selectedDevice = _viewDevices.Values.ToArray()[ClassListView.SelectedItem];
                    string json = selectedDevice.ToJson();
                    CurrentView = CreateJsonView(json);
                    UpdateSettings(selectedDevice);
                };
            }
            LeftPane.Add(ClassListView);

            SettingsPane = new FrameView("Settings")
            {
                X = Pos.Right(LeftPane),
                Y = 0, // for menu
                Width = Dim.Fill(),
                Height = 8,
                CanFocus = false,
                ColorScheme = Colors.TopLevel,
            };

            _deviceDetailsFrame = new FrameView("Device Details")
            {
                X = 0,
                Y = 0,
                Height = 6,
                Width = 50,
            };
            SettingsPane.Add(_deviceDetailsFrame);

            _deviceLocationFrame = new FrameView("Device Location")
            {
                X = Pos.Right(_deviceDetailsFrame),
                Y = Pos.Y(_deviceDetailsFrame),
                Height = 6,
                Width = 40,
            };

            SettingsPane.Add(_deviceLocationFrame);

            HostPane = new FrameView("")
            {
                X = Pos.Right(LeftPane),
                Y = Pos.Bottom(SettingsPane),
                Width = Dim.Fill(),
                Height = Dim.Fill(1), // + 1 for status bar
                ColorScheme = Colors.Dialog,
            };

            Top.Add(LeftPane, SettingsPane, HostPane);
            Top.Add(statusBar);

            if (_viewDevices?.Count > 0)
            {
                var firstItem = _viewDevices?.FirstOrDefault().Value;
                if (firstItem != null)
                {
                    CurrentView = CreateJsonView(firstItem.ToJson());
                    UpdateSettings(firstItem);
                }
            }
        }

        void UpdateSettings(Device device)
        {
            _deviceDetailsFrame.Clear();

            var labelId = new Label("Id:") { X = 0, Y = 0 };
            _deviceDetailsFrame.Add(labelId);
            var deviceId = new TextField($"{device.DeviceId}") { X = Pos.Right(labelId) + 1, Y = 0, Width = 40 };
            _deviceDetailsFrame.Add(deviceId);

            var labelDeviceLabel = new Label("Label:") { X = 0, Y = 1 };
            _deviceDetailsFrame.Add(labelDeviceLabel);
            var deviceLabel = new TextField($"{device?.Label}") { X = Pos.Right(labelDeviceLabel) + 1, Y = 1, Width = 40 };
            _deviceDetailsFrame.Add(deviceLabel);

            var labelType = new Label("Type:") { X = 0, Y = 2 };
            _deviceDetailsFrame.Add(labelType);
            var deviceType = new TextField($"{device.DeviceTypeName?.Trim()}") { X = Pos.Right(labelType) + 1, Y = 2, Width = 40 };
            _deviceDetailsFrame.Add(deviceType);

            var labelComponents = new Label("Components:") { X = 0, Y = 3 };
            _deviceDetailsFrame.Add(labelComponents);
            var deviceComponents = new TextField($"{device.Components.Count}") { X = Pos.Right(labelComponents) + 1, Y = 3, Width = 40 };
            _deviceDetailsFrame.Add(deviceComponents);

            // Device Location pane
            _deviceLocationFrame.Clear();

            string locationName = "";
            if (device.LocationId != null)
            {
                locationName = STClient.GetAllLocations().Items.Where(l => l.LocationId.ToString().Equals(device.LocationId))?.FirstOrDefault().Name;
            }

            string roomName = "";
            if (device.RoomId != null)
            {
                roomName = STClient.GetAllRooms(device.LocationId).Items.Where(r => r.RoomId.ToString().Equals(device.RoomId))?.FirstOrDefault().Name;
            }

            var labelLocation = new Label("Location:") { X = 0, Y = 0 };
            _deviceLocationFrame.Add(labelLocation);
            var deviceLocation = new TextField($"{locationName}") { X = Pos.Right(labelLocation) + 1, Y = 0, Width = 40 };
            _deviceLocationFrame.Add(deviceLocation);

            var labelRoom = new Label("Room:") { X = 0, Y = 1 };
            _deviceLocationFrame.Add(labelRoom);
            var deviceRoom = new TextField($"{roomName}") { X = Pos.Right(labelRoom) + 1, Y = 1, Width = 40 };
            _deviceLocationFrame.Add(deviceRoom);
        }

        private void Quit()
        {
            Application.RequestStop();
        }
    }
}
