using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repositories.Interfaces
{
    public interface IWorkPlaceRepository
    {
        Task<WorkPlace?> GetWorkPlaceFromIdAsync(int? workplaceId);

        Task<WorkPlace?> GetByNameAsync(string name);
        Task<WorkPlace> AddAsync(WorkPlace workPlace);
    }
}
