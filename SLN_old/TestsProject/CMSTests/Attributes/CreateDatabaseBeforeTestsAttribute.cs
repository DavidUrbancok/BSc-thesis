using System;
using System.Linq;
using System.Text;

namespace CMS.Tests
{
    /// <summary>
    /// <para>Indicates that database for isolated integration tests is created prior to executing any of the tests in the class. </para>
    /// <para>This database can be filled with custom objects during 'TestFixtrueSetup' / 'ClassInitialize'. </para>
    /// <para>Each test uses copy of this database.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CreateDatabaseBeforeTestsAttribute : Attribute
    {
    }
}
