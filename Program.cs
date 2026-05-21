using Photino.NET;
using System;

namespace TbBrowser
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            // Photino v3: Use Photino instance directly
            var app = new Photino();

            // 1. Toolbar Window (UI Strip)
            var toolbar = new PhotinoWindow("TB Toolbar", null)
                .SetSize(900, 50)
                .SetUseOsDefaultLocation(false)
                .SetLeft(200)
                .SetTop(200)
                .SetResizable(false)
                .SetMaximizable(false)
                .Load("wwwroot/toolbar.html");

            // 2. Browser Window (Content)
            var browser = new PhotinoWindow("TB Browser", null)
                .SetSize(900, 600)
                .SetUseOsDefaultLocation(false)
                .SetLeft(200)
                .SetTop(250)
                .Load("https://www.bing.com");

            // 3. Link: Toolbar → Browser
            toolbar.RegisterWebMessageReceivedHandler((sender, url) =>
            {
                browser.Load(url);
            });

            // Run the app (blocks until windows close)
            app.Run(toolbar);
        }
    }
}
