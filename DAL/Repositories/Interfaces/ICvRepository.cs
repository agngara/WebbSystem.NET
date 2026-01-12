using System;
using System.Collections.Generic;
using System.Text;
using Models;

namespace DAL.Repositories.Interfaces
{
    public interface ICvRepository
    {

        Task<CV> GetCvById(int id);

        Task<List<CV>> GetAllExtendedCVs();

        Task<List<CV>> GetTop20CVs();

        Task<List<CV>> GetTop20PublicCVs();

        void AddCv(CV cv);
    }
}
