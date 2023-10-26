namespace NetX.Monitor;

public class LoginWindow : BaseWindow
{
    public Func<ConnectionModel, Task<bool>> OnConnect { get; set; }
    public Action<ConnectionModel> OnLogin { get; set; }
    public Action OnExit { get; set; }

    public LoginWindow() : base("Login")
    {

    }

    protected override void Setup()
    {
        var nameLabel = new Label(0, 1, "Master Ip");
        var nameText = new TextField("")
        {
            X = Pos.Left(nameLabel),
            Y = Pos.Top(nameLabel) + 1,
            Width = Dim.Fill(),
        };

        Add(nameLabel);
        Add(nameText);

        var passwordLabel = new Label("Master Port")
        {
            X = Pos.Left(nameText),
            Y = Pos.Top(nameText) + 1,
            Width = Dim.Fill()
        };

        var passwordText = new TextField("")
        {
            X = Pos.Left(passwordLabel),
            Y = Pos.Top(passwordLabel) + 1,
            Width = Dim.Fill(),
            Secret = true
        };

        Add(passwordLabel);
        Add(passwordText);

        var loginButton = new Button("Connect", true)
        {
            Y = Pos.Top(passwordText) + 2,
            X = Pos.Center() - 15
        };

        var exitButton = new Button("Exit")
        {
            Y = Pos.Top(loginButton),
            X = Pos.Center() + 5
        };

        Add(loginButton);
        Add(exitButton);

        var progressBar = new ProgressBar()
        {
            Y = 12,
            X = Pos.Center(),
            Width = 20
        };
        bool Timer(MainLoop caller)
        {
            progressBar.Pulse();
            return true;
        }

        loginButton.Clicked += async () =>
        {
            try
            {
                if (nameText.Text.ToString().TrimStart().Length == 0)
                {
                    MessageBox.ErrorQuery(24, 8, "Error", "Ip cannot be empty.", "Ok");
                    return;
                }
                if (string.IsNullOrEmpty(passwordText.Text.ToString()))
                {
                    MessageBox.ErrorQuery(24, 8, "Error", "Port cannot be empty.", "Ok");
                    return;
                }
                Add(progressBar);
                var x = Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(300), Timer);

                var connectionModel = new ConnectionModel(ip: nameText.Text.ToString(), port: passwordText.Text.ToString());

                var result = await OnConnect.Invoke(connectionModel);

                Application.MainLoop.RemoveTimeout(x);
                Remove(progressBar);
                if (!result)
                    MessageBox.ErrorQuery(24, 8, "Error", "连接失败", "Ok");
                else
                    OnLogin?.Invoke(connectionModel);
            }
            catch (Exception ex)
            {
                throw;
            }
        };

        exitButton.Clicked += () =>
        {
            try
            {
                OnExit?.Invoke();
            }
            catch (Exception ex)
            {

                throw;
            }
        };
    }
}
