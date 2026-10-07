// RedDotSystem.cs
using System.Collections.Generic;
using GameLogic;
using UnityEngine;
namespace Game.RedDot
{
    public class RedDotSystem : Singleton<RedDotSystem>
    {
        // private static RedDotSystem instance;
        // public static RedDotSystem Instance => instance;

        private Dictionary<RedDotDefine.Node, RedDotNode> nodeDict = new Dictionary<RedDotDefine.Node, RedDotNode>();

        // void Awake()
        // {
        //     if (instance != null && instance != this)
        //     {
        //         Destroy(gameObject);
        //         return;
        //     }
        //     instance = this;
        //     DontDestroyOnLoad(gameObject);
        //     BuildTree();
        // }

        protected override void OnInit()
        {
            base.OnInit();
            BuildTree();
        }

        private void BuildTree()
        {
            // 创建根节点
            GetOrCreateNode(RedDotDefine.Node.Root);

            // 创建所有关联的节点
            foreach (var pair in RedDotDefine.ParentMap)
            {
                var childNode = GetOrCreateNode(pair.Key);
                var parentNode = GetOrCreateNode(pair.Value);
                childNode.SetParent(parentNode);
                parentNode.AddChild(childNode);
            }
        }

        private RedDotNode GetOrCreateNode(RedDotDefine.Node nodeId)
        {
            if (!nodeDict.ContainsKey(nodeId))
            {
                nodeDict[nodeId] = new RedDotNode(nodeId);
            }
            return nodeDict[nodeId];
        }

        public RedDotNode GetNode(RedDotDefine.Node nodeId)
        {
            nodeDict.TryGetValue(nodeId, out var node);
            return node;
        }

        public void SetRedCount(RedDotDefine.Node nodeId, int count)
        {
            var node = GetNode(nodeId);
            if (node != null)
            {
                node.SetRedCount(count);
            }
            else
            {
                Debug.LogError($"Node {nodeId} not found!");
            }
        }

        public void AddRedCount(RedDotDefine.Node nodeId, int delta)
        {
            var node = GetNode(nodeId);
            node?.AddRedCount(delta);
        }

        public void ClearNode(RedDotDefine.Node nodeId)
        {
            SetRedCount(nodeId, 0);
        }

        public void RegisterListener(RedDotDefine.Node nodeId, System.Action<bool, int> listener)
        {
            var node = GetNode(nodeId);
            node?.AddListener(listener);
        }

        public void UnregisterListener(RedDotDefine.Node nodeId, System.Action<bool, int> listener)
        {
            var node = GetNode(nodeId);
            node?.RemoveListener(listener);
        }
    }
}