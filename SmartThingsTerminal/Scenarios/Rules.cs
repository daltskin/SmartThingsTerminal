using Newtonsoft.Json;
using SmartThingsNet.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terminal.Gui;

namespace SmartThingsTerminal.Scenarios
{
    [ScenarioMetadata(Name: "Rules", Description: "SmartThings rules")]
    [ScenarioCategory("Rules")]
    class Rules : Scenario
    {
        public override void Setup()
        {
            Dictionary<string, dynamic> dataItemList = null;
            Dictionary<string, string> displayItemList = null;
            try
            {
                if (STClient.GetAllRules().Items?.Count > 0)
                {
                    dataItemList = STClient.GetAllRules().Items
                        .OrderBy(o=> o.Name)
                        .Select(t => new KeyValuePair<string, dynamic>(t.Id, t))
                        .ToDictionary(t => t.Key, t => t.Value);

                    displayItemList = STClient.GetAllRules().Items
                        .OrderBy(o => o.Name)
                        .Select(t => new KeyValuePair<string, string>(t.Id, t.Name))
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
            ConfigureWindows<Rule>(displayItemList, dataItemList);
        }

        public override void ConfigureStatusBar()
        {
            StatusBar = new StatusBar(new StatusItem[] {
                //new StatusItem(Key.F2, "~F2~ Import rule", () => ToggleImport()),
                //new StatusItem(Key.F3, "~F3~ Edit", () => EnableEditMode()),
                //new StatusItem(Key.F4, "~F4~ Save", () => SaveItem()),
                //new StatusItem(Key.F5, "~F5~ Refresh Data", () => RefreshScreen()),
                //new StatusItem(Key.F6, "~F6~ Copy Rule", () => SaveItem(true)),
                //new StatusItem(Key.F7, "~F7~ Delete Rule", () => DeleteItem()),
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
                    var rule = JsonConvert.DeserializeObject<Rule>(json);
                    RuleRequest ruleRequest = new RuleRequest(rule.Name, rule.Actions, null, rule.TimeZoneId);

                    string locationId = GetRuleLocation(rule);
                    if (copyCurrent)
                    {
                        ruleRequest.Name += $"-copy";
                        STClient.CreateRule(locationId, ruleRequest);
                    }
                    else
                    {
                        var response = STClient.UpdateRule(rule.Id, locationId, ruleRequest);
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
                Rule currentRule = (Rule)SelectedItem;
                try
                {
                    STClient.DeleteRule(currentRule.Id, GetRuleLocation(currentRule));
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

        private string GetRuleLocation(Rule rule)
        {
            // Get the locationId for this rule
            string locationId = null;
            foreach (var location in STClient.GetAllLocations().Items)
            {
                var locationRules = STClient.GetAllRules(location.LocationId.ToString()).Items.Where(r => r.Id == rule.Id);
                locationId = location.LocationId.ToString();
                break;
            }

            return locationId;
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
                ImportRule($"{currentDirectory}//{selectedDirectory}");
            };

            FilePicker.Add(directoryList);
            directoryList.SetFocus();
        }

        private void ImportRule(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                var rule = JsonConvert.DeserializeObject<Rule>(json);
                RuleRequest ruleRequest = new RuleRequest(rule.Name, rule.Actions, null, rule.TimeZoneId);

                // TODO: prompt for which location
                string locationId = STClient.GetAllLocations().Items.FirstOrDefault().LocationId.ToString();
                var response = STClient.CreateRule(locationId, ruleRequest);
                ShowMessage($"Rule added!");
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