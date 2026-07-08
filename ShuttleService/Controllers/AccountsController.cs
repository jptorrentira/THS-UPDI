using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShuttleService.Data;
using ShuttleService.Models;

using System.Net;
using System.Net.Mail;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace ShuttleService.Controllers
{
    public class AccountsController : Controller
    {
        private readonly ApplicationDbContext _context; // database connection
        private readonly UserManager<ApplicationUser> _userManager; // database connection for user management only
        private readonly SignInManager<ApplicationUser> _signInManager; // database connection for login
        string _customLocation = "makati";

        public AccountsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) // constructor for database connection
        {
            _context = context;
            _userManager = userManager; //for user management only
            _signInManager = signInManager; //for user login only
        }


        [Authorize(Policy = "RequireAdminRole")]
        public IActionResult Index()
        {
            ViewBag.Roles = new SelectList(_context.Roles, "Id", "Name");
            return View();
        }

        public IActionResult Login(string ReturnUrl = "")
        {
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }
            ViewBag.ReturnUrl = ReturnUrl;

            //if (System.Environment.MachineName == "ALUMINUM")
            if (System.Environment.MachineName == "CALIFORNIUM")
            {
                return View("Redirect");
            }
            else
            {
                return View();
            }

        }

        //[HttpPost]
        //public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        //{
        //    @ViewBag.m = "windows";
        //    returnUrl = returnUrl ?? Url.Content("~/");
        //    string domain = "";
        //    if (ModelState.IsValid)
        //    {
        //        string loginresult = "";
        //        if (Request.Form["ltype"] == "local") {
        //            var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);
        //            if (result.Succeeded)
        //            {
        //                loginresult = "OK";
        //            }
        //            domain = "";
        //        }
        //        else {
        //            //api for login

        //             loginresult = CallAPI("http://aluminum/ADAPI/api/values", model.Domain, model.UserName, model.Password);

        //            //end api for login


        //            //check the user from another database
        //            loginresult = loginresult.Replace("\"", "");
        //            domain = model.Domain;
        //        }

        //        if (loginresult == "OK")
        //        {
        //            var userList = _context.Users.ToList();
        //            var user = _context.Users
        //                .Include(u => u.Employee)
        //                    .ThenInclude(e => e.CompanyGroup)
        //                .FirstOrDefault(u => u.UserName == model.UserName && u.Domain == domain);


        //            if (user != null)
        //            {
        //                //you could firstly get the user name and password from the database then you could create the application user and use SignInAsync to login the user.
        //                //var userData = new ApplicationUser { UserName = model.UserName, Domain = model.Domain };

        //                if (user.Status != 1) {
        //                    ModelState.AddModelError(string.Empty, "Account disabled. Please contact the Administrator.");
        //                    return View();
        //                }


        //                var usermanagerData = await _userManager.FindByIdAsync(user.Id);

        //                await _signInManager.SignInAsync(usermanagerData, false);

        //                //string SystemName_ = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
        //                string SystemName_ = $"{this.Request.Scheme}://{this.Request.Host}-{this.Request.PathBase}";
        //                var option = new CookieOptions();
        //                option.Expires = DateTime.Now.AddMinutes(480);
        //                Response.Cookies.Append("testCookie", user.Domain + "/" + user.UserName, option);
        //                Response.Cookies.Append("userDomainWithName", user.Domain + "/" + user.UserName, option);
        //                Response.Cookies.Append("employeeId", user.EmployeeId.ToString(), option);
        //                Response.Cookies.Append("fullname", user.DisplayName, option);
        //                Response.Cookies.Append("companyGroup", user.Employee.CompanyGroup.CompanyGroupName, option);
        //                Response.Cookies.Append("showGuideline", "1");
        //                Response.Cookies.Append("SystemName", SystemName_);


        //                //HttpContext.Session.GetString("Session_employeeId");
        //                //var Session_employeeId = HttpContext.Session.GetString("Session_employeeId");

        //                HttpContext.Session.SetString("Session_testCookie", user.Domain + "/" + user.UserName);
        //                HttpContext.Session.SetString("Session_userDomainWithName", user.Domain + "/" + user.UserName);
        //                HttpContext.Session.SetString("Session_employeeId", user.EmployeeId.ToString());
        //                HttpContext.Session.SetString("Session_fullname", user.DisplayName);
        //                HttpContext.Session.SetString("Session_location", "makati");
        //                HttpContext.Session.SetString("Session_systemname", SystemName_);
        //                if (System.Environment.MachineName == "ANDROMEDA" || System.Environment.MachineName == "PEGASUS" || _customLocation == "calaca")
        //                {
        //                    HttpContext.Session.SetString("Session_location", "calaca");
        //                }
        //                else if(System.Environment.MachineName == "CRONUS")
        //                {
        //                    HttpContext.Session.SetString("Session_location", "minesite");
        //                }
        //                else
        //                {
        //                    HttpContext.Session.SetString("Session_location", "makati");
        //                }

        //                //return LocalRedirect(returnUrl);
        //                return RedirectToAction("Index","Home");

        //            }
        //            else {
        //                ModelState.AddModelError(string.Empty, "User does not exist. Please register or contact the Administrator.");
        //            }



        //        }
        //        else {
        //            loginresult = loginresult.Replace(".", ". ");
        //            ModelState.AddModelError(string.Empty, loginresult);
        //        }


        //        // This doesn't count login failures towards account lockout
        //        // To enable password failures to trigger account lockout, set lockoutOnFailure: true
        //        //var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);
        //        //if (result.Succeeded)
        //        //{
        //        //    //return LocalRedirect(returnUrl);
        //        //    return LocalRedirect("/Home");
        //        //}

        //        //else
        //        //{
        //        //    ModelState.AddModelError(string.Empty, "Invalid username and password.");

        //        //}
        //    }
        //    return View();
        //}

        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        //{
        //    // Normalize ReturnUrl for virtual directory hosting
        //    if (string.IsNullOrEmpty(returnUrl))
        //    {
        //        returnUrl = $"{Request.PathBase}/Home/Index";
        //    }

        //    if (!ModelState.IsValid)
        //    {
        //        ViewBag.ReturnUrl = returnUrl;
        //        return View(model);
        //    }

        //    string loginResult = string.Empty;
        //    string domain = string.Empty;

        //    // ================= LOCAL LOGIN =================
        //    if (Request.Form["ltype"] == "local")
        //    {
        //        var result = await _signInManager.PasswordSignInAsync(
        //            model.UserName,
        //            model.Password,
        //            model.RememberMe,
        //            lockoutOnFailure: true
        //        );

        //        if (result.Succeeded)
        //        {
        //            loginResult = "OK";
        //        }
        //    }
        //    // ================= DOMAIN LOGIN =================
        //    else
        //    {
        //        loginResult = CallAPI(
        //            "http://aluminum/ADAPI/api/values",
        //            model.Domain,
        //            model.UserName,
        //            model.Password
        //        );

        //        loginResult = loginResult.Replace("\"", "");
        //        domain = model.Domain;
        //    }

        //    // ================= LOGIN FAILED =================
        //    if (loginResult != "OK")
        //    {
        //        ModelState.AddModelError(string.Empty, loginResult.Replace(".", ". "));
        //        ViewBag.ReturnUrl = returnUrl;
        //        return View(model);
        //    }

        //    // ================= LOAD USER =================
        //    var user = _context.Users
        //        .Include(u => u.Employee)
        //            .ThenInclude(e => e.CompanyGroup)
        //        .FirstOrDefault(u =>
        //            u.UserName == model.UserName &&
        //            u.Domain == domain
        //        );

        //    if (user == null)
        //    {
        //        ModelState.AddModelError(string.Empty, "User does not exist. Please contact the Administrator.");
        //        ViewBag.ReturnUrl = returnUrl;
        //        return View(model);
        //    }

        //    if (user.Status != 1)
        //    {
        //        ModelState.AddModelError(string.Empty, "Account disabled. Please contact the Administrator.");
        //        ViewBag.ReturnUrl = returnUrl;
        //        return View(model);
        //    }

        //    // ================= SIGN IN =================
        //    var identityUser = await _userManager.FindByIdAsync(user.Id);
        //    await _signInManager.SignInAsync(identityUser, isPersistent: false);

        //    // ================= COOKIES =================
        //    var cookieOptions = new CookieOptions
        //    {
        //        Expires = DateTime.Now.AddMinutes(480)
        //    };

        //    string systemName = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        //    Response.Cookies.Append("testCookie", $"{user.Domain}/{user.UserName}", cookieOptions);
        //    Response.Cookies.Append("userDomainWithName", $"{user.Domain}/{user.UserName}", cookieOptions);
        //    Response.Cookies.Append("employeeId", user.EmployeeId.ToString(), cookieOptions);
        //    Response.Cookies.Append("fullname", user.DisplayName, cookieOptions);
        //    Response.Cookies.Append("companyGroup", user.Employee.CompanyGroup.CompanyGroupName, cookieOptions);
        //    Response.Cookies.Append("showGuideline", "1", cookieOptions);
        //    Response.Cookies.Append("SystemName", systemName, cookieOptions);

        //    // ================= SESSION =================
        //    HttpContext.Session.SetString("Session_testCookie", $"{user.Domain}/{user.UserName}");
        //    HttpContext.Session.SetString("Session_userDomainWithName", $"{user.Domain}/{user.UserName}");
        //    HttpContext.Session.SetString("Session_employeeId", user.EmployeeId.ToString());
        //    HttpContext.Session.SetString("Session_fullname", user.DisplayName);
        //    HttpContext.Session.SetString("Session_systemname", systemName);

        //    if (System.Environment.MachineName == "ANDROMEDA" ||
        //        System.Environment.MachineName == "PEGASUS" ||
        //        _customLocation == "calaca")
        //    {
        //        HttpContext.Session.SetString("Session_location", "calaca");
        //    }
        //    else if (System.Environment.MachineName == "CRONUS")
        //    {
        //        HttpContext.Session.SetString("Session_location", "minesite");
        //    }
        //    else
        //    {
        //        HttpContext.Session.SetString("Session_location", "makati");
        //    }

        //    // ================= REDIRECT (CORRECT) =================
        //    //if (Url.IsLocalUrl(returnUrl))
        //    //{
        //    //    return LocalRedirect(returnUrl);
        //    //}

        //    return LocalRedirect($"{Request.PathBase}/Home/Index");
        //}

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");


            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            bool loginSuccess = false;
            string domain = model.Domain ?? string.Empty;

            // ================= LOCAL LOGIN =================
            if (Request.Form["ltype"] == "local")
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.UserName,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: true
                );

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Invalid username/password.");
                    ViewBag.ReturnUrl = returnUrl;
                    return View(model);
                }

                loginSuccess = true;
                domain = model.Domain; // or set to your local domain value
            }
            // ================= DOMAIN LOGIN =================
            else
            {
                var apiResult = CallAPI(
                    "http://aluminum/ADAPI/api/values",
                    model.Domain,
                    model.UserName,
                    model.Password
                )?.Replace("\"", "");

                if (apiResult != "OK")
                {
                    ModelState.AddModelError("", apiResult?.Replace(".", ". "));
                    ViewBag.ReturnUrl = returnUrl;
                    return View(model);
                }

                loginSuccess = true;
            }

            if (!loginSuccess)
            {
                ModelState.AddModelError("", "Login failed.");
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            // ================= LOAD USER =================
            var user = await _context.Users
                .Include(u => u.Employee)
                    .ThenInclude(e => e.CompanyGroup)
                .FirstOrDefaultAsync(u =>
                    u.UserName == model.UserName &&
                    u.Domain == domain
                );

            if (user == null)
            {
                ModelState.AddModelError("", "User does not exist. Please contact the Administrator.");
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            if (user.Status != 1)
            {
                await _signInManager.SignOutAsync();
                ModelState.AddModelError("", "Account disabled. Please contact the Administrator.");
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            // ================= SIGN IN (ONLY FOR DOMAIN LOGIN) =================
            if (Request.Form["ltype"] != "local")
            {
                var identityUser = await _userManager.FindByIdAsync(user.Id);
                await _signInManager.SignInAsync(identityUser, isPersistent: false);
            }

            // ================= COOKIES =================
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(480),
                HttpOnly = true,
                IsEssential = true
            };

            string systemName = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

            Response.Cookies.Append("testCookie", $"{user.Domain}/{user.UserName}", cookieOptions);
            Response.Cookies.Append("userDomainWithName", $"{user.Domain}/{user.UserName}", cookieOptions);
            Response.Cookies.Append("employeeId", user.EmployeeId.ToString(), cookieOptions);
            Response.Cookies.Append("fullname", user.DisplayName, cookieOptions);
            Response.Cookies.Append("companyGroup", user.Employee?.CompanyGroup?.CompanyGroupName ?? "", cookieOptions);
            Response.Cookies.Append("showGuideline", "1", cookieOptions);
            Response.Cookies.Append("SystemName", systemName, cookieOptions);

            // ================= SESSION =================
            HttpContext.Session.SetString("Session_employeeId", user.EmployeeId.ToString());
            HttpContext.Session.SetString("Session_fullname", user.DisplayName);
            HttpContext.Session.SetString("Session_systemname", systemName);

            string location = "makati";

            if (Environment.MachineName == "ANDROMEDA" ||
                Environment.MachineName == "PEGASUS" ||
                _customLocation == "calaca")
            {
                location = "calaca";
            }
            else if (Environment.MachineName == "CRONUS")
            {
                location = "minesite";
            }

            HttpContext.Session.SetString("Session_location", location);

            // ================= SAFE REDIRECT =================
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");

        }



        [HttpPost]
        public async Task<IActionResult> LocalLogin(LocalLoginViewModel model, string returnUrl = null)
        {
            ViewBag.m = "local";
            returnUrl = returnUrl ?? Url.Content("~/");
 
            if (ModelState.IsValid)
            {
                string loginresult = "";
                if (Request.Form["ltype"] == "local")
                {
                    var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);
                    if (result.Succeeded)
                    {
                        loginresult = "OK";
                    }
                    else
                    {

                        ModelState.AddModelError(string.Empty, "Invalid username/password.");
                        return View("Login");

                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "error.");
                    return View("Login");
                }

                if (loginresult == "OK")
                {

                    var user = _context.Users
                        .Include(u => u.Employee)
                            .ThenInclude(e => e.CompanyGroup)
                        .FirstOrDefault(u => u.UserName == model.UserName);


                    if (user != null)
                    {
                        //you could firstly get the user name and password from the database then you could create the application user and use SignInAsync to login the user.
                        //var userData = new ApplicationUser { UserName = model.UserName, Domain = model.Domain };

                        if (user.Status != 1)
                        {
                            ModelState.AddModelError(string.Empty, "Account disabled. Please contact the Administrator.");
                            return View("Login");
                        }


                        var usermanagerData = await _userManager.FindByIdAsync(user.Id);

                        await _signInManager.SignInAsync(usermanagerData, false);

                        var option = new CookieOptions();
                        string SystemName_ = $"{this.Request.Scheme}://{this.Request.Host}-{this.Request.PathBase}";
                        option.Expires = DateTime.Now.AddMinutes(480);
                        Response.Cookies.Append("testCookie", user.Domain + "/" + user.UserName, option);
                        Response.Cookies.Append("userDomainWithName", user.Domain + "/" + user.UserName, option);
                        Response.Cookies.Append("employeeId", user.EmployeeId.ToString(), option);
                        Response.Cookies.Append("fullname", user.DisplayName, option);
                        Response.Cookies.Append("companyGroup", user.Employee.CompanyGroup.CompanyGroupName, option);
                        Response.Cookies.Append("showGuideline", "1");
                        Response.Cookies.Append("SystemName", SystemName_);



                        HttpContext.Session.SetString("Session_testCookie", user.Domain + "/" + user.UserName);
                        HttpContext.Session.SetString("Session_userDomainWithName", user.Domain + "/" + user.UserName);
                        HttpContext.Session.SetString("Session_employeeId", user.EmployeeId.ToString());
                        HttpContext.Session.SetString("Session_fullname", user.DisplayName);
                        HttpContext.Session.SetString("Session_location", "makati");
                        HttpContext.Session.SetString("Session_systemname", SystemName_);
                        if (System.Environment.MachineName == "ANDROMEDA" || System.Environment.MachineName == "PEGASUS" || _customLocation == "calaca")
                        {
                            HttpContext.Session.SetString("Session_location", "calaca");
                        }
                        else if (System.Environment.MachineName == "CRONUS")
                        {
                            HttpContext.Session.SetString("Session_location", "minesite");
                        }
                        else
                        {
                            HttpContext.Session.SetString("Session_location", "makati");
                        }



                        //return LocalRedirect(returnUrl);
                        return LocalRedirect(returnUrl);

                        //return RedirectToAction("Index","Home");

                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "User does not exist. Please register or contact the Administrator.");
                    }



                }
                else
                {
                    //ViewBag.m = "local";
                    loginresult = loginresult.Replace(".", ". ");
                    ModelState.AddModelError(string.Empty, loginresult);
                }


                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                //var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);
                //if (result.Succeeded)
                //{
                //    //return LocalRedirect(returnUrl);
                //    return LocalRedirect("/Home");
                //}

                //else
                //{
                //    ModelState.AddModelError(string.Empty, "Invalid username and password.");

                //}
            }

            return View("Login");
        }



        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> ShowUserLists()
        {
           

            // Initialization.
            string search = Request.Form["search[value]"];
            string draw = Request.Form["draw"];
            string order = Request.Form["order[0][column]"];
            string orderDir = Request.Form["order[0][dir]"];
            int startRec = Convert.ToInt32(Request.Form["start"]);
            int pageSize = Convert.ToInt32(Request.Form["length"]);

            var currentUser = await _userManager.GetUserAsync(User);
            var currentCompanyGroupId = _context.Employees.Where(emp => emp.Id == currentUser.EmployeeId)
                .Select(e => e.CompanyGroupId).FirstOrDefault();

            var DataList = _userManager.Users.Select(u => new {
                
                Id = u.Id,
                Company = u.Employee.CompanyList.CompanyName,
                EmployeeNo = u.Employee.EmployeeNo,
                DisplayName = u.DisplayName,
                UserName = u.UserName,
                Domain = u.Domain,
                Status = u.Status == 0 ? "<label style='color:#ec4758;'>Inactive</label>" : "<label style='color:#1ab394'>Active</label>",
                //Action = u.Status == 0 ? "<button class='btn btn-success dim btn-sm' onClick='EnabeUser(`"+ u.Id + "`)' title='Enable' type='button'><i class='fa fa-check'></i></button>" : "<button onClick='DisableUser(`" + u.Id + "`)'  class='btn btn-danger dim btn-sm'  title='Disable' type='button'><i class='fa fa-remove'></i></button><button onClick='EditUser(`" + u.Id + "`)'  class='btn btn-success dim btn-sm' title='Update Role' type='button'><i class='fa fa-edit'></i></button>"
                Action = u.Status == 0 ? "" : "<a href='javascript:void(0);' onClick='EditUser(`" + u.Id + "`)'  class='btn btn-sm btn-primary' > Update Role </a> ",
                CompanyGroupId = u.Employee.CompanyGroupId

            });

            DataList = DataList.Where(d => d.CompanyGroupId == currentCompanyGroupId);


            //var _user = _context.Users.FirstOrDefault(x => x.Id == Id);

            //var roles = _userManager.GetRolesAsync(_user);

            //ViewBag.UserRoles = "";
            //if (roles != null && roles.Count > 0)
            //{
            //    ViewBag.UserRoles = roles[0];
            //}


            //var DataList = _context.Employees.
            //                Join(_context.Employees,
            //                        emp1 => emp1.SupervisorEmployeeNo,
            //                        emp2 => emp2.EmployeeNo,
            //                        (emp1, emp2) => new { emp1, emp2 }).
            //                OrderBy(u => u.emp1.FirstName).
            //                Select(u => new
            //                {
            //                    Id = "<a href='" + @Url.Content("~/employees") + "/edit/" + u.emp1.Id + "'  class='btn btn-sm btn-primary' > View/Update </a>",
            //                    Company = u.emp1.CompanyList.CompanyName,
            //                    EmployeeID = u.emp1.EmployeeNo,
            //                    FirstName = u.emp1.FirstName,
            //                    MiddleName = u.emp1.MiddleName,
            //                    LastName = u.emp1.LastName,
            //                    FullName = u.emp1.FirstName + " " + (u.emp1.MiddleName == null ? "" : u.emp1.MiddleName[0] + ". ") + u.emp1.LastName,
            //                    Position = u.emp1.Position,
            //                    IsSupervisor = u.emp1.IsImmediateHead == false ? "" : "Yes",
            //                    Supervisor = u.emp2.FirstName + " " + u.emp2.LastName
            //                });


            // Total record count.
            int totalRecords = DataList.Count();

            // Verification.
            if (!string.IsNullOrEmpty(search))
            {   // Apply search
                DataList = DataList.Where(x => x.DisplayName.ToLower().Contains(search.ToLower()) || x.Company.Contains(search.ToLower()) ||  x.EmployeeNo.Contains(search.ToLower()) || x.UserName.Contains(search.ToLower()) || x.Domain.Contains(search.ToLower()) || x.Status.Contains(search.ToLower()) );
            }
            // Sorting.
            //string[] sort = new string[] { "Name", "Name" };
            //var sortfield = sort[int.Parse(order)];
            //DataList = DataList.OrderBy(x=> x.Name);

            // Filter record count.
            int recFilter = DataList.Count();

            // Apply pagination.
            DataList = DataList.Skip(startRec).Take(pageSize);

            var jsonData = new
            {
                draw = Convert.ToInt32(draw),
                recordsTotal = totalRecords,
                recordsFiltered = recFilter,
                data = DataList.ToList(),
            };

            return new JsonResult(jsonData);

        }




        [HttpPost]
        public ActionResult checkAD()
        {


            var _empResult = _context.Employees.FirstOrDefault(e => e.EmployeeNo.Equals(Request.Form["employeeId"].ToString().ToLower()));
            var _userResult = 0;
            var _empResultCount = 0;
            var _userADResult = 0;
            if (_empResult != null) {
                _userResult = _context.Users.Count(e => e.EmployeeId.Equals(_empResult.Id));
                _empResultCount = 1;
                if (_userResult <= 0) {
                    _userADResult = _context.Users.Count(e => e.UserName.Equals(Request.Form["userName"].ToString().ToLower()) && e.Domain.Equals(Request.Form["domain"].ToString().ToLower()) );
                }
            }
            


            //api for login

            string loginresult = CallAPI("http://aluminum/ADAPI/api/values", Request.Form["domain"].ToString().ToLower(), Request.Form["userName"].ToString().ToLower(), Request.Form["password"].ToString());

            //end api for login

            
            //check the user from another database
            loginresult = loginresult.Replace("\"", "");

            
            var jsonData = new
            {
                result = loginresult,
                empResult = _empResultCount,
                userResult = _userResult,
                userADResult = _userADResult,
                employeeData = _empResult
            };



            return new JsonResult(jsonData);

        }


        [HttpPost]
        public async Task<ActionResult> checkLocal()
        {


            var _empResult = _context.Employees.FirstOrDefault(e => e.EmployeeNo.Equals(Request.Form["employeeId"].ToString().ToLower()));
            var _userResult = 0;
            var _empResultCount = 0;
            var _userADResult = 0;
            if (_empResult != null)
            {
                _userResult = _context.Users.Count(e => e.EmployeeId.Equals(_empResult.Id));
                _empResultCount = 1;
                if (_userResult <= 0)
                {
                    _userADResult = _context.Users.Count(e => e.UserName.Equals(Request.Form["employeeId"].ToString().ToLower()) && e.Domain.Equals("local"));
                }
            }


            string loginresult = "OK";

            ////api for login

            //string loginresult = CallAPI("http://aluminum/ADAPI/api/values", Request.Form["domain"].ToString().ToLower(), Request.Form["userName"].ToString().ToLower(), Request.Form["password"].ToString());

            ////end api for login


            ////check the user from another database
            //loginresult = loginresult.Replace("\"", "");


            var jsonData = new
            {
                result = loginresult,
                empResult = _empResultCount,
                userResult = _userResult,
                userADResult = _userADResult,
                employeeData = _empResult
            };



            return new JsonResult(jsonData);

        }

        

        public IActionResult AccessDenied()
        {

            return View();

        }
        public IActionResult Register()
        {
            return View();

        }        

        public string CallAPI(string url, string Domain, string UserName, string Password)
        {
            using (var wb = new WebClient())
            {
                var data = new NameValueCollection();
                data["Domain"] = Domain;
                data["Username"] = UserName;
                data["Password"] = Password;
      

                var response = wb.UploadValues(url, "POST", data);
                string responseInString = Encoding.UTF8.GetString(response);

                return responseInString;
            }

        }
        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_context.DepartmentLists, "Id", "DepartmentName");
            return View();
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TemporaryUser temporaryUser)
        {
            var returnSuccess = 1;
            var returnMessage = "";

            using (var transaction = _context.Database.BeginTransaction())
            {

                try
                {           //it can be put on after create( =>[Bind("Status,Id,Name,NormalizedName,ConcurrencyStamp")]  )

                    var checkWindows = Request.Form["checkwindows"].ToString();
                            
                    var employeeValue = _context.Employees.FirstOrDefault(e=>e.EmployeeNo.Equals(temporaryUser.EmployeeId));

                    string username = "";
                    if (checkWindows == "1")
                    {
                        username = temporaryUser.UserName;
                    }
                    else if (checkWindows == "0")
                    {
                        username = temporaryUser.EmployeeId;

                    }

                    var user = new ApplicationUser
                    {
                        EmployeeId = employeeValue.Id,
                        Domain = temporaryUser.Domain,
                        UserName = username,
                        Email = temporaryUser.Email,
                        PhoneNumber = temporaryUser.PhoneNumber,
                        DisplayName = employeeValue.FirstName + " " + (employeeValue.MiddleName == null || employeeValue.MiddleName == "" ? "" : employeeValue.MiddleName[0] + ". ") + employeeValue.LastName,
                        DepartmentId = employeeValue.DepartmentListId,
                        Status = 1
                    };

                    IdentityResult result = new IdentityResult();
                    if (checkWindows == "1")
                    {
                            result = await _userManager.CreateAsync(user);
                    }
                    else if (checkWindows == "0")
                    {
                            result = await _userManager.CreateAsync(user, temporaryUser.Password);

                    }
                    else {
                        throw new System.Exception("Account method error.");
                    }

                    if (result.Succeeded)
                    {
                        //update employee table
                        var employees = _context.Employees.Where(e => e.EmployeeNo == temporaryUser.EmployeeId);

                        if (employees.Count() <= 0)
                        {
                            throw new System.Exception("Employee not found.");
                        }

                        await employees.ForEachAsync(e => {
                            e.AlternativeEmail = temporaryUser.AlternateEmail;
                            e.CompanyEmail = temporaryUser.Email;
                            e.LocalNumber = temporaryUser.LocalNumber;
                            e.MobileNumber = temporaryUser.PhoneNumber;
                        });
                        await _context.SaveChangesAsync();
                        //end update

                        var result1 =  await _userManager.AddToRoleAsync(user, "Requestor");
                    }
                    foreach (var error in result.Errors)
                    {
                        throw new System.Exception("Error on saving user. Code: " + error.Description);
                    }

                    returnMessage = "Success";
                    returnSuccess = 1;
                    transaction.Commit();

                }
                catch (Exception e)
                {
                    returnSuccess = 0;
                    returnMessage = e.Message;
                    transaction.Rollback();
                    //throw e;
                }

            }

            var jsonData = new
            {
                message = returnMessage,
                success = returnSuccess
            };
            return new JsonResult(jsonData);

        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost]
        public async Task<IActionResult> updateRole(string id)
        {

            //var _rolename = "";
            var User = _userManager.Users.Where(r => r.Id == id).Select(u => new {
                EmployeeNumber = u.Employee.EmployeeNo,
                Domain = u.Domain,
                UserName = u.UserName,
                DisplayName = u.DisplayName,
                Company = u.Employee.CompanyList.CompanyName,
                UserId = u.Id,
            });


            var _user = _context.Users.AsNoTracking().FirstOrDefault(x => x.Id == id);

            var roles = await _userManager.GetRolesAsync(_user);

            var checkroleIds = _context.Roles.AsNoTracking().Where(r => roles.Contains(r.Name)).Select(u => new {
                RoleIds = u.Id
            }); 


            var jsonData = new
            {
                userData = User.ToList().ToArray(),
                roleName = roles,
                roleId = checkroleIds.ToList(),
                Success = 1
            };

            return new JsonResult(jsonData);

        }


        [Authorize]
        [HttpGet]
        public IActionResult Logout()
        {
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

      


        [HttpPost]
        public async Task<IActionResult> saveUserRole(string UserId)
        {

            var returnSuccess = 1;
            var returnMessage = "";

            var roleIds = Request.Form["RoleId"].ToString();
            string[] roleArray = roleIds.Split(",");
            //End Manage Parameters
          
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {

                    var _user = _context.Users.FirstOrDefault(x => x.Id == UserId);

                    foreach (var _roleid in roleArray)
                    {
                        var getRoleName = _context.Roles.Where(x => x.Id == _roleid).Select(u => new {
                            RoleName = u.Name
                        }).ToList().ToArray();
                        foreach (var _roleidSave in getRoleName)
                        {
                             await _userManager.AddToRoleAsync(_user, _roleidSave.RoleName);
                     
                        }
                           
                    }

                    var getRoleNotChosenName = _context.Roles.Where(x => !roleArray.Contains(x.Id)).Select(u => new {
                        RoleName = u.Name
                    }).ToList().ToArray();


                    foreach (var _removeRole in getRoleNotChosenName)
                    {
                        await _userManager.RemoveFromRoleAsync(_user, _removeRole.RoleName);

                    }



                    returnMessage = "Success";
                    returnSuccess = 1;
                    transaction.Commit();

                }
                catch (Exception e)
                {
                    returnSuccess = 0;
                    returnMessage = e.Message;
                    transaction.Rollback();
                    //throw e;
                }

            }

            var jsonData = new
            {
                message = returnMessage,
                success = returnSuccess
            };
            return new JsonResult(jsonData);
        }


        public string GetApproverMobileNumber()
        {
            string mobile = "";

            var users = _userManager.GetUsersInRoleAsync("RoleName").Result.ToList().ToArray();



            return mobile;

        }


        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmationNull()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmationNotLocal()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel forgotPasswordModel)
        {
            if (!ModelState.IsValid)
                return View(forgotPasswordModel);

            var user = await _userManager.FindByEmailAsync(forgotPasswordModel.Email);
            if (user == null)
                return RedirectToAction(nameof(ForgotPasswordConfirmationNull));

            if (user.Domain != null)
            {
                return RedirectToAction(nameof(ForgotPasswordConfirmationNotLocal));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callback = Url.Action(nameof(ResetPassword), "Accounts", new { token, email = user.Email }, Request.Scheme);

            //var message = new Message(new string[] { "ebelayda@semirarampc.com" }, "Reset password token", callback, null);
            // await _emailSender.SendEmailAsync(message);


            MailMessage mail = new MailMessage();
            //SmtpClient SmtpServer = new SmtpClient("mail.hoaccess.com");
            SmtpClient SmtpServer = new SmtpClient("relay.smcdacon.com");
            mail.From = new MailAddress("webhelpdeskadmin@semirarampc.com", "Transport Hub System");
            mail.To.Add(forgotPasswordModel.Email);
            mail.Subject = "RESET PASSWORD - TRANSPORT HUB SYSTEM";
            mail.Body = callback;
            mail.IsBodyHtml = true;
            SmtpServer.Send(mail);

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPasswordModel { Token = token, Email = email };
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel resetPasswordModel)
        {
            if (!ModelState.IsValid)
                return View(resetPasswordModel);

            var user = await _userManager.FindByEmailAsync(resetPasswordModel.Email);
            if (user == null)
                RedirectToAction(nameof(ResetPasswordConfirmation));




            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPasswordModel.Token, resetPasswordModel.Password);
            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }

                return View();
            }

            return RedirectToAction(nameof(ResetPasswordConfirmation));

        }


    }
}