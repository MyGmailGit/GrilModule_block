// RedDotNode.cs
using System;
using System.Collections.Generic;
namespace Game.RedDot
{

    public class RedDotNode
    {
        public RedDotDefine.Node NodeId { get; private set; }
        public int RedCount { get; private set; }
        public bool IsActive => RedCount > 0;

        private RedDotNode parent;
        private List<RedDotNode> children = new List<RedDotNode>();
        private List<Action<bool, int>> listeners = new List<Action<bool, int>>();

        public RedDotNode(RedDotDefine.Node nodeId)
        {
            NodeId = nodeId;
        }

        public void SetParent(RedDotNode parentNode)
        {
            parent = parentNode;
        }

        public void AddChild(RedDotNode child)
        {
            children.Add(child);
        }

        public void AddListener(Action<bool, int> listener)
        {
            if (!listeners.Contains(listener))
                listeners.Add(listener);
            // 立即回调当前状态
            listener?.Invoke(IsActive, RedCount);
        }

        public void RemoveListener(Action<bool, int> listener)
        {
            listeners.Remove(listener);
        }

        public void SetRedCount(int count)
        {
            if (RedCount == count) return;

            RedCount = count > 0 ? count : 0;
            NotifyListeners();

            // 通知父节点刷新
            parent?.RefreshFromChild();
        }

        public void AddRedCount(int delta)
        {
            SetRedCount(RedCount + delta);
        }

        private void RefreshFromChild()
        {
            int totalCount = 0;
            foreach (var child in children)
            {
                totalCount += child.RedCount;
            }

            if (RedCount != totalCount)
            {
                RedCount = totalCount;
                NotifyListeners();
                parent?.RefreshFromChild();
            }
        }

        private void NotifyListeners()
        {
            foreach (var listener in listeners)
            {
                listener?.Invoke(IsActive, RedCount);
            }
        }

        public void ClearAllListeners()
        {
            listeners.Clear();
        }
    }
}