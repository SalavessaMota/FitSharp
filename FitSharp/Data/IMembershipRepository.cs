﻿using FitSharp.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace FitSharp.Data
{
    public interface IMembershipRepository : IGenericRepository<Membership>
    {
        IEnumerable<SelectListItem> GetComboMemberships();
    }
}