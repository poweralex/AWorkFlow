using AWorkFlow.Core.Models;
using AWorkFlow.Core.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWorkFlow.InMemoryRepo
{
    public class WorkInMemRepo : IWorkRepository
    {
        static readonly object lockObj = new object();
        static readonly List<WorkDto> _works = new List<WorkDto>();

        public Task<bool> CancelWork(string id, string user)
        {
            throw new System.NotImplementedException();
        }

        public Task<WorkDto> GetWork(string id)
        {
            return Task.FromResult(_works.FirstOrDefault(x => x.WorkId == id));
        }

        public Task HoldWork(string id, string user)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> InsertWork(WorkDto work, string user)
        {
            try
            {
                if (string.IsNullOrEmpty(work?.WorkId))
                {
                    return Task.FromResult(false);
                }
                lock (lockObj)
                {
                    if (_works.Any(x => x.WorkId == work?.WorkId))
                    {
                        return Task.FromResult(false);
                    }
                    _works.Add(work);
                }
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
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
