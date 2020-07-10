using Newtonsoft.Json;
using SmartThingsNet.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace SmartThingsTerminal.Scenarios
{
    [ScenarioMetadata(Name: "Schedules", Description: "SmartThings application schedules")]
    [ScenarioCategory("Schedules")]
    class Schedules : Scenario
    {
        public override void Setup()
        {
            Dictionary<string, dynamic> displayItemList = null;
            try
            {
                if (STClient.GetAllSchedules().Items?.Count > 0)
                {
                    displayItemList = STClient.GetAllSchedules().Items
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
            ConfigureWindows<Scenario>(displayItemList);
        }

        public override void ConfigureStatusBar()
        {
            StatusBar = new StatusBar(new StatusItem[] {
                new StatusItem(Key.F5, "~F5~ Refresh Data", () => RefreshScreen()),
                new StatusItem(Key.F6, "~F6~ Copy Schedule", () => SaveItem(true)),
                new StatusItem(Key.F9, "~F9~ Delete Schedule", () => DeleteItem()),
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
                    var schedule = JsonConvert.DeserializeObject<Schedule>(json);
                    var scheduleRequest = new ScheduleRequest(
                        cron: schedule.Cron,
                        name: schedule.Name);

                    if (copyCurrent)
                    {
                        int nameCounter = STClient.GetAllSchedules().Items.Where(n => n.Name.Equals(schedule.Name)).Count();
                        nameCounter++;
                        scheduleRequest.Name += $"-copy {nameCounter}";
                        STClient.CreateSchedule(schedule.InstalledAppId.ToString(), scheduleRequest);
                    }
                    else
                    {
                        var response = STClient.CreateSchedule(schedule.InstalledAppId.ToString(), scheduleRequest);
                    }
                    RefreshScreen();
                }
                catch (System.Exception exp)
                {
                    ShowStatusBarMessage($"Error updating: {exp.Message}");
                }
            }
            return true;
        }

        public override void DeleteItem()
        {
            if (SelectedItem != null)
            {
                Schedule currentItem = (Schedule)SelectedItem;
                try
                {
                    STClient.DeleteSchedule(currentItem.InstalledAppId.ToString(), currentItem.Name);
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
