
using Microsoft.AspNetCore.Mvc;
using portfolio.Data;
using portfolio.ViewModels;

namespace portfolio.Controllers
{
    public class PortfolioViewController : Controller
    {
        private readonly PortfolioRepository _portfolio;

        public PortfolioViewController(PortfolioRepository portfolio)
        {
            _portfolio = portfolio;
        }

        // GET: /PortfolioView/Create
        public IActionResult Create()
        {
            return View(new PortfolioViewModel());
        }

        // POST: /PortfolioView/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PortfolioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _portfolio.AddPortfolioAsync(model);

                TempData["Success"] =
                    "Portfolio created successfully!";

                return RedirectToAction(nameof(Create));
            }
            catch (Exception)
            {
                ModelState.AddModelError("",
                    "Unable to save portfolio. Please check your database connection.");

                return View(model);
            }
        }
    }
}
