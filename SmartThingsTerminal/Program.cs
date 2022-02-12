using NStack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Terminal.Gui;
using Rune = System.Rune;

namespace SmartThingsTerminal
{
    class Program
    {
        private static Toplevel _top;
        private static MenuBar _menu;
        private static int _nameColumnWidth;
        private static FrameView _leftPane;
        private static List<string> _categories;
        private static ListView _categoryListView;
        private static FrameView _rightPane;
        private static FrameView _appTitlePane;
        private static List<Type> _scenarios;
        private static ListView _scenarioListView;
        private static StatusBar _statusBar;
        private static StatusItem _capslock;
        private static StatusItem _numlock;
        private static StatusItem _scrolllock;
        private static int _categoryListViewItem;
        private static int _scenarioListViewItem;
        private static Scenario _runningScenario = null;
        private static bool _useSystemConsole = false;
        private static SmartThingsClient _stClient;

        static void Main(string[] args)
        {
            Startup startup = new Startup();
            if (startup.Configure(args))
            {
                Init(startup.Options);
            }
        }

        private static void Init(Options opts)
        {
            Console.Title = "SmartThings Terminal";
            _stClient = new SmartThingsClient(opts.AccessToken);

            if (Debugger.IsAttached)
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");

            _scenarios = Scenario.GetDerivedClasses<Scenario>().OrderBy(t => Scenario.ScenarioMetadata.GetName(t)).ToList();

            if (opts.ApiName != null)
            {
                var item = _scenarios.FindIndex(t => Scenario.ScenarioMetadata.GetName(t).Equals(opts.ApiName, StringComparison.OrdinalIgnoreCase));

                try
                {
                    var selectedScenario = _scenarios[item];
                    if (selectedScenario != null)
                    {
                        _runningScenario = (Scenario)Activator.CreateInstance(selectedScenario);
                        Application.Init();
                        _runningScenario.Init(Application.Top, _baseColorScheme, _stClient);
                        _runningScenario.Setup();
                        _runningScenario.Run();
                        _runningScenario = null;
                        Application.Shutdown();
                        return;
                    }
                }
                catch (Exception)
                {
                    // invalid option
                }
            }

            Scenario scenario;
            while ((scenario = GetScenarioToRun()) != null)
            {
                Application.UseSystemConsole = _useSystemConsole;
                Application.Init();
                scenario.Init(Application.Top, _baseColorScheme, _stClient);
                scenario.Setup();
                scenario.Run();
            }
            Application.Shutdown();
        }

        /// <summary>
        /// This shows the selection UI. Each time it is run, it calls Application.Init to reset everything.
        /// </summary>
        /// <returns></returns>
        private static Scenario GetScenarioToRun()
        {
            Application.UseSystemConsole = false;
            Application.Init();

            // Set this here because not initilzied until driver is loaded
            _baseColorScheme = Colors.Base;

            _menu = MenuHelper.GetStandardMenuBar(_baseColorScheme);

            _leftPane = new FrameView("API")
            {
                X = 0,
                Y = 1, // for menu
                Width = 30,
                Height = Dim.Fill(1),
                CanFocus = true,
            };

            _categories = Scenario.GetAllCategories().OrderBy(c => c).ToList();
            _categoryListView = new ListView(_categories)
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(0),
                AllowsMarking = false,
                CanFocus = true
            };
            _categoryListView.OpenSelectedItem += (a) =>
            {
                
                _rightPane.SetFocus();
            };

            _categoryListView.SelectedItemChanged += CategoryListView_SelectedChanged;
            _leftPane.Add(_categoryListView);

            Label appNameView = new Label() { X = 0, Y = 0, Height = Dim.Fill(), Width = Dim.Fill(), CanFocus = false, Text = MenuHelper.GetAppTitle() };
            _appTitlePane = new FrameView()
            {
                X = 30,
                Y = 1, // for menu
                Width = Dim.Fill(),
                Height = 9,
                CanFocus = false,
            };
            _appTitlePane.Add(appNameView);

            _rightPane = new FrameView("API Description")
            {
                X = 30,
                //Y = 1, // for menu
                Y = Pos.Bottom(_appTitlePane),
                Width = Dim.Fill(),
                Height = Dim.Fill(1),
                CanFocus = true,
            };

            _nameColumnWidth = Scenario.ScenarioMetadata.GetName(_scenarios.OrderByDescending(t => Scenario.ScenarioMetadata.GetName(t).Length).FirstOrDefault()).Length;
            _scenarioListView = new ListView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(0),
                AllowsMarking = false,
                CanFocus = true,
            };

            _scenarioListView.OpenSelectedItem += _scenarioListView_OpenSelectedItem;
            _rightPane.Add(_scenarioListView);

            //_categoryListView.SelectedItem = _categoryListViewItem;
            _categoryListView.OnSelectedChanged();

            _capslock = new StatusItem(Key.CharMask, "Caps", null);
            _numlock = new StatusItem(Key.CharMask, "Num", null);
            _scrolllock = new StatusItem(Key.CharMask, "Scroll", null);

            _statusBar = new StatusBar(new StatusItem[] {
                _capslock,
                _numlock,
                _scrolllock,
                new StatusItem(Key.F5, "~F5~ Refresh Data", () => {
                    _stClient.ResetData();
                }),
                new StatusItem(Key.F9, "~F9~ Menu", () => {
                    _stClient.ResetData();
                }),
                new StatusItem(Key.Q | Key.CtrlMask, "~CTRL-Q~ Quit", () => {
                    if (_runningScenario is null){
						// This causes GetScenarioToRun to return null
						_runningScenario = null;
                        Application.RequestStop();
                    } else {
                        _runningScenario.RequestStop();
                    }
                }),
            });

            MenuHelper.SetColorScheme(_baseColorScheme);
            _top = Application.Top;
            _top.KeyDown += KeyDownHandler;
            _top.Add(_menu);
            _top.Add(_leftPane);
            _top.Add(_appTitlePane);
            _top.Add(_rightPane);
            _top.Add(_statusBar);
            _top.CanFocus = true;
            _top.Ready += () =>
            {
                if (_runningScenario != null)
                {
                    _runningScenario = null;
                }
            };

            Application.Run(_top);
            Application.Shutdown();
            return _runningScenario;
        }

        static ColorScheme _baseColorScheme;

        private static void _scenarioListView_OpenSelectedItem(EventArgs e)
        {
            if (_runningScenario is null)
            {
                _scenarioListViewItem = _scenarioListView.SelectedItem;
                var source = _scenarioListView.Source as ScenarioListDataSource;
                _runningScenario = (Scenario)Activator.CreateInstance(source.Scenarios[_scenarioListView.SelectedItem]);
                Application.RequestStop();
            }
        }

        internal class ScenarioListDataSource : IListDataSource
        {
            public List<Type> Scenarios { get; set; }

            public bool IsMarked(int item) => false;

            public int Count => Scenarios.Count;

            public int Length => Scenarios.Count;

            public ScenarioListDataSource(List<Type> itemList) => Scenarios = itemList;

            public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width, int start = 0)
            {
                container.Move(col, line);
                // Equivalent to an interpolated string like $"{Scenarios[item].Name, -widtestname}"; if such a thing were possible
                var s = String.Format(String.Format("{{0,{0}}}", -_nameColumnWidth), Scenario.ScenarioMetadata.GetName(Scenarios[item]));
                RenderUstr(driver, $"{s}  {Scenario.ScenarioMetadata.GetDescription(Scenarios[item])}", col, line, width);
            }

            public void SetMark(int item, bool value)
            {
            }

            // A slightly adapted method from: https://github.com/migueldeicaza/gui.cs/blob/fc1faba7452ccbdf49028ac49f0c9f0f42bbae91/Terminal.Gui/Views/ListView.cs#L433-L461
            private void RenderUstr(ConsoleDriver driver, ustring ustr, int col, int line, int width)
            {
                int used = 0;
                int index = 0;
                while (index < ustr.Length)
                {
                    (var rune, var size) = Utf8.DecodeRune(ustr, index, index - ustr.Length);
                    var count = Rune.ColumnWidth(rune);
                    if (used + count >= width) break;
                    driver.AddRune(rune);
                    used += count;
                    index += size;
                }

                while (used < width)
                {
                    driver.AddRune(' ');
                    used++;
                }
            }

            public IList ToList()
            {
                return Scenarios;
            }
        }

        /// <summary>
        /// When Scenarios are running we need to override the behavior of the Menu 
        /// and Statusbar to enable Scenarios that use those (or related key input)
        /// to not be impacted. Same as for tabs.
        /// </summary>
        /// <param name="ke"></param>
        private static void KeyDownHandler(View.KeyEventEventArgs a)
        {
            //if (a.KeyEvent.Key == Key.Tab || a.KeyEvent.Key == Key.BackTab) {
            //	// BUGBUG: Work around Issue #434 by implementing our own TAB navigation
            //	if (_top.MostFocused == _categoryListView)
            //		_top.SetFocus (_rightPane);
            //	else
            //		_top.SetFocus (_leftPane);
            //}

            if (a.KeyEvent.IsCapslock)
            {
                _capslock.Title = "Caps: On";
                _statusBar.SetNeedsDisplay();
            }
            else
            {
                _capslock.Title = "Caps: Off";
                _statusBar.SetNeedsDisplay();
            }

            if (a.KeyEvent.IsNumlock)
            {
                _numlock.Title = "Num: On";
                _statusBar.SetNeedsDisplay();
            }
            else
            {
                _numlock.Title = "Num: Off";
                _statusBar.SetNeedsDisplay();
            }

            if (a.KeyEvent.IsScrolllock)
            {
                _scrolllock.Title = "Scroll: On";
                _statusBar.SetNeedsDisplay();
            }
            else
            {
                _scrolllock.Title = "Scroll: Off";
                _statusBar.SetNeedsDisplay();
            }
        }

        private static void CategoryListView_SelectedChanged(ListViewItemEventArgs e)
        {
            if (_categoryListViewItem != _categoryListView.SelectedItem)
            {
                _scenarioListViewItem = 0;
            }
            _categoryListViewItem = _categoryListView.SelectedItem;
            var item = _categories[_categoryListView.SelectedItem];
            List<Type> newlist;
            if (item.Equals("All"))
            {
                newlist = _scenarios;
            }
            else
            {
                newlist = _scenarios.Where(t => Scenario.ScenarioCategory.GetCategories(t).Contains(item)).ToList();
            }
            _scenarioListView.Source = new ScenarioListDataSource(newlist);
            _scenarioListView.SelectedItem = _scenarioListViewItem;
        }
    }
}
