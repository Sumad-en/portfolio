using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using portfolio.Data;
using portfolio.ViewModels;

namespace portfolio.Controllers
{
    public class PortfolioViewController : Controller
    {
        private readonly PortfolioRepository _portfolio;
        private readonly SupabaseStorageService _storage;

        public PortfolioViewController(PortfolioRepository portfolio, SupabaseStorageService storage)
        {
            _portfolio = portfolio;
            _storage = storage;
        }

        // PUBLIC: Anyone can view the portfolio
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var portfolios = await _portfolio.GetAllPortfoliosAsync();
            return View(portfolios);
        }

        // PUBLIC: Fallback endpoint if any older bytea images exist in PostgreSQL
        [HttpGet]
        public async Task<IActionResult> GetImage(int id)
        {
            var image = await _portfolio.GetImageByIdAsync(id);
            if (image == null)
            {
                return NotFound();
            }

            return File(image.Value.Data, image.Value.ContentType);
        }

        // LOCKED: Editor pages require Manager sign-in
        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new PortfolioViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PortfolioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        ModelState.AddModelError("", $"[VALIDATION - {entry.Key}]: {error.ErrorMessage}");
                    }
                }
                return View(model);
            }

            try
            {
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    model.ProjectImage.ImageUrl = await _storage.UploadImageAsync(model.ImageFile);
                }

                await _portfolio.AddPortfolioAsync(model);
                TempData["Success"] = "Portfolio created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"[ERROR]: {ex.Message}");
                return View(model);
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var portfolio = await _portfolio.GetPortfolioByCourseIdAsync(id);
            if (portfolio == null)
            {
                return NotFound();
            }

            return View(portfolio);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PortfolioViewModel model)
        {
            if (id != model.Course.CourseId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    await _storage.DeleteImageAsync(model.ProjectImage.ImageUrl);
                    model.ProjectImage.ImageUrl = await _storage.UploadImageAsync(model.ImageFile);
                }

                await _portfolio.UpdatePortfolioAsync(model);
                TempData["Success"] = "Archive record updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"[ERROR]: {ex.Message}");
                return View(model);
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var portfolio = await _portfolio.GetPortfolioByCourseIdAsync(id);
            if (portfolio == null)
            {
                return NotFound();
            }

            return View(portfolio);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, IFormCollection form)
        {
            try
            {
                var portfolio = await _portfolio.GetPortfolioByCourseIdAsync(id);
                if (portfolio != null)
                {
                    await _storage.DeleteImageAsync(portfolio.ProjectImage?.ImageUrl);
                    await _portfolio.DeletePortfolioAsync(id);
                }

                TempData["Success"] = "Record suppressed and purged from mainframe.";
            }
            catch (Exception)
            {
                TempData["Success"] = "ERROR: Failed to suppress record from mainframe.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
