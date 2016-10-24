using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using NoraGrace.Sql;

namespace NoraGrace.Web.Controllers
{
    public class DbGamesController : Controller
    {
        private ChessDb db = new ChessDb();

        // GET: DbGames
        public ActionResult Index()
        {
            return View(db.Games.ToList());
        }

        // GET: DbGames/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DbGame dbGame = db.Games.Find(id);
            if (dbGame == null)
            {
                return HttpNotFound();
            }
            return View(dbGame);
        }

        // GET: DbGames/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DbGames/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "GameId,White,Black,Result,ResultReason")] DbGame dbGame)
        {
            if (ModelState.IsValid)
            {
                db.Games.Add(dbGame);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(dbGame);
        }

        // GET: DbGames/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DbGame dbGame = db.Games.Find(id);
            if (dbGame == null)
            {
                return HttpNotFound();
            }
            return View(dbGame);
        }

        // POST: DbGames/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "GameId,White,Black,Result,ResultReason")] DbGame dbGame)
        {
            if (ModelState.IsValid)
            {
                db.Entry(dbGame).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(dbGame);
        }

        // GET: DbGames/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DbGame dbGame = db.Games.Find(id);
            if (dbGame == null)
            {
                return HttpNotFound();
            }
            return View(dbGame);
        }

        // POST: DbGames/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DbGame dbGame = db.Games.Find(id);
            db.Games.Remove(dbGame);
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
