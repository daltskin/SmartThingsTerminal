using System.Collections.Generic;
using SmartThingsNet.Model;

namespace SmartThingsTerminal
{
    public class DeviceHelper
    {
        private readonly SmartThingsClient smartThingsClient;
        private readonly Device device;

        public DeviceHelper(SmartThingsClient smartThingsClient, Device device)
        {
            this.smartThingsClient = smartThingsClient;
            this.device = device;
        }

        public bool ToggleDevice()
        {
            if (ToggleDeviceSwitch() || ToggleDeviceSwitchLevel())
            {
                return true;
            }

            return false;
        }

        private bool ToggleDeviceSwitch()
        {
            var deviceCurrentStatus = new Dictionary<string, AttributeState>();
            if (this.smartThingsClient.TryGetDeviceCapabilityStatus(this.device.DeviceId, this.device.Components[0].Id, "switch", out deviceCurrentStatus)
                && deviceCurrentStatus.Count > 0)
            {
                string state = deviceCurrentStatus["switch"].Value.ToString().ToLower();
                string newState = state == "on" ? "off" : "on";

                DeviceCommandsRequest commandsRequest = new DeviceCommandsRequest() { Commands = new List<DeviceCommand>() };
                DeviceCommand command = new DeviceCommand(capability: "switch", command: newState);
                commandsRequest.Commands.Add(command);
                this.smartThingsClient.ExecuteDevicecommand(this.device.DeviceId, commandsRequest);

                return true;
            }
            else
            {
                return false;
            }
        }

        private bool ToggleDeviceSwitchLevel()
        {
            var deviceCurrentStatus = new Dictionary<string, AttributeState>();
            if (this.smartThingsClient.TryGetDeviceCapabilityStatus(this.device.DeviceId, this.device.Components[0].Id, "switchLevel", out deviceCurrentStatus)
                && deviceCurrentStatus.Count > 0)
            {
                string currentLevel = deviceCurrentStatus["level"].Value.ToString();

                var commandArgs = new List<object>() { { currentLevel == "0" ? 100 : 0 } };
                var command = new DeviceCommand(capability: "switchLevel", command: "setLevel", arguments: commandArgs);

                var commandsRequest = new DeviceCommandsRequest() { Commands = new List<DeviceCommand>() };
                commandsRequest.Commands.Add(command);
                this.smartThingsClient.ExecuteDevicecommand(this.device.DeviceId, commandsRequest);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}