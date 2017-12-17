using CMS.Membership;
using CMS.Tests;

using NUnit.Framework;

namespace Tests
{
    public class Tests : UnitTests
    {
        [SetUp]
        public void Init()
        {
            Fake<UserInfo, UserInfoProvider>()
                .WithData(
                    new UserInfo { UserID = 1, FirstName = "Bruce", LastName = "Wayne" },
                    new UserInfo { UserID = 2, FirstName = "Peter", LastName = "Parker" }
                );

            Fake<UserSettingsInfo, UserSettingsInfoProvider>()
                .WithData(
                    new UserSettingsInfo { UserSettingsID = 1, UserSettingsUserID = 1, UserDescription = "Batman" },
                    new UserSettingsInfo { UserSettingsID = 2, UserSettingsUserID = 2, UserDescription = "Spider-man" },
                    new UserSettingsInfo { UserSettingsID = 3, UserSettingsUserID = 3, UserDescription = "Mr. Incredible" }
                );
        }

        [Test]
        public void JoinTest()
        {
            var superheroes = UserInfoProvider.GetUsers().Source(s => s.Join<UserSettingsInfo>("UserID", "UserSettingsUserID"));

            Assert.That(superheroes.Count, Is.EqualTo(2));
        }
    }
}
