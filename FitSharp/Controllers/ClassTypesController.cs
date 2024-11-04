using FitSharp.Data;
using FitSharp.Data.Entities;
using FitSharp.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Vereyon.Web;

namespace FitSharp.Controllers
{
    [Authorize(Roles = "Employee")]
    public class ClassTypesController : Controller
    {
        private readonly IClassTypeRepository _ClassTypeRepository;
        private readonly IFlashMessage _flashMessage;

        public ClassTypesController(
            IClassTypeRepository ClassTypeRepository,
            IFlashMessage flashMessage)
        {
            _ClassTypeRepository = ClassTypeRepository;
            _flashMessage = flashMessage;
        }

        public IActionResult Index()
        {
            return View(_ClassTypeRepository.GetAll());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassType ClassType)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _ClassTypeRepository.CreateAsync(ClassType);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    _flashMessage.Danger("This ClassType already exists");
                }

                return View(ClassType);
            }

            return View(ClassType);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("ClassTypeNotFound");
            }

            var ClassType = await _ClassTypeRepository.GetByIdAsync(id.Value);
            if (ClassType == null)
            {
                return new NotFoundViewResult("ClassTypeNotFound");
            }
            return View(ClassType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClassType ClassType)
        {
            if (ModelState.IsValid)
            {
                await _ClassTypeRepository.UpdateAsync(ClassType);
                return RedirectToAction(nameof(Index));
            }

            return View(ClassType);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("ClassTypeNotFound");
            }

            var ClassType = await _ClassTypeRepository.GetByIdAsync(id.Value);
            if (ClassType == null)
            {
                return new NotFoundViewResult("ClassTypeNotFound");
            }

            try
            {
                await _ClassTypeRepository.DeleteAsync(ClassType);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("DELETE"))
                {
                    ViewBag.ErrorTitle = $"The ClassType {ClassType.Name} is probably being used!!";
                    ViewBag.ErrorMessage = $"{ClassType.Name}  can't be deleted because it is being used.</br></br>";
                }

                return View("Error");
            }
        }
    }
}