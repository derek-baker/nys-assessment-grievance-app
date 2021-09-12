using Library.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Library.Services.Csv
{
    public interface ICsvGeneratorService
    {
        Task<byte[]> Generate(IEnumerable<GrievanceApplication> data);
    }
}