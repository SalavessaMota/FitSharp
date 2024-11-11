﻿using FitSharp.Data.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public interface IGroupClassRepository : IGenericRepository<GroupClass>
    {
        Task<GroupClass> GetGroupClassWithAllRelatedDataAsync(int id);

        Task<IEnumerable<GroupClass>> GetGroupClassesWithAllRelatedDataAsync();

        IQueryable<GroupClass> GetAllGroupClassesWithRelatedDataByUserName(string userName);
    }
}