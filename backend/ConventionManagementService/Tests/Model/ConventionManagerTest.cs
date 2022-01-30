using ConventionManagementService.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ConventionManagementServiceTest.Model
{
    [TestClass]
    public class ConventionManagerTest
    {
        private IConventionManager _ConventionManager;
        private List<Convention> _Conventions;

        [TestInitialize]
        public void TestInitialize()
        {
            _Conventions = new List<Convention>();
            _Conventions.Add(new Convention
            {
                Title = "Sample: Julie and Nikolaj's wedding",
                From = new DateTime(2022, 05, 1, 16, 0, 0),
                To = new DateTime(2022, 05, 2, 23, 0, 0),
                Events = new[]
                {
                    new Event { Title = "Wine tasting", Type = EventType.Venue, From = new DateTime(2022, 05, 1, 20, 0, 0), To = new DateTime(2022, 05, 1, 23, 0, 0) },
                    new Event { Title = "Jazz musik", Type = EventType.Event, From = new DateTime(2022, 05, 2, 14, 0, 0), To = new DateTime(2022, 05, 2, 18, 0, 0) },
                }
            });
            _Conventions.Add(new Convention
            {
                Title = "Sample: Tobias birthday",
                From = new DateTime(2022, 06, 14, 11, 0, 0),
                To = new DateTime(2022, 06, 16, 15, 0, 0),
                Events = new[]
                {
                    new Event { Title = "X Jump", Type = EventType.Event, From = new DateTime(2022, 06, 14, 11, 15, 0), To = new DateTime(2022, 06, 14, 14, 45, 0) },
                }
            });

            _ConventionManager = new InMemoryConventionManager();
            foreach (Convention convention in _Conventions)
            {
                _ConventionManager.CreateConvention(convention).GetAwaiter().GetResult(); ;
            }
        }

        [TestMethod]
        public async Task RegisterConventionWithFirstTimeRegistrationExpectUserRegistered()
        {
            string conventionId1 = _Conventions[0].Id;
            string conventionId2 = _Conventions[1].Id;
            string userId1 = "test1@cm.com";
            string userId2 = "test2@cm.com";

            await _ConventionManager.RegisterConvention(conventionId1, userId1, 3);
            await _ConventionManager.RegisterConvention(conventionId2, userId1, 2);
            Convention convention1 = await _ConventionManager.GetConvention(conventionId1);
            Convention convention2 = await _ConventionManager.GetConvention(conventionId2);
            Assert.AreEqual(3, convention1.TotalNumberOfParticipants);
            Assert.AreEqual(2, convention2.TotalNumberOfParticipants);
            IEnumerable<Convention> registeredConventions = await _ConventionManager.GetRegisteredConvetions(userId1);
            Assert.AreEqual(2, registeredConventions.Count());
            CollectionAssert.AreEqual(_Conventions.Select(item => item.Id).OrderBy(item => item).ToArray(),
                registeredConventions.Select(item => item.Id).OrderBy(item => item).ToArray());

            await _ConventionManager.RegisterConvention(conventionId1, userId2, 5);
            convention1 = await _ConventionManager.GetConvention(conventionId1);
            convention2 = await _ConventionManager.GetConvention(conventionId2);
            Assert.AreEqual(8, convention1.TotalNumberOfParticipants);
            Assert.AreEqual(2, convention2.TotalNumberOfParticipants);
            registeredConventions = await _ConventionManager.GetRegisteredConvetions(userId2);
            Assert.AreEqual(1, registeredConventions.Count());
            Assert.AreEqual(conventionId1, registeredConventions.First().Id);
        }

        [TestMethod]
        public async Task RegisterConventionWithUpdateParticipantExpectNumberUpdated()
        {
            string conventionId1 = _Conventions[0].Id;
            string userId1 = "test1@cm.com";
            string userId2 = "test2@cm.com";

            await _ConventionManager.RegisterConvention(conventionId1, userId1, 3);
            await _ConventionManager.RegisterConvention(conventionId1, userId2, 5);
            Convention convention1 = await _ConventionManager.GetConvention(conventionId1);
            Assert.AreEqual(8, convention1.TotalNumberOfParticipants);
            IEnumerable<Convention> registeredConventions = await _ConventionManager.GetRegisteredConvetions(userId1);
            Assert.AreEqual(1, registeredConventions.Count());

            await _ConventionManager.RegisterConvention(conventionId1, userId1, 5);
            convention1 = await _ConventionManager.GetConvention(conventionId1);
            Assert.AreEqual(10, convention1.TotalNumberOfParticipants);
            registeredConventions = await _ConventionManager.GetRegisteredConvetions(userId1);
            Assert.AreEqual(1, registeredConventions.Count());

            await _ConventionManager.RegisterConvention(conventionId1, userId1, 0);
            convention1 = await _ConventionManager.GetConvention(conventionId1);
            Assert.AreEqual(5, convention1.TotalNumberOfParticipants);
            registeredConventions = await _ConventionManager.GetRegisteredConvetions(userId1);
            //Register 0 NumberOfParticipant will remove the registration
            Assert.AreEqual(0, registeredConventions.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public async Task RegisterEventWithoutRegisterConventionExpectException()
        {
            string conventionId = _Conventions[0].Id;
            string eventId = _Conventions[0].Events.First().Id;
            string userId = "test1@cm.com";

            await _ConventionManager.RegisterEvent(conventionId, eventId, userId, 3);
            Assert.IsTrue(false, "Exception should be thrown before assert");

        }

        [TestMethod]
        public async Task RegisterEventWithFirstTimeRegistrationExpectUserRegistered()
        {
            string conventionId = _Conventions[0].Id;
            Event[] events = _Conventions[0].Events.ToArray();
            string eventId1 = events[0].Id;
            string eventId2 = events[1].Id;
            string userId1 = "test1@cm.com";
            string userId2 = "test2@cm.com";

            await _ConventionManager.RegisterConvention(conventionId, userId1, 10);
            await _ConventionManager.RegisterConvention(conventionId, userId2, 10);

            await _ConventionManager.RegisterEvent(conventionId, eventId1, userId1, 3);
            await _ConventionManager.RegisterEvent(conventionId, eventId2, userId1, 5);
            Convention convention = await _ConventionManager.GetConvention(conventionId);
            Event event1 = convention.Events.FirstOrDefault(item => item.Id == eventId1);
            Assert.IsNotNull(event1);
            Assert.AreEqual(3, event1.TotalNumberOfParticipants);
            Event event2 = convention.Events.FirstOrDefault(item => item.Id == eventId2);
            Assert.IsNotNull(event2);
            Assert.AreEqual(5, event2.TotalNumberOfParticipants);

            await _ConventionManager.RegisterEvent(conventionId, eventId1, userId2, 1);
            await _ConventionManager.RegisterEvent(conventionId, eventId2, userId2, 1);
            convention = await _ConventionManager.GetConvention(conventionId);
            event1 = convention.Events.FirstOrDefault(item => item.Id == eventId1);
            Assert.IsNotNull(event1);
            Assert.AreEqual(4, event1.TotalNumberOfParticipants);
            event2 = convention.Events.FirstOrDefault(item => item.Id == eventId2);
            Assert.IsNotNull(event2);
            Assert.AreEqual(6, event2.TotalNumberOfParticipants);
        }

        [TestMethod]
        public async Task GetConventionsByUserExpectUserRegistrationInfo()
        {
            string conventionId = _Conventions[0].Id;
            Event[] events = _Conventions[0].Events.ToArray();
            string eventId1 = events[0].Id;
            string userId = "test1@cm.com";

            await _ConventionManager.RegisterConvention(conventionId, userId, 10);
            await _ConventionManager.RegisterEvent(conventionId, eventId1, userId, 3);
            Convention[] conventions = (await _ConventionManager.GetConvetions(userId, 0))?.ToArray();
            Assert.AreEqual(2, conventions.Count());
            Assert.AreEqual(10, conventions[0].UserInfo.NumberOfParticipants);
            Assert.AreEqual(3, conventions[0].Events.ToArray()[0].UserInfo.NumberOfParticipants);
            Assert.IsNull(conventions[1].UserInfo);
            Assert.IsNull(conventions[1].Events.ToArray()[0].UserInfo);
        }

        [Ignore, TestMethod]
        public async Task SaveConventionsToFile()
        {
            string json = JsonConvert.SerializeObject(_Conventions);
            await File.WriteAllTextAsync(@".\ConventionData.json", json);

            await _ConventionManager.PopulateData();
        }
    }
}
