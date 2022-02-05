using ConventionManagementService;
using ConventionManagementService.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ConventionManagementServiceTest.Model
{
    [TestClass]
    public class ConventionManagerTest
    {
        private List<Tuple<IConventionManager, List<Convention>>> _Managers;

        [TestInitialize]
        public void TestInitialize()
        {
            _Managers = new List<Tuple<IConventionManager, List<Convention>>>();
            _Managers.Add(new Tuple<IConventionManager, List<Convention>>(new InMemoryConventionManager(), CreateConventions()));
            var dbConfig = ReadDbConfig();
            var dbManager = new DatabaseConventionManager(dbConfig);
            _Managers.Add(new Tuple<IConventionManager, List<Convention>>(dbManager, CreateConventions()));
            foreach (var pair in _Managers)
            {
                IConventionManager manager = pair.Item1;
                foreach (Convention convention in pair.Item2)
                {
                    manager.CreateConvention(convention).GetAwaiter().GetResult();
                }
            }
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            foreach (var pair in _Managers)
            {
                pair.Item1.Clear().GetAwaiter().GetResult(); 
            }
        }

        private List<Convention> CreateConventions()
        {
            return new List<Convention>
            {
                new Convention
                {
                    Title = "Sample: Julie and Nikolaj's wedding",
                    From = new DateTime(2022, 05, 1, 16, 0, 0),
                    To = new DateTime(2022, 05, 2, 23, 0, 0),
                    Events = new[]
                    {
                        new Event { Title = "Wine tasting", Type = EventType.Venue, From = new DateTime(2022, 05, 1, 20, 0, 0), To = new DateTime(2022, 05, 1, 23, 0, 0) },
                        new Event { Title = "Jazz musik", Type = EventType.Event, From = new DateTime(2022, 05, 2, 14, 0, 0), To = new DateTime(2022, 05, 2, 18, 0, 0) },
                    }
                },
                new Convention
                {
                    Title = "Sample: Tobias birthday",
                    From = new DateTime(2022, 06, 14, 11, 0, 0),
                    To = new DateTime(2022, 06, 16, 15, 0, 0),
                    Events = new[]
                    {
                        new Event { Title = "X Jump", Type = EventType.Event, From = new DateTime(2022, 06, 14, 11, 15, 0), To = new DateTime(2022, 06, 14, 14, 45, 0) },
                    }
                }
            };
        }

        private CosmosDbConfig ReadDbConfig()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json").Build();

            var section = config.GetSection("CosmosDb");
            var dbConfig = section.Get<CosmosDbConfig>();
            return dbConfig;
        }

        [TestMethod]
        public async Task RegisterConventionWithFirstTimeRegistrationExpectUserRegistered()
        {
            foreach (var pair in _Managers)
            {
                IConventionManager manager = pair.Item1;
                List<Convention> conventions = pair.Item2;
                string conventionId1 = conventions[0].Id;
                string conventionId2 = conventions[1].Id;
                string userId1 = "test1@cm.com";
                string userId2 = "test2@cm.com";

                await manager.RegisterConvention(conventionId1, userId1, 3);
                await manager.RegisterConvention(conventionId2, userId1, 2);
                Convention convention1 = await manager.GetConvention(conventionId1);
                Convention convention2 = await manager.GetConvention(conventionId2);
                Assert.AreEqual(3, convention1.TotalNumberOfParticipants);
                Assert.AreEqual(2, convention2.TotalNumberOfParticipants);
                List<Convention> registeredConventions = await ToListAsync(manager.GetRegisteredConvetions(userId1));
                Assert.AreEqual(2, registeredConventions.Count());
                CollectionAssert.AreEqual(conventions.Select(item => item.Id).OrderBy(item => item).ToArray(),
                    registeredConventions.Select(item => item.Id).OrderBy(item => item).ToArray());

                await manager.RegisterConvention(conventionId1, userId2, 5);
                convention1 = await manager.GetConvention(conventionId1);
                convention2 = await manager.GetConvention(conventionId2);
                Assert.AreEqual(8, convention1.TotalNumberOfParticipants);
                Assert.AreEqual(2, convention2.TotalNumberOfParticipants);
                registeredConventions = await ToListAsync(manager.GetRegisteredConvetions(userId2));
                Assert.AreEqual(1, registeredConventions.Count());
                Assert.AreEqual(conventionId1, registeredConventions.First().Id);
            }
        }

        [TestMethod]
        public async Task RegisterConventionWithUpdateParticipantExpectNumberUpdated()
        {
            foreach (var pair in _Managers)
            {
                IConventionManager manager = pair.Item1;
                List<Convention> conventions = pair.Item2;
                string conventionId1 = conventions[0].Id;
                string userId1 = "test1@cm.com";
                string userId2 = "test2@cm.com";

                await manager.RegisterConvention(conventionId1, userId1, 3);
                await manager.RegisterConvention(conventionId1, userId2, 5);
                Convention convention1 = await manager.GetConvention(conventionId1);
                Assert.AreEqual(8, convention1.TotalNumberOfParticipants);
                IEnumerable<Convention> registeredConventions = await ToListAsync(manager.GetRegisteredConvetions(userId1));
                Assert.AreEqual(1, registeredConventions.Count());

                await manager.RegisterConvention(conventionId1, userId1, 5);
                convention1 = await manager.GetConvention(conventionId1);
                Assert.AreEqual(10, convention1.TotalNumberOfParticipants);
                registeredConventions = await ToListAsync(manager.GetRegisteredConvetions(userId1));
                Assert.AreEqual(1, registeredConventions.Count());

                await manager.RegisterConvention(conventionId1, userId1, 0);
                convention1 = await manager.GetConvention(conventionId1);
                Assert.AreEqual(5, convention1.TotalNumberOfParticipants);
                registeredConventions = await ToListAsync(manager.GetRegisteredConvetions(userId1));
                //Register 0 NumberOfParticipant will remove the registration
                Assert.AreEqual(0, registeredConventions.Count());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public async Task RegisterEventWithoutRegisterConventionExpectException()
        {
            foreach (var pair in _Managers)
            {
                IConventionManager manager = pair.Item1;
                List<Convention> conventions = pair.Item2;
                string conventionId = conventions[0].Id;
                string eventId = conventions[0].Events.First().Id;
                string userId = "test1@cm.com";

                await manager.RegisterEvent(conventionId, eventId, userId, 3);
                Assert.IsTrue(false, "Exception should be thrown before assert");
            }
        }

        [TestMethod]
        public async Task RegisterEventWithFirstTimeRegistrationExpectUserRegistered()
        {
            foreach (var pair in _Managers)
            {
                IConventionManager manager = pair.Item1;
                List<Convention> conventions = pair.Item2;
                string conventionId = conventions[0].Id;
                Event[] events = conventions[0].Events.ToArray();
                string eventId1 = events[0].Id;
                string eventId2 = events[1].Id;
                string userId1 = "test1@cm.com";
                string userId2 = "test2@cm.com";

                await manager.RegisterConvention(conventionId, userId1, 10);
                await manager.RegisterConvention(conventionId, userId2, 10);

                await manager.RegisterEvent(conventionId, eventId1, userId1, 3);
                await manager.RegisterEvent(conventionId, eventId2, userId1, 5);
                Convention convention = await manager.GetConvention(conventionId);
                Event event1 = convention.Events.FirstOrDefault(item => item.Id == eventId1);
                Assert.IsNotNull(event1);
                Assert.AreEqual(3, event1.TotalNumberOfParticipants);
                Event event2 = convention.Events.FirstOrDefault(item => item.Id == eventId2);
                Assert.IsNotNull(event2);
                Assert.AreEqual(5, event2.TotalNumberOfParticipants);

                await manager.RegisterEvent(conventionId, eventId1, userId2, 1);
                await manager.RegisterEvent(conventionId, eventId2, userId2, 1);
                convention = await manager.GetConvention(conventionId);
                event1 = convention.Events.FirstOrDefault(item => item.Id == eventId1);
                Assert.IsNotNull(event1);
                Assert.AreEqual(4, event1.TotalNumberOfParticipants);
                event2 = convention.Events.FirstOrDefault(item => item.Id == eventId2);
                Assert.IsNotNull(event2);
                Assert.AreEqual(6, event2.TotalNumberOfParticipants);
            }
        }

        [Ignore, TestMethod]
        public async Task GetConventionsByUserExpectUserRegistrationInfo()
        {
            foreach (var pair in _Managers)
            {
                IConventionManager manager = pair.Item1;
                List<Convention> conventions = pair.Item2;
                string conventionId = conventions[0].Id;
                Event[] events = conventions[0].Events.ToArray();
                string eventId1 = events[0].Id;
                string userId = "test1@cm.com";

                await manager.RegisterConvention(conventionId, userId, 10);
                await manager.RegisterEvent(conventionId, eventId1, userId, 3);
                List<Convention> allconventions = await ToListAsync(manager.GetConvetions(userId, 0));
                Assert.AreEqual(2, allconventions.Count());
                Assert.AreEqual(10, allconventions[0].UserInfo.NumberOfParticipants);
                Assert.AreEqual(3, allconventions[0].Events.ToArray()[0].UserInfo.NumberOfParticipants);
                Assert.IsNull(allconventions[1].UserInfo);
                Assert.IsNull(allconventions[1].Events.ToArray()[0].UserInfo);
            }
        }

        [Ignore, TestMethod]
        public async Task SaveConventionsToFile()
        {
            IConventionManager manager = _Managers[0].Item1;
            List<Convention> conventions = _Managers[0].Item2;
            string json = JsonConvert.SerializeObject(conventions);
            await File.WriteAllTextAsync(@".\ConventionData.json", json);

            await manager.PopulateData();
        }

        private static async Task<List<T>> ToListAsync<T>(IAsyncEnumerable<T> items)
        {
            var results = new List<T>();
            await foreach (var item in items)
            {
                results.Add(item);
            }
            return results;
        }
    }
}
