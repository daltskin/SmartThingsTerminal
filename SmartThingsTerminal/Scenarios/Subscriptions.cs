using Newtonsoft.Json;
using SmartThingsNet.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace SmartThingsTerminal.Scenarios
{
    [ScenarioMetadata(Name: "Subscriptions", Description: "SmartThings application subscriptions *untested*")]
    [ScenarioCategory("Subscriptions")]
    class Subscriptions : Scenario
    {
        public override void Setup()
        {
            Dictionary<string, dynamic> dataItemList = null;
            Dictionary<string, string> displayItemList = null;
            try
            {
                if (STClient.GetAllSubscriptions().Items?.Count > 0)
                {
                    dataItemList = STClient.GetAllSubscriptions().Items
                       .OrderBy(t => t.Id)
                       .Select(t => new KeyValuePair<string, dynamic>(t.Id, t))
                       .ToDictionary(t => t.Key, t => t.Value);

                    displayItemList = STClient.GetAllSubscriptions().Items
                        .OrderBy(o => o.Id)
                        .Select(t => new KeyValuePair<string, string>(t.Id, t.Id))
                        .ToDictionary(t => t.Key, t => t.Value);
                }
            }
            catch (SmartThingsNet.Client.ApiException exp)
            {
                ShowErrorMessage($"Error calling API: {exp.Source} {exp.ErrorCode} {exp.Message}");
            }
            catch (Exception exp)
            {
                ShowErrorMessage($"Unknown error calling API: {exp.Message}");
            }
            ConfigureWindows<Subscription>(displayItemList, dataItemList);
        }

        public override void ConfigureStatusBar()
        {
            StatusBar = new StatusBar(new StatusItem[] {
                new StatusItem(Key.F5, "~F5~ Refresh Data", () => RefreshScreen()),
                new StatusItem(Key.F6, "~F6~ Copy Subscription", () => SaveItem(true)),
                new StatusItem(Key.F9, "~F9~ Delete Subscription", () => DeleteItem()),
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
                    var subscription = JsonConvert.DeserializeObject<Subscription>(json);
                    var subscriptionRequest = new SubscriptionRequest(
                        (SubscriptionSource)subscription.SourceType,
                        subscription.Device,
                        subscription.Capability,
                        subscription.Mode,
                        subscription.DeviceLifecycle,
                        subscription.DeviceHealth,
                        subscription.SecurityArmState,
                        subscription.HubHealth,
                        subscription.SceneLifecycle);

                    STClient.SaveSubscription(subscription.InstalledAppId, subscriptionRequest);
                    RefreshScreen();
                }
                catch (Exception exp)
                {
                    ShowErrorMessage($"Error updating: {exp.Message}");
                }
            }
            return true;
        }

        public override void DeleteItem()
        {
            if (SelectedItem != null)
            {
                Subscription currentItem = (Subscription)SelectedItem;
                try
                {
                    STClient.DeleteSubscription(currentItem.InstalledAppId, currentItem.Id);
                    base.DeleteItem();
                    RefreshScreen();
                }
                catch (Exception exp)
                {
                    ShowErrorMessage($"Error deleting: {exp.Message}");
                }
            }
        }
    }
}
