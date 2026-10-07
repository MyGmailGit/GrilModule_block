#if TEST_MODE
using UnityEngine;

namespace StandaloneDebugger
{
    public sealed partial class Debugger
    {
        public abstract class ScrollableDebuggerWindowBase : IDebuggerWindow
        {
            private const float TITLE_WIDTH = 240f;
            private Vector2 _scrollPosition = Vector2.zero;

            public virtual void Initialize(params object[] args)
            {
            }

            public virtual void Shutdown()
            {
            }

            public virtual void OnEnter()
            {
            }

            public virtual void OnLeave()
            {
            }

            public virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
            {
            }

            public void OnDraw()
            {
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
                {
                    OnDrawScrollableWindow();
                }
                GUILayout.EndScrollView();
            }

            protected abstract void OnDrawScrollableWindow();

            protected static void DrawItem(string title, string content)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(title, GUILayout.Width(TITLE_WIDTH));
                    if (GUILayout.Button(content, "label"))
                    {
                        CopyToClipboard(content);
                    }
                }
                GUILayout.EndHorizontal();
            }

            protected static string GetByteLengthString(long byteLength)
            {
                if (byteLength < 1024L) // 2 ^ 10
                {
                    return $"{byteLength} Bytes";
                }

                if (byteLength < 1048576L) // 2 ^ 20
                {
                    return $"{byteLength / 1024f:F2} KB";
                }

                if (byteLength < 1073741824L) // 2 ^ 30
                {
                    return $"{byteLength / 1048576f:F2} MB";
                }

                if (byteLength < 1099511627776L) // 2 ^ 40
                {
                    return $"{byteLength / 1073741824f:F2} GB";
                }

                if (byteLength < 1125899906842624L) // 2 ^ 50
                {
                    return $"{byteLength / 1099511627776f:F2} TB";
                }

                if (byteLength < 1152921504606846976L) // 2 ^ 60
                {
                    return $"{byteLength / 1125899906842624f:F2} PB";
                }

                return $"{byteLength / 1152921504606846976f:F2} EB";
            }
        }
    }
}
#endif