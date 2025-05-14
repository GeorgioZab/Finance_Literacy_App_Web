using Finance_Literacy_App_Web.Data;
using Finance_Literacy_App_Web.Models;
using Finance_Literacy_App_Web.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Finance_Literacy_App_Web.ViewModelBuilders
{
    public class ModulesVmBuilder
    {
        private readonly Context _context;

        public ModulesVmBuilder(Context context)
        {
            _context = context;
        }

        public List<ModuleViewModel> LoadData()
        {
            var modules = _context.Modules
                .Include(m => m.Lessons)
                .ThenInclude(l => l.Tasks)
                .ToList();

            return modules.Select(m => new ModuleViewModel(m)).ToList();
        }
    }
}