﻿using FitSharp.Data.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public interface IGroupClassRepository : IGenericRepository<GroupClass>
    {
        Task<GroupClass> GetGroupClassWithAllRelatedDataAsync(int id);

        IQueryable<GroupClass> GetAllGroupClassesWithRelatedData();

        IQueryable<GroupClass> GetAllGroupClassesWithRelatedDataByUserName(string userName);
    }
}