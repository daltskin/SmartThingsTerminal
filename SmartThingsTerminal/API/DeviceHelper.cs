using System;
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
            if (!ToggleDeviceSwitch())
            {
                return ToggleDeviceWindowShade();
            }

            return true;
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

        private bool ToggleDeviceWindowShade()
        {
            var deviceCurrentStatus = new Dictionary<string, AttributeState>();
            if (this.smartThingsClient.TryGetDeviceCapabilityStatus(this.device.DeviceId, this.device.Components[0].Id, "windowShade", out deviceCurrentStatus)
                && deviceCurrentStatus.Count > 0)
            {
                string state = deviceCurrentStatus["windowShade"].Value.ToString().ToLower();
                string newState = state == "open" ? "close" : "open";

                DeviceCommandsRequest commandsRequest = new DeviceCommandsRequest() { Commands = new List<DeviceCommand>() };
                DeviceCommand command = new DeviceCommand(capability: "windowShade", command: newState);
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