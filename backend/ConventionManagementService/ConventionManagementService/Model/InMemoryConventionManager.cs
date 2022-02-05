using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ConventionManagementService.Model
{
    /// <summary>
    /// Manage conventions and registrations in memory
    /// </summary>
    public class InMemoryConventionManager : IConventionManager
    {
        private object SysObj = new object();
        private Dictionary<string, Convention> _Conventions = new Dictionary<string, Convention>();
        private Dictionary<string, List<UserRegistrationInfo<Convention>>> _UserConventions = new Dictionary<string, List<UserRegistrationInfo<Convention>>>();
        private Dictionary<string, List<UserRegistrationInfo<Event>>> _UserEvents = new Dictionary<string, List<UserRegistrationInfo<Event>>>();

        /// <inheritdoc />
        public Task<Convention> CreateConvention(Convention convention)
        {
            lock (SysObj)
            {
                EnsureIds(convention);
                if (_Conventions.ContainsKey(convention.Id))
                {
                    throw new ValidationException($"Convention {convention.Id} has already exist");
                }

                _Conventions.Add(convention.Id, convention);
            }
            return Task.FromResult(convention);
        }

        private void EnsureIds(Convention convention)
        {
            convention.Id = string.IsNullOrEmpty(convention.Id) ? Guid.NewGuid().ToString() : convention.Id;
            foreach(Event ev in convention.Events)
            {
                ev.Id = string.IsNullOrEmpty(ev.Id) ? Guid.NewGuid().ToString() : ev.Id;
            }
        }

        /// <inheritdoc />
        public Task<Convention> UpdateConvention(Convention convention)
        {
            lock (SysObj)
            {
                string id = convention.Id;
                if (!_Conventions.ContainsKey(id))
                {
                    throw new ValidationException($"Convention {id} does not exist");
                }
                _Conventions[id] = convention;
                return Task.FromResult(convention);
            }
        }

        /// <inheritdoc />
        public Task DeleteConvention(string id)
        {
            lock (SysObj)
            {
                if (_Conventions.ContainsKey(id))
                {
                    _Conventions.Remove(id);
                }
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<Convention> GetConvention(string conventionId)
        {
            Convention convention;
            lock (SysObj)
            {
                if (!_Conventions.TryGetValue(conventionId, out convention))
                {
                    convention = null;
                }
            }
            return Task.FromResult(convention);
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Convention> GetConvetions(int max)
        {
            lock (SysObj)
            {
                int count = 0;
                foreach(var convention in _Conventions.Values)
                {
                    if (max != 0 && count++ >= max) { break; }
                    yield return convention;
                }
            }

            await Task.Delay(1);
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Convention> GetConvetions(string userId, int max = 0)
        {
            lock (SysObj)
            {
                int count = 0;
                foreach(Convention convention in _Conventions.Values) {
                    if (max > 0 && count++ > max)
                    {
                        break;
                    }
                    var userConvention = (Convention)convention.Clone(); //TODO: use json serialization instead of clone
                    UserRegistrationInfo<Convention>  registrationInfo = GetRegisteredConvention(convention.Id, userId);
                    if (registrationInfo != null)
                    {
                        userConvention.UserInfo = new UserInfo
                        {
                            UserId = userId,
                            NumberOfParticipants = registrationInfo.NumberOfParticipants
                        };
                    }

                    foreach(Event ev in convention.Events)
                    {
                        UserRegistrationInfo<Event> eventRegistrationInfo = GetRegisteredEvent(convention.Id, ev.Id, userId);
                        if (eventRegistrationInfo != null)
                        {
                            ev.UserInfo = new UserInfo
                            {
                                UserId = userId,
                                NumberOfParticipants = eventRegistrationInfo.NumberOfParticipants
                            };
                        }
                    }
                    yield return userConvention;
                }
            }

            await Task.Delay(1);
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Convention> GetRegisteredConvetions(string userId)
        {
            lock (SysObj)
            {
                List<UserRegistrationInfo<Convention>> registeredConventions;
                if (_UserConventions.TryGetValue(userId, out registeredConventions))
                {
                    foreach (UserRegistrationInfo<Convention> registrationInfo in registeredConventions)
                    {
                        Convention convention = registrationInfo.Target;
                        var userConvention = (Convention)convention.Clone(); //TODO: use json serialization instead of clone
                        userConvention.UserInfo = new UserInfo
                        {
                            UserId = userId,
                            NumberOfParticipants = registrationInfo.NumberOfParticipants
                        };
                        foreach (Event ev in convention.Events)
                        {
                            UserRegistrationInfo<Event> eventRegistrationInfo = GetRegisteredEvent(convention.Id, ev.Id, userId);
                            if (eventRegistrationInfo != null)
                            {
                                ev.UserInfo = new UserInfo
                                {
                                    UserId = userId,
                                    NumberOfParticipants = eventRegistrationInfo.NumberOfParticipants
                                };
                            }
                        }
                        yield return userConvention;
                    }
                }
            }

            await Task.Delay(1);
        }

        /// <inheritdoc />
        public Task RegisterConvention(string conventionId, string userId, int numberOfParticipants)
        {
            lock (SysObj)
            {
                Convention convention;
                if (!_Conventions.TryGetValue(conventionId, out convention))
                {
                    throw new ValidationException($"Convention {conventionId} does not exist");
                }
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ValidationException($"userId is empty");
                }
                List<UserRegistrationInfo<Convention>> conventions;
                if (!_UserConventions.TryGetValue(userId, out conventions))
                {
                    conventions = new List<UserRegistrationInfo<Convention>>();
                }
                UserRegistrationInfo<Convention> info = conventions?.FirstOrDefault(item => item.Target.Id == conventionId);
                if (info == null) //add new registration
                {
                    if (numberOfParticipants == 0)
                    {
                        return Task.CompletedTask;
                    }
                    convention.TotalNumberOfParticipants += numberOfParticipants;
                    conventions.Add(new UserRegistrationInfo<Convention>(userId, numberOfParticipants, convention));
                }
                else //update existing registraction
                {
                    if (numberOfParticipants == 0)
                    {
                        _UserConventions.Remove(userId);
                        convention.TotalNumberOfParticipants -= info.NumberOfParticipants;
                        return Task.CompletedTask;
                    }
                    convention.TotalNumberOfParticipants += numberOfParticipants - info.NumberOfParticipants;
                    info.NumberOfParticipants = numberOfParticipants;
                }
                _UserConventions[userId] = conventions;
                return Task.CompletedTask;
            }
        }

        /// <inheritdoc />
        public Task RegisterEvent(string conventionId, string eventId, string userId, int numberOfParticipants)
        {
            lock (SysObj)
            {
                Convention convention;
                if (!_Conventions.TryGetValue(conventionId, out convention))
                {
                    throw new ValidationException($"Convention {conventionId} does not exist");
                }
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ValidationException($"userId is empty");
                }
                Event ev = convention.Events.FirstOrDefault(item => item.Id == eventId);
                if (ev == null)
                {
                    throw new ValidationException($"Event {eventId} does not exist");
                }
                UserRegistrationInfo<Convention> registeredConventionInfo = GetRegisteredConvention(conventionId, userId);
                if (registeredConventionInfo == null)
                {
                    throw new ValidationException($"User {userId} must register convention before registering event {eventId}");
                }
                if (registeredConventionInfo.NumberOfParticipants < numberOfParticipants)
                {
                    throw new ValidationException($"Number of event participants cannot exceed number of convention participants");
                }
                List<UserRegistrationInfo<Event>> events;
                if (!_UserEvents.TryGetValue(userId, out events))
                {
                    events = new List<UserRegistrationInfo<Event>>();
                }
                UserRegistrationInfo<Event> info = events.FirstOrDefault(item => item.Target.Id == ev.Id);
                if (info == null) //add new registration
                {
                    if (numberOfParticipants == 0)
                    {
                        return Task.CompletedTask;
                    }
                    ev.TotalNumberOfParticipants += numberOfParticipants;
                    events.Add(new UserRegistrationInfo<Event>(userId, numberOfParticipants, ev));
                }
                else //update existing registraction
                {
                    if (numberOfParticipants == 0)
                    {
                        _UserEvents.Remove(userId);
                        ev.TotalNumberOfParticipants -= info.NumberOfParticipants;
                        return Task.CompletedTask;
                    }
                    ev.TotalNumberOfParticipants += numberOfParticipants - info.NumberOfParticipants;
                    info.NumberOfParticipants = numberOfParticipants;
                }
                _UserEvents[userId] = events;
                return Task.CompletedTask;
            }
        }

        private UserRegistrationInfo<Convention> GetRegisteredConvention(string conventionId, string userId)
        {
            List<UserRegistrationInfo<Convention>> conventionRegistrations;
            if (!_UserConventions.TryGetValue(userId, out conventionRegistrations))
            {
                return null;
            }
            var conventionRegistrationInfo = conventionRegistrations.FirstOrDefault(item => item.Target.Id == conventionId);
            return conventionRegistrationInfo;
        }

        private UserRegistrationInfo<Event> GetRegisteredEvent(string conventionId, string eventId, string userId)
        {
            List<UserRegistrationInfo<Event>> eventRegistrations;
            if (!_UserEvents.TryGetValue(userId, out eventRegistrations))
            {
                return null;
            }
            var eventRegistrationInfo = eventRegistrations.FirstOrDefault(item => item.Target.Id == eventId);
            return eventRegistrationInfo;
        }

        /// <inheritdoc />
        /// This method is not thread safe. It is called only once at the service startup.
        public async Task PopulateData()
        {
            if (_Conventions != null && _Conventions.Count > 0)
            {
                return;
            }
            IEnumerable<Convention> conventions = await ReadInitData();
            foreach (Convention convention in conventions)
            {
                await CreateConvention(convention);
            }
        }

        private async Task<IEnumerable<Convention>> ReadInitData()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string text = await File.ReadAllTextAsync("ConventionData.json");
            var conventions = JsonConvert.DeserializeObject<IEnumerable<Convention>>(text);
            return conventions;
        }

        public Task Clear()
        {
            lock(SysObj)
            {
                _Conventions.Clear();
                _UserConventions.Clear();
                _UserEvents.Clear();
                return Task.CompletedTask;
            }
        }

        private class UserRegistrationInfo<T>
        {
            public string UserId { get; set; }
            public int NumberOfParticipants { get; set; }
            public T Target { get; set; }

            public UserRegistrationInfo(string userId, int numberOfParticipants, T target)
            {
                UserId = userId;
                NumberOfParticipants = numberOfParticipants;
                Target = target;
            }
        }
    }
}
