using NoraGrace.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using NoraGrace.Engine.Evaluation;

namespace NoraGrace.Engine.Tests
{
    
    
    /// <summary>
    ///This is a test class for ChessEvalSettingsTest and is intended
    ///to contain all ChessEvalSettingsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ChessEvalSettingsTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion




        /// <summary>
        ///A test for CloneDeep
        ///</summary>
        [TestMethod()]
        public void CloneDeepTest()
        {
            Settings settings = Settings.Default(); // TODO: Initialize to an appropriate value

            var cloned = settings.CloneDeep();

            Assert.AreEqual(cloned, settings);
        }
    }
}
