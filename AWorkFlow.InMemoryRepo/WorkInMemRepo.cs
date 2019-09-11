using AWorkFlow.Core.Models;
using AWorkFlow.Core.Repositories.Interfaces;
using System.Threading.Tasks;

namespace AWorkFlow.InMemoryRepo
{
    public class WorkInMemRepo : RepoBase<WorkDto>, IWorkRepository
    {
        public Task<bool> CancelWork(string id, string user)
        {
            throw new System.NotImplementedException();
        }

        public Task<WorkDto> GetWork(string id)
        {
            throw new System.NotImplementedException();
        }

        public Task HoldWork(string id, string user)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> InsertWork(WorkDto work, string user)
        {
            throw new System.NotImplementedException();
        }

        public Task RestartWork(string id, string user)
        {
            throw new System.NotImplementedException();
        }

        public Task ResumeWork(string id, string user)
        {
            throw new System.NotImplementedException();
        }
    }
}
