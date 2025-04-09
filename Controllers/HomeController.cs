using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OTP_Share.Models;
using OTP_Share.Services;

namespace OTP_Share.Controllers;

public class HomeController : Controller
{
  private readonly ILogger<HomeController> _logger;
  private readonly DatalayerService _datalayerService;

  public HomeController(ILogger<HomeController> logger, DatalayerService datalayer)
  {
    _logger = logger;
    _datalayerService = datalayer;
  }

  public IActionResult Index()
  {
    return View();
  }

  public IActionResult Privacy()
  {
    return View();
  }

  [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
  public IActionResult Error()
  {
    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
  }
}