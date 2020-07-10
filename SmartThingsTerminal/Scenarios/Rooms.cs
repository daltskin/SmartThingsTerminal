using Newtonsoft.Json;
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
        public override void Setup()
        {
            Dictionary<string, dynamic> displayItemList = null;
            try
            {
                if (STClient.GetAllRooms().Items?.Count > 0)
                {
                    displayItemList = STClient.GetAllRooms().Items
                        .OrderBy(t => t.Name)
                        .Select(t => new KeyValuePair<string, dynamic>(t.Name, t))
                        .ToDictionary(t => t.Key, t => t.Value);
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
            ConfigureWindows<Room>(displayItemList);
        }

        public override void ConfigureStatusBar()
        {
            StatusBar = new StatusBar(new StatusItem[] {
                new StatusItem(Key.F3, "~F3~ Edit", () => EnableEditMode()),
                new StatusItem(Key.F4, "~F4~ Save", () => SaveItem()),
                new StatusItem(Key.F5, "~F5~ Refresh Data", () => RefreshScreen()),
                new StatusItem(Key.F6, "~F6~ Copy Room", () => SaveItem(true)),
                new StatusItem(Key.F9, "~F9~ Delete Room", () => DeleteItem()),
                new StatusItem(Key.Home, "~Home~ Back", () => Quit())
            });
        }

        public override bool SaveItem(bool copyCurrent = false)
        {
            var json = JsonView?.Text.ToString();

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var room = JsonConvert.DeserializeObject<Room>(json);
                    if (copyCurrent)
                    {
                        int nameCounter = STClient.GetAllRooms().Items.Where(n => n.Name.Equals(room.Name)).Count();
                        nameCounter++;
                        string newRoomName = room.Name += $"-copy {nameCounter}";
                        var roomRequest = new CreateRoomRequest(newRoomName);
                        STClient.CreateRoom(room.LocationId.ToString(), roomRequest);
                    }
                    else
                    {
                        var roomRequest = new UpdateRoomRequest(room.Name);
                        STClient.UpdateRoom(room.LocationId.ToString(), room.RoomId.ToString(), roomRequest);
                    }
                    RefreshScreen();
                }
                catch (System.Exception exp)
                {
                    ShowStatusBarMessage($"Error updating: {exp}");
                }
            }
            return true;
        }

        public override void DeleteItem()
        {
            if (SelectedItem != null)
            {
                Room currentItem = (Room)SelectedItem;
                try
                {
                    STClient.DeleteRoom(currentItem.LocationId.ToString(), currentItem.RoomId.ToString());
                    base.DeleteItem();
                    RefreshScreen();
                }
                catch (Exception exp)
                {
                    ShowStatusBarMessage($"Error deleting: {exp.Message}");
                }
            }
        }
    }
}
