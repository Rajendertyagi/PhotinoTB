using Photino.NET;
using System;
using System.Drawing;

namespace TbBrowser
{
    class Program
    {
        static PhotinoWindow? browserWindow;

        [STAThread]
        static void Main()
        {
            // 1. Toolbar Window (UI Strip)
            var toolbar = new PhotinoWindow()
                .SetTitle("TB Toolbar")
                .SetUseOsDefaultSize(false)
                .SetSize(new Size(900, 50))
                .SetUseOsDefaultLocation(false)
                .SetLeft(200)
                .SetTop(200)
                .SetResizable(false)
                .Load("wwwroot/toolbar.html");

            // 2. Browser Window (Content)
            browserWindow = new PhotinoWindow(toolbar) // parent = toolbar
                .SetTitle("TB Browser")
                .SetUseOsDefaultSize(false)
                .SetSize(new Size(900, 600))
                .SetUseOsDefaultLocation(false)
                .SetLeft(200)
                .SetTop(250)
                .Load("https://www.bing.com");

            // 3. Link: Toolbar → Browser
            toolbar.RegisterWebMessageReceivedHandler((sender, url) =>
            {
                browserWindow?.Load(url);
            });

            // 4. Run app
            toolbar.WaitForClose();
        }
    }
}
