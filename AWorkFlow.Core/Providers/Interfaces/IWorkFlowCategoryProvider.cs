using AWorkFlow.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Providers.Interfaces
{
    public interface IWorkFlowCategoryProvider
    {
        Task<IEnumerable<WorkFlowCategoryDto>> List();
        Task AddCategory(WorkFlowCategoryDto category);
        Task DeleteCategory(WorkFlowCategoryDto category);
        Task ActiveCategory(WorkFlowCategoryDto category);
        Task DeactiveCategory(WorkFlowCategoryDto category);
    }
}
