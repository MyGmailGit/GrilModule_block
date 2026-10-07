// RedDotDefine.cs
using System.Collections.Generic;

namespace Game.RedDot
{

    public static class RedDotDefine
    {
        public enum Node
        {
            Root = 0,
            // 一级节点
            Daily = 1,
            Treasure = 2,
            // Mail = 1,
            // Task = 2,
            // Activity = 3,
            // Shop = 4,

            // // 二级节点 - 邮件
            // Mail_System = 101,
            // Mail_Friend = 102,
            // Mail_Guild = 103,

            // // 二级节点 - 任务
            // Task_Daily = 201,
            // Task_Achieve = 202,

            // // 三级节点 - 每日任务
            // Task_Daily_Kill = 2011,
            // Task_Daily_Login = 2012,
        }

        // 父子关系配置
        public static readonly Dictionary<Node, Node> ParentMap = new Dictionary<Node, Node>
        {
            {Node.Daily, Node.Root},
            {Node.Treasure, Node.Root},
            // {Node.Mail_System, Node.Mail},
            // {Node.Mail_Friend, Node.Mail},
            // {Node.Mail_Guild, Node.Mail},
            // {Node.Task_Daily, Node.Task},
            // {Node.Task_Achieve, Node.Task},
            // {Node.Task_Daily_Kill, Node.Task_Daily},
            // {Node.Task_Daily_Login, Node.Task_Daily},
        };
    }
}