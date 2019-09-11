using AWorkFlow.Core.Models;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Repositories.Interfaces
{
    public interface IWorkRepository
    {
        Task<WorkDto> GetWork(string id);
        Task<string> InsertWork(WorkDto work, string user);
        Task<bool> CancelWork(string id, string user);
        Task HoldWork(string id, string user);
        Task RestartWork(string id, string user);
        Task ResumeWork(string id, string user);
    }
}
