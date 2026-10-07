using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using Watermelon;
namespace ball
{
    #region MoveDataPool
    public class MoveData
    {
        public Vector3 pos;
        public int orderLayer = 0;
    }
    public static class MoveDataPool
    {
        private static readonly Stack<MoveData> moveDatasPool = new();
        public static MoveData GetMoveData()
        {
            if (moveDatasPool.Count > 0)
            {
                return moveDatasPool.Pop();
            }
            return new MoveData();
        }

        public static void PutBackMoveData(MoveData data)
        {
            if (data != null)
            {
                moveDatasPool.Push(data);
            }
        }
    }



    #endregion


    public class BallMove : MonoBehaviour
    {
        [Header("移动速度(单位/秒)")]
        float moveSpeed = 4.5f;

        [Header("最高抛物线高度")]
        float arcHeight = 0.7f;

        [Header("认为X相等的误差")]
        float xTolerance = 0.01f;

        [Header("移动速度")]

        [Header("最短移动时间")]
        private float minDuration = 0.10f;

        [Header("最长移动时间")]
        private float maxDuration = 0.4f;

        [Header("距离加速系数")]
        private float speedScale = 9.8f;


        private readonly Queue<MoveData> _points = new();

        private Coroutine _moveRoutine;

        private bool _moving;
        private BallController _ballController;

        public Action startMovingAction;
        public Action endMovingAction;

        #region Public
        public void SetBallController(BallController ballControllerArg)
        {
            this._ballController = ballControllerArg;
        }
        /// <summary>
        /// 清空当前路径，开始新的移动
        /// </summary>
        public void MoveTo(Vector3 pos, int orderLayer = 0)
        {
            _points.Clear();
            var data = MoveDataPool.GetMoveData();
            data.pos = pos;
            data.orderLayer = orderLayer;
            _points.Enqueue(data);

            RestartMove();
        }

        // /// <summary>
        // /// 清空当前路径，开始新的移动
        // /// </summary>
        // public void MoveTo(IEnumerable<Vector3> positions)
        // {
        //     _points.Clear();

        //     foreach (var p in positions)
        //     {
        //         var data = MoveDataPool.GetMoveData();
        //         data.pos = p;
        //         data.orderLayer = 0;
        //         _points.Enqueue(data);
        //     }

        //     RestartMove();
        // }

        /// <summary>
        /// 添加一个目标
        /// </summary>
        public void AddPos(Vector3 pos, int orderLayerArg = 0, bool shouldPlayaudio = false)
        {
            var data = MoveDataPool.GetMoveData();
            data.pos = pos;
            data.orderLayer = orderLayerArg;

            _points.Enqueue(data);

            if (!_moving)
                _moveRoutine = StartCoroutine(MoveLoop());

            if (shouldPlayaudio)
            {
                AudioController.PlaySound(AudioController.AudioClips.slide);
            }
        }

        /// <summary>
        /// 添加多个目标
        /// </summary>
        // public void AddPos(IEnumerable<Vector3> positions)
        // {
        //     foreach (var p in positions)
        //         _points.Enqueue(p);

        //     if (!_moving)
        //         _moveRoutine = StartCoroutine(MoveLoop());
        // }

        #endregion
        private float CalculateDuration(Vector3 start, Vector3 target)
        {
            float distance = Vector3.Distance(start, target);

            // 距离越远，速度越快
            float speed = Mathf.Lerp(
                moveSpeed,
                moveSpeed * speedScale,
                Mathf.Clamp01(distance / 6f));

            float duration = distance / speed;

            // 限制时间范围
            duration = Mathf.Clamp(duration, minDuration, maxDuration);

            return duration;
        }
        private void RestartMove()
        {
            if (_moveRoutine != null)
                StopCoroutine(_moveRoutine);

            _moveRoutine = StartCoroutine(MoveLoop());
        }

        IEnumerator MoveLoop()
        {
            _moving = true;

            startMovingAction?.Invoke();

            while (_points.Count > 0)
            {
                var target = _points.Dequeue();
                Vector3 targetPos = target.pos;
                int orderLayer = target.orderLayer;

                MoveDataPool.PutBackMoveData(target);

                if (Mathf.Abs(targetPos.x - transform.position.x) < xTolerance)
                {
                    yield return MoveLine(targetPos, orderLayer);
                }
                else
                {
                    yield return MoveArc(targetPos, orderLayer);
                }
            }

            AudioController.PlaySound(AudioController.AudioClips.ballDrop);

            _moving = false;

            endMovingAction?.Invoke();
        }

        IEnumerator MoveLine(Vector3 target, int orderLayer)
        {
            Vector3 start = transform.position;

            float distance = Vector3.Distance(start, target);

            if (distance < 0.001f)
                yield break;

            _ballController.SetRendererOrderLayer(orderLayer);

            // float duration = distance / moveSpeed;
            float duration = CalculateDuration(start, target);

            float t = 0;

            while (t < 1)
            {
                t += Time.deltaTime / duration;

                transform.position = Vector3.Lerp(start, target, t);

                yield return null;
            }

            transform.position = target;
            _ballController.SetRendererOrderLayer(0);
        }

        IEnumerator MoveArc(Vector3 target, int orderLayer)
        {
            Vector3 start = transform.position;

            float distance = Vector3.Distance(start, target);

            if (distance < 0.001f)
                yield break;

            _ballController.SetRendererOrderLayer(orderLayer);

            // float duration = distance / moveSpeed;
            float duration = CalculateDuration(start, target);

            float t = 0;

            while (t < 1)
            {
                t += Time.deltaTime / duration;

                // Vector3 pos = Vector3.Lerp(start, target, t);

                // float height = Mathf.Sin(t * Mathf.PI) * arcHeight;

                // pos.y += height;
                float curveT = Mathf.SmoothStep(0, 1, t);

                Vector3 pos = Vector3.Lerp(start, target, curveT);

                float height = Mathf.Sin(curveT * Mathf.PI) * arcHeight;

                pos.y += height;

                transform.position = pos;

                yield return null;
            }

            transform.position = target;

            _ballController.SetRendererOrderLayer(0);
        }
    }

    // [Header("移动参数")]
    // private float speed = 3.2f;
    // private float arcHeight = 1f;

    // // 当前路径
    // private readonly List<Vector3> path = new List<Vector3>();

    // // 当前段索引
    // private int currentIndex = 0;

    // private bool isMoving = false;

    // private Vector3 segmentStart;
    // private Vector3 segmentEnd;

    // private float elapsedTime;
    // private float segmentDuration;

    // public Action startMovingAction;
    // public Action endMovingAction;

    // private void FixedUpdate()
    // {
    //     if (!isMoving)
    //         return;

    //     elapsedTime += Time.fixedDeltaTime;

    //     float t = Mathf.Clamp01(elapsedTime / segmentDuration);

    //     // X相同走直线，否则走抛物线
    //     if (Mathf.Approximately(segmentStart.x, segmentEnd.x))
    //     {
    //         transform.position = LinearMove(segmentStart, segmentEnd, t);
    //     }
    //     else
    //     {
    //         transform.position = ParabolicMove(segmentStart, segmentEnd, t);
    //     }

    //     if (t >= 1f)
    //     {
    //         transform.position = segmentEnd;

    //         currentIndex++;

    //         // 还有下一段
    //         if (currentIndex < path.Count - 1)
    //         {
    //             StartSegment(currentIndex);
    //         }
    //         else
    //         {
    //             // 整条路径结束
    //             isMoving = false;

    //             // 保留最后一个点作为新的起点
    //             Vector3 last = path[path.Count - 1];
    //             path.Clear();
    //             path.Add(last);

    //             OnPathComplete();
    //         }
    //     }
    // }

    // /// <summary>
    // /// 添加一个目标点
    // /// </summary>
    // public void AddPath(Vector3 point)
    // {
    //     // 第一次添加
    //     if (path.Count == 0)
    //     {
    //         path.Add(transform.position);
    //         startMovingAction?.Invoke();
    //     }

    //     path.Add(point);

    //     // 当前没有移动，立即开始
    //     if (!isMoving && path.Count >= 2)
    //     {
    //         currentIndex = 0;
    //         StartSegment(currentIndex);
    //         isMoving = true;
    //     }
    // }

    // /// <summary>
    // /// 放弃当前路径，立即开始新的移动
    // /// </summary>
    // public void ForceNewPath(Vector3 point)
    // {
    //     startMovingAction?.Invoke();

    //     path.Clear();

    //     path.Add(transform.position);
    //     path.Add(point);

    //     currentIndex = 0;

    //     StartSegment(currentIndex);

    //     isMoving = true;
    // }

    // /// <summary>
    // /// 清空路径
    // /// </summary>
    // public void ClearAllPaths()
    // {
    //     isMoving = false;

    //     path.Clear();

    //     Debug.Log("已清空路径");
    // }

    // /// <summary>
    // /// 开始某一段移动
    // /// </summary>
    // private void StartSegment(int index)
    // {
    //     if (index >= path.Count - 1)
    //     {
    //         isMoving = false;
    //         return;
    //     }

    //     segmentStart = path[index];
    //     segmentEnd = path[index + 1];

    //     float distance = Vector3.Distance(segmentStart, segmentEnd);

    //     segmentDuration = distance / speed;

    //     if (segmentDuration < 0.001f)
    //         segmentDuration = 0.001f;

    //     elapsedTime = 0f;

    //     transform.position = segmentStart;
    // }

    // /// <summary>
    // /// 直线
    // /// </summary>
    // private Vector3 LinearMove(Vector3 start, Vector3 end, float t)
    // {
    //     return Vector3.Lerp(start, end, t);
    // }

    // /// <summary>
    // /// 抛物线
    // /// </summary>
    // private Vector3 ParabolicMove(Vector3 start, Vector3 end, float t)
    // {
    //     Vector3 pos = Vector3.Lerp(start, end, t);

    //     pos.y += arcHeight * 4f * t * (1f - t);

    //     return pos;
    // }

    // /// <summary>
    // /// 修改速度
    // /// </summary>
    // public void SetSpeed(float newSpeed)
    // {
    //     if (newSpeed <= 0)
    //         return;

    //     float progress = 0f;

    //     if (isMoving)
    //     {
    //         progress = elapsedTime / segmentDuration;
    //     }

    //     speed = newSpeed;

    //     if (isMoving)
    //     {
    //         float distance = Vector3.Distance(segmentStart, segmentEnd);

    //         segmentDuration = Mathf.Max(distance / speed, 0.001f);

    //         elapsedTime = progress * segmentDuration;
    //     }
    // }

    // /// <summary>
    // /// 路径完成
    // /// </summary>
    // private void OnPathComplete()
    // {
    //     Debug.Log("路径执行完成");
    //     endMovingAction?.Invoke();
    // }

    // [Header("移动参数")]
    // public float speed = 5f;                  // 移动速度（单位/秒）
    // public float arcHeight = 3f;              // 抛物线高度

    // // 路径队列
    // private Queue<List<Vector3>> pathQueue = new Queue<List<Vector3>>();

    // // 当前正在执行的路径
    // private List<Vector3> currentPath = null;
    // private int currentIndex = 0;
    // private float elapsedTime = 0f;
    // private bool isMoving = false;

    // // 起点和终点（用于当前段）
    // private Vector3 segmentStart;
    // private Vector3 segmentEnd;
    // private float segmentDuration;            // 根据距离和速度计算

    // void FixedUpdate()
    // {
    //     if (!isMoving || currentPath == null) return;

    //     // 更新当前段的移动
    //     if (elapsedTime < segmentDuration)
    //     {
    //         elapsedTime += Time.fixedDeltaTime;
    //         float t = elapsedTime / segmentDuration;

    //         // 判断当前段是直线还是抛物线
    //         if (Mathf.Approximately(segmentStart.x, segmentEnd.x))
    //         {
    //             // X相等 → 直线移动（只移动Y和Z）
    //             transform.position = LinearMove(segmentStart, segmentEnd, t);
    //         }
    //         else
    //         {
    //             // X不等 → 抛物线移动
    //             transform.position = ParabolicMove(segmentStart, segmentEnd, t);
    //         }
    //     }
    //     else
    //     {
    //         // 当前段完成，移动到下一段
    //         transform.position = segmentEnd;
    //         currentIndex++;

    //         if (currentIndex < currentPath.Count)
    //         {
    //             // 还有下一段，继续
    //             StartSegment(currentIndex);
    //         }
    //         else
    //         {
    //             // 整个路径完成
    //             isMoving = false;
    //             currentPath = null;
    //             OnPathComplete();

    //             // 检查队列中是否还有待执行的路径
    //             if (pathQueue.Count > 0)
    //             {
    //                 ExecuteNextPath();
    //             }
    //         }
    //     }
    // }

    // /// <summary>
    // /// 函数1：添加路径到队列
    // /// </summary>
    // public void AddPath(List<Vector3> path)
    // {
    //     if (path == null || path.Count < 2)
    //     {
    //         Debug.LogWarning("路径至少需要2个点");
    //         return;
    //     }

    //     // 如果当前没有移动，立即执行
    //     if (!isMoving)
    //     {
    //         ExecutePath(path);
    //     }
    //     else
    //     {
    //         // 否则加入队列
    //         pathQueue.Enqueue(path);
    //         Debug.Log($"路径已加入队列，当前队列长度：{pathQueue.Count}");
    //     }
    // }

    // /// <summary>
    // /// 函数2：立即放弃当前移动，开始新路径
    // /// </summary>
    // public void ForceNewPath(List<Vector3> path)
    // {
    //     if (path == null || path.Count < 2)
    //     {
    //         Debug.LogWarning("路径至少需要2个点");
    //         return;
    //     }

    //     // 清空队列
    //     pathQueue.Clear();

    //     // 放弃当前移动
    //     isMoving = false;
    //     currentPath = null;

    //     // 立即执行新路径
    //     ExecutePath(path);

    //     Debug.Log("已放弃之前的移动，开始新路径");
    // }

    // /// <summary>
    // /// 执行路径（从队列中取出并执行）
    // /// </summary>
    // private void ExecuteNextPath()
    // {
    //     if (pathQueue.Count > 0)
    //     {
    //         List<Vector3> nextPath = pathQueue.Dequeue();
    //         ExecutePath(nextPath);
    //     }
    // }

    // /// <summary>
    // /// 执行路径
    // /// </summary>
    // private void ExecutePath(List<Vector3> path)
    // {
    //     currentPath = path;
    //     currentIndex = 0;
    //     StartSegment(0);
    //     isMoving = true;
    // }

    // /// <summary>
    // /// 开始执行路径中的某一段
    // /// </summary>
    // private void StartSegment(int index)
    // {
    //     if (currentPath == null || index >= currentPath.Count - 1)
    //     {
    //         isMoving = false;
    //         return;
    //     }

    //     segmentStart = currentPath[index];
    //     segmentEnd = currentPath[index + 1];

    //     // 根据距离和速度计算持续时间
    //     float distance = Vector3.Distance(segmentStart, segmentEnd);
    //     segmentDuration = distance / speed;

    //     // 防止除零错误（如果距离为0，设置一个极小值）
    //     if (segmentDuration < 0.001f)
    //     {
    //         segmentDuration = 0.001f;
    //     }

    //     elapsedTime = 0f;

    //     // 移动到起点
    //     transform.position = segmentStart;
    // }

    // /// <summary>
    // /// 直线移动（X相等时使用）
    // /// </summary>
    // private Vector3 LinearMove(Vector3 start, Vector3 end, float t)
    // {
    //     // X保持不变，Y和Z线性插值
    //     float x = start.x;
    //     float y = Mathf.Lerp(start.y, end.y, t);
    //     float z = Mathf.Lerp(start.z, end.z, t);
    //     return new Vector3(x, y, z);
    // }

    // /// <summary>
    // /// 抛物线移动（X不等时使用）
    // /// </summary>
    // private Vector3 ParabolicMove(Vector3 start, Vector3 end, float t)
    // {
    //     // X方向：线性移动
    //     float x = Mathf.Lerp(start.x, end.x, t);

    //     // Y方向：抛物线（先上升后下降）
    //     float y = Mathf.Lerp(start.y, end.y, t) + arcHeight * 4 * t * (1 - t);

    //     // Z方向：线性移动
    //     float z = Mathf.Lerp(start.z, end.z, t);

    //     return new Vector3(x, y, z);
    // }

    // /// <summary>
    // /// 路径完成回调
    // /// </summary>
    // private void OnPathComplete()
    // {
    //     Debug.Log("路径执行完成！");
    // }

    // /// <summary>
    // /// 清空所有路径（包括队列和当前移动）
    // /// </summary>
    // public void ClearAllPaths()
    // {
    //     pathQueue.Clear();
    //     isMoving = false;
    //     currentPath = null;
    //     Debug.Log("已清空所有路径");
    // }


    // /// <summary>
    // /// 动态修改速度（运行时）
    // /// </summary>
    // public void SetSpeed(float newSpeed)
    // {
    //     if (newSpeed > 0)
    //     {
    //         speed = newSpeed;

    //         // 如果正在移动，重新计算当前段的持续时间
    //         if (isMoving && currentPath != null)
    //         {
    //             float distance = Vector3.Distance(segmentStart, segmentEnd);
    //             segmentDuration = distance / speed;
    //             if (segmentDuration < 0.001f)
    //             {
    //                 segmentDuration = 0.001f;
    //             }

    //             // 重新计算当前进度，保持位置连续性
    //             float currentProgress = elapsedTime / segmentDuration;
    //             if (currentProgress > 1f)
    //             {
    //                 currentProgress = 1f;
    //             }
    //             // 注意：这里不重置elapsedTime，而是让Update继续使用新的segmentDuration
    //             // 但为了保持位置正确，需要调整elapsedTime
    //             elapsedTime = currentProgress * segmentDuration;
    //         }
    //     }
    // }
    // }
}