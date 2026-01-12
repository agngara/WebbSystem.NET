using System;
using System.Collections.Generic;
using System.Text;
using Models;

namespace DAL.Repositories.Interfaces
{
    public interface IInstitutionRepository
    {
        Task<Institution> GetInstitutionByNameAsync(string institutionName);

        Task AddInstitutionAsync(Institution institution);
    }
}
