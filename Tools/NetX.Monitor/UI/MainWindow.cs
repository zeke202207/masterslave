using NetX.MasterSDK;
using System.Reflection;

namespace NetX.Monitor;

public class MainWindow : BaseWindow
{
    private IEnumerable<WorkerNode> _nodes;
    private FrameView LeftPane;
    private FrameView RightPane;
    private ListView NodeListView;
    private CommunicationService _communication;
    private NodeInfoView _nodeInfoView;

    public MainWindow(CommunicationService communication)
        : base("")
    {
        _communication = communication;
    }

    protected override void Setup()
    {
        //1. 创建菜单栏
        StatusBar = new StatusBar()
        {
            Visible = true,
        };
        StatusBar.Items = new StatusItem[] {
                new StatusItem(Key.CharMask, $"Version:{Assembly.GetExecutingAssembly()?.GetName()?.Version.ToString()}", null)
            };

        Add(StatusBar);

        //2. LayOut
        LeftPane = new FrameView("Nodes")
        {
            X = 0,
            Y = 1, // for menu
            Width = 30,
            Height = Dim.Fill(1),
            CanFocus = true,
            Shortcut = Key.F5
        };
        LeftPane.Title = $"{LeftPane.Title} ({LeftPane.ShortcutTag})";
        LeftPane.ShortcutAction = async () => await RefreshNodes();

        NodeListView = new ListView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(0),
            Height = Dim.Fill(0),
            AllowsMarking = false,
            CanFocus = true,
        };
        NodeListView.OpenSelectedItem += (a) =>
        {
            RightPane.SetFocus();
        };
        NodeListView.SelectedItemChanged += NodeListView_SelectedChanged;
        LeftPane.Add(NodeListView);

        RightPane = new FrameView("")
        {
            X = 30,
            Y = 1, // for menu
            Width = Dim.Fill(),
            Height = Dim.Fill(1),
            CanFocus = true
        };
        RightPane.ShortcutAction = () => RightPane.SetFocus();

        Add(LeftPane);
        Add(RightPane);
        Task.Run(async () => await RefreshNodes());
    }

    /// <summary>
    /// 刷新Node节点
    /// </summary>
    /// <returns></returns>
    private async Task RefreshNodes()
    {
        try
        {
            LeftPane.SetFocus();
            await LoadWorkNodes();
            await NodeListView.SetSourceAsync(_nodes.Select(p => p).ToList());
            if (_nodes.Count() > 0)
                NodeListView.SelectedItem = 0;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    /// <summary>
    /// 加载节点数据
    /// </summary>
    /// <returns></returns>
    private async Task LoadWorkNodes()
    {
        try
        {
            _nodes = await _communication.GetWorkersAsync();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    private void NodeListView_SelectedChanged(ListViewItemEventArgs args)
    {
        try
        {
            RightPane.RemoveAll();
            if (null != _nodeInfoView)
                _nodeInfoView.Dispose();
            _nodeInfoView = new NodeInfoView(RightPane.Bounds, args.Value as WorkerNode, _communication);
            RightPane.Add(_nodeInfoView);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
