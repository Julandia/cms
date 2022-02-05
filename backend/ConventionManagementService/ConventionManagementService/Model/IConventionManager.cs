using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConventionManagementService.Model
{
    /// <summary>
    /// Manage the conventions and user registration to convention.
    /// </summary>
    public interface IConventionManager
    {
        /// <summary>
        /// Create a convention
        /// </summary>
        /// <param name="convention"></param>
        /// <returns></returns>
        Task<Convention> CreateConvention(Convention convention);

        /// <summary>
        /// Get convention by convention id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Convention> GetConvention(string id);

        /// <summary>
        /// Update a convention
        /// </summary>
        /// <param name="convention"></param>
        /// <returns></returns>
        Task<Convention> UpdateConvention(Convention convention);

        /// <summary>
        /// Delete a convention
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteConvention(string id);

        /// <summary>
        /// Get all the conventions with max count
        /// </summary>
        /// <param name="max">Return all if max is 0</param>
        /// <returns></returns>
        IAsyncEnumerable<Convention> GetConvetions(int max);

        /// <summary>
        /// Get all the conventions registered by a user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        IAsyncEnumerable<Convention> GetRegisteredConvetions(string userId);

        
        /// <summary>
        /// Register user's participant in a convention
        /// if the user is already registered for the convention, numberOfParticipants will be updated
        /// if numberOfParticipants is 0, the registration is removed.
        /// </summary>
        /// <param name="conventionId"></param>
        /// <param name="userId"></param>
        /// <param name="numberOfParticipants"></param>
        /// <returns></returns>
        Task RegisterConvention(string conventionId, string userId, int numberOfParticipants);

        /// <summary>
        /// Register user's participant in a specific event
        /// if the user is already registered for the event, numberOfParticipants will be updated
        /// if numberOfParticipants is 0, the registration is removed.
        /// </summary>
        /// <param name="conventionId"></param>
        /// <param name="eventId"></param>
        /// <param name="userId"></param>
        /// <param name="numberOfParticipants"></param>
        /// <returns></returns>
        Task RegisterEvent(string conventionId, string eventId, string userId, int numberOfParticipants);

        /// <summary>
        /// Populate initial data. 
        /// NB: this method will clear existing data
        /// </summary>
        /// <param name="conventions"></param>
        /// <returns></returns>
        Task PopulateData();

        /// <summary>
        /// Clear all the conventions and registrations
        /// </summary>
        /// <returns></returns>
        Task Clear();
    }

    public class ValidationException : Exception
    {
        public ValidationException() : base() { }

        public ValidationException(string message) : base(message) { }
    }
}
