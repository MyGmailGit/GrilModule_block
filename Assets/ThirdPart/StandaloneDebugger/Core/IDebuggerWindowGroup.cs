#if TEST_MODE
namespace StandaloneDebugger
{
    /// <summary>
    /// 调试器窗口组接口。
    /// </summary>
    public interface IDebuggerWindowGroup : IDebuggerWindow
    {
        /// <summary>
        /// 获取调试器窗口数量。
        /// </summary>
        int DebuggerWindowCount { get; }

        /// <summary>
        /// 获取或设置当前选中的调试器窗口索引。
        /// </summary>
        int SelectedIndex { get; set; }

        /// <summary>
        /// 获取当前选中的调试器窗口。
        /// </summary>
        IDebuggerWindow SelectedWindow { get; }

        /// <summary>
        /// 获取调试组的调试器窗口名称集合。
        /// </summary>
        string[] GetDebuggerWindowNames();

        /// <summary>
        /// 获取调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>要获取的调试器窗口。</returns>
        IDebuggerWindow GetDebuggerWindow(string path);

        /// <summary>
        /// 注册调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <param name="debuggerWindow">要注册的调试器窗口。</param>
        void RegisterDebuggerWindow(string path, IDebuggerWindow debuggerWindow);

        /// <summary>
        /// 解除注册调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>是否解除注册调试器窗口成功。</returns>
        bool UnregisterDebuggerWindow(string path);

        /// <summary>
        /// 选中调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>是否成功选中调试器窗口。</returns>
        bool SelectDebuggerWindow(string path);
    }
}
#endif