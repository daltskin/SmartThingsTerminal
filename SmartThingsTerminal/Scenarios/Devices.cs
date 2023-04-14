using Newtonsoft.Json;
using SmartThingsNet.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Gui;
using TextField = Terminal.Gui.TextField;

namespace SmartThingsTerminal.Scenarios
{
    [ScenarioMetadata(Name: "Devices", Description: "SmartThings devices")]
    [ScenarioCategory("Devices")]
    class Devices : Scenario
    {
        private FrameView _deviceDetailsFrame;
        private FrameView _deviceLocationFrame;
        private TextView _capabilitiesStatusJsonView;
        private ListView _componentList;
        private FrameView _componentFrame;
        private int _selectedCapabilityIndex = 0;

        public override void Setup()
        {
            ConfigureLeftPane(GetName());
            ConfigureSettingsPane();

            Dictionary<string, dynamic> dataItemList = null;
            Dictionary<string, string> displayItemList = null;
            try
            {
                if (STClient.GetAllDevices()?.Items?.Count > 0)
                {
                    dataItemList = STClient.GetAllDevices().Items
                        .OrderBy(t => t.Label)
                        .Select(t => new KeyValuePair<string, dynamic>(t.DeviceId, t))
                        .ToDictionary(t => t.Key, t => t.Value);

                    displayItemList = STClient.GetAllDevices().Items
                    .OrderBy(o => o.Label)
                    .Select(t => new KeyValuePair<string, string>(t.DeviceId, t.Label))
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

            ConfigureWindows<Device>(displayItemList, dataItemList, false, false);
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
                    if (_capabilitiesStatusJsonView != null)
                    {
                        HostPane.Remove(_capabilitiesStatusJsonView);
                        _capabilitiesStatusJsonView = null;
                    }
                };
            }
        }

        public override void ConfigureSettingsPane()
        {
            SettingsPane = new FrameView("Settings")
            {
                Id = "Settings",
                X = Pos.Right(LeftPane),
                Y = 1, // for menu
                Width = Dim.Fill(),
                Height = 8,
                CanFocus = true,
                ColorScheme = Colors.TopLevel,
            };

            _deviceDetailsFrame = new FrameView("Device Details")
            {
                Id = "Device_Details",
                X = 0,
                Y = 0,
                Height = 6,
                Width = Dim.Percent(50),
                CanFocus = true,
            };
            SettingsPane.Add(_deviceDetailsFrame);

            _deviceLocationFrame = new FrameView("Device Location")
            {
                Id = "Device_Location",
                X = Pos.Right(_deviceDetailsFrame),
                Y = Pos.Y(_deviceDetailsFrame),
                Height = 6,
                Width = Dim.Percent(50),
            };

            SettingsPane.Add(_deviceLocationFrame);
            ConfigureHostPane("");
        }

        public override void UpdateSettings<T>(object selectedItem)
        {
            Device device = (Device)selectedItem;
            _deviceDetailsFrame.RemoveAll();

            var labelId = new Label("Id:") { X = 0, Y = 0 };
            _deviceDetailsFrame.Add(labelId);
            var deviceId = new TextField($"{device.DeviceId}") { X = Pos.Right(labelId) + 1, Y = 0, Width = 40 };
            deviceId.ColorScheme = Colors.Base;
            deviceId.ReadOnly = true;
            _deviceDetailsFrame.Add(deviceId);

            var labelDeviceLabel = new Label("Label:") { X = 0, Y = 1 };
            _deviceDetailsFrame.Add(labelDeviceLabel);
            var deviceLabel = new TextField($"{device?.Label}") { X = Pos.Right(labelDeviceLabel) + 1, Y = 1, Width = 40 };
            deviceLabel.ColorScheme = Colors.Base;
            deviceLabel.ReadOnly = true;
            _deviceDetailsFrame.Add(deviceLabel);

            var labelType = new Label("Type:") { X = 0, Y = 2 };
            _deviceDetailsFrame.Add(labelType);
            var deviceType = new TextField($"{device.DeviceTypeName?.Trim()}") { X = Pos.Right(labelType) + 1, Y = 2, Width = 40 };
            deviceType.ColorScheme = Colors.Base;
            deviceType.ReadOnly = true;
            _deviceDetailsFrame.Add(deviceType);

            var labelComponents = new Label("Components:") { X = 0, Y = 3 };
            _deviceDetailsFrame.Add(labelComponents);
            var deviceComponents = new TextField($"{device.Components.Count}") { X = Pos.Right(labelComponents) + 1, Y = 3, Width = 40 };
            deviceComponents.ColorScheme = Colors.Base;
            deviceComponents.ReadOnly= true;
            _deviceDetailsFrame.Add(deviceComponents);

            // Device Location pane
            _deviceLocationFrame.RemoveAll();

            string locationName = "";
            if (device.LocationId != null)
            {
                locationName = STClient.GetAllLocations().Items.Where(l => l.LocationId.ToString().Equals(device.LocationId))?.FirstOrDefault().Name;
            }

            string roomName = "";
            if (device.RoomId != null)
            {
                roomName = STClient.GetAllRooms(device.LocationId).Items.Where(r => r.RoomId.ToString().Equals(device.RoomId))?.FirstOrDefault()?.Name;
            }

            var labelLocation = new Label("Location:") { X = 0, Y = 0 };
            _deviceLocationFrame.Add(labelLocation);
            var deviceLocation = new TextField($"{locationName}") { X = Pos.Right(labelLocation) + 1, Y = 0, Width = 40 };
            deviceLocation.ReadOnly= true;
            deviceLocation.ColorScheme = Colors.Base;
            _deviceLocationFrame.Add(deviceLocation);

            var labelRoom = new Label("Room:") { X = 0, Y = 1 };
            _deviceLocationFrame.Add(labelRoom);
            var deviceRoom = new TextField($"{roomName}") { X = Pos.Right(labelRoom) + 1, Y = 1, Width = 40 };
            deviceRoom.ColorScheme = Colors.Base;
            deviceRoom.ReadOnly= true;
            _deviceLocationFrame.Add(deviceRoom);

            if (_componentFrame != null)
            {
                ToggleComponentStatus();
            }

        }

        public override void ConfigureStatusBar()
        {
            StatusBar = new StatusBar(new StatusItem[] {
                new StatusItem(Key.F1, "~F1~ Component Status", () => ToggleComponentStatus()),
                new StatusItem(Key.F4, "~F4~ Toggle Device Switch", () => ToggleDevice()),
                new StatusItem(Key.F5, "~F5~ Refresh Data", () => RefreshScreen()),
                new StatusItem(Key.F9, "~F9~ Menu", () => { }),
                new StatusItem(Key.F12, "~F12~ Back", () => Quit())
            });
        }

        private void ConfigureComponentsStatusPane(Device selectedDevice)
        {
            _componentFrame = new FrameView()
            {
                X = 0,
                Y = 0,
                Height = Dim.Fill(),
                Width = Dim.Fill(),
                Title = "main",
                ColorScheme = Colors.TopLevel
            };

            _componentList = new ListView(selectedDevice.Components.FirstOrDefault().Capabilities.Select(c => c.Id).ToList());

            _componentList.X = 0;
            _componentList.Y = 0;
            _componentList.Width = Dim.Percent(30);
            _componentList.Height = Dim.Fill();
            _componentList.AllowsMarking = false;
            _componentList.ColorScheme = Colors.TopLevel;

            _componentList.SelectedItemChanged += (args) =>
            {
                _selectedCapabilityIndex = args.Item;
                GetComponentStatus(selectedDevice, args.Item);
            };

            _capabilitiesStatusJsonView = new TextView()
            {
                Y = 0,
                X = Pos.Right(_componentList),
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ReadOnly = true,
                ColorScheme = Colors.Dialog,
            };

            _componentFrame.Add(_componentList, _capabilitiesStatusJsonView);

            HostPane.Add(_componentFrame);

            _componentList.SetFocus();
            GetComponentStatus(selectedDevice, 0);
            HostPane.ColorScheme = Colors.TopLevel;
        }

        private void ToggleDevice()
        {
            if (SelectedItem != null)
            {
                Device selectedDevice = (Device)SelectedItem;
                try
                {
                    var deviceHelper = new DeviceHelper(STClient, selectedDevice);
                    if (!deviceHelper.ToggleDevice())
                    {
                        ShowErrorMessage($"Cannot toggle {selectedDevice.Name}.");
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
            }
        }

        private void UpdateComponentStatus()
        {
            if (SelectedItem != null && _componentFrame != null)
            {
                Device selectedDevice = (Device)SelectedItem;
                try
                {
                    var componentCapabilityStatus = JsonConvert.DeserializeObject<AttributeState>(_capabilitiesStatusJsonView.Text.ToString());

                    var commandsRequest = new DeviceCommandsRequest() { Commands = new List<DeviceCommand>() };

                    //foreach (var statusUpdate in componentCapabilityStatus)
                    //{
                    //    var command = new DeviceCommand(
                    //  capability: statusUpdate.Key, // selectedDevice.Components.FirstOrDefault().Capabilities[_selectedCapabilityIndex].Id,
                    //  command: statusUpdate.Value);

                    //    commandsRequest.Commands.Add(command);
                    //}

                    var command = new DeviceCommand(
                        capability: selectedDevice.Components.FirstOrDefault().Capabilities[_selectedCapabilityIndex].Id,
                        command: (string)componentCapabilityStatus.Value);

                    commandsRequest.Commands.Add(command);
                    object response = STClient.ExecuteDevicecommand(selectedDevice.DeviceId, commandsRequest);
                    ShowStatusBarMessage($"Executed: {DateTime.UtcNow.ToLongTimeString()}");
                }
                catch (SmartThingsNet.Client.ApiException exp)
                {
                    ShowErrorMessage($"Error {exp.ErrorCode}{Environment.NewLine}{exp.Message}");
                }
                catch (Exception exp)
                {
                    ShowErrorMessage($"Error {exp.Message}");
                }
            }
        }

        private void GetComponentStatus(Device selectedDevice, int selectedItemIndex)
        {
            try
            {
                var componentCapabilityStatus = STClient.GetDeviceCapabilityStatus(
                           selectedDevice.DeviceId,
                           selectedDevice.Components.FirstOrDefault().Id,
                           selectedDevice.Components.FirstOrDefault().Capabilities[selectedItemIndex].Id);

                if (componentCapabilityStatus != null)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var capabilityStatus in componentCapabilityStatus)
                    {
                        sb.AppendLine($"\"{capabilityStatus.Key}\":");
                        sb.AppendLine(capabilityStatus.Value.ToJson());
                    }
                    _capabilitiesStatusJsonView.Text = FormatJson(sb.ToString());
                }
            }
            catch (SmartThingsNet.Client.ApiException exp)
            {
                // 403 errors can come from trying to read inaccessible component status eg:
                // https://api.smartthings.com/v1/devices/{guid}/components/main/capabilities/configuration/status
                _capabilitiesStatusJsonView.Text = $"Error: {exp.ErrorCode}";
            }
            catch (Exception exp)
            {
                ShowErrorMessage($"Error {exp.Message}");
            }
        }

        private void ToggleComponentStatus()
        {
            if (SelectedItem != null)
            {
                if (_componentFrame != null)
                {
                    HostPane.Remove(_componentFrame);
                    HostPane.Remove(_componentList);
                    HostPane.Remove(_capabilitiesStatusJsonView);
                    _componentFrame = null;
                    _componentList = null;
                    _capabilitiesStatusJsonView = null;
                    ClassListView.SetFocus();
                }
                else
                {
                    Device selectedDevice = (Device)SelectedItem;
                    ConfigureComponentsStatusPane(selectedDevice);
                }
            }
        }
    }
}
