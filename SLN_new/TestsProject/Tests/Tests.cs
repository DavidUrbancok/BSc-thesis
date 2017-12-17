using System;

using CMS.Membership;
using CMS.Tests;

using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class Tests : UnitTests
    {
        [SetUp]
        public void Init()
        {
            Fake<UserInfo, UserInfoProvider>()
                .WithData(
                    // Have matching UserSettings
                    new UserInfo { UserID = 1, FirstName = "Bruce", LastName = "Wayne" },
                    new UserInfo { UserID = 2, FirstName = "Peter", LastName = "Parker" },
                    new UserInfo { UserID = 3, FirstName = "Jonathan", LastName = "Osterman" },
                    new UserInfo { UserID = 4, FirstName = "Charles", LastName = "Xavier" },
                    new UserInfo { UserID = 5, FirstName = "Natalia", LastName = "Romanova" },
                    new UserInfo { UserID = 6, FirstName = "Clark", LastName = "Kent" },
                    new UserInfo { UserID = 7, FirstName = "Logan", LastName = "Howlett" },
                    new UserInfo { UserID = 8, FirstName = "Floyd", LastName = "Lawton" },
                    new UserInfo { UserID = 9, FirstName = "Bruce", LastName = "Banner" },
                    new UserInfo { UserID = 10, FirstName = "Erik", LastName = "Lensherr" },
                    new UserInfo { UserID = 11, FirstName = "Barry", LastName = "Allen" },
                    new UserInfo { UserID = 12, FirstName = "Scott", LastName = "Summers" },
                    new UserInfo { UserID = 13, FirstName = "Orin", LastName = "Curry" },
                    new UserInfo { UserID = 14, FirstName = "Steve", LastName = "Rogers" },
                    new UserInfo { UserID = 15, FirstName = "Alan", LastName = "Scott" },
                    new UserInfo { UserID = 16, FirstName = "Hal", LastName = "Jordan" },
                    new UserInfo { UserID = 17, FirstName = "Ororo", LastName = "Monroe" },
                    new UserInfo { UserID = 18, FirstName = "Matthew", LastName = "Murdock" },
                    new UserInfo { UserID = 19, FirstName = "Pricess", LastName = "Diana" },
                    new UserInfo { UserID = 20, FirstName = "Robert", LastName = "Parr" },

                    // Do not have matching UserSettings
                    new UserInfo { UserID = 101, FirstName = "Hank", LastName = "McCoy" },
                    new UserInfo { UserID = 102, FirstName = "Joseph", LastName = "Luthor" },
                    new UserInfo { UserID = 103, FirstName = "Raven", LastName = "Darkholme" },
                    new UserInfo { UserID = 104, FirstName = "Kurt", LastName = "Wagner" },
                    new UserInfo { UserID = 105, FirstName = "James", LastName = "Bond" }
                );

            Fake<UserSettingsInfo, UserSettingsInfoProvider>()
                .WithData(
                    // Have matching Users
                    new UserSettingsInfo { UserSettingsID = 5, UserSettingsUserID = 1, UserDescription = "Batman" },
                    new UserSettingsInfo { UserSettingsID = 4, UserSettingsUserID = 2, UserDescription = "Spider-Man" },
                    new UserSettingsInfo { UserSettingsID = 3, UserSettingsUserID = 3, UserDescription = "Doctor Manhattan" },
                    new UserSettingsInfo { UserSettingsID = 2, UserSettingsUserID = 4, UserDescription = "Professor X" },
                    new UserSettingsInfo { UserSettingsID = 1, UserSettingsUserID = 5, UserDescription = "Black Widow" },
                    new UserSettingsInfo { UserSettingsID = 10, UserSettingsUserID = 6, UserDescription = "Superman" },
                    new UserSettingsInfo { UserSettingsID = 9, UserSettingsUserID = 7, UserDescription = "Wolverine" },
                    new UserSettingsInfo { UserSettingsID = 8, UserSettingsUserID = 8, UserDescription = "Deadshot" },
                    new UserSettingsInfo { UserSettingsID = 7, UserSettingsUserID = 9, UserDescription = "Incredible Hulk" },
                    new UserSettingsInfo { UserSettingsID = 6, UserSettingsUserID = 10, UserDescription = "Magneto" },
                    new UserSettingsInfo { UserSettingsID = 15, UserSettingsUserID = 11, UserDescription = "The Flash" },
                    new UserSettingsInfo { UserSettingsID = 14, UserSettingsUserID = 12, UserDescription = "Cyclops" },
                    new UserSettingsInfo { UserSettingsID = 13, UserSettingsUserID = 13, UserDescription = "Aquaman" },
                    new UserSettingsInfo { UserSettingsID = 12, UserSettingsUserID = 14, UserDescription = "Captain America" },
                    new UserSettingsInfo { UserSettingsID = 11, UserSettingsUserID = 15, UserDescription = "Green Lantern" },
                    new UserSettingsInfo { UserSettingsID = 20, UserSettingsUserID = 16, UserDescription = "Parallax" },
                    new UserSettingsInfo { UserSettingsID = 19, UserSettingsUserID = 17, UserDescription = "Storm" },
                    new UserSettingsInfo { UserSettingsID = 18, UserSettingsUserID = 18, UserDescription = "Daredevil" },
                    new UserSettingsInfo { UserSettingsID = 17, UserSettingsUserID = 19, UserDescription = "Wonder Woman" },
                    new UserSettingsInfo { UserSettingsID = 16, UserSettingsUserID = 20, UserDescription = "Mr. Incredible" },

                    // Do not have matching Users
                    new UserSettingsInfo { UserSettingsID = 101, UserSettingsUserID = 201, UserDescription = "The Thing" },
                    new UserSettingsInfo { UserSettingsID = 102, UserSettingsUserID = 202, UserDescription = "Robin" },
                    new UserSettingsInfo { UserSettingsID = 103, UserSettingsUserID = 203, UserDescription = "The Joker" }
                );
        }


        [Test]
        public void InnerJoinTest()
        {
            var superheroes = UserInfoProvider.GetUsers().Source(s => s.InnerJoin<UserSettingsInfo>("UserID", "UserSettingsUserID"));

            var firstHero = superheroes.FirstObject;

            var hero = superheroes.Result.Tables[0].Rows[13]; // should be Captain America if the join is successful

            CMSAssert.All(() =>
            {
                // Test if the number of superheroes is correct
                Assert.That(superheroes.Count, Is.EqualTo(20));

                // Test if the generated query is correct
                Assert.That(superheroes.ToString(), Is.EqualTo("SELECT *\r\nFROM [ExternalSource] INNER JOIN [ExternalSource] ON [ExternalSource].[UserID] = [ExternalSource].[UserSettingsUserID]"));

                // Test if the join is successful
                Assert.That(firstHero.FirstName, Is.EqualTo("Bruce"));
                Assert.That(firstHero.LastName, Is.EqualTo("Wayne"));
                Assert.That(firstHero.UserDescription, Is.EqualTo("Batman"));

                Assert.That(hero["FirstName"], Is.EqualTo("Steve"));
                Assert.That(hero["LastName"], Is.EqualTo("Rogers"));
                Assert.That(hero["UserSettingsInfo_UserDescription"], Is.EqualTo("Captain America"));
            });
        }


        [Test]
        public void LeftJoinTest()
        {
            var superheroes = UserInfoProvider.GetUsers().Source(s => s.LeftJoin<UserSettingsInfo>("UserID", "UserSettingsUserID"));

            var hero = superheroes.Result.Tables[0].Rows[19]; // should be Mr. Incredible if the join is successful

            var unknown = superheroes.Result.Tables[0].Rows[24]; // should be James Bond if the join is successful

            CMSAssert.All(() =>
            {
                // Test if the number of superheroes is correct
                Assert.That(superheroes.Count, Is.EqualTo(25));

                // Test if the generated query is correct
                Assert.That(superheroes.ToString(), Is.EqualTo("SELECT *\r\nFROM [ExternalSource] LEFT OUTER JOIN [ExternalSource] ON [ExternalSource].[UserID] = [ExternalSource].[UserSettingsUserID]"));

                Assert.That(hero["FirstName"], Is.EqualTo("Robert"));
                Assert.That(hero["LastName"], Is.EqualTo("Parr"));
                Assert.That(hero["UserSettingsInfo_UserDescription"], Is.EqualTo("Mr. Incredible"));


                Assert.That(unknown["FirstName"], Is.EqualTo("James"));
                Assert.That(unknown["LastName"], Is.EqualTo("Bond"));
                Assert.That(unknown["UserSettingsInfo_UserDescription"], Is.EqualTo(DBNull.Value));
            });
        }


        [Test]
        public void RightJoinTest()
        {
            var superheroes = UserInfoProvider.GetUsers().Source(s => s.RightJoin<UserSettingsInfo>("UserID", "UserSettingsUserID"));

            var hero = superheroes.Result.Tables[0].Rows[3]; // should be Charles Xavier if the join is successful

            var unknown = superheroes.Result.Tables[0].Rows[22]; // should be The Joker if the join is successful

            CMSAssert.All(() =>
            {
                // Test if the number of superheroes is correct
                Assert.That(superheroes.Count, Is.EqualTo(23));

                // Test if the generated query is correct
                Assert.That(superheroes.ToString(), Is.EqualTo("SELECT *\r\nFROM [ExternalSource] RIGHT OUTER JOIN [ExternalSource] ON [ExternalSource].[UserID] = [ExternalSource].[UserSettingsUserID]"));

                Assert.That(hero["FirstName"], Is.EqualTo("Charles"));
                Assert.That(hero["LastName"], Is.EqualTo("Xavier"));
                Assert.That(hero["UserDescription"], Is.EqualTo("Professor X"));

                Assert.That(unknown["FirstName"], Is.EqualTo(DBNull.Value));
                Assert.That(unknown["UserDescription"], Is.EqualTo("The Joker"));
            });
        }
    }
}
