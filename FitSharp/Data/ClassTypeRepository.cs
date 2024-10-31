using FitSharp.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace FitSharp.Data
{
    public class ClassTypeRepository : GenericRepository<ClassType>, IClassTypeRepository
    {
        private readonly DataContext _context;

        public ClassTypeRepository(DataContext context) : base(context)
        {
            _context = context;
        }


        public IEnumerable<SelectListItem> GetComboClassTypes()
        {
            var classTypes = _context.ClassTypes.Select(ct => new SelectListItem
            {
                Text = ct.Name,
                Value = ct.Id.ToString()
            }).OrderBy(ct => ct.Text).ToList();

            classTypes.Insert(0, new SelectListItem
            {
                Text = "(Select a class type...)",
                Value = "0"
            });

            return classTypes;
        }
    }
}
