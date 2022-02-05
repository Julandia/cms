using Microsoft.ApplicationInsights;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConventionManagementService.Model
{
    /// <summary>
    /// Manage conventions and registrations via cosmos db
    /// </summary>
    public class DatabaseConventionManager : IConventionManager
    {
        private const string _ConventionPartitionKey = "/id";
        private CosmosDbConfig _DbConfig;
        private CosmosClient _CosmosClient;
        private Database _Database;
        private Dictionary<string, Container> _ContainersMap;
        private TelemetryClient _Telemetry;

        public DatabaseConventionManager(IOptions<CosmosDbConfig> dbConfig, TelemetryClient telementry)
        {
            _DbConfig = dbConfig.Value;
            _Telemetry = telementry;
            _CosmosClient = new CosmosClient(_DbConfig.EndpointUrl, _DbConfig.AuthorizationKey);
            _ContainersMap = new Dictionary<string, Container>();

        }

        internal DatabaseConventionManager(CosmosDbConfig dbConfig)
        {
            _DbConfig = dbConfig;
            _CosmosClient = new CosmosClient(_DbConfig.EndpointUrl, _DbConfig.AuthorizationKey);
            _ContainersMap = new Dictionary<string, Container>();
        }

        private async Task<Database> GetOrCreateDatabase()
        {
            if (_Database != null) { return _Database; }
            DatabaseResponse response = await _CosmosClient.CreateDatabaseIfNotExistsAsync(_DbConfig.DatabaseId);
            _Telemetry?.TrackTrace($"Created Database: {response.Database.Id}\n");
            _Database = response.Database;
            return _Database;
        }

        private async Task<Container> GetOrCreateContainer(string containerName, string partitionKey)
        {
            Container container;
            if (!_ContainersMap.TryGetValue(containerName, out container))
            {
                var database = await GetOrCreateDatabase();
                ContainerResponse containerResponse = await database.CreateContainerIfNotExistsAsync(containerName, partitionKey);
                container = containerResponse.Container;
                _Telemetry?.TrackTrace($"Created Container: {containerResponse.Container.Id}\n");
                _ContainersMap.Add(containerName, container);
            }
            return container;
        }

        public async Task<Convention> CreateConvention(Convention convention)
        {
            Container conventionContainer = await GetOrCreateContainer(_DbConfig.ConventionContainerId, _ConventionPartitionKey);
            if (string.IsNullOrEmpty(convention.Id))
            {
                convention.Id = Guid.NewGuid().ToString();
            }
            if (convention.Events != null && convention.Events.Count() > 0)
            {
                foreach(Event ev in convention.Events)
                {
                    if (string.IsNullOrEmpty(ev.Id))
                    {
                        ev.Id = Guid.NewGuid().ToString();
                    }
                }
            }
            var dataItem = new DataItem { Id = convention.Id, Convention = convention };
            ItemResponse<DataItem> itemResponse = await conventionContainer.CreateItemAsync(dataItem, new PartitionKey(convention.Id));
            _Telemetry?.TrackTrace($"Created convention in database with id: {convention.Id}\n");
            return itemResponse.Resource.Convention;
        }

        public async Task<Convention> UpdateConvention(Convention convention)
        {
            Container conventionContainer = await GetOrCreateContainer(_DbConfig.ConventionContainerId, _ConventionPartitionKey);
            var dataItem = new DataItem { Id = convention.Id, Convention = convention };
            ItemResponse<DataItem> itemResponse = await conventionContainer.UpsertItemAsync(dataItem, new PartitionKey(convention.Id));
            _Telemetry?.TrackTrace($"Updated convention in database with id: {convention.Id}\n");
            return itemResponse.Resource.Convention;
        }

        public async Task DeleteConvention(string id)
        {
            var convention = await GetConvention(id);
            if (convention != null)
            {
                convention.IsDeleted = true;
                await UpdateConvention(convention);
            }
        }

        public async Task<Convention> GetConvention(string id)
        {
            return await GetItem<Convention>(id);
        }

        private async Task<DataItem> GetItem(string itemId)
        {
            Container container = await GetOrCreateContainer(_DbConfig.ConventionContainerId, _ConventionPartitionKey);
            try
            {
                ItemResponse<DataItem> itemResponse = await container.ReadItemAsync<DataItem>(itemId, new PartitionKey(itemId));
                _Telemetry?.TrackTrace($"Get item itemId:{itemId} returned\n");
                return itemResponse.Resource;
            } catch (CosmosException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _Telemetry?.TrackTrace($"{nameof(GetItem)} not found itemId:{itemId}\n");
                    return null;
                }
                throw ex;
            }
        }

        private async Task<T> GetItem<T>(string itemId) where T : class
        {
            DataType dataType = ValidateAndGetDataType<T>();
            
            Container container = await GetOrCreateContainer(_DbConfig.ConventionContainerId, _ConventionPartitionKey);
            try
            {
                ItemResponse<DataItem> itemResponse = await container.ReadItemAsync<DataItem>(itemId, new PartitionKey(itemId));
                _Telemetry?.TrackTrace($"{nameof(GetItem)} dataType:{dataType} itemId:{itemId} returned\n");
                return ConvertItem<T>(itemResponse.Resource, dataType);
                
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _Telemetry?.TrackTrace($"{nameof(GetItem)} not found itemId:{itemId}\n");
                    return null;
                }
                throw ex;
            }
        }

        private DataType ValidateAndGetDataType<T>()
        {
            DataType dataType;
            Type type = typeof(T);
            if (type == typeof(Convention))
            {
                dataType = DataType.Convention;
            }
            else if (type == typeof(Registration))
            {
                dataType = DataType.Registration;
            }
            else
            {
                throw new ArgumentException($"Not supported type {type.Name}");
            }
            return dataType;
        }

        private T ConvertItem<T>(DataItem dataItem, DataType dataType) where T : class
        {
            if (dataType == DataType.Convention)
            {
                return dataItem.Convention as T;
            }
            else
            {
                return dataItem.Registration as T;
            }
        }

        private async IAsyncEnumerable<T> GetItems<T>(int max, string[] itemIds = null) where T : class
        {
            DataType dataType = ValidateAndGetDataType<T>();
            Container container = await GetOrCreateContainer(_DbConfig.ConventionContainerId, _ConventionPartitionKey);

            QueryDefinition queryDefinition;
            if (itemIds != null && itemIds.Length > 0)
            {
                var inlist = new StringBuilder();
                inlist.Append("(");
                for (int i = 0; i < itemIds.Length; i++)
                {
                    inlist.Append("@ItemId" + i);
                    if (i < itemIds.Length - 1)
                    {
                        inlist.Append(", ");
                    }
                }
                inlist.Append(")");
                queryDefinition = new QueryDefinition($"select * from t where t.Type = @DataType and t.id in {inlist}")
                                    .WithParameter("@DataType", (int)dataType);
                for (int i = 0; i < itemIds.Length; i++)
                {
                    queryDefinition.WithParameter($"@ItemId{i}", itemIds[i]);
                }
            }
            else
            {
                queryDefinition = new QueryDefinition("select * from t where t.Type = @DataType")
                                    .WithParameter("@DataType", (int)dataType);
            }
            var requestOptions = max > 0 ? new QueryRequestOptions { MaxItemCount = max } : null;
            using (FeedIterator<DataItem> itemIterator = container.GetItemQueryIterator<DataItem>(queryDefinition, null, requestOptions))
            {
                while (itemIterator.HasMoreResults)
                {
                    FeedResponse<DataItem> response;
                    try
                    {
                        response = await itemIterator.ReadNextAsync();
                    }
                    catch (CosmosException ex)
                    {
                        _Telemetry?.TrackTrace($"Error when fetching items: {string.Join(',', itemIds)}\n");
                        throw ex;
                    }
                    foreach (DataItem item in response)
                    {
                        if (dataType == DataType.Convention)
                        {
                            yield return item.Convention as T;
                        }
                        else
                        {
                            yield return item.Registration as T;
                        }
                    }
                }
            }
            _Telemetry?.TrackTrace($"{nameof(GetItems)} dataType:{dataType} max:{max}");
        }

        public IAsyncEnumerable<Convention> GetConvetions(int max)
        {
            return GetItems<Convention>(max);
        }

        public async IAsyncEnumerable<Convention> GetRegisteredConvetions(string userId)
        {
            var registration = await GetItem<Registration>(userId);
            if (registration != null && registration.Items != null && registration.Items.Count > 0)
            {
                Dictionary<string, int> conventionToParticipants = registration.Items.Where(item => item.TargetType == RegistrationType.Convention)
                    .ToDictionary(item => item.TargetId, item => item.NumberOfParticipants);
                IAsyncEnumerable<Convention> conventions = GetItems<Convention>(0, conventionToParticipants.Keys.ToArray());
                await foreach (Convention convention in conventions)
                {
                    convention.UserInfo = new UserInfo { UserId = userId, NumberOfParticipants = conventionToParticipants[convention.Id] };
                    foreach (Event ev in convention.Events)
                    {
                        RegistrationItem item = registration.Items.FirstOrDefault(item => item.TargetType == RegistrationType.Event);
                        if (item != null)
                        {
                            ev.UserInfo = new UserInfo { UserId = userId, NumberOfParticipants = item.NumberOfParticipants };
                        }
                    }
                    yield return convention;
                }
            }
        }

        public async Task RegisterConvention(string conventionId, string userId, int numberOfParticipants)
        {
            Container container = await GetOrCreateContainer(_DbConfig.ConventionContainerId, _ConventionPartitionKey);
            DataItem conventionItem = await GetItem(conventionId);
            if (conventionItem == null)
            {
                throw new ValidationException($"Convention Id does not exist {conventionId}");
            }
            DataItem registrationItem = await GetItem(userId) ?? new DataItem { Type = DataType.Registration, Id = userId };
            Registration registration = registrationItem.Registration ?? new Registration { UserId = userId };
            int additionalParticipants = numberOfParticipants;
            RegistrationItem newItem = numberOfParticipants == 0 ? null :
                new RegistrationItem
                {
                    TargetId = conventionId,
                    TargetType = RegistrationType.Convention,
                    NumberOfParticipants = numberOfParticipants,
                };
            registration.Items = registration.Items ?? new List<RegistrationItem>();
            RegistrationItem existing = registration.Items.FirstOrDefault(item => item.TargetId == conventionId && item.TargetType == RegistrationType.Convention);
            if (existing != null)
            {
                registration.Items.Remove(existing);
                additionalParticipants -= existing.NumberOfParticipants;
            }
            if (newItem != null)
            {
                registration.Items.Add(newItem);
            }
            registrationItem.Registration = registration;
            await container.UpsertItemAsync<DataItem>(registrationItem, new PartitionKey(userId));
            conventionItem.Convention.TotalNumberOfParticipants += additionalParticipants;
            await container.UpsertItemAsync<DataItem>(conventionItem, new PartitionKey(conventionId));
        }

        public async Task RegisterEvent(string conventionId, string eventId, string userId, int numberOfParticipants)
        {
            Container container = await GetOrCreateContainer(_DbConfig.ConventionContainerId, _ConventionPartitionKey);
            DataItem conventionItem = await GetItem(conventionId);
            if (conventionItem == null)
            {
                throw new ValidationException($"Convention Id does not exist {conventionId}");
            }

            Event ev = conventionItem.Convention.Events.FirstOrDefault(ev => ev.Id == eventId);
            if (ev == null)
            {
                throw new ValidationException($"Event Id does not exist {eventId}");
            }
            DataItem registrationItem = await GetItem(userId) ?? new DataItem { Type = DataType.Registration, Id = userId };
            Registration registration = registrationItem.Registration ?? new Registration { UserId = userId };
            registration.Items = registration.Items ?? new List<RegistrationItem>();
            var registeredConvention = registration.Items.FirstOrDefault(item => item.TargetId == conventionId);
            if (registeredConvention == null)
            {
                throw new ValidationException($"User {userId} must register convention before registering event {eventId}");
            }
            if (registeredConvention.NumberOfParticipants < numberOfParticipants)
            {
                throw new ValidationException($"Number of event participants cannot exceed number of convention participants");
            }
            RegistrationItem newItem = new RegistrationItem
            {
                ParentId = conventionId,
                TargetId = eventId,
                TargetType = RegistrationType.Event,
                NumberOfParticipants = numberOfParticipants,
            };
            int additionalParticipants = numberOfParticipants;
            RegistrationItem existing = registration.Items.FirstOrDefault(item => item.TargetId == eventId && item.TargetType == RegistrationType.Event);
            if (existing != null)
            {
                registration.Items.Remove(existing);
                additionalParticipants -= existing.NumberOfParticipants;
            }
            registration.Items.Add(newItem);
            registrationItem.Registration = registration;
            await container.UpsertItemAsync<DataItem>(registrationItem, new PartitionKey(userId));
            ev.TotalNumberOfParticipants += additionalParticipants;
            await container.UpsertItemAsync<DataItem>(conventionItem, new PartitionKey(conventionId));
        }

        public async Task PopulateData()
        {
            Container conventionContainer = await GetOrCreateContainer(_DbConfig.ConventionContainerId, _ConventionPartitionKey);
            var queryDefinition = new QueryDefinition("select value COUNT(1) from t");
            using (FeedIterator<int> itemIterator = conventionContainer.GetItemQueryIterator<int>(queryDefinition, null))
            {
                if(itemIterator.HasMoreResults)
                {
                    FeedResponse<int> response;
                    try
                    {
                        response = await itemIterator.ReadNextAsync();
                    }
                    catch (CosmosException ex)
                    {
                        _Telemetry?.TrackTrace($"Error when fetching items\n");
                        throw ex;
                    }
                    int count = response.FirstOrDefault();
                    if (count > 0)
                    {
                        _Telemetry?.TrackTrace($"There are existing conventions and no data populated");
                        return;
                    }
                }
            }
            IEnumerable<Convention> conventions = await ReadInitData();
            foreach (Convention convention in conventions)
            {
                await CreateConvention(convention);
            }
            _Telemetry?.TrackTrace($"{conventions.Count()} conventions are populated in initialization");
        }

        public async Task Clear()
        {
            Container conventionContainer = await GetOrCreateContainer(_DbConfig.ConventionContainerId, _ConventionPartitionKey);
            if (conventionContainer != null)
            {
                await conventionContainer.DeleteContainerAsync();
            }
        }

        private async Task<IEnumerable<Convention>> ReadInitData()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string text = await File.ReadAllTextAsync("ConventionData.json");
            var conventions = JsonConvert.DeserializeObject<IEnumerable<Convention>>(text);
            return conventions;
        }

        private enum DataType
        {
            Convention = 0,
            Registration
        }

        private class DataItem
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            public DataType Type { get; set; }

            public Convention Convention { get; set; }

            public Registration Registration { get; set; }
        }

        private enum RegistrationType
        {
            Convention = 0,
            Event,
        }

        private class Registration
        {
            [JsonProperty(PropertyName = "id")]
            public string UserId { get; set; }

            public IList<RegistrationItem> Items { get; set; }

        }

        private class RegistrationItem
        {
            public string ParentId { get; set; }

            public string TargetId { get; set; }

            public RegistrationType TargetType { get; set; }

            public int NumberOfParticipants { get; set; }
        }
    }
}
