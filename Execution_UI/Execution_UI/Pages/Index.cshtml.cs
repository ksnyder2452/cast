using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Connections;
using System.Net.Mime;

namespace Execution_UI.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public SelectList Options { get; set; }

    [BindProperty]
    public string SelectedValue { get; set; }


    public void OnGet()
    {

    }


    private readonly IWebHostEnvironment _environment;

    public IndexModel(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    // Handler for a GET request, specified by "Download" in the URL
    /// <summary>
    /// Used to handle downloads of the Client DLL file (since it cannot be served statically).
    /// </summary>
    /// <returns></returns>
    public IActionResult OnGetDownloadClientDLL()
    {
        var folderPath = Path.Combine(_environment.ContentRootPath, "clients");
        var fileName = "CAST_Client_Service.dll";
        var filePath = Path.Combine(folderPath, fileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }
        return PhysicalFile(filePath, MediaTypeNames.Application.Octet, fileName);
    }
}

