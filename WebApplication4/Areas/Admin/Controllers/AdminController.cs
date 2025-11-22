using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCoza.MVC.Areas.Admin.Controllers
{

    [Authorize(Roles = "Admin")]
    public abstract class AdminController : Controller
    {
    }
}