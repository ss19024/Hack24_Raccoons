using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
namespace Rokid.UXR.Utility
{
    public class Loom : MonoBehaviour
    {
        /// <summary> A delayed queue item. </summary>
        public class DelayedQueueItem
        {
            /// <summary> The time. </summary>
            public float time;

            /// <summary> The action. </summary>
            public Action action;
        }

        /// <summary> The current. </summary>
        private static Loom m_Current;

        /// <summary> Number of. </summary>
        private int m_Count;

        /// <summary> True once initialization is complete. </summary>
        private static bool m_Initialized;

        /// <summary> Identifier for the thread. </summary>
        private static int m_ThreadId = -1;

        /// <summary> The actions. </summary>
        private List<Action> m_Actions = new List<Action>();

        /// <summary> The delayed. </summary>
        private List<Loom.DelayedQueueItem> m_Delayed = new List<Loom.DelayedQueueItem>();

        /// <summary> Gets the current. </summary>
        /// <value> The current. </value>
        public static Loom Current
        {
            get
            {
                if (!Loom.m_Initialized)
                {
                    Loom.Initialize();
                }
                return Loom.m_Current;
            }
        }

        /// <summary> Initializes this object. </summary>
        public static void Initialize()
        {
            bool flag = !Loom.m_Initialized;
            if (flag && Loom.m_ThreadId != -1 && Loom.m_ThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                return;
            }
            if (flag)
            {
                GameObject gameObject = new GameObject("MainThreadDispatcher");
                gameObject.hideFlags = HideFlags.DontSave;
                UnityEngine.Object.DontDestroyOnLoad(gameObject);
                if (Loom.m_Current)
                {
                    if (Application.isPlaying)
                    {
                        UnityEngine.Object.Destroy(Loom.m_Current.gameObject);
                    }
                    else
                    {
                        UnityEngine.Object.DestroyImmediate(Loom.m_Current.gameObject);
                    }
                }
                Loom.m_Current = gameObject.AddComponent<Loom>();
                UnityEngine.Object.DontDestroyOnLoad(Loom.m_Current);
                Loom.m_Initialized = true;
                Loom.m_ThreadId = Thread.CurrentThread.ManagedThreadId;
            }
        }

        /// <summary> Executes the 'destroy' action. </summary>
        private void OnDestroy()
        {
            Loom.m_Initialized = false;
        }

        /// <summary> Queue on main thread. </summary>
        /// <param name="action"> The action.</param>
        public static void QueueOnMainThread(Action action)
        {
            Loom.QueueOnMainThread(action, 0f);
        }

        /// <summary> Queue on main thread. </summary>
        /// <param name="action"> The action.</param>
        /// <param name="time">   The time.</param>
        public static void QueueOnMainThread(Action action, float time)
        {
            if (time != 0f)
            {
                List<Loom.DelayedQueueItem> delayed = Loom.Current.m_Delayed;
                lock (delayed)
                {
                    Loom.Current.m_Delayed.Add(new Loom.DelayedQueueItem
                    {
                        time = Time.time + time,
                        action = action
                    });
                }
            }
            else
            {
                List<Action> actions = Loom.Current.m_Actions;
                lock (actions)
                {
                    Loom.Current.m_Actions.Add(action);
                }
            }
        }

        /// <summary> Executes the 'asynchronous' operation. </summary>
        /// <param name="action"> The action.</param>
        public static void RunAsync(Action action)
        {
            new Thread(new ParameterizedThreadStart(Loom.RunAction))
            {
                Priority = System.Threading.ThreadPriority.Normal
            }.Start(action);
        }

        /// <summary> Executes the 'asynchronous' operation. </summary>
        /// <param name="action"> The action.</param>
        ///  <param name="success"> The success callback in main thread</param>
        public static void RunAsync(Action action, Action success)
        {
            Action action1 = () =>
            {
                action();
                QueueOnMainThread(success);
            };
            new Thread(new ParameterizedThreadStart(Loom.RunAction))
            {
                Priority = System.Threading.ThreadPriority.Normal
            }.Start(action1);
        }

        /// <summary> Executes the 'asynchronous' operation. </summary>
        /// <param name="func"> The action.</param>
        /// <param name="success"> The success callback in main thread</param>
        /// <param name="failed"> The failed callback in main thread</param>
        public static void RunAsync(Func<string> func, Action success, Action<string> failed)
        {
            Action action1 = () =>
            {
                string result = func();
                if (string.IsNullOrEmpty(result))
                {
                    QueueOnMainThread(success);
                }
                else
                {
                    QueueOnMainThread(() => { failed.Invoke(result); });
                }
            };
            new Thread(new ParameterizedThreadStart(Loom.RunAction))
            {
                Priority = System.Threading.ThreadPriority.Normal
            }.Start(action1);
        }

        /// <summary> Executes the action. </summary>
        /// <param name="action"> The action.</param>
        private static void RunAction(object action)
        {
            ((Action)action)?.Invoke();
        }

        /// <summary> Updates this object. </summary>
        private void Update()
        {
            List<Action> actions = this.m_Actions;
            if (actions.Count > 0)
            {
                lock (actions)
                {
                    for (int i = 0; i < this.m_Actions.Count; i++)
                    {
                        this.m_Actions[i]();
                    }
                    this.m_Actions.Clear();
                }
            }

            List<Loom.DelayedQueueItem> delayed = this.m_Delayed;
            if (delayed.Count > 0)
            {
                lock (delayed)
                {
                    for (int j = 0; j < this.m_Delayed.Count; j++)
                    {
                        Loom.DelayedQueueItem delayedQueueItem = this.m_Delayed[j];
                        if (delayedQueueItem.time <= Time.time)
                        {
                            delayedQueueItem.action();
                            this.m_Delayed.RemoveAt(j);
                            j--;
                        }
                    }
                }
            }
        }
    }
}

