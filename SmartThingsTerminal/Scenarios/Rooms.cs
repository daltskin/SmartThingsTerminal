using Newtonsoft.Json;
using SmartThingsNet.Model;
using System;
using System.Collections.Generic;
using System.IO;
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
            Dictionary<string, dynamic> dataItemList = null;
            Dictionary<string, string> displayItemList = null;
            try
            {
                if (STClient.GetAllRooms().Items?.Count > 0)
                {
                    dataItemList = STClient.GetAllRooms().Items
                        .OrderBy(t => t.Name)
                        .Select(t => new KeyValuePair<string, dynamic>(t.RoomId.ToString(), t))
                        .ToDictionary(t => t.Key, t => t.Value);

                    displayItemList = STClient.GetAllRooms().Items
                        .OrderBy(t => t.Name)
                        .Select(t => new KeyValuePair<string, string>(t.RoomId.ToString(), t.Name))
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
            ConfigureWindows<Room>(displayItemList, dataItemList);
        }

        public override void ConfigureStatusBar()
        {
            StatusBar = new StatusBar(new StatusItem[] {
                new StatusItem(Key.F3, "~F3~ Edit", () => EnableEditMode()),
                new StatusItem(Key.F4, "~F4~ Save", () => SaveItem()),
                new StatusItem(Key.F5, "~F5~ Refresh Data", () => RefreshScreen()),
                new StatusItem(Key.F6, "~F6~ Copy Room", () => SaveItem(true)),
                new StatusItem(Key.F7, "~F7~ Delete Room", () => DeleteItem()),
                new StatusItem(Key.F12, "~F12~ Back", () => Quit())
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
                        var roomRequest = new CreateRoomRequest(room.Name += "-copy");
                        STClient.CreateRoom(room.LocationId.ToString(), roomRequest);
                    }
                    else
                    {
                        var roomRequest = new UpdateRoomRequest(room.Name);
                        STClient.UpdateRoom(room.LocationId.ToString(), room.RoomId.ToString(), roomRequest);
                    }
                    RefreshScreen();
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

        public override void GetDirectoriesAndFileView(string currentDirectory)
        {
            var files = Directory.GetFiles(currentDirectory, "*.json").Select(t => t.Substring(t.LastIndexOf(@"\") + 1));

            var directoryList = new ListView(files.ToList());
            directoryList.Width = Dim.Fill();
            directoryList.Height = Dim.Fill();

            directoryList.OpenSelectedItem += (args) =>
            {
                string selectedDirectory = ((ListViewItemEventArgs)args).Value.ToString();
                ImportRoom($"{currentDirectory}//{selectedDirectory}");
            };

            FilePicker.Add(directoryList);
            directoryList.SetFocus();
        }

        private void ImportRoom(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                var room = JsonConvert.DeserializeObject<Room>(json);
                
                var roomRequest = new CreateRoomRequest(room.Name);
                STClient.CreateRoom(room.LocationId.ToString(), roomRequest);
                ShowMessage($"Room added!");
            }
            catch (SmartThingsNet.Client.ApiException exp)
            {
                ShowErrorMessage($"Error {exp.ErrorCode}{Environment.NewLine}{exp.Message}");
            }
            catch (Exception exp)
            {
                ShowErrorMessage($"Error {exp.Message}");
            }
            finally
            {
                ImportItem();
            }
        }
    }
}
