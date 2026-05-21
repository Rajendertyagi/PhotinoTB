using Photino.NET;

class Program
{
    [STAThread]
    static void Main()
    {
        // Toolbar Window (UI Strip)
        var toolbar = new PhotinoWindow()
            .SetTitle("TB Toolbar")
            .SetSize(900, 50)
            .SetUseOsDefaultLocation(false)
            .SetLeft(200)
            .SetTop(200)
            .SetResizable(false)
            .SetMaximizable(false)
            .Load("wwwroot/toolbar.html");

        // Browser Window (Content)
        var browser = new PhotinoWindow()
            .SetTitle("TB Browser")
            .SetSize(900, 600)
            .SetUseOsDefaultLocation(false)
            .SetLeft(200)
            .SetTop(250)
            .Load("https://www.bing.com");

        // Link: Toolbar → Browser
        toolbar.RegisterWebMessageReceivedHandler((sender, message) =>
        {
            browser.Load(message);
        });

        // Keep app running
        toolbar.WaitForClose();
    }
}
