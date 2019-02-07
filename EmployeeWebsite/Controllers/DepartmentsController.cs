using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EmployeeWebsite;
using EmployeeWebsite.Models;

namespace EmployeeWebsite.Controllers
{
    public class DepartmentsController : Controller
    {
        private mdbcontext db = new mdbcontext();

        // GET: Departments
        public ActionResult Index()
        {
            //making sure the user has permissions to view all the departments.
            if (Session["permissionTier"].ToString().Contains("silver"))
            {
                return View(db.Departments.ToList());
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Departments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department department = db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            if (Session["permissionTier"] == null)
            {
                return RedirectToAction("LogIn", "Users");
            }
            else if (Session["permissionTier"].ToString().Contains("silver"))
            {
                return View(department);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Departments/Create
        public ActionResult Create()
        {
            //checking permissions of the user
            if (Session["permissionTier"] == null)
            {
                return RedirectToAction("LogIn", "Users");
            }
            else if (Session["permissionTier"].ToString().Contains("silver"))
            {

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Departments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,name")] Department department)
        {
            if (ModelState.IsValid)
            {
                db.Departments.Add(department);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(department);
        }

        // GET: Departments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department department = db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            if (Session["permissionTier"] == null)
            {
                return RedirectToAction("LogIn", "Users");
            }
            else if (Session["permissionTier"].ToString().Contains("silver"))
            {

                return View(department);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Departments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,name")] Department department)
        {
            if (ModelState.IsValid)
            {
                db.Entry(department).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(department);
        }

        // GET: Departments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department department = db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            return View(department);
        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Department department = db.Departments.Find(id);
            db.Departments.Remove(department);
            db.SaveChanges();
            return RedirectToAction("Index");
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
