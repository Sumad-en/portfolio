

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

        // ==========================================
        // GET: /PortfolioView/Create
        // DISPLAY CREATE FORM
        // ==========================================

        [HttpGet]
        public IActionResult Create()
        {
            return View(new PortfolioViewModel());
        }

        // ==========================================
        // POST: /PortfolioView/Create
        // SAVE COURSE, PROJECT, AND IMAGE
        // ==========================================

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

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError("",
                    "Unable to save portfolio. Please check your database connection.");

                return View(model);
            }
        }

        // ==========================================
        // GET: /PortfolioView/Index
        // DISPLAY ALL PORTFOLIO RECORDS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var portfolios = await _portfolio.GetAllPortfoliosAsync();

            return View(portfolios);
        }
    }
}
