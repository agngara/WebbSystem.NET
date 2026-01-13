using Models;
namespace DAL.Repositories.Interfaces
{
    public interface ICVRepository : IRepository<CV>
    {
        public Task<List<CV>> GetAllExtendedCVs();
        public Task<List<CV>> GetTop20CVs();
        public Task<List<CV>> GetTop20PublicCVs();
        public Task<CV> GetCVForCV(int cvId);
        public Task SaveChangesAsync();
    }

}

