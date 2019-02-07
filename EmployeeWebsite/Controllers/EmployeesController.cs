using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using EmployeeWebsite;
using EmployeeWebsite.Models;

namespace EmployeeWebsite.Controllers
{
    public class EmployeesController : Controller
    {
        //mdbcontext is needed to manipulate the model to customize our edit sheets as we want HR-tier users to edit more info on a user and we want
        //employee tier users to edit a very few data fields.
        private mdbcontext db = new mdbcontext();
        private static Random random = new Random();

        // GET: Employees
        public ActionResult Index()
        {
            var employees = db.Employees.Include(e => e.Department).Include(e => e.User);
            //checking users permission, non- silver aka "HR"-tier view their personal page instead
            if (Session["permissionTier"].ToString().Contains("silver"))
            {
                return View(employees.ToList());
            }
            else
            {
                User user = db.Users.Find(int.Parse(Session["id"].ToString()));
                Employee e = user.Employees.First();
                //redirecting the user back to their "profile" page
                return RedirectToAction("Details", new RouteValueDictionary(
         new { controller = "Employees", action = "Details", e.Id }));
            }
        }

        // GET: Employees/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            if (Session["permissionTier"] == null)
            {
                return RedirectToAction("LogIn", "Users");
            }
            else if (Session["permissionTier"].ToString().Contains("silver") || Session["username"].ToString() == employee.User.username)
            {
                return View(employee);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Employees/Create
        public ActionResult Create()
        {
            if (Session["permissionTier"] == null)
            {
               return RedirectToAction("LogIn", "Users");
            }
            else if (Session["permissionTier"].ToString().Contains("silver"))
            {
                //to force all employees to have an user name attached to it while making it simple to use we needed to list out available users that may
                //have been created by employee prospects OR allow a randomly generated username that the employee can edit on his own.
                ViewBag.departmentId = new SelectList(db.Departments, "Id", "name");
                SelectList sl = new SelectList(db.Users, "Id", "username");
                SelectListItem sli = new SelectListItem();
                sli.Text = "Create new Employee";
                sli.Value = "0";
                ViewBag.userID = AddFirstItem(sl, sli);
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        public SelectList AddFirstItem(SelectList origList, SelectListItem firstItem)
        {
            //this code organizes and lists all the available users. This ensures that the create random user is on top of the list.
            List<SelectListItem> newList = origList.ToList();
            newList.Insert(0, firstItem);
            List<SelectListItem> removeItems = new List<SelectListItem>();
            foreach(SelectListItem sli in newList)
            {
                if(sli.Value != "0")
                {
                    User user = db.Users.Find(int.Parse(sli.Value));
                    if (user.Employees.Count != 0)
                    {
                        removeItems.Add(sli);
                    //removes the admin account as being attached to anything else.
                    }else if(user.username == "admin")
                    {
                        removeItems.Add(sli);
                    }
                }
            }
            foreach(SelectListItem sli in removeItems)
            {
                newList.Remove(sli);
            }


            var selectedItem = newList.FirstOrDefault(item => item.Selected);
            var selectedItemValue = String.Empty;
            if (selectedItem != null)
            {
                selectedItemValue = selectedItem.Value;
            }

            return new SelectList(newList, "Value", "Text", selectedItemValue);
        }

        // POST: Employees/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,payroll,badge,departmentId,name,address,city,state,zipcode,userID,vacationHours,PersonalTimeOff,HourlyRates,Notes")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                //If a new user is required to generate it we create one as requested.
                if(Request["userId"] == "0" && Session["permissionTier"].ToString().Contains("silver"))
                {
                    User user = new User();
                    user.username = employee.name + "#" + RandomString(4);
                    user.password = "123456";
                    //here we can have predefined department permissions if we wanted to expanded or reduce the a departments capabilities on the website.
                    if(employee.Department.name == "HR")
                    {
                        user.permissionTier = "guest,bronze,silver";
                    }
                    else
                    {
                        user.permissionTier = "guest,bronze";
                    }
                    db.Users.Add(user);
                    db.SaveChanges();
                    List<User> list = db.Users.Where(x => x.username == user.username).ToList();
                    user = list.First();
                    employee.badge = RandomString(8);

                    employee.userID = user.Id;
                    
                    db.Employees.Add(employee);
                    db.SaveChanges();
                    


                }
                else
                {
                    if (employee.Department.name == "HR")
                    {
                        User user = db.Users.Find(int.Parse(Request["userId"]));
                        user.permissionTier = "guest,bronze,silver";
                        db.Set<User>().AddOrUpdate(user);
                        db.SaveChanges();
                    }
                    employee.badge = RandomString(8);
                    db.Employees.Add(employee);
                    db.SaveChanges();
                   
                }
            }

            ViewBag.departmentId = new SelectList(db.Departments, "Id", "name", employee.departmentId);
            ViewBag.userID = new SelectList(db.Users, "Id", "username", employee.userID);
            return View(employee);
        }

        // GET: Employees/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            ViewBag.departmentId = new SelectList(db.Departments, "Id", "name", employee.departmentId);           
            ViewBag.checker = employee.User.Id;
            List<SelectListItem> newList = new SelectList(db.Users, "Id", "username", employee.userID).ToList();
            List<SelectListItem> removeItems = new List<SelectListItem>();
            foreach (SelectListItem sli in newList)
            {
                if (sli.Value != "0")
                {
                    //if the user is has an employee attached to the account we want to do nothing. however if it isn't we can attach a user account to it.
                    User user = db.Users.Find(int.Parse(sli.Value));
                    if (user.Employees.Contains(employee))
                    {

                    }
                    else if (user.Employees.Count != 0)
                    {
                        removeItems.Add(sli);
                    }
                    else if (user.username == "admin")
                    {
                        removeItems.Add(sli);
                    }
                }
            }
            foreach (SelectListItem sli in removeItems)
            {
                newList.Remove(sli);
            }
            ViewBag.userID = new SelectList(newList, "Value", "Text");
            if (Session["permissionTier"] == null)
            {
                return RedirectToAction("LogIn", "Users");
            }
            else if (Session["permissionTier"].ToString().Contains("silver") || Session["username"].ToString() == employee.User.username)
            {
                return View(employee);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,payroll,badge,departmentId,name,address,city,state,zipcode,userID,vacationHours,PersonalTimeOff,HourlyRates,Notes")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                Employee e = db.Employees.Find(employee.Id);
                if(employee.payroll == null)
                {
                    employee.payroll = e.payroll;
                    
                }
                if(employee.badge == null)
                {
                    employee.badge = e.badge;
                }
                if(employee.departmentId == null)
                {
                    employee.departmentId = e.departmentId;
                }
                if(employee.Department == null)
                {
                    employee.Department = e.Department;
                }
                if (employee.userID == null) {
                    employee.userID = e.userID;
                }
                if(employee.User == null)
                {
                    employee.User = e.User;
                }
                if (employee.vacationHours == null) {
                    employee.vacationHours = e.vacationHours;
                }
                if (employee.PersonalTimeOff == null) {
                    employee.PersonalTimeOff = e.PersonalTimeOff;
                }
                if (employee.HourlyRates == null) {
                    employee.HourlyRates = e.HourlyRates;
                }
                if(employee.Notes == null) {
                    employee.Notes = e.Notes;
                }
                //try
                //{

                    //db.Entry(employee).State = EntityState.Modified;
                    
                //since we modified the edit form we have to change the way we update the entity with this line of code instead.
                    db.Set<Employee>().AddOrUpdate(employee);
                    db.SaveChanges();
                
                    
                //}catch(Exception exc)
                //{
                  //  Console.Write(exc);
            //    }
            }
            ViewBag.departmentId = new SelectList(db.Departments, "Id", "name", employee.departmentId);
            ViewBag.userID = new SelectList(db.Users, "Id", "username", employee.userID);
            return RedirectToAction("Details", new RouteValueDictionary(
         new { controller = "Employees", action = "Details", employee.Id }));
        }

        // GET: Employees/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            if (Session["permissionTier"] == null)
            {
                return RedirectToAction("LogIn", "Users");
            }
            else if (Session["permissionTier"].ToString().Contains("silver"))
            {
                return View(employee);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Employee employee = db.Employees.Find(id);
            db.Employees.Remove(employee);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
