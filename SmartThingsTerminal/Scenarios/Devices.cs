using SmartThingsNet.Model;
using System.Linq;
using Terminal.Gui;

namespace SmartThingsTerminal.Scenarios
{
    [ScenarioMetadata(Name: "Devices", Description: "SmartThings devices")]
    [ScenarioCategory("Devices")]
    class Devices : Scenario
    {
        FrameView _deviceDetailsFrame;
        FrameView _deviceLocationFrame;

        public override void Setup()
        {
            ConfigureLeftPane(GetName());

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
            ConfigureHostPane("");
            ConfigureWindows<Device>(false, false);
        }

        public override void UpdateSettings<T>(object selectedItem)
        {
            Device device = (Device)selectedItem;

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
    }
}
