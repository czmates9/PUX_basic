using Microsoft.AspNetCore.Mvc;
using PUX.DirectoryChangeDetector.Models;
using PUX.DirectoryChangeDetector.Services;

namespace PUX.DirectoryChangeDetector.Controllers;

public sealed class ScanController : Controller
{
    private readonly IDirectoryScanService _directoryScanService;
    private readonly ILogger<ScanController> _logger;

    public ScanController(
        IDirectoryScanService directoryScanService,
        ILogger<ScanController> logger)
    {
        _directoryScanService = directoryScanService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new ScanViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ScanViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            model.Result = await _directoryScanService.ScanAsync(
                model.DirectoryPath ?? string.Empty,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Directory scan failed for path {DirectoryPath}", model.DirectoryPath);
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            Message = "Při zpracování požadavku došlo k neočekávané chybě."
        });
    }
}
