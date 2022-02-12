using Newtonsoft.Json;
using NStack;
using SmartThingsNet.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        public ListView FileList { get; set; }

        public dynamic SelectedItem { get; set; }

        public int SelectedItemIndex { get; set; }
        public FrameView HostPane { get; set; }

        public FrameView SettingsPane { get; set; }

        public FrameView FilePicker { get; set; }

        public Label ErrorView { get; set; }

        public TextView JsonView { get; set; }

        public StatusBar StatusBar { get; set; }

        public Button StatusButton { get; set; }

        public MenuBar MenuBar { get; set; }

        public string SearchText { get; set; }

        public System.Timers.Timer SearchTimer { get; set; }

        public string FormatJson(string json)
        {
            return json?.Replace("\r", "");
        }

        public virtual void UpdateJsonView(string json)
        {
            JsonView.Text = ustring.Make(FormatJson(json));
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

        public virtual void Quit()
        {
            Application.RequestStop();
        }

        public virtual void ConfigureStatusBar()
        {
            StatusBar = new StatusBar(new StatusItem[] {
                new StatusItem(Key.F5, "~F5~ Refresh", () => RefreshScreen()),
                new StatusItem(Key.F12, "~F12~ Back", () => Quit())
            });
        }

        public virtual bool SaveItem(bool copyCurrent = false)
        {
            return true;
        }

        public virtual void DeleteItem()
        {
            SelectedItem = null;
            SelectedItemIndex = 0;
        }

        public void EnableEditMode()
        {
            if (SelectedItem != null)
            {
                JsonView.ReadOnly = false;
                JsonView.ColorScheme = Colors.Menu;
                JsonView.SetFocus();
            }
        }

        public void DisableEditMode()
        {
            if (SelectedItem != null)
            {
                JsonView.ReadOnly = true;
                JsonView.ColorScheme = Colors.Dialog;
            }
        }

        public virtual void ConfigureLeftPane(string title)
        {
            LeftPane = new Window(title)
            {
                X = 0,
                Y = 1,
                Width = Dim.Percent(25),
                Height = Dim.Fill(),
                CanFocus = false,
                ColorScheme = Colors.TopLevel,
            };
        }

        public virtual void ConfigureHostPane(string title)
        {
            Pos settingsY = 1;
            if (SettingsPane?.Subviews.Count > 0)
            {
                settingsY = Pos.Bottom(SettingsPane);
            }

            HostPane = new FrameView(title)
            {
                Id = "HostPane",
                X = Pos.Right(LeftPane),
                Y = settingsY,
                Width = Dim.Fill(),
                Height = Dim.Fill(), // + 1 for status bar
                ColorScheme = Colors.Dialog,
            };
            ConfigureJsonPane();
        }

        public void ConfigureJsonPane()
        {
            JsonView = new TextView()
            {
                Id = "JsonView",
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };

            JsonView.ReadOnly = true;

            // Set the colorscheme to make it stand out
            JsonView.ColorScheme = Colors.Dialog;

            HostPane.Add(JsonView);
        }

        public virtual void ConfigureSettingsPane()
        {
            SettingsPane = new FrameView("Settings")
            {
                X = Pos.Right(LeftPane),
                Y = 1, // for menu
                Width = Dim.Fill(),
                Height = 8,
                CanFocus = false,
                ColorScheme = Colors.TopLevel,
              
            };
        }

        public virtual void UpdateSettings<T>(object selectedItem)
        {

        }

        public ListView GetClassListView<T>(Dictionary<string, string> displayItemList)
        {
            ListView classListView = null;
            if (displayItemList?.Keys.Count > 0)
            {
                classListView = new ListView(displayItemList.Values.ToList())
                {
                    X = 0,
                    Y = 0,
                    Width = Dim.Fill(0),
                    Height = Dim.Fill(), // for status bar
                    AllowsMarking = false,
                    ColorScheme = Colors.TopLevel
                };

                classListView.SelectedItemChanged += (args) =>
                {
                    try
                    {
                        SelectedItemIndex = classListView.SelectedItem;
                        SelectedItem = _dataItemList.Values.ToArray()[SelectedItemIndex];
                        UpdateJsonView(SelectedItem.ToJson());
                        HostPane.Title = displayItemList.Keys.ToArray()[classListView.SelectedItem];
                        UpdateSettings<T>(SelectedItem);

                        Top.BringSubviewToFront(StatusBar);
                    }
                    catch (Exception)
                    {
                        // eat
                        // Troubleshooting: https://github.com/daltskin/SmartThingsTerminal/issues/8
                    }
                };

                classListView.Enter += (args) =>
                {
                    DisableEditMode();
                };

                classListView.KeyDown += (args) =>
                {
                    if (!char.IsControl((char)args.KeyEvent.Key))
                    {
                        SearchText += args.KeyEvent.ToString();
                    }

                    if (SearchTimer == null)
                    {
                        SearchTimer = new System.Timers.Timer(500);
                        SearchTimer.Elapsed += SearchTimer_Elapsed;
                    }
                    if (!SearchTimer.Enabled)
                    {
                        SearchTimer.Start();
                    }
                };
            }
            ClassListView = classListView;
            return classListView;
        }

        private void SearchTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SearchTimer.Stop();

            if (!string.IsNullOrEmpty(SearchText))
            {
                var s = (List<string>)(ClassListView.Source.ToList());
                var i = s.FindIndex(0, s.Count, d => d.StartsWith(SearchText, StringComparison.InvariantCultureIgnoreCase));
                if (i >= 0)
                {
                    Application.MainLoop.Invoke(() => { ClassListView.SelectedItem = i; });
                }
                SearchText = null;
            }
        }

        public virtual void ExportItem()
        {
            if (SelectedItem != null)
            {
                string json = JsonConvert.SerializeObject(SelectedItem);
                string id = _dataItemList.Keys.ToArray()[SelectedItemIndex];
                string filePathName = $"{Directory.GetCurrentDirectory()}//{SelectedItem.GetType().Name}_{id}.json";
                File.WriteAllText(filePathName, json);

                ShowMessage($"Exported: {SelectedItem.GetType().Name}_{id}.json");
            }
        }

        public virtual void ImportItem()
        {
            if (FilePicker != null)
            {
                FilePicker.RemoveAll();
                LeftPane.Remove(FilePicker);
                FilePicker = null;
                RefreshScreen();
            }
            else
            {
                ShowImportFileMenu();
            }
        }

        public virtual void ShowImportFileMenu()
        {
            FilePicker = new FrameView("Select file")
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Percent(75),
                ColorScheme = Colors.Menu
            };

            FilePicker.Leave += (args) =>
            {
                LeftPane.Remove(FilePicker);
                FilePicker = null;
            };

            GetDirectoriesAndFileView(Directory.GetCurrentDirectory());
            LeftPane.Add(FilePicker);
            FilePicker.SetFocus();
        }

        public virtual void GetDirectoriesAndFileView(string currentDirectory)
        {

        }

        Dictionary<string, dynamic> _dataItemList = null;

        public virtual void ConfigureWindows<T>(
            Dictionary<string, string> displayItemList, 
            Dictionary<string, dynamic> dataItemList, 
            bool configureLeftPane = true, 
            bool configureHostPane = true)
        {
            _dataItemList = dataItemList;

            if (configureLeftPane)
            {
                ConfigureLeftPane(GetName());
            }
            if (configureHostPane)
            {
                ConfigureHostPane("");
            }

            ConfigureStatusBar();

            MenuBar menubar = null;
            if (displayItemList?.Count > 0)
            {
                var itemListView = GetClassListView<T>(displayItemList);
                LeftPane.Add(itemListView);

                string typeName = dataItemList.FirstOrDefault().Value.GetType().Name;
                switch ((T)dataItemList.FirstOrDefault().Value)
                {
                    case Device d:
                    case InstalledApp app:
                    case SceneSummary s:
                    case Subscription sub:
                    case CapabilitySummary cs:
                        menubar = MenuHelper.GetStandardMenuBar(Top.ColorScheme, typeName, ExportItem, null);
                        break;
                    default:
                        menubar = MenuHelper.GetStandardMenuBar(Top.ColorScheme, typeName, ExportItem, ImportItem);
                        break;
                }
            }
            else
            {
                menubar = MenuHelper.GetStandardMenuBar(Top.ColorScheme);
            }
            
            if (menubar != null)
            {
                Top.Add(menubar);
            }
            Top.Add(LeftPane);
            if (SettingsPane != null)
            {
                Top.Add(SettingsPane);
            }
            Top.Add(HostPane);
            Top.Add(StatusBar);

            if (displayItemList?.Count > 0)
            {
                dynamic itemToSelect = SelectedItem ?? dataItemList?.FirstOrDefault().Value;
                UpdateJsonView(itemToSelect.ToJson());
                if (SettingsPane != null)
                {
                    UpdateSettings<T>(itemToSelect);
                }
            }
        }

        public void ShowErrorMessage(string text)
        {
            MessageBox.ErrorQuery("SmartThings Terminal", $"\n{text}", "Ok");
        }

        public void ShowMessage(string text)
        {
            MessageBox.Query("SmartThings Terminal", $"\n{text}", "Ok");
        }

        public void ShowStatusBarMessage(string text)
        {
            StatusButton = new Button(text)
            {
                X = 0,
                Y = Pos.Bottom(HostPane),
                IsDefault = true,
            };
            StatusButton.Clicked += () => 
            { 
                Top.Remove(StatusButton); 
            };

            Top.Add(StatusButton);
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

