using System;

namespace Watermelon
{
    [StaticUnload]
    public static partial class AnalyticsModules
    {
        private static BaseAnalyticsModule[] analyticsModules;

        public static void Init()
        {
            analyticsModules = GetAnalyticsModules();

            if (analyticsModules != null)
            {
                foreach (BaseAnalyticsModule analyticsModule in analyticsModules)
                {
                    analyticsModule?.Init();
                }
            }
        }

        public static void OnModuleInitialized<T>()
        {
            Type type = typeof(T);

            if(!analyticsModules.IsNullOrEmpty())
            {
                foreach (BaseAnalyticsModule analyticsModule in analyticsModules)
                {
                    if (analyticsModule.GetType() == type)
                        analyticsModule.Init();
                }
            }
        }

        private static void UnloadStatic()
        {
            analyticsModules = null;
        }
    }
}
