using Microsoft.AspNetCore.Authorization;
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

        [HttpGet]
        public IActionResult Create()
        {
            return View(new PortfolioViewModel());
        }

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
                await _portfolio.AddPortfolioAsync(model);
                TempData["Success"] = "Portfolio created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"[DB ERROR]: {ex.Message}");
                return View(model);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var portfolios = await _portfolio.GetAllPortfoliosAsync();
            return View(portfolios);
        }

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
                await _portfolio.UpdatePortfolioAsync(model);
                TempData["Success"] = "Archive record updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to update portfolio. Please check your database connection.");
                return View(model);
            }
        }
        // ==========================================
        // GET: /PortfolioView/Delete/5
        // DISPLAY DELETE CONFIRMATION PAGE
        // ==========================================

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

        // ==========================================
        // POST: /PortfolioView/Delete/5
        // DELETE COURSE, PROJECT, AND IMAGE
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, IFormCollection form)
        {
            try
            {
                await _portfolio.DeletePortfolioAsync(id);
                TempData["Success"] = "Record suppressed and purged from mainframe.";
            }
            catch (Exception)
            {
                TempData["Success"] = "ERROR: Failed to suppress record from mainframe.";
            }

            return RedirectToAction(nameof(Index));
        }
        // Helper to convert uploaded file into byte[]
        private static async Task ProcessUploadedImageAsync(PortfolioViewModel model)
        {
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await model.ImageFile.CopyToAsync(ms);
                model.ProjectImage.ImageData = ms.ToArray();
                model.ProjectImage.ContentType = model.ImageFile.ContentType;
            }
        }

        // Endpoint that streams the PostgreSQL bytea image to <img> tags
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



    }
}
