using System.Reflection;

using NUnit.Framework;

namespace CMS.Tests
{
    /// <summary>
    /// Base class for assembly set up for <see cref="WebAppInstanceTests"/>.
    /// </summary>
    public class WebInstanceTestsAssemblySetUp 
    {
        internal static IWebInstanceTestsEnvironmentManager Manager
        {
            get;
            private set;
        }


        private bool TestsExcluded
        {
            get
            {
                return !TestsCategoryCheck.CheckAssemblySetUp(GetType());
            }
        }


        /// <summary>
        /// Sets up tests environment for <see cref="AbstractWebAppInstanceTests"/>. Call this method in set up method marked with <see cref="OneTimeSetUpAttribute"/>.
        /// </summary>
        protected void SetUpEnvironment()
        {
            if (TestsExcluded) return;

            var instanceName = Assembly.GetCallingAssembly().GetName().Name;
            Manager = GetManager(instanceName);
            Manager.SetUp();
        }

        
        /// <summary>
        /// Cleans up tests environment for <see cref="AbstractWebAppInstanceTests"/>. Call this method in set up method marked with <see cref="OneTimeTearDownAttribute"/>.
        /// </summary>
        protected void CleanUpEnvironment()
        {
            if (TestsExcluded) return;

            Manager.CleanUp();
        }


        /// <summary>
        /// Initializes new instance of web app instance manager.
        /// </summary>
        /// <param name="instanceName"></param>
        /// <returns></returns>
        protected virtual IWebInstanceTestsEnvironmentManager GetManager(string instanceName)
        {
            return new WebInstanceTestsEnvironmentManager(instanceName);
        }
    }
}
