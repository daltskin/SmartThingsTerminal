using System;
using System.Collections.Generic;
using System.Text;
using Terminal.Gui;

namespace SmartThingsTerminal
{
    public static class MenuHelper
    {
        public static string GetAboutMessage()
        {
            StringBuilder aboutMessage = new StringBuilder();
            aboutMessage.Append("\n");
            aboutMessage.Append(GetAppTitle());
            aboutMessage.Append("\n");
            aboutMessage.Append("SmartThings Terminal\n");
            aboutMessage.Append("Terminal for the SmartThings REST API\n");
            aboutMessage.Append("\n\n");
            aboutMessage.Append("SmartThings REST API: https://smartthings.developer.samsung.com/docs/api-ref/st-api.html\n");
            aboutMessage.Append("\n");
            aboutMessage.Append($"Version: {typeof(Program).Assembly.GetName().Version}\n");
            aboutMessage.Append($"Using Terminal.Gui Version: {typeof(Terminal.Gui.Application).Assembly.GetName().Version}\n");
            return aboutMessage.ToString();
        }

        public static MenuBar GetStandardMenuBar(ColorScheme colorScheme, string typeName = null, Action exportAction = null, Action importAction = null)
        {
            List<MenuItem> fileSubMenu = new List<MenuItem>();
            if (importAction != null)
            {
                fileSubMenu.Add(new MenuItem($"_Import {typeName}", "", () => importAction()));
            }
            if (exportAction != null)
            {
                fileSubMenu.Add(new MenuItem($"_Export {typeName}", "", () => exportAction()));
            }

            if (typeName == null)
            {
                fileSubMenu.Add(new MenuItem("_Quit", "", () => Application.RequestStop()));
            }
            var fileMenuBar = new MenuBarItem("_File", fileSubMenu.ToArray());

            var colorSchemeBar = new MenuBarItem("_Color Scheme", CreateColorSchemeMenuItems(colorScheme));
            var aboutBar = new MenuBarItem("_About", "", () => MessageBox.Query("About SmartThings Terminal", GetAboutMessage(), "Ok"));

            var menuBar = new MenuBar(new MenuBarItem[] { fileMenuBar, colorSchemeBar, aboutBar });
            return menuBar;
        }

        public static MenuItem[] CreateColorSchemeMenuItems(ColorScheme colorScheme)
        {
            List<MenuItem> menuItems = new List<MenuItem>();
            foreach (var sc in Colors.ColorSchemes)
            {
                var item = new MenuItem();
                item.Title = sc.Key;
                item.CheckType |= MenuItemCheckStyle.Radio;
                item.Checked = sc.Value == colorScheme;
                item.Action += () =>
                {
                    colorScheme = sc.Value;
                    SetColorScheme(colorScheme);
                    foreach (var menuItem in menuItems)
                    {
                        menuItem.Checked = menuItem.Title.Equals(sc.Key) && sc.Value == colorScheme;
                    }
                };
                menuItems.Add(item);
            }
            return menuItems.ToArray();
        }

        public static void SetColorScheme(ColorScheme colorScheme)
        {
            if (Application.Top != null)
            {
                SetViewColorScheme(Application.Top, colorScheme);
                Application.Top?.SetNeedsDisplay();
            }
        }

        public static void SetViewColorScheme(View view, ColorScheme colorScheme)
        {
            view.ColorScheme = colorScheme;
            if (view.Subviews.Count > 0)
            {
                foreach (var sv in view.Subviews)
                {
                    SetViewColorScheme(sv, colorScheme);
                }
            }
        }

        public static string GetAppTitle()
        {
            StringBuilder appName = new StringBuilder();
            appName.AppendLine(@"      _______.___________.___________.  ");
            appName.AppendLine(@"     /       |           |           |  ");
            appName.AppendLine(@"    |   (----`---|  |----`---|  |----`  ");
            appName.AppendLine(@"     \   \       |  |        |  |       ");
            appName.AppendLine(@" .----)   |mart  |  |hings   |  |erminal");
            appName.AppendLine(@" |_______/       |__|        |__|       ");
            appName.AppendLine($" Interactive CLI for SmartThings v{typeof(Program).Assembly.GetName().Version}");
            return appName.ToString();
        }
    }
}
