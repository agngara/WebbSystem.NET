using Models;

namespace DAL.Repositories.Interfaces
{
    public interface IInstitutionRepository
    {
        Task<Institution?> GetByNameAsync(string name);
        Task<Institution> AddAsync(Institution institution);
    }
}
