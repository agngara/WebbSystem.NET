using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repositories.Interfaces
{
    public interface ICvRepository
    {

        Task<List<CV>> GetAllExtendedCVs();

        Task<List<CV>> GetTop20CVs();

        Task<List<CV>> GetTop20PublicCVs();
    }
}
