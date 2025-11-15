using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Connections;

namespace Test_Execution_UI.Pages;

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
}
