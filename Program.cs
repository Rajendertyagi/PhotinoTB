using Photino.NET;
using System;

namespace TbBrowser
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            var app = new PhotinoApp("TB Browser");

            // 1. Create Toolbar Window (The UI Strip)
            var toolbarWindow = app.CreateMainWindow();
            toolbarWindow
                .SetTitle("TB Toolbar")
                .SetSize(900, 50) // Thin height
                .SetUseOsDefaultLocation(false)
                .SetLeft(200)
                .SetTop(200)
                .SetResizable(false)
                .SetMaximizable(false)
                .Load("wwwroot/toolbar.html");

            // 2. Create Browser Window (The Content)
            var browserWindow = app.CreateMainWindow();
            browserWindow
                .SetTitle("TB Browser")
                .SetSize(900, 600)
                .SetUseOsDefaultLocation(false)
                .SetLeft(200)
                .SetTop(250) // Positioned below toolbar
                .Load("https://www.bing.com"); // Default page

            // 3. Link Them: Toolbar sends URL -> Browser loads it
            toolbarWindow.RegisterWebMessageReceivedHandler((sender, message) =>
            {
                browserWindow.Load(message);
            });

            app.Run();
        }
    }
}
