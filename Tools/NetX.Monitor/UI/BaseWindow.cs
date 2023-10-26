using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NetX.Monitor;

public abstract class BaseWindow : Window
{
    public Action OnQuit { get; set; }

    public BaseWindow(string title)
        : base(title)
    {
        X = Pos.Center();
        Y = 1;
        Width = Dim.Fill();
        Height = Dim.Fill();
        Setup();
    }

    public MenuBar CreateMenuBar()
    {
        return new MenuBar(new MenuBarItem[]
        {
                new MenuBarItem("App", new MenuItem []
                {
                    new MenuItem("Quit", "Quit App", () => OnQuit?.Invoke(), null, null, Key.Q | Key.CtrlMask)
                }),
                new MenuBarItem ("_Help", new MenuItem [] {
                    new MenuItem ("_master slave", "", () => OpenUrl ("https://github.com/zeke202207"), null, null, Key.F1),
                    new MenuItem ("_About...",
                        "About", () =>  MessageBox.Query ("About UI", "Application created by zeke", "_Ok"), null, null, Key.CtrlMask | Key.A),
                })
        });
    }

    private void OpenUrl(string url)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                using (var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "xdg-open",
                        Arguments = url,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    }
                })
                {
                    process.Start();
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }
        catch
        {
            throw;
        }
    }

    protected abstract void Setup();
}
