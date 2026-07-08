using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShuttleService.Data;
using ShuttleService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Linq.Dynamic.Core;

namespace ShuttleService.Controllers
{
    [Authorize(Policy = "RequireAllRole")]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PeopleCoreLinkedServerDBService _linkedDbService;

        public EmployeesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, PeopleCoreLinkedServerDBService linkedDbService)
        {
            _context = context;
            _userManager = userManager;
            _linkedDbService = linkedDbService;
        }

        // GET: Employees
        [Authorize(Policy = "RequireAdminRole")]
        public IActionResult Index()
        {
            //checking if has session
            if (HttpContext.Session.GetString("Session_employeeId") == null || HttpContext.Session.GetString("Session_employeeId") == "")
            {
                return RedirectToAction("Login", "Accounts");
            }
            //end

            ViewData["CompanyListId"] = new SelectList(_context.CompanyLists, "Id", "CompanyName");
            ViewData["NationalityId"] = new SelectList(_context.Nationalities, "Id", "NationalityName");
            ViewBag.MachineName = System.Environment.MachineName;
            return View();
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.CompanyList)
                .Include(e => e.Nationality)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }


        // GET: Employees/Details/5
        [Authorize(Policy = "RequireAdminRole")]
        public IActionResult Heads()
        {
            return View();
        }

        [Authorize(Policy = "RequireAdminRole")]
        // GET: Employees/Create
        public async Task<IActionResult> Create()
        {
            ViewData["NationalityId"] = new SelectList(_context.Nationalities, "Id", "NationalityName");
            ViewData["SupervisorEmployeeNo"] = new SelectList(_context.Employees.Where(e => e.IsImmediateHead == true).Select(u => new { FullName = u.FirstName + " " + u.LastName, EmployeeNo = u.EmployeeNo }), "EmployeeNo", "FullName");



            var currentUser = await _userManager.GetUserAsync(User);
            var currentCompanyGroupId = _context.Employees.Where(emp => emp.Id == currentUser.EmployeeId)
                .Select(e => e.CompanyGroupId).FirstOrDefault();




            //var CompanyGroupList = _context.CompanyGroup.Where(m => m.Id == currentCompanyGroupId).OrderBy(m => m.Id).ToList();
            //ViewData["CompanyGroupId"] = new SelectList(CompanyGroupList, "Id", "CompanyGroupName");

            var CompanyGroupList = _context.CompanyGroup.Where(m => m.CompanyGroupName == "UPDI").ToList();
            ViewData["CompanyGroupId"] = new SelectList(CompanyGroupList, "Id", "CompanyGroupName");

            var LocationList = _context.Location.OrderBy(m => m.Id).ToList();
            ViewData["LocationListId"] = new SelectList(LocationList, "Id", "LocationName");

            var CompanyList = _context.CompanyLists.Where(m => m.CompanyGroupId == currentCompanyGroupId).OrderBy(m => m.CompanyName).ToList();
            ViewData["CompanyListId"] = new SelectList(CompanyList, "Id", "CompanyName");

            var DepartmentsList = _context.DepartmentLists.Where(m => m.CompanyGroupId == currentCompanyGroupId).OrderBy(m => m.DepartmentName).ToList();
            ViewData["DepartmentListId"] = new SelectList(DepartmentsList, "Id", "DepartmentName");

            var ChargingCompanysList = _context.ChargingCompanys.Where(m => m.CompanyGroupId == currentCompanyGroupId).OrderBy(m => m.CompanyName).ToList();
            ViewData["ChargingCompanyId"] = new SelectList(ChargingCompanysList, "Id", "CompanyName");

            var ChargingDepartmentsList = _context.ChargingDepartments.Where(m => m.CompanyGroupId == currentCompanyGroupId).OrderBy(m => m.DepartmentName).ToList();
            ViewData["ChargingDepartmentId"] = new SelectList(ChargingDepartmentsList, "Id", "DepartmentName");


            ViewBag.neworupdate = "Create";
            ViewBag.isHead = 0;
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,EmployeeNo,FirstName,MiddleName,LastName,Position,CompanyListId,Gender,NationalityId,MobileNumber,LocalNumber,CompanyEmail,AlternativeEmail,SupervisorEmployeeNo,Status,IsImmediateHead,AlternateImmediateHead,AlternateImmediateHeadValidFrom,AlternateImmediateHeadValidTo,AlternateImmediateHeadValidity,EncodedBy,LastModifiedBy,EncodeDate,ModifyDate")] Employee employee)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(employee);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["CompanyListId"] = new SelectList(_context.CompanyLists, "Id", "CompanyName", employee.CompanyListId);
        //    ViewData["NationalityId"] = new SelectList(_context.Nationalities, "Id", "NationalityName", employee.NationalityId);
        //    return View(employee);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            var returnSuccess = 1;
            var returnMessage = "";

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {

                    if (_context.Employees.Count(e => e.EmployeeNo == employee.EmployeeNo) > 0)
                    {
                        throw new System.Exception("Employee ID already exist.");
                    }

                    employee.EncodedBy = HttpContext.Session.GetString("Session_userDomainWithName");
                    employee.EncodeDate = DateTime.Now;
                    employee.ModifyDate = DateTime.Now;
                    employee.Status = 1;


                    employee.AlternateImmediateHeadValidFrom = null;
                    employee.AlternateImmediateHeadValidTo = null;

                    employee.EmployeeLocations = new List<EmployeeLocation>();

                    // map LocationIds -> EmployeeLocations
                    if (employee.LocationIds != null && employee.LocationIds.Any())
                    {
                        foreach (var locId in employee.LocationIds)
                        {
                            employee.EmployeeLocations.Add(new EmployeeLocation
                            {
                                LocationId = locId
                                // EmployeeId will be filled by EF once Employee is saved
                            });
                        }
                    }


                    _context.Add(employee);
                    var result = await _context.SaveChangesAsync();


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
        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.EmployeeLocations)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null)
            {
                return NotFound();
            }
            employee.LocationIds = employee.EmployeeLocations.Select(el => el.LocationId).ToList();

            var CompanyGroupList = _context.CompanyGroup.OrderBy(m => m.Id).ToList();
            ViewData["CompanyGroupId"] = new SelectList(CompanyGroupList, "Id", "CompanyGroupName");

            var LocationList = _context.Location.OrderBy(m => m.Id).ToList();
            ViewData["LocationListId"] = new SelectList(LocationList, "Id", "LocationName");

            var CompanyList = _context.CompanyLists.OrderBy(m => m.CompanyName).ToList();
            ViewData["CompanyListId"] = new SelectList(CompanyList, "Id", "CompanyName");

            ViewData["NationalityId"] = new SelectList(_context.Nationalities, "Id", "NationalityName", employee.NationalityId);

            var DepartmentsList = _context.DepartmentLists.Where(e => e.Id == employee.DepartmentListId).OrderBy(m => m.DepartmentName).ToList();
            ViewData["DepartmentListId"] = new SelectList(DepartmentsList, "Id", "DepartmentName");

            var ChargingCompanysList = _context.ChargingCompanys.OrderBy(m => m.CompanyName).ToList();
            ViewData["ChargingCompanyId"] = new SelectList(ChargingCompanysList, "Id", "CompanyName");

            var ChargingDepartmentsList = _context.ChargingDepartments.Where(e => e.Id == employee.ChargingDepartmentId).OrderBy(m => m.DepartmentName).ToList();
            ViewData["ChargingDepartmentId"] = new SelectList(ChargingDepartmentsList, "Id", "DepartmentName");


            ViewData["SupervisorEmployeeNo"] = new SelectList(_context.Employees.Where(e => e.IsImmediateHead == true).Select(u => new { FullName = u.FirstName + " " + u.LastName, EmployeeNo = u.EmployeeNo }), "EmployeeNo", "FullName", employee.SupervisorEmployeeNo);
            var supervisorInformation = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeNo == employee.SupervisorEmployeeNo);
            ViewBag.neworupdate = "Edit";
            ViewBag.supID = employee.SupervisorEmployeeNo;
            ViewBag.supName = supervisorInformation.FirstName + "  " + supervisorInformation.LastName;
            ViewBag.empID = employee.Id;

            ViewBag.varChargingDepartmentId = employee.ChargingDepartmentId.ToString();
            ViewBag.varChargingDepartmentName = _context.ChargingDepartments.FirstOrDefault(e => e.Id == employee.ChargingDepartmentId).DepartmentName;
            ViewBag.varDepartmentId = employee.DepartmentListId.ToString();
            ViewBag.varDepartment = _context.DepartmentLists.FirstOrDefault(e => e.Id == employee.DepartmentListId).DepartmentName;


            //if (employee.IsImmediateHead == true) {
            //    ViewBag.isHead = 1;
            //}
            //else{
            //    ViewBag.isHead = 0;
            //}


            return View("Create", employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                var returnSuccess = 1;
                var returnMessage = "";

                var dateAndTime = DateTime.Now;
                var date = dateAndTime.Date;

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        if (employee.IsImmediateHead == false && _context.Employees.Count(e => e.SupervisorEmployeeNo == employee.EmployeeNo) > 0)
                        {
                            throw new System.Exception("This employee has subordinate, cannot set as not Immediate Head.");
                        }



                        //check if the immediate supervisor was changed
                        var checkOriginalApprover = _context.Employees.Where(e => e.EmployeeNo == employee.EmployeeNo).Select(u => new { SupervisorEmployeeNo = u.SupervisorEmployeeNo });
                        foreach (var coa in checkOriginalApprover.ToList())
                        {
                            if (coa.SupervisorEmployeeNo != employee.SupervisorEmployeeNo) //when the immediate supervisor was changed
                            {
                                int[] ForApprovalStatus = { 1, 6 };
                                var shuttlepassengers = _context.ShuttlePassengers.Where(s => ForApprovalStatus.Contains(s.Status) && (s.EmployeeNo == employee.EmployeeNo || (s.PassengerTypeId == 2 && s.EmployeeId == id)));

                                if (shuttlepassengers.Count() > 0)
                                {
                                    var supervisor = _context.Employees.Where(es => es.EmployeeNo == employee.SupervisorEmployeeNo).Select(u => new { EmployeeNo = u.EmployeeNo, AlternateImmediateHead = u.AlternateImmediateHead, AlternateImmediateHeadValidTo = u.AlternateImmediateHeadValidTo });
                                    foreach (var sup in supervisor.ToList())
                                    {
                                        var supEmployeeNo = sup.EmployeeNo;
                                        if (sup.AlternateImmediateHeadValidTo >= date)
                                        {
                                            supEmployeeNo = sup.AlternateImmediateHead;
                                        }

                                        await shuttlepassengers.ForEachAsync(e =>
                                        {
                                            e.InitialApproverEmployeeNo = supEmployeeNo;
                                        });

                                        //await _context.SaveChangesAsync();
                                    }


                                }
                            }
                        }
                        //end

                        //var shuttlePassenger = _context.ShuttlePassengers.FirstOrDefault(s => s.Id == id);
                        //shuttlePassenger.Status = 5;
                        employee.LastModifiedBy = HttpContext.Session.GetString("Session_userDomainWithName");
                        employee.ModifyDate = DateTime.Now;

                        // fetch employee with locations tracked
                        var existingEmployee = await _context.Employees
                            .Include(e => e.EmployeeLocations)
                            .FirstOrDefaultAsync(e => e.Id == id);

                        _context.Entry(existingEmployee).CurrentValues.SetValues(employee);

                        // update locations
                        existingEmployee.EmployeeLocations.Clear();

                        if (employee.LocationIds != null)
                        {
                            foreach (var locId in employee.LocationIds)
                            {
                                existingEmployee.EmployeeLocations.Add(new EmployeeLocation
                                {
                                    EmployeeId = existingEmployee.Id,
                                    LocationId = locId
                                });
                            }
                        }

                        await _context.SaveChangesAsync();
                        _context.Entry(existingEmployee).State = EntityState.Detached;

                        _context.Database.ExecuteSqlCommand("execute AutoExpireAlternateHead");



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
            else {
                var jsonData = new
                {
                    message = "Error on validation",
                    success = 0
                };
                return new JsonResult(jsonData);
            }

        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.CompanyList)
                .Include(e => e.Nationality)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }


        [HttpPost]
        public ActionResult checkEmployee(string employeeId)
        {
            var employeeCount = _context.Employees.Count(e => e.EmployeeNo == employeeId);
            var _error = 0;
            if (employeeCount <= 0)
            {
                _error = 1;
                var jsonDataTemp = new
                {
                    error = _error,
                    message = "Employee does not exist."
                };

                return new JsonResult(jsonDataTemp);
            }

            var employeeData = _context.Employees.FirstOrDefault(e => e.EmployeeNo == employeeId);

            // new code for locked employee

            long unixTime = Convert.ToInt64(HttpContext.Request.Query["d"]);
            var dtimestamp = DateTimeOffset.FromUnixTimeSeconds(unixTime).ToString("yyyy-MM-dd");
            string _timestampfrom = dtimestamp + " 00:00:00";
            string _timestampto = dtimestamp + " 23:59:00";
            DateTime timestampfrom = DateTime.Parse(_timestampfrom);
            DateTime timestampto = DateTime.Parse(_timestampto);

            var checkLocked = _context.LockedEmployeeLogs.Where(e => e.EmployeeId.Equals(employeeData.Id) && e.LockedFrom <= timestampfrom && e.LockedTo >= timestampto);
            if (checkLocked.Count() > 0) {

                _error = 1;
                var jsonDataTemp = new
                {
                    error = _error,
                    message = "Employee is locked."
                };

                return new JsonResult(jsonDataTemp);

            }
            //new code


            string _nationalityName = _context.Nationalities.FirstOrDefault(e => e.Id.Equals(employeeData.NationalityId)).NationalityName;

            var chargingDept = _context.ChargingDepartments.FirstOrDefault(r => r.Id.Equals(employeeData.ChargingDepartmentId));
            //    ChargingDepartmentName =
            var _ChargingDepartment = "";
            var _ChargingDepartmentId = 0;
            if (chargingDept != null)
            {
                _ChargingDepartment = chargingDept.DepartmentName;
                _ChargingDepartmentId = chargingDept.Id;
            }


            var jsonData = new
            {
                error = _error,
                data = employeeData,
                ChargingDepartment = _ChargingDepartment,
                ChargingDepartmentId = _ChargingDepartmentId,
                nationalityName = _nationalityName
            };



            return new JsonResult(jsonData);

        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult checkEmployeeByName()
        {

            // Initialization.
            string search = Request.Form["search[value]"];
            string draw = Request.Form["draw"];
            string order = Request.Form["order[0][column]"];
            string orderDir = Request.Form["order[0][dir]"];
            int startRec = Convert.ToInt32(Request.Form["start"]);
            int pageSize = Convert.ToInt32(Request.Form["length"]);


            var EmployeeList = _context.Employees.Where(r => r.FirstName.ToLower().Contains(Request.Form["firstName"].ToString().ToLower()) &&
                                                                r.MiddleName.ToLower().Contains(Request.Form["middleName"].ToString().ToLower()) &&
                                                                r.LastName.ToLower().Contains(Request.Form["lastName"].ToString().ToLower()))
                                                                .Select(u => new {
                                                                    Id = "<a href='javascript:void(0);' onClick='EmployeeSearchAjax(`" + u.EmployeeNo + "`);'> Select </a>",
                                                                    FirstName = u.FirstName,
                                                                    MiddleName = u.MiddleName,
                                                                    LastName = u.LastName
                                                                });


            // Total record count.
            int totalRecords = EmployeeList.Count();

            // Verification.
            if (!string.IsNullOrEmpty(search))
            {   // Apply search
                EmployeeList = EmployeeList.Where(x => x.FirstName.ToLower().Contains(search.ToLower())
                                         && x.MiddleName.ToLower().Contains(search.ToLower())
                                         && x.LastName.ToLower().Contains(search.ToLower())
                                         );
            }
            // Sorting.
            string[] sort = new string[] { "Id", "FirstName", "MiddleName", "LastName" };
            //var sortfield = sort[int.Parse(order)];
            //EmployeeList = EmployeeList.OrderBy();

            // Filter record count.
            int recFilter = EmployeeList.Count();

            // Apply pagination.
            EmployeeList = EmployeeList.Skip(startRec).Take(pageSize);

            /*
            //string[,] EmployeeData = new string[0,0];
            var EmployeeData = new Dictionary<string, string>();
            var EmployeeJson = new Dictionary<int, object>();
            //object[] EmployeeJson = new object[0];
            int _x = 0;
            foreach (var emp in EmployeeList.ToList())
            {
                EmployeeData["id"] = string.Concat("<a href='/", emp.Id, "'> Select </a>");
                EmployeeData["firstName"] = emp.FirstName;
                EmployeeData["middleName"] = emp.MiddleName;
                EmployeeData["lastName"] = emp.LastName;
                //EmployeeJson[_x] = EmployeeData;
                EmployeeJson.Add(_x,EmployeeData);
                _x++;
            }
            */

            var jsonData = new
            {
                draw = Convert.ToInt32(draw),
                recordsTotal = totalRecords,
                recordsFiltered = recFilter,
                data = EmployeeList.ToList(),
                /*
                data = EmployeeJson.Values.ToList(),
                //data = EmployeeJson.ToList(),
                 */
            };



            return new JsonResult(jsonData);

        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult checkAlternate(int id)
        {
            var SupName = "";
            var ImmediateHeadName = "";
            var AlternateImmediateHeadValidFrom = "";
            var AlternateImmediateHeadValidTo = "";
            var AlternateImmediateHead = "";
            var SupervisorList = _context.Employees.FirstOrDefault(r => r.Id == id);

            if (SupervisorList != null) {
                ImmediateHeadName = SupervisorList.FirstName + " " + (SupervisorList.MiddleName == null || SupervisorList.MiddleName == "" ? "" : SupervisorList.MiddleName[0] + ". ") + SupervisorList.LastName;
                AlternateImmediateHeadValidFrom = SupervisorList.AlternateImmediateHeadValidFrom == null ? " " : SupervisorList.AlternateImmediateHeadValidFrom.GetValueOrDefault().ToString("MM/dd/yyyy");
                AlternateImmediateHeadValidTo = SupervisorList.AlternateImmediateHeadValidTo == null ? " " : SupervisorList.AlternateImmediateHeadValidTo.GetValueOrDefault().ToString("MM/dd/yyyy");
                AlternateImmediateHead = SupervisorList.AlternateImmediateHead == null ? "" : SupervisorList.AlternateImmediateHead;

                if (AlternateImmediateHead != "") {
                    var AlternateSup = _context.Employees.FirstOrDefault(r => r.EmployeeNo == SupervisorList.AlternateImmediateHead);
                    SupName = AlternateSup.FirstName + " " + (AlternateSup.MiddleName == null || AlternateSup.MiddleName == "" ? "" : AlternateSup.MiddleName[0] + ". ") + AlternateSup.LastName;

                }
            }



            //string[] supList = new string[];
            //int x = 0;

            //foreach (var sup in SupervisorList.ToList())
            //{
            //    supList[x] = Array("id"=>);
            //    x++;
            //}

            var jsonData = new
            {
                ImmediateHeadName = ImmediateHeadName,
                AlternateImmediateHeadValidFrom = AlternateImmediateHeadValidFrom,
                AlternateImmediateHeadValidTo = AlternateImmediateHeadValidTo,
                AlternateImmediateHead = AlternateImmediateHead,
                SupName = SupName,
                Id = SupervisorList.Id,
                Success = 1
            };

            return new JsonResult(jsonData);

        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> ShowEmployeeLists()
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

            var DataList = _context.Employees.Join(_context.Employees,
                                    emp1 => emp1.SupervisorEmployeeNo,
                                    emp2 => emp2.EmployeeNo,
                                    (emp1, emp2) => new { emp1, emp2 }).
                            OrderBy(u => u.emp1.FirstName).
                            Select(u => new
                            {
                                Id = "<a href='" + @Url.Content("~/employees") + "/edit/" + u.emp1.Id + "'  class='btn btn-sm btn-primary' > View/Update </a>",
                                Company = u.emp1.CompanyList.CompanyName,
                                EmployeeID = u.emp1.EmployeeNo,
                                FirstName = u.emp1.FirstName,
                                MiddleName = u.emp1.MiddleName,
                                LastName = u.emp1.LastName,
                                FullName = u.emp1.FirstName + " " + (u.emp1.MiddleName == null || u.emp1.MiddleName == "" ? "" : u.emp1.MiddleName[0] + ". ") + u.emp1.LastName,
                                Position = u.emp1.Position,
                                IsSupervisor = u.emp1.IsImmediateHead == false ? "" : "Yes",
                                Supervisor = u.emp2.FirstName + " " + u.emp2.LastName,
                                IsLocked = _context.LockedEmployeeLogs.Count(e => e.EmployeeId.Equals(u.emp1.Id) && e.LockedTo >= DateTime.Now) > 1 ? "Locked until " + _context.LockedEmployeeLogs.Where(e => e.EmployeeId.Equals(u.emp1.Id)).OrderByDescending(y => y.LockedTo).FirstOrDefault().LockedTo : "",
                                CompanyGroupId = u.emp1.CompanyGroupId
                            });

            DataList = DataList.Where(d => d.CompanyGroupId == currentCompanyGroupId);
            // Total record count.
            int totalRecords = DataList.Count();

            int recFilter = 0;
            recFilter = totalRecords;
            // Verification.
            if (!string.IsNullOrEmpty(search))
            {   // Apply search
                DataList = DataList.Where(x => x.FullName.ToLower().Contains(search.ToLower()) || x.Company.Contains(search.ToLower()) || x.EmployeeID.Contains(search.ToLower()) || x.Position.Contains(search.ToLower())
                    || x.Supervisor.Contains(search.ToLower())
                                         );
            }
            // Sorting.
            //string[] sort = new string[] { "Name", "Name" };
            //var sortfield = sort[int.Parse(order)];
            // Filter record count.

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


        [AllowAnonymous]
        [HttpPost]
        public ActionResult lockedEmployeeLists()
        {

            // Initialization.
            string search = Request.Form["search[value]"];
            string draw = Request.Form["draw"];
            string order = Request.Form["order[0][column]"];
            string orderDir = Request.Form["order[0][dir]"];
            int startRec = Convert.ToInt32(Request.Form["start"]);
            int pageSize = Convert.ToInt32(Request.Form["length"]);
            int empID = Convert.ToInt32(Request.Form["empID"]);

            var DataList = _context.LockedEmployeeLogs.
                            Where(e => e.EmployeeId.Equals(empID)).
                            OrderByDescending(u => u.LockedTo).
                            Select(u => new
                            {
                                Id = u.Id,
                                DateFrom = u.LockedFrom.GetValueOrDefault().ToString("MM/dd/yyyy"),
                                DateTo = u.LockedTo.GetValueOrDefault().ToString("MM/dd/yyyy"),
                                Remarks = u.LockedRemarks,
                                EncodedBy = u.ProcessedBy
                            });


            // Total record count.
            int totalRecords = DataList.Count();

            int recFilter = 0;
            recFilter = totalRecords;
            // Verification.
            //if (!string.IsNullOrEmpty(search))
            //{   // Apply search
            //    DataList = DataList.Where(x => x.FullName.ToLower().Contains(search.ToLower()) || x.Company.Contains(search.ToLower()) || x.EmployeeID.Contains(search.ToLower()) || x.Position.Contains(search.ToLower())
            //        || x.Supervisor.Contains(search.ToLower())
            //                             );
            //}
            // Sorting.
            //string[] sort = new string[] { "Name", "Name" };
            //var sortfield = sort[int.Parse(order)];
            // Filter record count.

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


        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> ShowHeadLists()
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
            var DataList = _context.Employees.
                           GroupJoin(_context.Employees,
                                    emp1 => emp1.AlternateImmediateHead,
                                    emp2 => emp2.EmployeeNo,
                                    (emp1, emp2) => new { emp1, emp2 }).SelectMany(
                              x => x.emp2.DefaultIfEmpty(),
                              (x, y) => new { MainEmp = x.emp1, SubEmp = y }).
                            Where(u => u.MainEmp.IsImmediateHead == true).
                            OrderBy(u => u.MainEmp.FirstName).
                            Select(u => new
                            {
                                Id = "<a href='javascript:void(0);' onClick='UpdateAlternateHead(" + u.MainEmp.Id + ")'  class='btn btn-sm btn-primary' > Assign Alternate </a>",
                                Company = u.MainEmp.CompanyList.CompanyName,
                                EmployeeID = u.MainEmp.EmployeeNo,
                                FirstName = u.MainEmp.FirstName,
                                MiddleName = u.MainEmp.MiddleName,
                                LastName = u.MainEmp.LastName,
                                FullName = u.MainEmp.FirstName + " " + (u.MainEmp.MiddleName == null || u.MainEmp.MiddleName == "" ? "" : u.MainEmp.MiddleName[0] + ". ") + u.MainEmp.LastName,
                                Position = u.MainEmp.Position,
                                IsAlternateValid = u.MainEmp.AlternateImmediateHeadValidity == false ? "" : "Yes",
                                AlternateSupervisor = u.SubEmp.FirstName + " " + u.SubEmp.LastName,
                                AlternateDateFrom = u.MainEmp.AlternateImmediateHeadValidFrom == null ? " " : u.MainEmp.AlternateImmediateHeadValidFrom.GetValueOrDefault().ToString("MM/dd/yyyy"),
                                AlternateDateTo = u.MainEmp.AlternateImmediateHeadValidTo == null ? " " : u.MainEmp.AlternateImmediateHeadValidTo.GetValueOrDefault().ToString("MM/dd/yyyy"),
                                CompanyGroupId = u.MainEmp.CompanyGroupId
                            });
            DataList = DataList.Where(d => d.CompanyGroupId == currentCompanyGroupId);
            // Total record count.
            int totalRecords = DataList.Count();


            // Verification.
            if (!string.IsNullOrEmpty(search))
            {   // Apply search
                DataList = DataList.Where(x => x.FullName.ToLower().Contains(search.ToLower()) || x.Company.Contains(search.ToLower()) || x.EmployeeID.Contains(search.ToLower()) || x.Position.Contains(search.ToLower())
                    || x.AlternateSupervisor.Contains(search.ToLower()) || x.AlternateDateFrom.ToString().Contains(search.ToLower()) || x.AlternateDateTo.ToString().Contains(search.ToLower())
                                         );
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
        public async Task<JsonResult> getSupervisorData(string dateFilter, string method) {

            var currentUser = await _userManager.GetUserAsync(User);
            var currentCompanyGroupId = _context.Employees.Where(emp => emp.Id == currentUser.EmployeeId)
                .Select(e => e.CompanyGroupId).FirstOrDefault();
            var SupervisorList = _context.Employees.Where(r => r.IsImmediateHead == true && r.CompanyGroupId == currentCompanyGroupId && (r.FirstName.Contains(Request.Form["q"].ToString().ToLower()) || r.LastName.Contains(Request.Form["q"].ToString().ToLower()) || (r.FirstName + " " + r.LastName).Contains(Request.Form["q"].ToString().ToLower())))
                                                           //.GroupBy(x => x.FirstName)
                                                           .Select(u => new
                                                           {
                                                               Id = u.EmployeeNo,
                                                               Text = u.FirstName + " " + u.LastName
                                                           });


            //string[] supList = new string[];
            //int x = 0;

            //foreach (var sup in SupervisorList.ToList())
            //{
            //    supList[x] = Array("id"=>);
            //    x++;
            //}

            var jsonData = new
            {
                Items = SupervisorList.ToList().ToArray(),
                Count = SupervisorList.Count()
            };

            return new JsonResult(jsonData);

        }


        [HttpPost]
        public async Task<JsonResult> getDepartmentList(int companyId)
        {
            var _keyword = string.IsNullOrWhiteSpace(Request.Form["q"]) ? "" : Request.Form["q"].ToString().ToLower();
            var DepartmentList = _context.DepartmentLists.Where(r => r.CompanyListId.Equals(companyId) && r.DepartmentName.Contains(_keyword))
                                                           .Select(u => new
                                                           {
                                                               Id = u.Id,
                                                               Text = u.DepartmentName
                                                           });

            var jsonData = new
            {
                Items = DepartmentList.ToList().ToArray(),
                Count = DepartmentList.Count()
            };

            return new JsonResult(jsonData);

        }

        [HttpPost]
        public async Task<JsonResult> getCompanyList(int companyGroupId)
        {
            var _keyword = string.IsNullOrWhiteSpace(Request.Form["q"]) ? "" : Request.Form["q"].ToString().ToLower();
            var companyList = _context.CompanyLists.Where(r => r.CompanyGroupId.Equals(companyGroupId) && r.CompanyName.Contains(_keyword))
                                                           .Select(u => new
                                                           {
                                                               Id = u.Id,
                                                               Text = u.CompanyName
                                                           });

            var jsonData = new
            {
                Items = companyList.ToList().ToArray(),
                Count = companyList.Count()
            };

            return new JsonResult(jsonData);

        }

        //[HttpPost]
        //public async Task<JsonResult> GetCompanyList(int companyGroupId)
        //{
        //    var keyword = Request.Form["q"].FirstOrDefault()?.Trim();

        //    var query = _context.CompanyLists
        //        .Where(c => c.CompanyGroupId == companyGroupId);

        //    if (!string.IsNullOrEmpty(keyword))
        //    {
        //        query = query.Where(c => c.CompanyName.Contains(keyword));
        //    }

        //    var items = await query
        //        .Select(c => new
        //        {
        //            Id = c.Id,
        //            Text = c.CompanyName
        //        })
        //        .ToListAsync();

        //    var jsonData = new
        //    {
        //        Items = items,
        //        Count = items.Count
        //    };

        //    return Json(jsonData);
        //}


        [HttpPost]
        public async Task<JsonResult> getChargingCompanyList(int companyGroupId)
        {
            var _keyword = string.IsNullOrWhiteSpace(Request.Form["q"]) ? "" : Request.Form["q"].ToString().ToLower();
            var companyList = _context.ChargingCompanys.Where(r => r.CompanyGroupId.Equals(companyGroupId) && r.CompanyName.Contains(_keyword))
                                                           .Select(u => new
                                                           {
                                                               Id = u.Id,
                                                               Text = u.CompanyName
                                                           });

            var jsonData = new
            {
                Items = companyList.ToList().ToArray(),
                Count = companyList.Count()
            };

            return new JsonResult(jsonData);

        }

        [HttpPost]
        public async Task<JsonResult> getChargingDepartmentList(int companyId)
        {
            var _keyword = string.IsNullOrWhiteSpace(Request.Form["q"]) ? "" : Request.Form["q"].ToString().ToLower();
            var DepartmentList = _context.ChargingDepartments.Where(r => r.ChargingCompanyId.Equals(companyId) && r.DepartmentName.Contains(_keyword))
                                                           .Select(u => new
                                                           {
                                                               Id = u.Id,
                                                               Text = u.DepartmentName
                                                           });
            var jsonData = new
            {
                Items = DepartmentList.ToList().ToArray(),
                Count = DepartmentList.Count()
            };

            return new JsonResult(jsonData);

        }

        [HttpPost]
        public async Task<IActionResult> saveAlternateSup(int id, [Bind("AlternateImmediateHead,AlternateImmediateHeadValidFrom,AlternateImmediateHeadValidTo")] Employee employeetemp)
        {
            var returnSuccess = 1;
            var returnMessage = "";

            //End Manage Parameters

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {

                    int[] ForApprovalStatus = { 1, 6 };


                    var employee = _context.Employees.FirstOrDefault(e => e.Id == id);


                    employee.LastModifiedBy = HttpContext.Session.GetString("Session_userDomainWithName");
                    employee.ModifyDate = DateTime.Now;
                    employee.AlternateImmediateHeadValidFrom = employeetemp.AlternateImmediateHeadValidFrom;
                    employee.AlternateImmediateHeadValidTo = employeetemp.AlternateImmediateHeadValidTo;
                    employee.AlternateImmediateHead = employeetemp.AlternateImmediateHead;

                    _context.Update(employee);

                    var dateAndTime = DateTime.Now;
                    var date = dateAndTime.Date;

                    if (employeetemp.AlternateImmediateHeadValidTo >= date) {
                        var shuttlepassengers = _context.ShuttlePassengers.Where(s => s.InitialApproverEmployeeNo.Equals(employee.EmployeeNo) && ForApprovalStatus.Contains(s.Status));
                        await shuttlepassengers.ForEachAsync(e => {
                            e.InitialApproverEmployeeNo = employee.AlternateImmediateHead;
                        });
                    }

                    await _context.SaveChangesAsync();

                    _context.Database.ExecuteSqlCommand("execute AutoExpireAlternateHead");

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

        //
        private int ReCompute()
        {
            var returnSuccess = 1;
            var returnMessage = "";
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.Database.ExecuteSqlCommand("execute AutoExpireAlternateHead");
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

            return returnSuccess;
        }



        [HttpGet]

        public async Task<IActionResult> Profile(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.EmployeeLocations)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            employee.LocationIds = employee.EmployeeLocations.Select(el => el.LocationId).ToList();

            var currentUser = await _userManager.GetUserAsync(User);
            var currentCompanyGroupId = _context.Employees.Where(emp => emp.Id == currentUser.EmployeeId)
                .Select(e => e.CompanyGroupId).FirstOrDefault();

            var CompanyGroupList = _context.CompanyGroup.OrderBy(m => m.Id).ToList();
            ViewData["CompanyGroupId"] = new SelectList(CompanyGroupList, "Id", "CompanyGroupName");

            var LocationList = _context.Location.OrderBy(m => m.Id).ToList();
            ViewData["LocationListId"] = new SelectList(LocationList, "Id", "LocationName");

            var CompanyList = _context.CompanyLists.OrderBy(m => m.CompanyName)
                    .Where(c => c.CompanyGroupId == currentCompanyGroupId);

            ViewData["CompanyListId"] = new SelectList(CompanyList, "Id", "CompanyName");

            ViewData["NationalityId"] = new SelectList(_context.Nationalities, "Id", "NationalityName", employee.NationalityId);

            var DepartmentsList = _context.DepartmentLists
                .Where(d => d.CompanyGroupId == currentCompanyGroupId && d.CompanyListId == employee.CompanyListId)
                .OrderBy(m => m.DepartmentName);
            ViewData["DepartmentListId"] = new SelectList(DepartmentsList, "Id", "DepartmentName");

            var ChargingCompanysList = _context.ChargingCompanys
                .Where(c => c.CompanyGroupId == currentCompanyGroupId)
                .OrderBy(m => m.CompanyName).ToList();
            ViewData["ChargingCompanyId"] = new SelectList(ChargingCompanysList, "Id", "CompanyName");

            var ChargingDepartmentsList = _context.ChargingDepartments
                .Where(e => e.CompanyGroupId == currentCompanyGroupId && e.ChargingCompanyId == employee.ChargingCompanyId)
                .OrderBy(m => m.DepartmentName).ToList();
            ViewData["ChargingDepartmentId"] = new SelectList(ChargingDepartmentsList, "Id", "DepartmentName");


            ViewData["SupervisorEmployeeNo"] = new SelectList(_context.Employees.Where(e => e.IsImmediateHead == true && e.CompanyGroupId == currentCompanyGroupId).Select(u => new { FullName = u.FirstName + " " + u.LastName, EmployeeNo = u.EmployeeNo }), "EmployeeNo", "FullName", employee.SupervisorEmployeeNo);
            var supervisorInformation = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeNo == employee.SupervisorEmployeeNo);
            ViewBag.neworupdate = "Edit";
            ViewBag.supID = employee.SupervisorEmployeeNo;
            ViewBag.supName = supervisorInformation.FirstName + "  " + supervisorInformation.LastName;




            //if (employee.IsImmediateHead == true) {
            //    ViewBag.isHead = 1;
            //}
            //else{
            //    ViewBag.isHead = 0;
            //}

            return View("Profile", employee);
        }


        // POST: Employees/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Profile(int id, Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            var returnSuccess = 1;
            var returnMessage = "";

            var dateAndTime = DateTime.Now;
            var date = dateAndTime.Date;

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {

                    //var shuttlePassenger = _context.ShuttlePassengers.FirstOrDefault(s => s.Id == id);
                    //shuttlePassenger.Status = 5;

                    var employeeData = await _context.Employees
                        .Include(e => e.EmployeeLocations) // include junction table
                        .FirstOrDefaultAsync(e => e.Id == id);
                    if (employeeData == null) {
                        throw new System.Exception("Invalid Profile.");
                    }
                    employeeData.LastModifiedBy = HttpContext.Session.GetString("Session_userDomainWithName");
                    employeeData.ModifyDate = DateTime.Now;
                    employeeData.AlternativeEmail = employee.AlternativeEmail;
                    employeeData.CompanyEmail = employee.CompanyEmail;
                    employeeData.LocalNumber = employee.LocalNumber;
                    employeeData.MobileNumber = employee.MobileNumber;


                    employeeData.FirstName = employee.FirstName;
                    employeeData.MiddleName = employee.MiddleName;
                    employeeData.LastName = employee.LastName;
                    employeeData.Position = employee.Position;
                    employeeData.CompanyListId = employee.CompanyListId;
                    employeeData.CompanyGroupId = employee.CompanyGroupId;
                    employeeData.Gender = employee.Gender;
                    employeeData.NationalityId = employee.NationalityId;

                    employeeData.DepartmentListId = employee.DepartmentListId;
                    employeeData.ChargingCompanyId = employee.ChargingCompanyId;
                    employeeData.ChargingDepartmentId = employee.ChargingDepartmentId;

                    employeeData.EmployeeLocations.Clear();

                    // Add selected locations from the form
                    if (employee.LocationIds != null && employee.LocationIds.Any())
                    {
                        foreach (var locId in employee.LocationIds)
                        {
                            employeeData.EmployeeLocations.Add(new EmployeeLocation
                            {
                                EmployeeId = employeeData.Id,
                                LocationId = locId
                            });
                        }
                    }

                    var option = new CookieOptions();
                    option.Expires = DateTime.Now.AddMinutes(480);
                    Response.Cookies.Append("companyGroup", _context.CompanyGroup.Find(employeeData.CompanyGroupId).CompanyGroupName, option);

                    _context.Update(employeeData);

                    //logs
                    Log _log = new Log();
                    _log.Process = "Updated Profile";
                    _log.ProcessedBy = HttpContext.Session.GetString("Session_userDomainWithName");
                    _log.ProcessedDate = DateTime.Now;
                    _context.Add(_log);
                    //end logsreturnMessage = "Success";

                    await _context.SaveChangesAsync();
                    _context.Entry(employeeData).State = EntityState.Detached;





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

        [HttpPost]
        public async Task<IActionResult> LockEmployee(int id, [Bind("LockedFrom,LockedTo,LockedRemarks,ProcessedBy,ProcessedDate,EmployeeId")]
            LockedEmployeeLog lockedEmployeeLog)
        {

            var returnSuccess = 1;
            var returnMessage = "";

            var dateAndTime = DateTime.Now;
            var date = dateAndTime.Date;

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {

                    //var shuttlePassenger = _context.ShuttlePassengers.FirstOrDefault(s => s.Id == id);
                    //shuttlePassenger.Status = 5;

                    var employeeData = _context.Employees.FirstOrDefault(e => e.Id == id);
                    if (employeeData == null)
                    {
                        throw new System.Exception("Invalid Profile.");
                    }


                    lockedEmployeeLog.ProcessedBy = HttpContext.Session.GetString("Session_userDomainWithName");
                    lockedEmployeeLog.ProcessedDate = DateTime.Now;
                    lockedEmployeeLog.EmployeeId = employeeData.Id;


                    _context.Add(lockedEmployeeLog);

                    //logs
                    Log _log = new Log();
                    _log.Process = "Added Locked Employee";
                    _log.ProcessedBy = HttpContext.Session.GetString("Session_userDomainWithName");
                    _log.ProcessedDate = DateTime.Now;
                    _context.Add(_log);
                    //end logsreturnMessage = "Success";

                    await _context.SaveChangesAsync();
                    ///_context.Entry(lockedEmployeeLog).State = EntityState.Detached;





                    returnSuccess = 1;
                    transaction.Commit();



                }
                catch (Exception e)
                {
                    returnSuccess = 0;
                    returnMessage = e.Message + " / " + e.InnerException;
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


        [HttpPost]
        public async Task<IActionResult> GetPeopleCoreEmployeeInfo(string employeeNo)
        {
            var employee = await _linkedDbService.GetUserInfoByEmployeeNo(employeeNo);
            if (employee == null || employee.IsEmpty())
            {
                return BadRequest();
            }
            else
            {
                return Ok(employee);
            }
        }
        [HttpPost]
        public async Task<IActionResult> GetPeopleCoreEmployeeInfoByEmployeeNoOrName()
        {
            var _keyword = string.IsNullOrWhiteSpace(Request.Form["q"]) ? "" : Request.Form["q"].ToString();
            var employees = await _linkedDbService.GetUserInfoByEmployeeNoOrName(_keyword);
            var results = employees.Select(e => new
            {
                Id = e,
                Text = e
            });
            var jsonData = new
            {
                Items = results.ToList().ToArray(),
                Count = results.Count()
            };
            return new JsonResult(jsonData);
        }
        [HttpPost]
        public async Task<IActionResult> GetPeopleCoreEmployeeEmail(string employeeName)
        {
            var employee = await _linkedDbService.GetUserInfoByEmployeeName(employeeName);
            return new JsonResult(new
            {
                Email = employee.Email,
                Department = employee.DepartmentCode
            });
        }
    }
}
