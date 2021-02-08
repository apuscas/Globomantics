using Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Globomantics.Services
{
    public interface IConferenceService
    {
        Task<IEnumerable<ConferenceModel>> GetAll();
        Task<ConferenceModel> GetById(int id);
        Task<StatisticsModel> GetStatistics();
        Task Add(ConferenceModel model);
    }
}
