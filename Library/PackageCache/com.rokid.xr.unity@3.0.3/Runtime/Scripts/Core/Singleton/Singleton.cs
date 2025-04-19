using System;

namespace Rokid.UXR
{
    /// <summary>
    /// Singleton
    /// </summary>
    public abstract class Singleton<T> where T : Singleton<T>
    {
        private static T m_Instance;

        private static object m_Locker = new object();

        public static T Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (m_Locker)
                    {
                        if (m_Instance == null)
                        {
                            m_Instance = Activator.CreateInstance<T>();
                            m_Instance.OnSingletonInit();
                        }
                    }
                }

                return m_Instance;
            }
        }

        protected Singleton() { }

        protected virtual void OnSingletonInit()
        {

        }
    }
}
