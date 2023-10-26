namespace NetX.Monitor;

internal class TerminalOrchestrator
{
    private Func<Task> running;
    private CommunicationService _communication;

    public TerminalOrchestrator(CommunicationService communicationService)
    {
        running = ShowLoginWindow;
        _communication = communicationService;
    }

    public async Task Run()
    {
        Application.Init();
        Colors.Base.Normal = Application.Driver.MakeAttribute(Color.BrightGreen, Color.Black);
        Console.OutputEncoding = System.Text.Encoding.Default;
        while (running is not null)
        {
            await running.Invoke();
        }
        Application.Shutdown();
    }

    private Task ShowLoginWindow()
    {
        var top = Application.Top;
        var win = new LoginWindow
        {
            OnConnect = async (connectModel) => await _communication.ConnectToMaster(connectModel),
            OnLogin = (connectModel) =>
            {
                Application.MainLoop.Invoke(() =>
                {
                    running = ShowMainWindow;
                    Application.RequestStop();
                });
            },

            OnExit = () =>
            {
                running = null;
                Application.RequestStop();
            },

            OnQuit = () =>
            {
                running = null;
                Application.RequestStop();
            },
        };

        top.Add(win);
        top.Add(win.CreateMenuBar());
        Application.Run();
        return Task.CompletedTask;
    }

    private Task ShowMainWindow()
    {
        var top = Application.Top;
        Application.MainLoop.Invoke(() =>
        {
            var win = new MainWindow(_communication)
            {
                OnQuit = () =>
                {
                    running = null;
                    Application.RequestStop();
                }
            };
            top.Add(win);
            top.Add(win.CreateMenuBar());
        });
        Application.Run();
        return Task.CompletedTask;
    }
}
