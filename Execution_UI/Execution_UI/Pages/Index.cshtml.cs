using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IWebHostEnvironment _environment;

    public IndexModel(ILogger<IndexModel> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public void OnGet() { }

    public IActionResult OnGetDownloadClientDLL()
    {
        var folderPath = Path.Combine(_environment.ContentRootPath, "clients");
        var fileName = "CAST_Client_Service.dll";
        var filePath = Path.Combine(folderPath, fileName);

        if (!System.IO.File.Exists(filePath))
            return NotFound();

        return PhysicalFile(filePath, MediaTypeNames.Application.Octet, fileName);
    }
}
