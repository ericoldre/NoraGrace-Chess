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
            NoraGrace.Web.Model.GameService gameService = new Model.GameService(db);

            Web.Model.GameInfo gameInfo;
            try
            {
                gameInfo = gameService.Find(id.Value);
            }
            catch (ArgumentOutOfRangeException)
            {
                return HttpNotFound();
            }

            ViewModels.Games.DetailsViewModel viewModel = new ViewModels.Games.DetailsViewModel();
            viewModel.GameInfo = gameInfo;
            return View(viewModel);
        }

        // GET: DbGames/Create
        public ActionResult Create()
        {
            ViewModels.Games.CreateViewModel vm = new ViewModels.Games.CreateViewModel();
            return View(vm);
        }

        // POST: DbGames/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ViewModels.Games.CreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                NoraGrace.Web.Model.GameService gameService = new Model.GameService(db);
                var created = gameService.Create(new Model.GameCreateOptions() { White = viewModel.White, Black = viewModel.Black });

                return RedirectToAction("Index");
            }

            return View(viewModel);
        }

        public ActionResult ApplyMove(int id, string move)
        {
            NoraGrace.Web.Model.GameService gameService = new Model.GameService(db);
            var info = gameService.ApplyMove(id, move);


            return RedirectToAction("Details", new { id = id });


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
