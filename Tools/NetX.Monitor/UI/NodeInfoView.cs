using NetX.MasterSDK;
using System.Data;
using System.Reflection;

namespace NetX.Monitor;

internal class NodeInfoView : View
{
    private WorkerNode _node;
    private FrameView _systemInfoPanel;
    private FrameView _diskInfoPanel;
    private FrameView _jobInfoPanel;
    private TableView _jobTrackerView;
    private TableView _diskView;
    private CommunicationService _communicationService1;

    internal NodeInfoView(Rect bound, WorkerNode node, CommunicationService communicationService)
        : base(bound)
    {
        _node = node;
        _communicationService1 = communicationService;
        LayoutPanel();
        Task.Run(async () => await GetDetails(_node.Id));
        Task.Run(async () => await JobTracker(_node.Id));
    }

    /// <summary>
    /// 布局设置
    /// </summary>
    private void LayoutPanel()
    {
        var width = base.Bounds.Width / 2 - 1;
        var height = base.Bounds.Height / 2 - 1;

        _systemInfoPanel = new FrameView($"System Info")
        {
            X = 0,
            Y = 1,
            Width = width,
            Height = height,
            Shortcut = Key.CtrlMask | Key.F5
        };
        _systemInfoPanel.Title = $"System Info({_systemInfoPanel.ShortcutTag})";
        _systemInfoPanel.ShortcutAction = async () => await GetDetails(_node.Id);

        _diskInfoPanel = new FrameView("Disk Info")
        {
            X = Pos.Right(_systemInfoPanel),
            Y = 1,
            Width = width,
            Height = height,
        };

        _diskView = new TableView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            FullRowSelect = true,
            CanFocus = false
        };
        _diskInfoPanel.Add(_diskView);

        _jobInfoPanel = new FrameView("Job Tracker")
        {
            X = 0,
            Y = Pos.Bottom(_systemInfoPanel),
            Width = Dim.Fill() - 2,
            Height = Dim.Fill() - 2,
            Shortcut = Key.F6
        };
        _jobInfoPanel.Title = $"Job Tracker({_jobInfoPanel.ShortcutTag})";
        _jobInfoPanel.ShortcutAction = async () => await JobTracker(_node.Id);

        _jobTrackerView = new TableView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            FullRowSelect = true,
            CanFocus = false,
            WantContinuousButtonPressed = true,
            VerticalTextAlignment = VerticalTextAlignment.Middle,
        };

        _jobInfoPanel.Add(_jobTrackerView);

        Add(_systemInfoPanel, _diskInfoPanel, _jobInfoPanel);

    }

    /// <summary>
    /// 获取节点信息
    /// </summary>
    /// <returns></returns>
    private async Task GetDetails(string nodeId)
    {
        try
        {
            var nodeInfo = await _communicationService1.GetWorkerNodeInfoAsync(nodeId);
            PrintSystemInfo(nodeInfo.Platform, nodeInfo.Cpu, nodeInfo.Memory);
            PrintDiskInfo(nodeInfo.Disks);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    /// <summary>
    /// 系统信息
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private void PrintSystemInfo(PlatformInfo platform, CpuInfo cpu, MemoryInfo memory)
    {
        var y = Pos.Y(_systemInfoPanel) + 2;
        int i = 0;

        var properties = platform.GetType().GetProperties()
            .Where(p => null != p.GetCustomAttribute<DisplayAtribute>())
            .OrderBy(p => p.GetCustomAttribute<DisplayAtribute>().Order);
        foreach (PropertyInfo property in properties)
        {
            var propertyDescriptionAttribute = property.GetCustomAttribute<DisplayAtribute>();
            object propertyValue = property.GetValue(platform);
            var lblKey = new Label($"{propertyDescriptionAttribute.DescriptionName}")
            {
                X = _systemInfoPanel.X + 2,
                Y = y + i
            };
            var lblValue = new Label($"{propertyValue}")
            {
                X = Pos.Right(lblKey) + 1,
                Y = y + i
            };
            Add(lblKey, lblValue);
            i += 2;
        }
        ////CPU
        //var lblCpuKey = new Label($"CPU:")
        //{
        //    X = _systemInfoPanel.X + 2,
        //    Y = y + i
        //};
        //var lblCpuValue = new Label($"{cpu.CPULoad}")
        //{
        //    X = Pos.Right(lblCpuKey) + 1,
        //    Y = y + i
        //};
        //Add(lblCpuKey, lblCpuValue);
        //i += 2;
        //Memory
        var lblMemoryKey = new Label($"      内存:")
        {
            X = _systemInfoPanel.X + 2,
            Y = y + i
        };
        var lblMemoryValue = new Label($"{memory.ToString()}")
        {
            X = Pos.Right(lblMemoryKey) + 1,
            Y = y + i
        };
        Add(lblMemoryKey, lblMemoryValue);
        i += 2;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    private void PrintDiskInfo(List<DiskInfo> info)
    {
        try
        {
            _diskView.Table = info.ToDataTable();
            _diskView.Update();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    /// <summary>
    /// 任务状态跟踪
    /// </summary>
    /// <param name="nodeId">节点唯一标识</param>
    /// <returns></returns>
    private async Task JobTracker(string nodeId)
    {
        try
        {
            var jobDetails = await _communicationService1.GetJobTracker(nodeId);
            _jobTrackerView.Table = jobDetails.ToDataTable();
            _jobTrackerView.Update();
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
