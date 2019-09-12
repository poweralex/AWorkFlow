using AutoMapper;
using Mcs.SF.Common.ServiceProviders.CommonModel;
using Mcs.SF.Common.ServiceProviders.UnitOfWorkNS;
using Mcs.SF.WorkFlow.Api.Models.Statuses;
using Mcs.SF.WorkFlow.Api.Repos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mcs.SF.WorkFlow.Api.Providers
{
    /// <summary>
    /// provides operations for status
    /// </summary>
    public class StatusProvider
    {
        private readonly IMapper _mapper;
        private readonly UnitOfWorkProvider _unitOfWorkProvider;

        /// <summary>
        /// operating user
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="unitOfWorkProvider"></param>
        public StatusProvider(IMapper mapper, UnitOfWorkProvider unitOfWorkProvider)
        {
            _mapper = mapper;
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        /// <summary>
        /// insert status
        /// </summary>
        /// <param name="targetId"></param>
        /// <param name="status"></param>
        /// <param name="qty"></param>
        /// <returns></returns>
        public async Task<OperationResult> InsertStatus(string targetId, string status, int? qty)
        {
            using (var uow = _unitOfWorkProvider.CreateUnitOfWork())
            {
                DateTime now = DateTime.UtcNow;
                var res = await uow.Repo<StatusRepository>().Insert(new Models.Entities.StatusEntity
                {
                    TargetId = targetId,
                    Status = status,
                    Qty = qty,
                    CreatedAt = now,
                    CreatedBy = User,
                    UpdatedAt = now,
                    UpdatedBy = User
                });
                return res;
            }
        }

        /// <summary>
        /// search status(es)
        /// </summary>
        /// <param name="targetIds"></param>
        /// <param name="statuses"></param>
        /// <returns></returns>
        public async Task<OperationResult<IEnumerable<StatusResponse>>> SearchStatus(IEnumerable<string> targetIds, IEnumerable<string> statuses)
        {
            using (var uow = _unitOfWorkProvider.CreateUnitOfWork())
            {
                var res = await uow.Repo<StatusRepository>().Search(targetIds, statuses);
                if (res?.Success == true)
                {
                    return new OperationResult<IEnumerable<StatusResponse>>(res) { Data = _mapper.Map<List<StatusResponse>>(res?.Data) };
                }
                else
                {
                    return new OperationResult<IEnumerable<StatusResponse>>(res);
                }
            }
        }
    }
}
