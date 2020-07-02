using Microsoft.VisualBasic;
using NStack;
using SmartThingsNet.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Terminal.Gui;

namespace SmartThingsTerminal
{
    public class Scenario : IDisposable
    {
        private bool _disposedValue;

        public Toplevel Top { get; set; }

        public Window Win { get; set; }

        public SmartThingsClient STClient { get; set; }

        public Window LeftPane { get; set; }
        public ListView ClassListView { get; set; }
        public FrameView HostPane { get; set; }

        public FrameView SettingsPane { get; set; }

        public Label ErrorView { get; set; }

        public View CurrentView { get; set; }

        public StatusBar StatusBar { get; set; }

        public virtual View CreateJsonView(string json)
        {
            var view = new TextView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            string formattedJson = json.Replace("\r", "");

            view.Text = ustring.Make(formattedJson);
            view.ReadOnly = true;

            // Set the colorscheme to make it stand out
            view.ColorScheme = Colors.Dialog;

            // Add
            HostPane.Add(view);
            HostPane.LayoutSubviews();
            HostPane.Clear();
            HostPane.SetNeedsDisplay();
            HostPane.Title = "Raw Data";
            return view;
        }

        public virtual void SetErrorView(string message)
        {
            ErrorView = new Label()
            {
                X = Pos.Center(),
                Y = Pos.Center(),
                Width = Dim.Fill(),
                Height = 5
            };

            ErrorView.Text = ustring.Make(message);
        }

        public virtual void DisplayErrorView()
        {
            if (ErrorView != null)
            {
                HostPane.Add(ErrorView);
            }
        }

        public virtual void ClearClass(View view)
        {
            // Remove existing class, if any
            if (view != null)
            {
                HostPane.Remove(view);
                HostPane.Clear();
            }
        }

        public virtual void RefreshScreen()
        {
            ErrorView = null;
            STClient.ResetData();
            Top.RemoveAll();
            Setup();
            Run();
        }

        private void Quit()
        {
            Application.RequestStop();
        }

        public virtual void ConfigureStatusBar()
        {
            StatusBar = new StatusBar(new StatusItem[] {
                new StatusItem(Key.ControlR, "~F5~ Refresh Data", () => RefreshScreen()),
                new StatusItem(Key.Home, "~Home~ Back", () => Quit())
            });
        }

        public virtual void ConfigureLeftPane(string title)
        {
            LeftPane = new Window(title)
            {
                X = 0,
                Y = 0, // for menu
                Width = 40,
                Height = Dim.Fill(),
                CanFocus = false,
                ColorScheme = Colors.TopLevel,
            };
        }

        public virtual void ConfigureHostPane(string title)
        {
            Pos settingsY = new Pos();
            if (SettingsPane?.Subviews.Count > 0)
            {
                settingsY = Pos.Bottom(SettingsPane);
            }
            
            HostPane = new FrameView(title)
            {
                X = Pos.Right(LeftPane),
                Y = settingsY,
                Width = Dim.Fill(),
                Height = Dim.Fill(1), // + 1 for status bar
                ColorScheme = Colors.Dialog,
            };
        }

        public virtual void ConfigureSettingsPane()
        {
            SettingsPane = new FrameView("Settings")
            {
                X = Pos.Right(LeftPane),
                Y = 0, // for menu
                Width = Dim.Fill(),
                Height = 8,
                CanFocus = false,
                ColorScheme = Colors.TopLevel,
            };
        }

        public virtual void UpdateSettings<T>(object selectedItem)
        { 
        
        }

        public ListView GetClassListView<T>(Dictionary<string, T> displayItemList)
        {
            ListView classListView = null;

            if (displayItemList?.Keys.Count > 0)
            {
                classListView = new ListView(displayItemList.Keys.ToList())
                {
                    X = 0,
                    Y = 0,
                    Width = Dim.Fill(0),
                    Height = Dim.Fill(), // for status bar
                    AllowsMarking = false,
                    ColorScheme = Colors.TopLevel,
                };

                classListView.SelectedItemChanged += (args) =>
                {
                    ClearClass(CurrentView);
                    var selectedItem = displayItemList.Values.ToArray()[classListView.SelectedItem];
                    string json = ((dynamic)selectedItem).ToJson();
                    CurrentView = CreateJsonView(json);
                    UpdateSettings<T>(selectedItem);
                };
            }
            return classListView;
        }

        public Dictionary<string, dynamic> GetDisplayData<T>()
        {
            Dictionary<string, dynamic> retVal = null;

            if (typeof(T) == typeof(SceneSummary))
            {
                if (STClient.GetAllScenes().Items?.Count > 0)
                {
                    retVal = STClient.GetAllScenes().Items
                        .OrderBy(t => t.SceneName)
                        .Select(t => new KeyValuePair<string, dynamic>(t.SceneName, t))
                        .ToDictionary(t => t.Key, t => t.Value);
                }
            }
            else if (typeof(T) == typeof(Device))
            {
                if (STClient.GetAllDevices().Items?.Count > 0)
                {
                    retVal = STClient.GetAllDevices().Items
                        .OrderBy(t => t.Name)
                        .Select(t => new KeyValuePair<string, dynamic>(t.Label, t))
                        .ToDictionary(t => t.Key, t => t.Value);
                }
            }
            else if (typeof(T) == typeof(Subscription))
            {
                if (STClient.GetAllSubscriptions().Items?.Count > 0)
                {
                    retVal = STClient.GetAllSubscriptions().Items
                       .OrderBy(t => t.Id)
                       .Select(t => new KeyValuePair<string, dynamic>(t.Id, t))
                       .ToDictionary(t => t.Key, t => t.Value);
                }
            }
            else if (typeof(T) == typeof(InstalledApp))
            {
                if (STClient.GetAllInstalledApps().Items?.Count > 0)
                {
                    retVal = STClient.GetAllInstalledApps().Items
                        .OrderBy(t => t.DisplayName)
                        .Select(t => new KeyValuePair<string, dynamic>(t.DisplayName, t))
                        .ToDictionary(t => t.Key, t => t.Value);
                }
            }
            else if (typeof(T) == typeof(Location))
            {
                if (STClient.GetAllLocations().Items?.Count > 0)
                {
                    retVal = STClient.GetAllLocations().Items
                        .OrderBy(t => t.Name)
                        .Select(t => new KeyValuePair<string, dynamic>(t.Name, t))
                        .ToDictionary(t => t.Key, t => t.Value);
                }
            }
            else if (typeof(T) == typeof(Room))
            {
                if (STClient.GetAllRooms().Items?.Count > 0)
                {
                    retVal = STClient.GetAllRooms().Items
                        .OrderBy(t => t.Name)
                        .Select(t => new KeyValuePair<string, dynamic>(t.Name, t))
                        .ToDictionary(t => t.Key, t => t.Value);
                }
            }
            else if (typeof(T) == typeof(Rule))
            {
                if (STClient.GetAllRules().Items?.Count > 0)
                {
                    retVal = STClient.GetAllRules().Items
                       .OrderBy(t => t.Name)
                       .Select(t => new KeyValuePair<string, dynamic>(t.Name, t))
                       .ToDictionary(t => t.Key, t => t.Value);
                }
            }
            else if (typeof(T) == typeof(Schedule))
            {
                if (STClient.GetAllSchedules().Items?.Count > 0)
                {
                    retVal = STClient.GetAllSchedules().Items
                       .OrderBy(t => t.Name)
                       .Select(t => new KeyValuePair<string, dynamic>(t.Name, t))
                       .ToDictionary(t => t.Key, t => t.Value);
                }
            }

            return retVal;
        }

        public virtual void ConfigureWindows<T>(bool configureLeftPane = true, bool configureHostPane = true)
        {
            if (configureLeftPane)
            {
                ConfigureLeftPane(GetName());
            }
            if (configureHostPane)
            {
                ConfigureHostPane("");
            }

            ConfigureStatusBar();

            Dictionary<string, dynamic> displayItemList = null;

            try
            {
                displayItemList = GetDisplayData<T>();
            }
            catch (SmartThingsNet.Client.ApiException exp)
            {
                SetErrorView($"Error calling API: {exp.Source} {exp.ErrorCode} {exp.Message}");
            }
            catch (System.Exception exp)
            {
                SetErrorView($"Unknown error calling API: {exp.Message}");
            }

            var itemListView = GetClassListView(displayItemList);
            if (itemListView != null)
            {
                LeftPane.Add(itemListView);
            }
            else
            {
                SetErrorView($"You have no {GetName()} configured");
            }

            if (SettingsPane == null)
            {
                Top.Add(LeftPane, HostPane);
            }
            else
            {
                Top.Add(LeftPane, SettingsPane, HostPane);
            }

            Top.Add(StatusBar);

            if (displayItemList?.Count > 0)
            {
                var firstItem = displayItemList?.FirstOrDefault().Value;
                if (firstItem != null)
                {
                    CurrentView = CreateJsonView(firstItem.ToJson());
                    if (SettingsPane != null)
                    {
                        UpdateSettings<T>(firstItem);
                    }
                }
            }

            DisplayErrorView();
        }

        /// <summary>
        /// Helper that provides the default <see cref="Terminal.Gui.Window"/> implementation with a frame and 
        /// label showing the name of the <see cref="Scenario"/> and logic to exit back to 
        /// the Scenario picker UI.
        /// Override <see cref="Init(Toplevel)"/> to provide any <see cref="Terminal.Gui.Toplevel"/> behavior needed.
        /// </summary>
        /// <param name="top">The Toplevel created by the UI Catalog host.</param>
        /// <param name="colorScheme">The colorscheme to use.</param>
        /// <remarks>
        /// <para>
        /// Thg base implementation calls <see cref="Application.Init"/>, sets <see cref="Top"/> to the passed in <see cref="Toplevel"/>, creates a <see cref="Window"/> for <see cref="Win"/> and adds it to <see cref="Top"/>.
        /// </para>
        /// <para>
        /// Overrides that do not call the base.<see cref="Run"/>, must call <see cref="Application.Init "/> before creating any views or calling other Terminal.Gui APIs.
        /// </para>
        /// </remarks>
        public virtual void Init(Toplevel top, ColorScheme colorScheme, SmartThingsClient smartThingsClient)
        {
            Application.Init();

            Top = top;
            if (Top == null)
            {
                Top = Application.Top;
            }

            Win = new Window($"CTRL-Q to Close - Scenario: {GetName()}")
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ColorScheme = colorScheme,
            };
            Top.Add(Win);

            STClient = smartThingsClient;
        }

        /// <summary>
        /// Defines the metadata (Name and Description) for a <see cref="Scenario"/>
        /// </summary>
        [System.AttributeUsage(System.AttributeTargets.Class)]
        public class ScenarioMetadata : System.Attribute
        {
            /// <summary>
            /// <see cref="Scenario"/> Name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// <see cref="Scenario"/> Description
            /// </summary>
            public string Description { get; set; }

            public ScenarioMetadata(string Name, string Description)
            {
                this.Name = Name;
                this.Description = Description;
            }

            /// <summary>
            /// Static helper function to get the <see cref="Scenario"/> Name given a Type
            /// </summary>
            /// <param name="t"></param>
            /// <returns></returns>
            public static string GetName(Type t) => ((ScenarioMetadata)System.Attribute.GetCustomAttributes(t)[0]).Name;

            /// <summary>
            /// Static helper function to get the <see cref="Scenario"/> Description given a Type
            /// </summary>
            /// <param name="t"></param>
            /// <returns></returns>
            public static string GetDescription(Type t) => ((ScenarioMetadata)System.Attribute.GetCustomAttributes(t)[0]).Description;
        }

        /// <summary>
        /// Helper to get the <see cref="Scenario"/> Name (defined in <see cref="ScenarioMetadata"/>)
        /// </summary>
        /// <returns></returns>
        public string GetName() => ScenarioMetadata.GetName(this.GetType());

        /// <summary>
        /// Helper to get the <see cref="Scenario"/> Description (defined in <see cref="ScenarioMetadata"/>)
        /// </summary>
        /// <returns></returns>
        public string GetDescription() => ScenarioMetadata.GetDescription(this.GetType());

        /// <summary>
        /// Defines the category names used to catagorize a <see cref="Scenario"/>
        /// </summary>
        [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
        public class ScenarioCategory : System.Attribute
        {
            /// <summary>
            /// Category Name
            /// </summary>
            public string Name { get; set; }

            public ScenarioCategory(string Name) => this.Name = Name;

            /// <summary>
            /// Static helper function to get the <see cref="Scenario"/> Name given a Type
            /// </summary>
            /// <param name="t"></param>
            /// <returns>Name of the catagory</returns>
            public static string GetName(Type t) => ((ScenarioCategory)System.Attribute.GetCustomAttributes(t)[0]).Name;

            /// <summary>
            /// Static helper function to get the <see cref="Scenario"/> Categories given a Type
            /// </summary>
            /// <param name="t"></param>
            /// <returns>list of catagory names</returns>
            public static List<string> GetCategories(Type t) => System.Attribute.GetCustomAttributes(t)
                .ToList()
                .Where(a => a is ScenarioCategory)
                .Select(a => ((ScenarioCategory)a).Name)
                .ToList();
        }

        /// <summary>
        /// Helper function to get the list of categories a <see cref="Scenario"/> belongs to (defined in <see cref="ScenarioCategory"/>)
        /// </summary>
        /// <returns>list of catagory names</returns>
        public List<string> GetCategories() => ScenarioCategory.GetCategories(this.GetType());

        /// <inheritdoc/>
        public override string ToString() => $"{GetName(),-30}{GetDescription()}";

        /// <summary>
        /// Override this to implement the <see cref="Scenario"/> setup logic (create controls, etc...). 
        /// </summary>
        /// <remarks>This is typically the best place to put scenario logic code.</remarks>
        public virtual void Setup()
        {
        }

        /// <summary>
        /// Runs the <see cref="Scenario"/>. Override to start the <see cref="Scenario"/> using a <see cref="Toplevel"/> different than `Top`.
        /// 
        /// </summary>
        /// <remarks>
        /// Overrides that do not call the base.<see cref="Run"/>, must call <see cref="Application.Shutdown"/> before returning.
        /// </remarks>
        public virtual void Run()
        {
            // This method already performs a later automatic shutdown.
            Application.Run(Top);
        }

        /// <summary>
        /// Stops the scenario. Override to change shutdown behavior for the <see cref="Scenario"/>.
        /// </summary>
        public virtual void RequestStop()
        {
            Application.RequestStop();
        }

        /// <summary>
        /// Returns a list of all Categories set by all of the <see cref="Scenario"/>s defined in the project.
        /// </summary>
        internal static List<string> GetAllCategories()
        {
            List<string> categories = new List<string>() { "All" };
            foreach (Type type in typeof(Scenario).Assembly.GetTypes()
             .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(Scenario))))
            {
                List<System.Attribute> attrs = System.Attribute.GetCustomAttributes(type).ToList();
                categories = categories.Union(attrs.Where(a => a is ScenarioCategory).Select(a => ((ScenarioCategory)a).Name)).ToList();
            }
            return categories;
        }

        /// <summary>
        /// Returns an instance of each <see cref="Scenario"/> defined in the project. 
        /// https://stackoverflow.com/questions/5411694/get-all-inherited-classes-of-an-abstract-class
        /// </summary>
        public static List<Type> GetDerivedClasses<T>()
        {
            List<Type> objects = new List<Type>();
            foreach (Type type in typeof(T).Assembly.GetTypes()
             .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
            {
                objects.Add(type);
            }
            return objects;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    STClient.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

