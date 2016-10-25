using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoraGrace.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NoraGrace.Web.Controllers.Tests
{
    [TestClass()]
    public class DbGamesControllerTests
    {
        //[TestMethod()]
        //public void IndexTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void DetailsTest()
        //{
        //    Assert.Fail();
        //}

        [TestMethod()]
        public void CreateTest()
        {
            var controller = new Web.Controllers.DbGamesController();
            var result = controller.Create();

            Assert.IsInstanceOfType(result, typeof(System.Web.Mvc.ViewResult));
            ViewResult vresult = (ViewResult)result;
            Assert.AreEqual("Create", vresult.ViewName);
            Assert.IsTrue(vresult.Model is Web.ViewModels.Games.CreateViewModel);
        }

        [TestMethod()]
        public void CreatePostValidTest()
        {
            var controller = new Web.Controllers.DbGamesController();
            var result = controller.Create(new ViewModels.Games.CreateViewModel()
            {
                White = "W", Black = "B"
            });

            Assert.IsInstanceOfType(result, typeof(System.Web.Mvc.RedirectToRouteResult));

            var typedResult = result as RedirectToRouteResult;
            Assert.AreEqual("Index", typedResult.RouteValues["Action"]);
        }

        //[TestMethod()]
        //public void EditTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void EditTest1()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void DeleteTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void DeleteConfirmedTest()
        //{
        //    Assert.Fail();
        //}
    }
}