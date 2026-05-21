using Photino.NET;
using System;
using System.Drawing;

namespace TbBrowser
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            var window = new PhotinoWindow()
                .SetTitle("TB Browser")
                .SetUseOsDefaultSize(false)
                .SetSize(new Size(900, 700)) // Total height: 50 (toolbar) + 650 (browser)
                .SetUseOsDefaultLocation(false)
                .SetLeft(200)
                .SetTop(200)
                .SetResizable(true)
                .Load("wwwroot/index.html"); // Single HTML file with split layout

            window.WaitForClose();
        }
    }
}
