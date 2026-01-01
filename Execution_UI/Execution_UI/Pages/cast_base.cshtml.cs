using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Connections;
using Mysqlx.Crud;
using System.Threading.Channels;
using System.Globalization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Execution_UI.Services;

namespace Execution_UI.Pages;

public class CastBaseModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly AuthenticationService _authService;

    public CastBaseModel(ILogger<IndexModel> logger, AuthenticationService authService)
    {
        _logger = logger;
        _authService = authService;
    }

    public bool IsLoginRequired { get; set; }
    public bool IsAuthenticated { get; set; }
    public string AuthenticationError { get; set; } = string.Empty;
    public SelectList? Options { get; set; }

    [BindProperty]
    public string? SelectedValue { get; set; }

    public string rootDir = @Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar;

    /// <summary>
    /// The RabbitMQ Server pulled from appsettings.json
    /// </summary>
    public static string? rabbitmq_home = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["rabbitmq_home"];
    /// <summary>
    /// The RabbitMQ Port pulled from appsettings.json
    /// </summary>
    public static string? rabbitmq_port = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["rabbitmq_port"];
    /// <summary>
    /// The RabbitMQ UI Account pulled from appsettings.json
    /// </summary>
    public string? rabbitmq_user = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["rabbitmq_user"];
    /// <summary>
    /// The RabbitMQ UI password pulled from appsettings.json
    /// </summary>
    public string? rabbitmq_pwd = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["rabbitmq_pwd"];

    /// <summary>
    /// The MySQL Server pulled from appsettings.json
    /// Note that database access here is only for SELECT statements to populate the UI
    /// </summary>
    public static string? mysql_Server = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["mysql_Server"];
    /// <summary>
    /// The MySQL Port pulled from appsettings.json
    /// </summary>
    public static string? mysql_Port = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["mysql_Port"];
    /// <summary>
    /// The MySQL Port pulled from appsettings.json
    /// </summary>
    public static string? mysql_Database = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["mysql_Database"];
    /// <summary>
    /// The MySQL Account pulled from appsettings.json
    /// </summary>
    public static string? mysql_User = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["mysql_User"];
    /// <summary>
    /// The MySQL password pulled from appsettings.json
    /// </summary>
    public static string? mysql_Password = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["mysql_Password"];
    public static string connectString = "Server=" + mysql_Server + "; Database=" + mysql_Database + "; Uid=" + mysql_User + "; Pwd=" + mysql_Password + "; Port=" + mysql_Port;

    public List<string> originatorUUIDs = new List<string>();
    /// <summary>
    /// The list of Client display names
    /// </summary>
    public List<string> displayNames = new List<string>();
    /// <summary>
    /// The StartRun Action for each Client
    /// </summary>
    public List<string> startRun = new List<string>();
    /// <summary>
    /// The StopRun Action for each Client
    /// </summary>
    public List<string> stopRun = new List<string>();
    /// <summary>
    /// The PauseRun Action for each Client
    /// </summary>
    public List<string> pauseRun = new List<string>();
    /// <summary>
    /// The ResumeRun Action for each Client
    /// </summary>
    public List<string> resumeRun = new List<string>();
    /// <summary>
    /// The AbortRun Action for each Client
    /// </summary>
    public List<string> abortRun = new List<string>();
    /// <summary>
    /// The RestartRun Action for each Client
    /// </summary>
    public List<string> restartRun = new List<string>();
    /// <summary>
    /// The current state text for each Client (to display in the UI)
    /// </summary>
    public List<string> currentState = new List<string>();
    /// <summary>
    /// The current state color for each Client (to colorize the state text in the UI)
    /// </summary>
    public List<string> currentStateColor = new List<string>();
    /// <summary>
    /// The current Group filter
    /// </summary>
    public static string currentGroup = "All";
    /// <summary>
    /// The current Owner filter
    /// </summary>
    public static string currentOwner = "All";
    /// <summary>
    /// The current Location filter
    /// </summary>
    public static string currentLocation = "All";
    /// <summary>
    /// The current Keyword filter
    /// </summary>
    public static string currentKeyword = "All";
    /// <summary>
    /// The current Service State list
    /// </summary>
    public List<string> serviceState = new List<string>();
    /// <summary>
    /// The current Service Name list
    /// </summary>
    public List<string> serviceName = new List<string>();
    /// <summary>
    /// All custom action names
    /// </summary>
    public List<string> customActionName = new List<string>();
    /// <summary>
    /// All custom action descriptions
    /// </summary>
    public List<string> customActionDescription = new List<string>();
    /// <summary>
    /// All custom action icons
    /// </summary>
    public List<string> customActionIcon = new List<string>();
    /// <summary>
    /// All custom action full names
    /// </summary>
    public List<string> customActionFullName = new List<string>();
    /// <summary>
    /// Define when each custom action is displayed
    /// </summary>
    public List<bool> customActionHideBeforeStart = new List<bool>();
    /// <summary>
    /// Define when each custom action is displayed
    /// </summary>
    public List<bool> customActionHideAfterStart = new List<bool>();
    /// <summary>
    /// Define when each custom action is displayed
    /// </summary>
    public List<bool> customActionHideAfterComplete = new List<bool>();
    /// <summary>
    /// The current list of all filters
    /// </summary>
    public List<string> filterFrameworks = new List<string>();
    /// <summary>
    /// The current list of all Group filters
    /// </summary>
    public List<string> filterFrameworksOnGroup = new List<string>();
    /// <summary>
    /// The current list of all Owner filters
    /// </summary>
    public List<string> filterFrameworksOnOwner = new List<string>();
    /// <summary>
    /// The current list of all Location filters
    /// </summary>
    public List<string> filterFrameworksOnLocation = new List<string>();
    /// <summary>
    /// The current list of all Keyword filters
    /// </summary>
    public List<string> filterFrameworksOnKeyword = new List<string>();
    /// <summary>
    /// The current list of all scheduled runs
    /// </summary>
    public static List<string> scheduleList = new List<string>();

    static string select_framework_info = "select reference_uuid, display_name from logger where message like 'Started Client Service%' and display_name NOT LIKE 'SETUP New Framework - IGNORE THIS ENTRY' and filter_on_group like '%' and filter_on_owner like '%' and filter_on_location like '%' and filter_on_keyword like '%' and virtual_delete = 0 order by order_in_system DESC";
    string pre_select_framework_info = "select reference_uuid, display_name from logger where message like 'Started Client Service%' and display_name NOT LIKE 'SETUP New Framework - IGNORE THIS ENTRY'";
    static string group_select_framework_info = " and filter_on_group like '%'";
    static string owner_select_framework_info = " and filter_on_owner like '%'";
    static string location_select_framework_info = " and filter_on_location like '%'";
    //filter_on_keyword format is |keyword1|keyword2|...|keyword#|
    static string keyword_select_framework_info = " and filter_on_keyword like '%'";
    string post_select_framework_info = " and virtual_delete = 0 order by order_in_system DESC";
    bool updateSelectStatement = false;


    /// <summary>
    /// The RabbitMQ Connection Factory
    /// </summary>
    ConnectionFactory factory = new ConnectionFactory();

    /// <summary>
    /// Used to handle Schedule requests, filter requests and populate the variables for the UI
    /// </summary>
    /// <returns>IActionResult</returns>
    public IActionResult? OnGet(string? param2)
    {
        // Check if login is enabled
        var enableLogin = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["enable_login"];
        IsLoginRequired = enableLogin != null && enableLogin.ToString().ToLower() == "true";
        IsAuthenticated = _authService.IsAuthenticated();

        // If login is required and user is not authenticated, show login page
        if (IsLoginRequired && !IsAuthenticated)
        {
            return Page();
        }

        if (param2 != null)
        {
            if (param2.StartsWith("schedule_") && !scheduleList.Contains(param2.Substring(9)))
            {
                string newParam2 = param2.Substring(9);
                scheduleList.Add(newParam2);
                Console.WriteLine(newParam2);
                string referenceUUID = newParam2.Substring(0, newParam2.IndexOf(" "));
                string scheduledTime = newParam2.Substring(newParam2.IndexOf(" for ") + 5);
                string format = "yyyy-MM-dd HH:mm:ss";
                scheduledTime = DateTime.ParseExact(scheduledTime, format, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal).ToString(format);

                Guid startmyuuid = Guid.NewGuid();
                string startmyuuidAsString = startmyuuid.ToString();
                string insertSchedule = "insert into state(uuid, reference_uuid, state, event_time_dt, scheduled_time) values('" + startmyuuidAsString + "', '" + referenceUUID + "', 'SCHEDULED for " + scheduledTime + "', NOW(), '" + scheduledTime + "')";
                submitSchedule(insertSchedule);
                updateSelectStatement = false;
                return RedirectToPage("./cast_base");
            }
            else
            {
                if (param2.StartsWith("group_"))
                {
                    updateSelectStatement = true;
                    string newParam2 = param2.Substring(6);
                    Console.WriteLine("Filter for group '" + newParam2 + "'");
                    currentGroup = newParam2;
                    if (newParam2.Equals("All"))
                    {
                        group_select_framework_info = " and filter_on_group like '%'";
                    }
                    else
                    {
                        group_select_framework_info = " and filter_on_group = '" + newParam2 + "'";
                    }
                }
                else if (param2.StartsWith("owner_"))
                {
                    updateSelectStatement = true;
                    string newParam2 = param2.Substring(6);
                    Console.WriteLine("Filter for owner '" + newParam2 + "'");
                    currentOwner = newParam2;
                    if (newParam2.Equals("All"))
                    {
                        owner_select_framework_info = " and filter_on_owner like '%'";
                    }
                    else
                    {
                        owner_select_framework_info = " and filter_on_owner = '" + newParam2 + "'";
                    }
                }
                else if (param2.StartsWith("location_"))
                {
                    updateSelectStatement = true;
                    string newParam2 = param2.Substring(9);
                    Console.WriteLine("Filter for location '" + newParam2 + "'");
                    currentLocation = newParam2;
                    if (newParam2.Equals("All"))
                    {
                        location_select_framework_info = " and filter_on_location like '%'";
                    }
                    else
                    {
                        location_select_framework_info = " and filter_on_location = '" + newParam2 + "'";
                    }
                }
                else if (param2.StartsWith("keyword_"))
                {
                    updateSelectStatement = true;
                    string newParam2 = param2.Substring(8);
                    Console.WriteLine("Filter for keyword '" + newParam2 + "'");
                    currentKeyword = newParam2;
                    if (newParam2.Equals("All"))
                    {
                        keyword_select_framework_info = " and filter_on_keyword like '%'";
                    }
                    else
                    {
                        keyword_select_framework_info = " and filter_on_keyword like '%|" + newParam2 + "|%'";
                    }
                }
                select_framework_info = pre_select_framework_info + group_select_framework_info + owner_select_framework_info + location_select_framework_info + keyword_select_framework_info + post_select_framework_info;
                return RedirectToPage("./cast_base");
            }
        }
        else
        {
            var mysqlDictionary = new Dictionary<string, string> { };
            using (MySqlConnection conn = new MySqlConnection(connectString))
            {
                conn.Open();

                using (MySqlCommand command = new MySqlCommand(select_framework_info, conn))
                {
                    MySqlDataReader rdr = command.ExecuteReader();

                    while (rdr.Read())
                    {
                        mysqlDictionary.Add((string)rdr[0], (string)rdr[1]);
                    }
                    rdr.Close();
                }

                string selectCastState = "select state, name from cast_state_tracker";
                serviceState.Clear();
                serviceName.Clear();
                using (MySqlCommand command = new MySqlCommand(selectCastState, conn))
                {
                    MySqlDataReader rdr = command.ExecuteReader();

                    while (rdr.Read())
                    {
                        serviceState.Add((string)rdr[0]);
                        serviceName.Add((string)rdr[1]);
                    }
                    rdr.Close();
                }
                string selectFrameworkNames = "select distinct(filter_on_group) from logger";
                filterFrameworksOnGroup.Clear();
                filterFrameworksOnGroup.Add("All");
                using (MySqlCommand command = new MySqlCommand(selectFrameworkNames, conn))
                {
                    MySqlDataReader rdr = command.ExecuteReader();

                    while (rdr.Read())
                    {
                        if (rdr[0] != DBNull.Value)
                        {
                            filterFrameworksOnGroup.Add((string)rdr[0]);
                        }
                    }
                    rdr.Close();
                }
                filterFrameworksOnGroup.RemoveAll(item => item == null);
                filterFrameworksOnGroup.RemoveAll(item => item == "");

                selectFrameworkNames = "select distinct(filter_on_owner) from logger";
                filterFrameworksOnOwner.Clear();
                filterFrameworksOnOwner.Add("All");
                using (MySqlCommand command = new MySqlCommand(selectFrameworkNames, conn))
                {
                    MySqlDataReader rdr = command.ExecuteReader();

                    while (rdr.Read())
                    {
                        if (rdr[0] != DBNull.Value)
                        {
                            filterFrameworksOnOwner.Add((string)rdr[0]);
                        }
                    }
                    rdr.Close();
                }
                filterFrameworksOnOwner.RemoveAll(item => item == null);
                filterFrameworksOnOwner.RemoveAll(item => item == "");

                selectFrameworkNames = "select distinct(filter_on_location) from logger";
                filterFrameworksOnLocation.Clear();
                filterFrameworksOnLocation.Add("All");
                using (MySqlCommand command = new MySqlCommand(selectFrameworkNames, conn))
                {
                    MySqlDataReader rdr = command.ExecuteReader();

                    while (rdr.Read())
                    {
                        if (rdr[0] != DBNull.Value)
                        {
                            filterFrameworksOnLocation.Add((string)rdr[0]);
                        }
                    }
                    rdr.Close();
                }
                filterFrameworksOnLocation.RemoveAll(item => item == null);
                filterFrameworksOnLocation.RemoveAll(item => item == "");

                //Reset sql statement when keyword table or row is available
                selectFrameworkNames = "select distinct(filter_on_keyword) from logger";
                filterFrameworksOnKeyword.Clear();
                filterFrameworksOnKeyword.Add("All");
                using (MySqlCommand command = new MySqlCommand(selectFrameworkNames, conn))
                {
                    MySqlDataReader rdr = command.ExecuteReader();

                    while (rdr.Read())
                    {
                        if (rdr[0] != DBNull.Value)
                        {
                            string allKeywords = (string)rdr[0];
                            string[] arrayOfKeywords = allKeywords.Split('|');
                            foreach (string currentKeyword in arrayOfKeywords)
                            {
                                filterFrameworksOnKeyword.Add(currentKeyword);
                            }
                        }
                    }
                    rdr.Close();
                }
                filterFrameworksOnKeyword.RemoveAll(item => item == null);
                filterFrameworksOnKeyword.RemoveAll(item => item == "");





                string selectReferenceUUID = "select distinct(reference_uuid) from custom_actions";
                string selectCustomActions = "select name, description, icon from custom_actions";
                customActionName.Clear();
                customActionDescription.Clear();
                customActionIcon.Clear();
                customActionFullName.Clear();
                customActionHideBeforeStart.Clear();
                customActionHideAfterStart.Clear();
                customActionHideAfterComplete.Clear();

                using (MySqlCommand command = new MySqlCommand(selectReferenceUUID, conn))
                {
                    MySqlDataReader rdr = command.ExecuteReader();

                    while (rdr.Read())
                    {
                        if (rdr[0] != DBNull.Value)
                        {
                            customActionFullName.Add((string)rdr[0]);
                        }
                    }
                    rdr.Close();
                }
                customActionFullName.RemoveAll(item => item == null);
                customActionFullName.RemoveAll(item => item == "");
                foreach (string currentFullName in customActionFullName)
                {
                    selectCustomActions = "select name, description, icon, hide_before_start, hide_after_start, hide_after_complete from custom_actions where reference_uuid = '" + currentFullName + "'";
                    using (MySqlCommand command = new MySqlCommand(selectCustomActions, conn))
                    {
                        MySqlDataReader rdr = command.ExecuteReader();

                        while (rdr.Read())
                        {
                            if (rdr[0] != DBNull.Value)
                            {
                                customActionName.Add((string)rdr[0]);
                                customActionDescription.Add((string)rdr[1]);
                                customActionIcon.Add((string)rdr[2]);
                                customActionHideBeforeStart.Add((bool)rdr[3]);
                                customActionHideAfterStart.Add((bool)rdr[4]);
                                customActionHideAfterComplete.Add((bool)rdr[5]);
                            }
                        }
                        rdr.Close();
                    }
                }
                customActionName.RemoveAll(item => item == null);
                customActionName.RemoveAll(item => item == "");
                customActionDescription.RemoveAll(item => item == null);
                customActionDescription.RemoveAll(item => item == "");
                customActionIcon.RemoveAll(item => item == null);
                customActionIcon.RemoveAll(item => item == "");
                //customActionHideBeforeStart.RemoveAll(item => item == null);
                //customActionHideAfterStart.RemoveAll(item => item == null);
                //customActionHideAfterComplete.RemoveAll(item => item == null);






                foreach (string originatorUUID in mysqlDictionary.Keys)
                {
                    originatorUUIDs.Add(originatorUUID);
                }
                foreach (string displayName in mysqlDictionary.Values)
                {
                    displayNames.Add(displayName);
                }
                foreach (string originator in mysqlDictionary.Keys)
                {
                    string currentStateStr = "";
                    string currentStateColorStr = "black";
                    string startRunStr = "no";
                    string stopRunStr = "no";
                    string pauseRunStr = "no";
                    string resumeRunStr = "no";
                    string abortRunStr = "no";
                    string restartRunStr = "no";
                    bool startRunEnabled = false;
                    bool stopRunEnabled = false;
                    bool pauseRunEnabled = false;
                    bool resumeRunEnabled = false;
                    bool abortRunEnabled = false;
                    bool restartRunEnabled = false;

                    string selectState = "select state, color from state where reference_uuid = '" + originator + "' and order_in_system = (select MAX(order_in_system) from state where reference_uuid = '" + originator + "' and virtual_delete = 0)";
                    using (MySqlCommand command = new MySqlCommand(selectState, conn))
                    {
                        MySqlDataReader rdr = command.ExecuteReader();

                        while (rdr.Read())
                        {
                            currentStateStr = (string)rdr[0];
                            currentStateColorStr = (string)rdr[1];
                        }
                        rdr.Close();
                    }
                    string selectFrameworkFunctionality = "select start_supported, stop_supported, pause_supported, resume_supported, abort_supported, restart_supported from client_functionality where reference_uuid = '" + originator + "'";
                    using (MySqlCommand command = new MySqlCommand(selectFrameworkFunctionality, conn))
                    {
                        MySqlDataReader rdr = command.ExecuteReader();

                        while (rdr.Read())
                        {
                            startRunEnabled = (bool)rdr[0];
                            stopRunEnabled = (bool)rdr[1];
                            pauseRunEnabled = (bool)rdr[2];
                            resumeRunEnabled = (bool)rdr[3];
                            abortRunEnabled = (bool)rdr[4];
                            restartRunEnabled = (bool)rdr[5];
                        }
                        rdr.Close();
                    }

                    if (currentStateStr.ToUpper().StartsWith("RUNNING"))
                    {
                        startRunStr = "no";
                        if (stopRunEnabled)
                        {
                            stopRunStr = "yes";
                        }
                        if (pauseRunEnabled)
                        {
                            pauseRunStr = "yes";
                        }
                        resumeRunStr = "no";
                        if (abortRunEnabled)
                        {
                            abortRunStr = "yes";
                        }
                        restartRunStr = "no";
                    }
                    else if (currentStateStr.ToUpper().Equals("PAUSED"))
                    {
                        startRunStr = "no";
                        if (stopRunEnabled)
                        {
                            stopRunStr = "yes";
                        }
                        pauseRunStr = "no";
                        if (resumeRunEnabled)
                        {
                            resumeRunStr = "yes";
                        }
                        if (abortRunEnabled)
                        {
                            abortRunStr = "yes";
                        }
                        restartRunStr = "no";
                    }
                    else if (currentStateStr.ToUpper().Equals("RESUMED"))
                    {
                        startRunStr = "no";
                        if (stopRunEnabled)
                        {
                            stopRunStr = "yes";
                        }
                        if (pauseRunEnabled)
                        {
                            pauseRunStr = "yes";
                        }
                        resumeRunStr = "no";
                        if (abortRunEnabled)
                        {
                            abortRunStr = "yes";
                        }
                        restartRunStr = "no";
                    }
                    else if (currentStateStr.ToUpper().Equals("ABORTED"))
                    {
                        if (startRunEnabled)
                        {
                            startRunStr = "yes";
                        }
                        if (stopRunEnabled)
                        {
                            stopRunStr = "yes";
                        }
                        pauseRunStr = "no";
                        resumeRunStr = "no";
                        abortRunStr = "no";
                        if (restartRunEnabled)
                        {
                            restartRunStr = "yes";
                        }
                    }
                    else if (currentStateStr.ToUpper().Equals("RESTARTED"))
                    {
                        startRunStr = "no";
                        if (stopRunEnabled)
                        {
                            stopRunStr = "yes";
                        }
                        if (pauseRunEnabled)
                        {
                            pauseRunStr = "yes";
                        }
                        resumeRunStr = "no";
                        if (abortRunEnabled)
                        {
                            abortRunStr = "yes";
                        }
                        restartRunStr = "no";
                    }
                    else if (currentStateStr.ToUpper().Equals("STOPPED"))
                    {
                        if (startRunEnabled)
                        {
                            startRunStr = "yes";
                        }
                        stopRunStr = "no";
                        pauseRunStr = "no";
                        resumeRunStr = "no";
                        abortRunStr = "no";
                        restartRunStr = "no";
                    }
                    else if (currentStateStr.ToUpper().StartsWith("READY"))
                    {
                        if (startRunEnabled)
                        {
                            startRunStr = "yes";
                        }
                        stopRunStr = "no";
                        pauseRunStr = "no";
                        resumeRunStr = "no";
                        abortRunStr = "no";
                        restartRunStr = "no";
                    }
                    else if (currentStateStr.ToUpper().Equals("SCHEDULED"))
                    {
                        if (startRunEnabled)
                        {
                            startRunStr = "no";
                        }
                        stopRunStr = "no";
                        pauseRunStr = "no";
                        resumeRunStr = "no";
                        abortRunStr = "no";
                        restartRunStr = "no";
                    }
                    else if (currentStateStr.ToUpper().Equals("SHUTDOWN") || currentStateStr.ToUpper().StartsWith("OFFLINE"))
                    {
                        if (startRunEnabled)
                        {
                            startRunStr = "no";
                        }
                        stopRunStr = "no";
                        pauseRunStr = "no";
                        resumeRunStr = "no";
                        abortRunStr = "no";
                        restartRunStr = "no";
                    }
                    else if (currentStateStr.ToUpper().Equals("CLEANUP"))
                    {
                        if (startRunEnabled)
                        {
                            startRunStr = "no";
                        }
                        stopRunStr = "no";
                        pauseRunStr = "no";
                        resumeRunStr = "no";
                        abortRunStr = "no";
                        restartRunStr = "no";
                    }
                    else if (currentStateStr.ToUpper().StartsWith("COMPLETED ") || (currentStateStr.ToUpper().EndsWith(" WAS STOPPED")) || (currentStateStr.ToUpper().EndsWith(" WAS ABORTED")))
                    {
                        if (restartRunEnabled)
                        {
                            startRunStr = "yes";
                        }
                        else
                        {
                            startRunStr = "no";
                        }
                        if (stopRunEnabled)
                        {
                            stopRunStr = "no";
                        }
                        pauseRunStr = "no";
                        if (resumeRunEnabled)
                        {
                            resumeRunStr = "no";
                        }
                        if (abortRunEnabled)
                        {
                            abortRunStr = "no";
                        }
                        restartRunStr = "no";
                    }
                    currentState.Add(currentStateStr);
                    currentStateColor.Add(currentStateColorStr);
                    startRun.Add(startRunStr);
                    stopRun.Add(stopRunStr);
                    pauseRun.Add(pauseRunStr);
                    resumeRun.Add(resumeRunStr);
                    abortRun.Add(abortRunStr);
                    restartRun.Add(restartRunStr);

                }
            }
        }
        return null!;
    }

    /// <summary>
    /// Handles user login
    /// </summary>
    public IActionResult OnPostLogin(string username, string password)
    {
        if (_authService.ValidateCredentials(username, password))
        {
            var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{username}:{password}"));
            _authService.SetAuthenticationToken(token);
            _authService.SetUsername(username);
            return RedirectToPage("./cast_base");
        }
        else
        {
            AuthenticationError = "Invalid username or password";
            IsLoginRequired = true;
            IsAuthenticated = false;
            return Page();
        }
    }

    /// <summary>
    /// Handles user logout and clears cache
    /// </summary>
    public IActionResult OnPostLogout()
    {
        _authService.ClearAuthentication();
        return RedirectToPage("./cast_base");
    }

    /// <summary>
    /// Deprecated. Was used to simulate test runs
    /// </summary>
    /// <returns>IActionResult</returns>
    public async Task<IActionResult> OnPostMyAction()
    {
        factory.HostName = rabbitmq_home ?? "";
        factory.Port = int.Parse(rabbitmq_port ?? "5672");
        factory.UserName = rabbitmq_user ?? "";
        factory.Password = rabbitmq_pwd ?? "";
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        string clientUUID = SelectedValue ?? "";
        Console.WriteLine("clientUUID = " + clientUUID);

        //Create sample_test_script.txt in File_Storage_Service/temp/outbound_queue/UUID
        string localDirectoryReference = rootDir + "File_Storage_Service" + Path.DirectorySeparatorChar + "temp" + Path.DirectorySeparatorChar + "outbound_queue" + Path.DirectorySeparatorChar + clientUUID + Path.DirectorySeparatorChar;
        string localWorkingDirectoryReference = rootDir + "File_Storage_Service" + Path.DirectorySeparatorChar + "temp" + Path.DirectorySeparatorChar + "working_queue" + Path.DirectorySeparatorChar + clientUUID + Path.DirectorySeparatorChar;
        string fileName = "sample_test_script.txt";
        Directory.CreateDirectory(localDirectoryReference);
        Directory.CreateDirectory(localWorkingDirectoryReference);
        System.IO.File.WriteAllText(localWorkingDirectoryReference + fileName, "This is a sample test script");
        //Zip file to represent script package
        System.IO.Compression.ZipFile.CreateFromDirectory(localWorkingDirectoryReference, localDirectoryReference + "sample_test_script.zip");
        System.IO.File.Delete(localWorkingDirectoryReference + fileName);
        Directory.Delete(localWorkingDirectoryReference);

        string firstMessage = "message for " + clientUUID + ": local: action: simulate test run with test script";
        string lastMessage = "message for " + clientUUID + ": local: action: simulate test run";

        var body = Encoding.UTF8.GetBytes(firstMessage);
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "execution_service", body: body);
        Thread.Sleep(5000);

        body = Encoding.UTF8.GetBytes(lastMessage);
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "execution_service", body: body);

        return RedirectToPage("./cast_base");
    }

    /// <summary>
    /// Process Start Run Action and submit message to the Execution Service via RabbitMQ
    /// </summary>
    /// <returns>IActionResult</returns>
    public async Task<IActionResult> OnPostMyAction2(string id)
    {
        string originatorUUID = id;
        Console.WriteLine("StartRun for clientUUID = " + originatorUUID);
        factory.UserName = rabbitmq_user ?? "";
        factory.Password = rabbitmq_pwd ?? "";
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        string clientUUID = originatorUUID;
        string message = "message for client_service_" + clientUUID + ": local: action: start run";

        var body = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "execution_service", body: body);

        return RedirectToPage("./cast_base");
    }

    /// <summary>
    /// Process Stop Run Action and submit message to the Execution Service via RabbitMQ
    /// </summary>
    /// <returns>IActionResult</returns>
    public async Task<IActionResult> OnPostMyAction3(string id)
    {
        string originatorUUID = id;
        factory.UserName = rabbitmq_user ?? "";
        factory.Password = rabbitmq_pwd ?? "";
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        string clientUUID = originatorUUID;
        Console.WriteLine("StopRun for clientUUID = " + clientUUID);
        string message = "message for client_service_" + clientUUID + ": local: action: stop run";
        var body = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "execution_service", body: body);
        return RedirectToPage("./cast_base");
    }

    /// <summary>
    /// Process Pause Run Action and submit message to the Execution Service via RabbitMQ
    /// </summary>
    /// <returns>IActionResult</returns>
    public async Task<IActionResult> OnPostMyAction4(string id)
    {
        string originatorUUID = id;
        factory.UserName = rabbitmq_user ?? "";
        factory.Password = rabbitmq_pwd ?? "";
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        string clientUUID = originatorUUID;
        Console.WriteLine("PauseRun for clientUUID = " + clientUUID);
        string message = "message for client_service_" + clientUUID + ": local: action: pause run";
        var body = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "execution_service", body: body);
        return RedirectToPage("./cast_base");
    }

    /// <summary>
    /// Process Resume Run Action and submit message to the Execution Service via RabbitMQ
    /// </summary>
    /// <returns>IActionResult</returns>
    public async Task<IActionResult> OnPostMyAction5(string id)
    {
        string originatorUUID = id;
        factory.UserName = rabbitmq_user ?? "";
        factory.Password = rabbitmq_pwd ?? "";
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        string clientUUID = originatorUUID;
        Console.WriteLine("ResumeRun for clientUUID = " + clientUUID);
        string message = "message for client_service_" + clientUUID + ": local: action: resume run";
        var body = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "execution_service", body: body);
        return RedirectToPage("./cast_base");
    }

    /// <summary>
    /// Process Abort Run Action and submit message to the Execution Service via RabbitMQ
    /// </summary>
    /// <returns>IActionResult</returns>
    public async Task<IActionResult> OnPostMyAction6(string id)
    {
        string originatorUUID = id;
        factory.UserName = rabbitmq_user ?? "";
        factory.Password = rabbitmq_pwd ?? "";
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        string clientUUID = originatorUUID;
        Console.WriteLine("AbortRun for clientUUID = " + clientUUID);
        string message = "message for client_service_" + clientUUID + ": local: action: abort run";
        var body = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "execution_service", body: body);
        return RedirectToPage("./cast_base");
    }

    /// <summary>
    /// Process Restart Run Action and submit message to the Execution Service via RabbitMQ
    /// </summary>
    /// <returns>IActionResult</returns>
    public async Task<IActionResult> OnPostMyAction7(string id)
    {
        string originatorUUID = id;
        factory.UserName = rabbitmq_user ?? "";
        factory.Password = rabbitmq_pwd ?? "";
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        string clientUUID = originatorUUID;
        Console.WriteLine("RestartRun for clientUUID = " + clientUUID);
        string message = "message for client_service_" + clientUUID + ": local: action: restart run";
        var body = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "execution_service", body: body);
        return RedirectToPage("./cast_base");
    }

    /// <summary>
    /// Debug command
    /// </summary>
    /// <returns>IActionResult</returns>
    public async Task<IActionResult> OnPostMyAction8([FromBody] string model)
    {
        Console.WriteLine(model);

        return RedirectToPage("./cast_base");
    }

    /// <summary>
    /// Process Custom Action and submit message to the Execution Service via RabbitMQ
    /// </summary>
    /// <returns>IActionResult</returns>
    public async Task<IActionResult> OnPostMyAction9(string id)
    {
        Console.WriteLine("Action is " + id);
        string originatorUUID = id.Substring(0, id.IndexOf("|"));
        string action = id.Substring(id.IndexOf("|") + 1);
        factory.UserName = rabbitmq_user ?? "";
        factory.Password = rabbitmq_pwd ?? "";
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        string clientUUID = originatorUUID;
        Console.WriteLine("CustomAction for " + originatorUUID + " = " + action);
        string message = "message for client_service_" + clientUUID + ": local: action: custom action " + action;
        var body = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "execution_service", body: body);
        return RedirectToPage("./cast_base");
    }

    /// <summary>
    /// Process Cleanup Action and submit message to the Execution Service via RabbitMQ
    /// </summary>
    /// <returns>IActionResult</returns>
    public async Task<IActionResult> OnPostMyAction10(string id)
    {
        string originatorUUID = id;
        Console.WriteLine("Cleanup clientUUID = " + originatorUUID);
        factory.UserName = rabbitmq_user ?? "";
        factory.Password = rabbitmq_pwd ?? "";
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        string clientUUID = originatorUUID;

        Guid startmyuuid = Guid.NewGuid();
        string startmyuuidAsString = startmyuuid.ToString();
        string updateState = "insert into state(uuid, reference_uuid, state, event_time_dt) values('" + startmyuuidAsString + "', '" + id + "', 'OFFLINE', NOW())";
        cleanupClient(updateState);
        updateSelectStatement = false;
        return RedirectToPage("./cast_base");
    }

    /// <summary>
    /// Submit a Schedule Request to the logger Service via RabbitMQ
    /// </summary>
    /// <returns>IActionResult</returns>
    public async void submitSchedule(string insertSchedule)
    {
        var factory = new ConnectionFactory();
        factory.HostName = rabbitmq_home ?? "";
        factory.Port = int.Parse(rabbitmq_port ?? "5672");
        factory.UserName = rabbitmq_user ?? "";
        factory.Password = rabbitmq_pwd ?? "";
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        var body = Encoding.UTF8.GetBytes(insertSchedule);
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body);
    }

    /// <summary>
    /// Submit a Schedule Request to the logger Service via RabbitMQ
    /// </summary>
    /// <returns>IActionResult</returns>
    public async void cleanupClient(string cleanupStatement)
    {
        var factory = new ConnectionFactory();
        factory.HostName = rabbitmq_home ?? "";
        factory.Port = int.Parse(rabbitmq_port ?? "5672");
        factory.UserName = rabbitmq_user ?? "";
        factory.Password = rabbitmq_pwd ?? "";
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        var body = Encoding.UTF8.GetBytes(cleanupStatement);
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body);
    }
}
