using FitSharp.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FitSharp.Data
{
    public class MembershipRepository : GenericRepository<Membership>, IMembershipRepository
    {
        private readonly DataContext _context;

        public MembershipRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public IEnumerable<SelectListItem> GetComboMemberships()
        {
            var list = _context.Memberships.Select(g => new SelectListItem
            {
                Text = g.Name,
                Value = g.Id.ToString()
            }).OrderBy(sli => sli.Text).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "(Select a membership...)",
                Value = "0"
            });

            return list;
        }
    }
}