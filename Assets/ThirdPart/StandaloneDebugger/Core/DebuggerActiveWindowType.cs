#if TEST_MODE
namespace StandaloneDebugger
{
    /// <summary>
    /// 调试器激活窗口类型。
    /// </summary>
    public enum DebuggerActiveWindowType
    {
        /// <summary>
        /// 调试器窗口始终打开。
        /// </summary>
        AlwaysOpen,

        /// <summary>
        /// 调试器窗口仅在开发环境打开。
        /// </summary>
        OnlyOpenWhenDevelopment,

        /// <summary>
        /// 调试器窗口仅在编辑器打开。
        /// </summary>
        OnlyOpenInEditor
    }
}
#endif