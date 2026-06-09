using System;
using System.Linq;
using System.Web.Mvc;
using WorkOps.Filters;
using WorkOps.Models;

namespace WorkOps.Controllers
{
    [AdminOnly]
    public class HolidayController : Controller
    {
        private readonly WorkOpsDBEntities db = new WorkOpsDBEntities();

        public ActionResult Index(int? year)
        {
            ViewBag.Title = "Holiday Calendar";
            var selectedYear = year ?? DateTime.Today.Year;

            var holidays = db.Holidays
                .ToList()
                .Where(h => h.HolidayDate.Year == selectedYear || (h.IsRecurring ?? false))
                .Select(h => new HolidayListItemViewModel
                {
                    HolidayID = h.HolidayID,
                    HolidayName = h.HolidayName,
                    HolidayDate = (h.IsRecurring ?? false)
                        ? new DateTime(selectedYear, h.HolidayDate.Month, h.HolidayDate.Day)
                        : h.HolidayDate,
                    IsRecurring = h.IsRecurring ?? false,
                    Description = h.Description
                })
                .OrderBy(h => h.HolidayDate)
                .ToList();

            ViewBag.Years = new SelectList(
                Enumerable.Range(DateTime.Today.Year - 2, 8)
                    .Select(y => new { Value = y, Text = y.ToString() }),
                "Value",
                "Text",
                selectedYear
            );

            return View(new HolidayIndexViewModel
            {
                Holidays = holidays,
                Year = selectedYear
            });
        }

        public ActionResult Create()
        {
            ViewBag.Title = "Add Holiday";
            return View(new HolidayFormViewModel
            {
                HolidayDate = DateTime.Today
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HolidayFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (db.Holidays.Any(h =>
                h.HolidayDate == model.HolidayDate &&
                h.HolidayName == model.HolidayName))
            {
                ModelState.AddModelError("", "A holiday with the same name and date already exists.");
                return View(model);
            }

            db.Holidays.Add(new Holiday
            {
                HolidayName = model.HolidayName.Trim(),
                HolidayDate = model.HolidayDate.Date,
                Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
                IsRecurring = model.IsRecurring,
                CreatedDate = DateTime.Now
            });
            db.SaveChanges();

            TempData["SuccessMessage"] = "Holiday created successfully.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            ViewBag.Title = "Edit Holiday";
            var holiday = db.Holidays.Find(id);
            if (holiday == null)
                return HttpNotFound();

            return View(new HolidayFormViewModel
            {
                HolidayID = holiday.HolidayID,
                HolidayName = holiday.HolidayName,
                HolidayDate = holiday.HolidayDate,
                Description = holiday.Description,
                IsRecurring = holiday.IsRecurring ?? false
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(HolidayFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var holiday = db.Holidays.Find(model.HolidayID);
            if (holiday == null)
                return HttpNotFound();

            holiday.HolidayName = model.HolidayName.Trim();
            holiday.HolidayDate = model.HolidayDate.Date;
            holiday.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            holiday.IsRecurring = model.IsRecurring;

            db.SaveChanges();
            TempData["SuccessMessage"] = "Holiday updated successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, int? year)
        {
            var holiday = db.Holidays.Find(id);
            if (holiday == null)
            {
                TempData["ErrorMessage"] = "Holiday not found.";
                return RedirectToAction("Index", new { year });
            }

            db.Holidays.Remove(holiday);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Holiday deleted.";
            return RedirectToAction("Index", new { year });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
