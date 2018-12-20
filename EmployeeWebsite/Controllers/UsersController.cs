using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using EmployeeWebsite;
using EmployeeWebsite.Models;

namespace EmployeeWebsite.Controllers
{
    public class UsersController : Controller
    {
        private mdbcontext db = new mdbcontext();
        private static Random random = new Random();
        private static int HashSize = 16;
        private static int SaltSize = 16;


        public static string Hash(string password, int iterations)
        {
            // Create salt
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[SaltSize]);

            // Create hash
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
            var hash = pbkdf2.GetBytes(HashSize);

            // Combine salt and hash
            var hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            // Convert to base64
            var base64Hash = Convert.ToBase64String(hashBytes);

            // Format hash with extra information
            return string.Format("$MYHASH$V1${0}${1}", iterations, base64Hash);
        }

        // <summary>
        // Creates a hash from a password with 10000 iterations
        // </summary>
        // <param name="password">The password.</param>
        // <returns>The hash.</returns>
        public static string Hash(string password)
        {
            return Hash(password, 10000);
        }

        // <summary>
        // Checks if hash is supported.
        // </summary>
        // <param name="hashString">The hash.</param>
        // <returns>Is supported?</returns>
        public static bool IsHashSupported(string hashString)
        {
            return hashString.Contains("$MYHASH$V1$");
        }

        // <summary>
        // Verifies a password against a hash.
        // </summary>
        // <param name="password">The password.</param>
        // <param name="hashedPassword">The hash.</param>
        // <returns>Could be verified?</returns>
        public static bool CheckPassword(string password, string hashedPassword)
        {
            // Check hash
            if (!IsHashSupported(hashedPassword))
            {
                throw new NotSupportedException("The hashtype is not supported");
            }

            // Extract iteration and Base64 string
            var splittedHashString = hashedPassword.Replace("$MYHASH$V1$", "").Split('$');
            var iterations = int.Parse(splittedHashString[0]);
            var base64Hash = splittedHashString[1];

            // Get hash bytes
            var hashBytes = Convert.FromBase64String(base64Hash);

            // Get salt
            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Create hash with given salt
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            // Get result
            for (var i = 0; i < HashSize; i++)
            {
                if (hashBytes[i + SaltSize] != hash[i])
                {
                    return false;
                }
            }
            return true;
        }
    
    


        //private byte[] CreateSalt()
        //{
        //    byte[] salt;
        //    new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
        //    return salt;

        //}

        //private byte[] HashValue(string password, byte[] salt)
        //{
        //    var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
        //    byte[] hash = pbkdf2.GetBytes(20);
        //    return hash;
        //}
        //private void Storage(byte[] salt, byte[] hash)
        //{
        //    byte[] hashBytes = new byte[36];
        //    Array.Copy(salt, 0, hashBytes, 0, 16);
        //    Array.Copy(hash, 0, hashBytes, 16, 20);
        //}
        //private void VerifyPassword()
        //{
        //    /* Fetch the stored value */
        //    string savedPasswordHash = DBContext.GetUser(u => u.UserName == user).Password;
        //    /* Extract the bytes */
        //    byte[] hashBytes = Convert.FromBase64String(savedPasswordHash);
        //    /* Get the salt */
        //    byte[] salt = new byte[16];
        //    Array.Copy(hashBytes, 0, salt, 0, 16);
        //    /* Compute the hash on the password the user entered */
        //    var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
        //    byte[] hash = pbkdf2.GetBytes(20);
        //    /* Compare the results */
        //    for (int i = 0; i < 20; i++)
        //        if (hashBytes[i + 16] != hash[i])
        //            throw new UnauthorizedAccessException();
        //}

        private void CheckForAdmin()
        {
            List<User> list = db.Users.Where(x => x.permissionTier == "guest,bronze,silver,gold").ToList();
            if(list.Count == 0)
            {
                User admin = new User();
                admin.username = "admin";
                admin.password = Hash("123456");
                admin.permissionTier = "guest,bronze,silver,gold";
                db.Users.Add(admin);
                db.SaveChanges();

            }
        }

        // GET: Users
        public ActionResult Index()
        {
            CheckForAdmin();            
            if(Session["username"] != null)
            {
                
                User user = db.Users.Find(int.Parse(Session["id"].ToString()));
                
                if(user.permissionTier.Contains("silver"))
                {
                    return View(db.Users.ToList());
                }
                else
                {
                    return RedirectToAction("Details", new RouteValueDictionary(
                        new { controller = "Users", action = "Details", Id = user.Id }));
                }

            }
            else
            {
                return RedirectToAction("LogIn");
            }
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            if (Session["permissionTier"] == null)
            {
                return RedirectToAction("LogIn", "Users");
            }
            else if (Session["permissionTier"].ToString().Contains("silver") || Session["username"].ToString() == user.username)
            {
                
                if (user.Employees.Count != 0)
                {
                    Employee e = user.Employees.First();
                    return RedirectToAction("Details", new RouteValueDictionary(
                       new { controller = "Employees", action = "Details", Id = e.Id }));
                    
                }
                else
                {
                    return View(user);
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,username,password")] User user)
        {
            if (ModelState.IsValid)
            {
                user.permissionTier = "guest";
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            else if(Session["permissionTier"].ToString().Contains("silver") || Session["username"].ToString() == user.username)
            {
                return View(user);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }


        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,username,password,permissionTier")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            if (Session["permissionTier"] == null)
            {
                return RedirectToAction("LogIn", "Users");
            }
            else if (Session["permissionTier"].ToString().Contains("gold"))
            {
                return View(user);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }            
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult LogIn()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Verify()
        {
            List<User> temp = new List<User>();
            string usrname = Request["username"];
            temp = db.Users.Where(x => x.username == usrname).ToList();
            if(temp.Count != 0)
            {
                User user = temp.First();

                if (user.username == usrname && CheckPassword(Request["password"].ToString(), user.password))
                {
                    Session["username"] = user.username;
                    Session["permissionTier"] = user.permissionTier;
                    Session["id"] = user.Id;
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("LogIn");
                }
            }
            else
            {
                return RedirectToAction("LogIn");
            }
        }
        public ActionResult LogOut()
        {
            Session["username"] = null;
            Session["permissionTier"] = null;
            Session["id"] = null;
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
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public ActionResult CreateRandomUser()
        {
            if (Session["permissionTier"].ToString() == "silver" || Session["permissionTier"].ToString() == "gold")
            {
                User user = new User();
                user.username = RandomString(10);
                user.password = Hash("123456");
                user.permissionTier = "guest";
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Create", "Employees");
            }
            else
            {
                return RedirectToAction("Index", "Employees");
            }
        }
    }
}
