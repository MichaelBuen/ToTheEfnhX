﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestProject.SampleModel;


using System.Linq;
using System.Data.SqlClient;


using Ienablemuch.ToTheEfnhX;

using EF = Ienablemuch.ToTheEfnhX.EntityFramework;
using NH = Ienablemuch.ToTheEfnhX.NHibernate;

using Ienablemuch.ToTheEfnhX.Memory;


// using Ienablemuch.ToTheEfnhX.CommonExtensions;

using System.Collections.Generic;


using System.Data.Entity;
using NHibernate.Linq;

using Ienablemuch.ToTheEfnhX.ForImplementorsOnly;
using System.Transactions;
using System.Data;
using System.Reflection;


using System.Linq.Expressions;

namespace TestProject
{


    /// <summary>
    ///This is a test class for IRepositoryTest and is intended
    ///to contain all IRepositoryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TestTheIRepository
    {

        static object heed = new object();

        // Called for every TestMethod

        string connectionString = "Data Source=localhost; Initial Catalog=TestEfnhX; User Id=sa; Password=P@$$w0rd";

        // [TestInitialize]
        public void EmptyDatabase()
        {

            using (var con = new SqlConnection(connectionString))
            {
                lock (con)
                {


                    con.Open();

                    var cmd = new SqlCommand(
    @"
delete from ProductPrice; 
delete from Product; 


delete from AnswerComment;
delete from Answer;
delete from QuestionComment;
delete from Question;
", con);
                    cmd.ExecuteNonQuery();
                }

            }
        }


        [TestMethod]
        public void Memory_CanSave()
        {
            IRepository<Product> db = new Repository<Product>();
            Common_CanSave(db);
        }
        [TestMethod]
        public void Ef_CanSave()
        {
            EmptyDatabase();
            IRepository<Product> db = new EF.Repository<Product>(new EfDbMapper(connectionString));
            Common_CanSave(db);
        }
        [TestMethod]
        public void Nh_CanSave()
        {
            EmptyDatabase();
            IRepository<Product> db = new NH.Repository<Product>(NhModelsMapper.GetSession(connectionString));
            Common_CanSave(db);
        }
        void Common_CanSave(IRepository<Product> db)
        {

            db.Save(new Product { ProductName = "Optimus", Category = "Autobots", MinimumPrice = 7 });
            db.Save(new Product { ProductName = "Bumble Bee", Category = "Autobots", MinimumPrice = 8 });
            db.Save(new Product { ProductName = "Megatron", Category = "Decepticon", MinimumPrice = 9 });



            Assert.AreEqual(7 + 8 + 9, db.All.Sum(x => x.MinimumPrice));
            Assert.AreEqual(3, db.All.Count());
        }


        [TestMethod]
        public void Nh_CanSaveHeaderDetail()
        {
            EmptyDatabase();
            IRepository<Product> db = new NH.Repository<Product>(NhModelsMapper.GetSession(connectionString));
            // throw new Exception("Provider : " + db.All.Provider.GetType().ToString()); // NHibernate.Linq.NhQueryProvider
            Common_CanSaveHeaderDetail(db);
        }

        [TestMethod]
        public void Ef_CanSaveHeaderDetail()
        {
            EmptyDatabase();
            IRepository<Product> db = new EF.Repository<Product>(new EfDbMapper(connectionString));
            // throw new Exception("Provider : " + db.All.Provider.GetType().ToString()); // System.Data.Entity.Internal.Linq.DbQueryProvider
            Common_CanSaveHeaderDetail(db);


        }


        [TestMethod]
        public void Memory_CanSaveHeaderDetail()
        {
            IRepository<Product> db = new Repository<Product>();
            // throw new Exception("Provider: " + db.All.Provider.GetType().ToString()); // System.Linq.EnumerableQuery`1[TestProject.SampleModel.Product]
            Common_CanSaveHeaderDetail(db);
        }


        void Common_CanSaveHeaderDetail(IRepository<Product> db)
        {


            // Arrange
            var px = new Product
            {
                ProductName = "Optimus",
                Category = "Autobots",
                MinimumPrice = 7
            };
            px.PriceList =
                new List<ProductPrice>()
                {
                    new ProductPrice { Product = px, Price = 777, EffectiveDate = DateTime.Today },
                    new ProductPrice { Product = px, Price = 888, EffectiveDate = DateTime.Today },
                    new ProductPrice { Product = px, Price = 999, EffectiveDate = DateTime.Today },
                    new ProductPrice { Product = px, Price = 222, EffectiveDate = DateTime.Today },

                };


            // Act
            db.SaveGraph(px);



            Assert.AreEqual(4, px.PriceList.Count());


            px = db.GetEager(px.ProductId);

            

            Assert.AreEqual("Optimus", px.ProductName);
            px.ProductName = px.ProductName + "!";
            px.PriceList[2].Price = 333;
            Assert.AreEqual(4, px.PriceList.Count);


            db.SaveGraph(px);





            // Assert

            Assert.AreEqual(px.ProductName, px.ProductName);
            Assert.AreEqual(px.ProductId, px.ProductId);
            Assert.AreEqual(4, px.PriceList.Count);
        }



        [TestMethod]
        public void Saving_is_ok()
        {



            // Arrange
            /*EmptyDatabase();
            IRepository<Product> db = new NhRepository<Product>(NhModelsMapper.GetSession(connectionString));*/

            IRepository<Product> db = new Repository<Product>();

            var px = new Product
            {
                ProductName = "Optimus",
                Category = "Autobots",
                MinimumPrice = 7
            };
            px.PriceList =
                new List<ProductPrice>()
		    {
			    new ProductPrice { Product = px, Price = 777, EffectiveDate = DateTime.Today },
			    new ProductPrice { Product = px, Price = 888, EffectiveDate = DateTime.Today },
			    new ProductPrice { Product = px, Price = 999, EffectiveDate = DateTime.Today },
			    new ProductPrice { Product = px, Price = 222, EffectiveDate = DateTime.Today },

		    };
            // Act
            db.Save(px);

            /*var query = (from x in db.All
                            where x.ProductId == px.ProductId
                            select x).EagerLoadMany(x => x.PriceList);
             
                var xxx = query.Single();
                */

            var xxx = db.GetEager(px.ProductId);




            Assert.AreEqual(px.ProductName, xxx.ProductName);
            Assert.AreEqual(px.ProductId, xxx.ProductId);
            Assert.AreEqual(4, xxx.PriceList.Count());

        }





        [TestMethod]
        public void Memory_CanDelete()
        {
            IRepository<Product> db = new Repository<Product>();
            Common_CanDelete(db);
        }


        [TestMethod]
        public void Ef_CanDelete()
        {
            EmptyDatabase();
            IRepository<Product> db = new EF.Repository<Product>(new EfDbMapper(connectionString));
            Common_CanDelete(db);
        }
        [TestMethod]
        public void Nh_CanDelete()
        {
            EmptyDatabase();
            IRepository<Product> db = new NH.Repository<Product>(NhModelsMapper.GetSession(connectionString));
            Common_CanDelete(db);
        }
        public void Common_CanDelete(IRepository<Product> db)
        {

            db.Save(new Product { ProductName = "Optimus", Category = "Autobots", MinimumPrice = 7 });

            var px = new Product { ProductName = "Bumble Bee", Category = "Autobots", MinimumPrice = 8 };
            db.Save(px);

            db.Save(new Product { ProductName = "Megatron", Category = "Decepticon", MinimumPrice = 9 });

            db.Delete(px.ProductId, px.RowVersion);

            Assert.AreEqual(7 + 9, db.All.Sum(x => x.MinimumPrice));
            Assert.AreEqual(null, db.Get(px.ProductId));
            Assert.AreEqual(2, db.All.Count());
        }


        [TestMethod]
        public void Memory_CanUpdate()
        {
            IRepository<Product> db = new Repository<Product>();
            Common_CanUpdate(db);
        }
        [TestMethod]
        public void Ef_CanUpdate()
        {
            EmptyDatabase();
            IRepository<Product> db = new EF.Repository<Product>(new EfDbMapper(connectionString));
            Common_CanUpdate(db);
        }
        [TestMethod]
        public void Nh_CanUpdate()
        {
            EmptyDatabase();
            IRepository<Product> db = new NH.Repository<Product>(NhModelsMapper.GetSession(connectionString));
            Common_CanUpdate(db);
        }




        public void Common_CanUpdate(IRepository<Product> db)
        {




            // Arrange            

            var px = new Product
                {
                    ProductName = "Bumble Bee",
                    Category = "Autobots",
                    MinimumPrice = 8
                };



            px.PriceList =
                new List<ProductPrice>()
                {
                    new ProductPrice { Product = px, Price = 234, EffectiveDate = DateTime.Today },
                    new ProductPrice { Product = px, Price = 300, EffectiveDate = DateTime.Today.AddDays(100) }
                };
            db.SaveGraph(px);




            // int n = px.PriceList[0].ProductPriceId;





            // simulate web(i.e. stateless)

            int productPriceId = db.Get(px.ProductId).PriceList[0].ProductPriceId;



            var fromWeb =
                    new Product
                    {
                        ProductId = px.ProductId,
                        ProductName = "hahaha", // px.ProductName + "---" + Guid.NewGuid().ToString(),
                        Category = px.Category,
                        MinimumPrice = px.MinimumPrice,
                        RowVersion = db.Get(px.ProductId).RowVersion,
                        PriceList = new List<ProductPrice>()


                    };





            fromWeb.PriceList.Add(new ProductPrice { Product = fromWeb, ProductPriceId = productPriceId, Price = 767676, EffectiveDate = DateTime.Today });
            fromWeb.PriceList.Add(new ProductPrice { Product = fromWeb, Price = 2148, EffectiveDate = DateTime.Today });
            fromWeb.PriceList.Add(new ProductPrice { Product = fromWeb, Price = 2048, EffectiveDate = DateTime.Today });
            fromWeb.PriceList.Add(new ProductPrice { Product = fromWeb, Price = 222, EffectiveDate = DateTime.Today });




            /*fromWeb.PriceList.Add(new ProductPrice { Product = fromWeb, Price = 888, EffectiveDate = DateTime.Today });
            fromWeb.PriceList.Add(new ProductPrice { Product = fromWeb, Price = 222, EffectiveDate = DateTime.Today });*/



            // Act
            string expecting = "Bumble Bee Battle Mode hickhickhick";
            fromWeb.ProductName = expecting;
            db.SaveGraph(fromWeb);


            // Assert            
            Assert.AreEqual(expecting, db.Get(fromWeb.ProductId).ProductName);
            // Assert.AreNotEqual(px.ProductName, db.Get(fromWeb.ProductId).ProductName);


        }

        [TestMethod]
        public void Memory_HasRowVersion()
        {
            IRepository<Product> db = new Repository<Product>();
            Common_HasRowVersion(db);
        }
        [TestMethod]
        public void Ef_HasRowVersion()
        {
            EmptyDatabase();
            IRepository<Product> db = new EF.Repository<Product>(new EfDbMapper(connectionString));
            Common_HasRowVersion(db);
        }
        [TestMethod]
        public void Nh_HasRowVersion()
        {
            EmptyDatabase();
            IRepository<Product> db = new NH.Repository<Product>(NhModelsMapper.GetSession(connectionString));
            Common_HasRowVersion(db);
        }
        public void Common_HasRowVersion(IRepository<Product> db)
        {

            string expected = "Jollibee";

            // Arrange
            var px = new Product { ProductName = "Bumble Bee", Category = "Autobots", MinimumPrice = 8 };


            // Act
            db.Save(px);
            byte[] originalVersion = px.RowVersion;


            // Noted corner case, when there's no changes in any of the properties, NHibernate still persist the class,
            // while EF don't; hence when the following line is commented, the rowversion doesn't change on EF.
            px.ProductName = expected;

            db.Save(px);
            byte[] newVersion = px.RowVersion;


            // Assert
            Assert.AreEqual(expected, px.ProductName);
            Assert.AreNotEqual(null, px.RowVersion);            
            CollectionAssert.AreNotEqual(originalVersion, newVersion);            
        }


        [TestMethod]
        [ExpectedException(typeof(DbChangesConcurrencyException))]
        public void Memory_CanDetectUpdateConflictingUpdate()
        {
            IRepository<Product> db = new Repository<Product>();
            Common_CanDetectUpdateConflictingUpdate(db);
        }
        [TestMethod]
        [ExpectedException(typeof(DbChangesConcurrencyException))]
        public void Ef_CanDetectUpdateConflictingUpdate()
        {
            EmptyDatabase();
            IRepository<Product> db = new EF.Repository<Product>(new EfDbMapper(connectionString));
            Common_CanDetectUpdateConflictingUpdate(db);
        }
        [TestMethod]
        [ExpectedException(typeof(DbChangesConcurrencyException))]
        public void Nh_CanDetectUpdateConflictingUpdate()
        {
            EmptyDatabase();
            IRepository<Product> db = new NH.Repository<Product>(NhModelsMapper.GetSession(connectionString));
            Common_CanDetectUpdateConflictingUpdate(db);
        }
        public void Common_CanDetectUpdateConflictingUpdate(IRepository<Product> db)
        {



            // Arrange            
            var px = new Product { ProductName = "Bumble Bee", Category = "Autobots", MinimumPrice = 8 };
            db.Save(px);


            // Act
            // simulate web(i.e. stateless)
            Product firstOpener = new Product { ProductId = px.ProductId, ProductName = px.ProductName, Category = px.Category, RowVersion = px.RowVersion };
            Product secondOpener = new Product { ProductId = px.ProductId, ProductName = px.ProductName, Category = px.Category, RowVersion = px.RowVersion };
            db.Save(firstOpener);


            // Expected to fail
            db.Save(secondOpener);
        }



        [TestMethod]
        [ExpectedException(typeof(DbChangesConcurrencyException))]
        public void Memory_CanDetectUpdateConflictingDelete()
        {
            IRepository<Product> db = new Repository<Product>();
            Common_CanDetectUpdateConflictingDelete(db);
        }
        [TestMethod]
        [ExpectedException(typeof(DbChangesConcurrencyException))]
        public void Ef_CanDetectUpdateConflictingDelete()
        {
            EmptyDatabase();
            IRepository<Product> db = new EF.Repository<Product>(new EfDbMapper(connectionString));
            Common_CanDetectUpdateConflictingDelete(db);
        }
        [TestMethod]
        [ExpectedException(typeof(DbChangesConcurrencyException))]
        public void Nh_CanDetectUpdateConflictingDelete()
        {
            EmptyDatabase();
            IRepository<Product> db = new NH.Repository<Product>(NhModelsMapper.GetSession(connectionString));
            Common_CanDetectUpdateConflictingDelete(db);
        }
        public void Common_CanDetectUpdateConflictingDelete(IRepository<Product> db)
        {





            // Arrange            
            var px = new Product { ProductName = "Bumble Bee", Category = "Autobots", MinimumPrice = 8 };
            db.Save(px);


            // Act
            // simulate web(i.e. stateless)
            Product firstOpener = new Product { ProductId = px.ProductId, ProductName = px.ProductName, Category = px.Category, RowVersion = px.RowVersion };
            Product secondOpener = new Product { ProductId = px.ProductId, ProductName = px.ProductName, Category = px.Category, RowVersion = px.RowVersion };
            db.Save(firstOpener);


            // Expected to fail
            db.Delete(secondOpener.ProductId, secondOpener.RowVersion);

        }





        [TestMethod]
        [ExpectedException(typeof(DbChangesConcurrencyException))]
        public void Memory_CanDetectDeleteConflictingUpdate()
        {
            IRepository<Product> db = new Repository<Product>();
            Common_CanDetectDeleteConflictingUpdate(db);
        }
        [TestMethod]
        [ExpectedException(typeof(DbChangesConcurrencyException))]
        public void Ef_CanDetectDeleteConflictingUpdate()
        {
            EmptyDatabase();
            IRepository<Product> db = new EF.Repository<Product>(new EfDbMapper(connectionString));
            Common_CanDetectDeleteConflictingUpdate(db);
        }
        [TestMethod]
        [ExpectedException(typeof(DbChangesConcurrencyException))]
        public void Nh_CanDetectDeleteConflictingUpdate()
        {
            EmptyDatabase();
            IRepository<Product> db = new NH.Repository<Product>(NhModelsMapper.GetSession(connectionString));
            Common_CanDetectDeleteConflictingUpdate(db);
        }
        public void Common_CanDetectDeleteConflictingUpdate(IRepository<Product> db)
        {




            // Arrange
            var px = new Product { ProductName = "Bumble Bee", Category = "Autobots", MinimumPrice = 8 };
            db.Save(px);


            // Act
            Product firstOpener = new Product { ProductId = px.ProductId, ProductName = px.ProductName, Category = px.Category, RowVersion = px.RowVersion };
            Product secondOpener = new Product { ProductId = px.ProductId, ProductName = px.ProductName, Category = px.Category, RowVersion = px.RowVersion };
            db.Delete(firstOpener.ProductId, firstOpener.RowVersion);

            // Assert
            // Expected to fail
            db.Save(secondOpener);

        }


        [TestMethod]
        [ExpectedException(typeof(DbChangesConcurrencyException))]
        public void Memory_CanDetectDeleteConflictingDelete()
        {
            IRepository<Product> db = new Repository<Product>();
            Common_CanDetectDeleteConflictingDelete(db);
        }

        // culprit?
        [TestMethod]
        [ExpectedException(typeof(DbChangesConcurrencyException))]
        public void Ef_CanDetectDeleteConflictingDelete()
        {
            EmptyDatabase();
            IRepository<Product> db = new EF.Repository<Product>(new EfDbMapper(connectionString));
            Common_CanDetectDeleteConflictingDelete(db);
        }
        [TestMethod]
        [ExpectedException(typeof(DbChangesConcurrencyException))]
        public void Nh_CanDetectDeleteConflictingDelete()
        {
            EmptyDatabase();
            IRepository<Product> db = new NH.Repository<Product>(NhModelsMapper.GetSession(connectionString));
            Common_CanDetectDeleteConflictingDelete(db);
        }
        public void Common_CanDetectDeleteConflictingDelete(IRepository<Product> db)
        {




            // Arrange
            var px = new Product { ProductName = "Bumble Bee", Category = "Autobots", MinimumPrice = 8 };
            db.Save(px);


            // Act
            Product firstOpener = new Product { ProductId = px.ProductId, ProductName = px.ProductName, Category = px.Category, RowVersion = px.RowVersion };
            Product secondOpener = new Product { ProductId = px.ProductId, ProductName = px.ProductName, Category = px.Category, RowVersion = px.RowVersion };
            db.Delete(firstOpener.ProductId, firstOpener.RowVersion);

            // Assert
            // Expected to fail
            db.Save(secondOpener);
        }



        [TestMethod]
        public void Memory_CanHaveIncrementingKey()
        {
            IRepository<Product> db = new Repository<Product>();
            Common_CanHaveIncrementingKey(db);
        }
        [TestMethod]
        public void Ef_CanHaveIncrementingKey()
        {
            EmptyDatabase();
            IRepository<Product> db = new EF.Repository<Product>(new EfDbMapper(connectionString));
            Common_CanHaveIncrementingKey(db);
        }
        [TestMethod]
        public void Nh_CanHaveIncrementingKey()
        {
            EmptyDatabase();
            IRepository<Product> db = new NH.Repository<Product>(NhModelsMapper.GetSession(connectionString));
            Common_CanHaveIncrementingKey(db);
        }
        public void Common_CanHaveIncrementingKey(IRepository<Product> db)
        {



            // decimal ny = db.All.Where(x => x.ProductId == 111).Max(x => x.MinimumPrice);
            // decimal nx = db.All.Where(x => x.ProductId == 111).Select(x => new { TheMinimumPrice = (decimal?)x.MinimumPrice }).Max(x => x.TheMinimumPrice) ?? 0;
            // decimal nx = db.All.Where(x => x.ProductId == 111).Select(x => (decimal?)x.MinimumPrice).Max() ?? 0;




            // Arrange
            var optimusPrime = new Product { ProductName = "Optimus", Category = "Autobots", MinimumPrice = 7 };
            var bumbleBee = new Product { ProductName = "Bumble Bee", Category = "Autobots", MinimumPrice = 8 };
            var megatron = new Product { ProductName = "Megatron", Category = "Decepticon", MinimumPrice = 9 };


            // Act
            db.Save(optimusPrime);
            db.Save(bumbleBee);
            db.Save(megatron);

            int n = optimusPrime.ProductId;

            // Assert
            Assert.AreEqual(n, optimusPrime.ProductId);
            Assert.AreEqual(n + 1, bumbleBee.ProductId);
            Assert.AreEqual(n + 2, megatron.ProductId);

        }


        [TestMethod]
        public void Fetching_strategies_of_NHibernate_helper_has_no_problem_on_mocked_IQueryable()
        {
            IRepository<Product> db = new Repository<Product>();
            // var z = db.All.Where(x => x.ProductId == 1).EagerLoadMany(x => x.PriceList).ToList();
            var z = NH.EagerExtensionHelper.EagerLoad(db.All.Where(x => x.ProductId == 1), "PriceList").ToList();
        }

        [TestMethod]
        public void Fetching_strategies_of_EntityFramework_helper_has_no_problem_on_mocked_IQueryable()
        {
            IRepository<Product> db = new Repository<Product>();
            // var z = db.All.Where(x => x.ProductId == 1).EagerLoadMany(x => x.PriceList).ToList();
            var z = EF.EagerExtensionHelper.EagerLoad(db.All.Where(x => x.ProductId == 1), "PriceList").ToList();
        }


        [TestMethod]
        public void Ef_Can_Orm_SaveGraph_Can_Create()
        {
            EmptyDatabase();
            IRepository<Question> db = new EF.Repository<Question>(new EfDbMapper(connectionString));
            Common_Can_Orm_SaveGraph_Can_Create(db);



        }


        [TestMethod]
        public void Nh_Can_Orm_SaveGraph_Can_Create()
        {
            EmptyDatabase();
            IRepository<Question> db = new NH.Repository<Question>(NhModelsMapper.GetSession(connectionString));            
            Common_Can_Orm_SaveGraph_Can_Create(db);
        }


        void Common_Can_Orm_SaveGraph_Can_Create(IRepository<Question> repo)
        {

            // Arrange
            Question importantQuestion = Refactored_Common_SaveGraph_Arrange_Create(repo);

            // Act            
            int questionId = importantQuestion.QuestionId;
            byte[] rowVersion = importantQuestion.RowVersion;
            Question retrievedQuestion = repo.GetEager(questionId);


            // Assert            
            Assert.IsNotNull(importantQuestion.RowVersion);
            Assert.AreNotEqual(0, importantQuestion.QuestionId);

            Assert.AreNotSame(importantQuestion, retrievedQuestion);


            Assert.IsNotNull(retrievedQuestion.RowVersion);
            Assert.AreEqual("The answer to life", retrievedQuestion.Text);

            Assert.IsNotNull(retrievedQuestion.Answers, "Answers not populated");
            CollectionAssert.AllItemsAreUnique(retrievedQuestion.Answers.Select(x => x.Poster).ToList());

            CollectionAssert.AreEqual(
                new string[] { "John", "Elton", "Paul" }.OrderBy(x => x).ToList(),
                retrievedQuestion.Answers.Select(x => x.Poster).OrderBy(x => x).ToList());

            CollectionAssert.AreNotEqual(
                new string[] { "John", "Paul" }.OrderBy(x => x).ToList(),
                retrievedQuestion.Answers.Select(x => x.Poster).OrderBy(x => x).ToList());

            CollectionAssert.AreNotEqual(
                new string[] { "John", "Yoko" }.OrderBy(x => x).ToList(),
                retrievedQuestion.Answers.Select(x => x.Poster).OrderBy(x => x).ToList());

            CollectionAssert.AreNotEqual(
                new string[] { "John", "Elton", "Paul", "Elvis" }.OrderBy(x => x).ToList(),
                retrievedQuestion.Answers.Select(x => x.Poster).OrderBy(x => x).ToList());


            Assert.AreEqual("George", retrievedQuestion.Comments.Where(x => x.Text == "Is There?").Single().Poster);


            Assert.AreEqual("Ringo", retrievedQuestion.Answers.Single(x => x.Poster == "Paul").Comments.Single().Poster);

        }


        // real culprit
        [TestMethod]
        public void Ef_Can_Orm_SaveGraph_Can_Update()
        {
            EmptyDatabase();
            IRepository<Question> db = new EF.Repository<Question>(new EfDbMapper(connectionString));
            Common_Orm_SaveGraph_Can_Update(db);
        }


        [TestMethod]
        public void Nh_Can_Orm_SaveGraph_Can_Update()
        {
            EmptyDatabase();
            IRepository<Question> db = new NH.Repository<Question>(NhModelsMapper.GetSession(connectionString));
            Common_Orm_SaveGraph_Can_Update(db);
        }


        void Common_Orm_SaveGraph_Can_Update(IRepository<Question> repo)
        {



            // Arrange
            Question importantQuestion = Refactored_Common_SaveGraph_Arrange_Create(repo);
            string expectedText = "number 9, number 9, number 9...";


            // Act            
            int questionId = importantQuestion.QuestionId;
            byte[] rowVersion = importantQuestion.RowVersion;





            Question retrievedQuestion = repo.GetEager(questionId);

            int originalAnswersCount = retrievedQuestion.Answers.Count;
            // throw new Exception(retrievedQuestion.GetType().ToString());


            retrievedQuestion.Text = "Hello";
            retrievedQuestion.Answers.Single(x => x.Poster == "John").Text = expectedText;

            var z = retrievedQuestion.Answers.Single(x => x.Poster == "John");
            z.Text = expectedText;

            var a = retrievedQuestion.Answers.Single(x => x.Poster == "Paul");
            retrievedQuestion.Answers.Remove(a);




            repo.SaveGraph(retrievedQuestion); // save the whole object graph



            Question retrievedMergedQuestion = repo.GetEager(questionId);




            // Assert            
            Assert.AreNotSame(importantQuestion, retrievedQuestion);
            Assert.AreNotSame(retrievedQuestion, retrievedMergedQuestion);
            
            Assert.AreEqual(expectedText, retrievedMergedQuestion.Answers.Single(x => x.Poster == "John").Text);
            Assert.AreEqual("Hello", retrievedMergedQuestion.Text);

            Assert.AreEqual(3, originalAnswersCount);
            Assert.AreEqual(2, retrievedMergedQuestion.Answers.Count);



        }

        Question Refactored_Common_SaveGraph_Arrange_Create(IRepository<Question> repo)
        {

            var importantQuestion = PopulateQuestion();


            repo.SaveGraph(importantQuestion);



            return importantQuestion;
        }



        private Question PopulateQuestion()
        {

            var importantQuestion = new Question { Text = "The answer to life", Poster = "Boy", Answers = new List<Answer>(), Comments = new List<QuestionComment>() };
            var answerA = new Answer { Question = importantQuestion, Text = "42", Poster = "John", Comments = new List<AnswerComment>() };
            var answerB = new Answer { Question = importantQuestion, Text = "143", Poster = "Paul", Comments = new List<AnswerComment>() };
            var answerC = new Answer { Question = importantQuestion, Text = "888", Poster = "Elton", Comments = new List<AnswerComment>() };
            importantQuestion.Answers.Add(answerA);
            importantQuestion.Answers.Add(answerB);
            importantQuestion.Answers.Add(answerC);

            var commentToImportantQuestion = new QuestionComment { Question = importantQuestion, Text = "Is There?", Poster = "George" };
            importantQuestion.Comments.Add(commentToImportantQuestion);
            var commentToAnswerB = new AnswerComment { Answer = answerB, Text = "Isn't the answer is 7 times 6?", Poster = "Ringo" };
            answerB.Comments.Add(commentToAnswerB);
            return importantQuestion;
        }




        [TestMethod]
        public void Nh_Can_queue_changes()
        {
            EmptyDatabase();
            IRepository<Question> db = new NH.Repository<Question>(NhModelsMapper.GetSession(connectionString));
            Common_Can_queue_changes(db);
        }


        [TestMethod]
        public void Ef_Can_queue_changes()
        {
            EmptyDatabase();
            IRepository<Question> db = new EF.Repository<Question>(new EfDbMapper(connectionString));
            Common_Can_queue_changes(db);
        }

        void Common_Can_queue_changes(IRepository<Question> repo)
        {

            // Arrange
            Question importantQuestion = Refactored_Common_SaveGraph_Arrange_Create(repo);


            int questionId = importantQuestion.QuestionId;
            byte[] rowVersion = importantQuestion.RowVersion;



            /*
            Question retrievedQuestion;

            {
                var query = repo.All.Where(x => x.QuestionId == questionId);

                if (repo.All.Provider.GetType() == typeof(NHibernate.Linq.NhQueryProvider))
                {
                    retrievedQuestion = repo.Get(questionId);
                }
                else
                {
                    query = query.Include("Answers");
                    query = query.Include("Comments");
                    query = query.Include("Answers.Comments");
                    retrievedQuestion = query.Single();
                }
            }*/

            Question retrievedQuestion = repo.GetEager(questionId);

            retrievedQuestion.Text = "Hello";
            retrievedQuestion.Answers.Single(x => x.Poster == "John").Text = "number 9, number 9, number 9...";



            repo.Evict(questionId); // must Evict transient changes so it will not affect the next Save



            // Act            
            repo.Save(new Question { Text = "Hi", Poster = "Optimus" });

            /*
            Question testConflicter;  // let's check if the two aggregate root didn't affect each other
            {
                var query = repo.All.Where(x => x.QuestionId == questionId);

                if (repo.All.Provider.GetType() == typeof(NHibernate.Linq.NhQueryProvider))
                {
                    testConflicter = repo.Get(questionId);

                    // throw new Exception(testConflicter.Text + " " + testConflicter.Comments.Single().Text);
                }
                else
                {
                    query = query.Include("Answers");
                    query = query.Include("Comments");
                    query = query.Include("Answers.Comments");
                    testConflicter = query.Single();
                }
            }
            */


            Question testConflicter = repo.GetEager(questionId);


            // Assert            
            Assert.AreNotSame(importantQuestion, retrievedQuestion);
            Assert.AreNotSame(retrievedQuestion, testConflicter);

            Assert.AreEqual("The answer to life", testConflicter.Text);
            Assert.AreEqual("42", testConflicter.Answers.Single(x => x.Poster == "John").Text);


            Assert.AreEqual("Hello", retrievedQuestion.Text);

            /*
             Evicting the object from EF results on collections becoming empty, NHibernate left the stale object as is.
             
            throw new Exception(retrievedQuestion.Answers.Count.ToString()); // zero on Entity Framework after marking objects detached
            
            // Single won't work, there's no element
            Assert.AreEqual("number 9, number 9, number 9...", retrievedQuestion.Answers.Single(x => x.Poster == "John").Text);
            */





        }






        [TestMethod]
        public void Nh_Can_Orm_do_cascaded_deletions()
        {
            EmptyDatabase();
            IRepository<Question> db = new NH.Repository<Question>(NhModelsMapper.GetSession(connectionString));
            Common_Can_Orm_do_cascaded_deletions(db);
        }

        [TestMethod]
        public void Ef_Can_Orm_do_cascaded_deletions()
        {
            EmptyDatabase();
            IRepository<Question> db = new EF.Repository<Question>(new EfDbMapper(connectionString));
            Common_Can_Orm_do_cascaded_deletions(db);
        }

        void Common_Can_Orm_do_cascaded_deletions(IRepository<Question> db)
        {
            Question q = Refactored_Common_SaveGraph_Arrange_Create(db);

            db.DeleteCascade(q.QuestionId, q.RowVersion);

            Assert.IsNotNull(q);

            Question retrievedQ = db.Get(q.QuestionId);

            Assert.IsNull(retrievedQ);


        }



        // disable [TestMethod]
        public void TestNhibernateQuery()
        {


            // var s =;
            // var s = 

            /*
            var t = s.Query<Question>().Select(x => new { x.Text, Cx = x.Answers.AsQueryable().Count() } );
            t.ToList();*/

            // IRepository<Question> q = new NhRepository<Question>(NhModelsMapper.GetSession(connectionString));
            IRepository<Question> q = new EF.Repository<Question>(new EfDbMapper(connectionString));


            /*
            var t = s.Query<Question>().Any(x => x.Answers.Any(y => y.Text == "Paul"));

            var xx = new NhRepository<Question>(s);

            var yy = xx.All.Any(x => x.Answers.Any(y => y.Text == "John"));

            var zz = xx.All.Where(x => x.Text== "See");
             */

            /*
            IRepository<Question> db = new EfRepository<Question>(new EfDbMapper(connectionString));
            var zzz = db.All.Any(x => x.Answers.Any(y => y.Text == "John"));*/


            /*
            var t = db.Set<Question>().Select(x => new { x.Text, Cx = x.Answers.AsQueryable().Count() });

            var xxx = t.ToList();
            */


            var www = from x in q.All
                      from y in x.Answers.DefaultIfEmpty()
                      group y by new { QuestionId = x.QuestionId } into grp
                      select new { grp.Key.QuestionId, Count = grp.Sum(x => x.Question.QuestionId != null ? 1 : 0) };


            www.ToList();




        }

        [TestMethod]
        public void Can_do_deep_clone()
        {
            Question orig = PopulateQuestion();

            Question clone = (Question)orig.Clone();



            Assert.AreNotSame(orig, clone);
            Assert.AreNotSame(orig.Answers, clone.Answers);

            Assert.AreSame(orig, orig.Answers[0].Question);
            Assert.AreSame(clone, clone.Answers[0].Question);

            Assert.AreNotSame(orig.Answers[0], clone.Answers[0]);
            Assert.AreNotSame(orig.Answers[0].Question, clone.Answers[0].Question);
            Assert.AreNotSame(orig.Answers[1].Question, clone.Answers[1].Question);

            Assert.AreNotSame(orig.Answers[1].Comments, clone.Answers[1].Comments);
            Assert.AreNotSame(orig.Answers[1].Comments[0], clone.Answers[1].Comments[0]);

            Assert.AreSame(orig.Answers[1], orig.Answers[1].Comments[0].Answer);
            Assert.AreSame(clone.Answers[1], clone.Answers[1].Comments[0].Answer);

            Assert.AreEqual(orig.Text, clone.Text);
            Assert.AreEqual(orig.Answers.Count, clone.Answers.Count);
            Assert.AreEqual(orig.Answers[0].Text, clone.Answers[0].Text);
            Assert.AreEqual(orig.Answers[1].Comments[0].Text, clone.Answers[1].Comments[0].Text);


        }



        [TestMethod]
        public void Ef_Can_rollback_transaction()
        {
            EmptyDatabase();

            var x = new EfDbMapper(connectionString);
            ITransactionBoundFactory xf = new EF.TransactionBoundFactory();
            
            IRepository<Product> prod = new EF.Repository<Product>(x);
            IRepository<Question> ques = new EF.Repository<Question>(x);
            Common_Can_rollback_transaction(xf, prod, ques);
        }

        [TestMethod]
        public void Nh_Can_rollback_transaction()
        {
            EmptyDatabase();

            NHibernate.ISession x =  NhModelsMapper.GetSession(connectionString);
            ITransactionBoundFactory xf = new NH.TransactionBoundFactory(x);


            IRepository<Product> prod = new NH.Repository<Product>(x);
            IRepository<Question> ques = new NH.Repository<Question>(x);
            Common_Can_rollback_transaction(xf, prod, ques);
        }


        
        void Common_Can_rollback_transaction(ITransactionBoundFactory xf, IRepository<Product> p, IRepository<Question> q)
        {

            try
            {
                // Arrange
                using (var tx = xf.BeginTransaction())
                {
                    p.Save(new Product { ProductName = "Hello", Category = "What", MinimumPrice = 2 });

                    int n = 0;

                    // Act
                    int px = 7 / n;
                    q.Save(new Question { Text = "Answer to life", Poster = "42" });



                    tx.Complete();
                }

            }
            catch(DivideByZeroException)
            {
            }
            finally
            {
                // Assert
                Assert.AreEqual(0, p.All.Count()); // product Count should be zero. automatically rolled back
            }
            
         
        }



        [TestMethod]
        public void Ef_Can_save_transaction()
        {
            EmptyDatabase();

            var x = new EfDbMapper(connectionString);
            ITransactionBoundFactory xf = new EF.TransactionBoundFactory();

            IRepository<Product> prod = new EF.Repository<Product>(x);
            IRepository<Question> ques = new EF.Repository<Question>(x);
            Common_Can_save_transaction(xf, prod, ques);
        }

        [TestMethod]
        public void Nh_Can_save_transaction()
        {
            EmptyDatabase();

            NHibernate.ISession x = NhModelsMapper.GetSession(connectionString);
            ITransactionBoundFactory xf = new NH.TransactionBoundFactory(x);


            IRepository<Product> prod = new NH.Repository<Product>(x);
            IRepository<Question> ques = new NH.Repository<Question>(x);
            Common_Can_save_transaction(xf, prod, ques);
        }



        [TestMethod]
        public void Memory_CanSaveTransaction()
        {
            EmptyDatabase();


            ITransactionBoundFactory xf = new MemoryTransactionBoundFactory();

            IRepository<Product> prod = new Repository<Product>();
            IRepository<Question> ques = new Repository<Question>();
            Common_Can_save_transaction(xf, prod, ques);
        }

        void Common_Can_save_transaction(ITransactionBoundFactory xf, IRepository<Product> p, IRepository<Question> q)
        {

            using (var tx = xf.BeginTransaction())
            {
                p.Save(new Product { ProductName = "Hello", Category = "What", MinimumPrice = 2 });
                tx.Complete();              
            }

            Assert.AreEqual(1, p.All.Count()); 



        }

        [TestMethod]
        public void Nh_Can_Detect_Transaction_From_Session_Begin_Transaction()
        {
            // Arrange
            NHibernate.ISession sess = NhModelsMapper.GetSession(connectionString);
            Assert.IsNotNull(sess.Transaction);
            Assert.IsFalse(sess.Transaction.IsActive);


            // Act
            var tx = sess.BeginTransaction();


            // Assert
            Assert.IsTrue(sess.Transaction.IsActive);
            Assert.AreEqual(sess.Transaction.IsActive, tx.IsActive);

            Assert.IsNotNull(tx);
            Assert.IsNotNull(sess.Transaction);
            Assert.AreSame(sess.Transaction, tx);

            tx.Commit();
            Assert.IsFalse(sess.Transaction.IsActive);
            Assert.AreEqual(sess.Transaction.IsActive, tx.IsActive);

        }


        [TestMethod]
        public void Nh_SessionTransaction_cannot_detect_transaction_commitness()
        {
            // Arrange
            NHibernate.ISession sess = NhModelsMapper.GetSession(connectionString);
            Assert.IsFalse(sess.Transaction.IsActive);
            
            using(var tx = sess.BeginTransaction())
            {
                Assert.IsTrue(sess.Transaction.IsActive);
                Assert.IsTrue(tx.IsActive);

                tx.Commit();
                
                Assert.IsFalse(sess.Transaction.WasCommitted);
                Assert.IsTrue(tx.WasCommitted);


                Assert.IsFalse(sess.Transaction.IsActive);
                Assert.IsFalse(tx.IsActive);
            }

        }


        [TestMethod]
        public void Nh_SessionTransaction_can_detect_its_own_transaction_commitness_only()
        {
            // Arrange
            NHibernate.ISession sess = NhModelsMapper.GetSession(connectionString);
            Assert.IsFalse(sess.Transaction.IsActive);

            sess.Transaction.Begin();
            Assert.IsTrue(sess.Transaction.IsActive);
            
            Assert.IsFalse(sess.Transaction.WasCommitted);
            
        }


        [TestMethod]
        public void Nh_Can_Detect_Transaction_From_Session_Transaction_Begin()
        {
            // Arrange
            NHibernate.ISession sess = NhModelsMapper.GetSession(connectionString);
            Assert.IsNotNull(sess.Transaction);
            Assert.IsFalse(sess.Transaction.IsActive);


            // Act
            sess.Transaction.Begin();


            // Assert
            Assert.IsTrue(sess.Transaction.IsActive);
            Assert.IsNotNull(sess.Transaction);



        }


        [TestMethod]
        public void Nh_Can_Save_Then_Update()
        {
            EmptyDatabase();
            IRepository<Product> dbProd = new NH.Repository<Product>(NhModelsMapper.GetSession(connectionString));
            IRepository<ProductPrice> dbPrice = new NH.Repository<ProductPrice>(NhModelsMapper.GetSession(connectionString));
            Common_Can_Save_Then_Update(dbProd, dbPrice);
        }

        [TestMethod]
        public void Ef_Can_Save_Then_Update()
        {
            EmptyDatabase();
            IRepository<Product> dbProd = new EF.Repository<Product>(new EfDbMapper(connectionString));
            IRepository<ProductPrice> dbPrice = new EF.Repository<ProductPrice>(new EfDbMapper(connectionString));
            Common_Can_Save_Then_Update(dbProd, dbPrice);
        }


        public void Common_Can_Save_Then_Update(IRepository<Product> dbProd, IRepository<ProductPrice> dbPrice)
        {
            // Arrange
            string expected = "Bumble bee";
            var bee = new Product { ProductName = expected, Category = "Autobots", MinimumPrice = 8 };
            dbProd.Save(new Product { ProductName = "Optimus", Category = "Autobots", MinimumPrice = 7 });
            dbProd.Save(bee);
            dbProd.Save(new Product { ProductName = "Megatron", Category = "Decepticon", MinimumPrice = 9 });


            Assert.AreEqual(7 + 8 + 9, dbProd.All.Sum(x => x.MinimumPrice));
            Assert.AreEqual(3, dbProd.All.Count());
            Assert.AreNotEqual(0, bee.ProductId);


            // Act 
            var beex = dbProd.Get(bee.ProductId);
            beex.ProductName = beex.ProductName + "yo";
            dbProd.Save(beex);

            var beey = dbProd.Get(beex.ProductId);

            // Assert
            Assert.AreEqual(beex.ProductId, beey.ProductId);
            Assert.AreEqual(expected + "yo", beey.ProductName);
            

            
            
            ///////////////////////
            

            // Arrange

            // this has error, to fix later:
            // var pp = new ProductPrice { Product = dbProd.LoadStub(beex.ProductId), Price = 7, EffectiveDate = DateTime.Today };

            var pp = new ProductPrice { Product = bee, Price = 7, EffectiveDate = DateTime.Today };
            
            

            // Act            

            dbPrice.Save(pp);

            var ppx = dbPrice.Get(pp.ProductPriceId);            
            ppx.Price = ppx.Price + 1;
            dbPrice.Save(ppx);

            var ppy = dbPrice.Get(ppx.ProductPriceId);

            // Assert
            Assert.AreEqual(ppx.ProductPriceId, ppy.ProductPriceId);
            Assert.AreEqual(8, ppy.Price);
            
        }


        [TestMethod]
        public void Test_Ef_Get_PK()
        {
            // Arrange
            DbContext db = new EfDbMapper(connectionString);
            var objectContext = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)db).ObjectContext;
            System.Data.Objects.ObjectSet<Product> set =
                               objectContext.CreateObjectSet<Product>();

            //Act
            IEnumerable<string> keyNames = set.EntitySet.ElementType
                                                        .KeyMembers
                                                        .Select(k => k.Name);

            // Assert
            Assert.AreEqual("ProductId", string.Join(",", keyNames.ToArray()));

        }


        [TestMethod]
        public void Test_Ef_Get_Pk_via_pure_reflection()
        {
            // logic sourced here: http://stackoverflow.com/questions/7253943/entity-framework-code-first-find-primary-key

            // Arrange
            DbContext db = new EfDbMapper(connectionString);

            var objectContext = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)db).ObjectContext;
            Type t = typeof(Product);

            // http://stackoverflow.com/a/232621
            MethodInfo m = objectContext.GetType().GetMethod("CreateObjectSet", new Type[] { });
            MethodInfo generic = m.MakeGenericMethod(t);            
            object set = generic.Invoke(objectContext, null);

            PropertyInfo entitySetPI = set.GetType().GetProperty("EntitySet");
            System.Data.Metadata.Edm.EntitySet entitySet = (System.Data.Metadata.Edm.EntitySet) entitySetPI.GetValue(set, null);


            // Act 
            IEnumerable<string> keyNames = entitySet.ElementType.KeyMembers.Select(k => k.Name);


            // Assert
            Assert.AreEqual("ProductId", string.Join(",", keyNames.ToArray()));

        }

        [TestMethod]
        public void Test_Ef_Get_PK_via_dynamic()
        {
            // Arrange
            DbContext db = new EfDbMapper(connectionString);

            var objectContext = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)db).ObjectContext;
            Type t = typeof(Product);
                       
            
            // http://stackoverflow.com/a/232621
            MethodInfo m = objectContext.GetType().GetMethod("CreateObjectSet",new Type[] {});
            MethodInfo generic = m.MakeGenericMethod(t);
            dynamic set = generic.Invoke(objectContext, null);
            

            // Act
            var ks = (System.Data.Metadata.Edm.ReadOnlyMetadataCollection<System.Data.Metadata.Edm.EdmMember>)set.EntitySet.ElementType.KeyMembers;           
            IEnumerable<string> keyNames = ks.Select(k => k.Name);


            // Assert
            Assert.AreEqual("ProductId", string.Join(",", keyNames.ToArray()));
        }


        [TestMethod]
        public void Nh_CanSaveHeaderDetail_ThenHeaderOnly()
        {
            EmptyDatabase();
            IRepository<Product> db = new NH.Repository<Product>(NhModelsMapper.GetSession(connectionString));
            // throw new Exception("Provider : " + db.All.Provider.GetType().ToString()); // NHibernate.Linq.NhQueryProvider
            Common_CanSaveHeaderDetail_ThenHeaderOnly(db);
        }

        [TestMethod]
        public void Ef_CanSaveHeaderDetail_ThenHeaderOnly()
        {
            EmptyDatabase();
            IRepository<Product> db = new EF.Repository<Product>(new EfDbMapper(connectionString));
            // throw new Exception("Provider : " + db.All.Provider.GetType().ToString()); // NHibernate.Linq.NhQueryProvider
            Common_CanSaveHeaderDetail_ThenHeaderOnly(db);
        }

        void Common_CanSaveHeaderDetail_ThenHeaderOnly(IRepository<Product> db)
        {
            // NHibernate.ISession xxx = NhModelsMapper.GetSession(connectionString);
            

            // Arrange
            var px = new Product
            {
                ProductName = "Optimus",
                Category = "Autobots",
                MinimumPrice = 7
            };
            px.PriceList =
                new List<ProductPrice>()
                {
                    new ProductPrice { Product = px, Price = 777, EffectiveDate = DateTime.Today },
                    new ProductPrice { Product = px, Price = 888, EffectiveDate = DateTime.Today },
                    new ProductPrice { Product = px, Price = 999, EffectiveDate = DateTime.Today },
                    new ProductPrice { Product = px, Price = 222, EffectiveDate = DateTime.Today },

                };


            // Act
            db.SaveGraph(px);
            // xxx.Merge(px);

            byte[] rv = px.RowVersion;

            Assert.AreEqual(4, px.PriceList.Count());


            px = db.GetEager(px.ProductId);



            Assert.AreEqual("Optimus", px.ProductName);
            px.ProductName = px.ProductName + "?";
            px.PriceList[2].Price = 333;
            Assert.AreEqual(4, px.PriceList.Count);

            
            db.SaveGraph(px);

            Assert.AreNotEqual(0, px.ProductId);
            CollectionAssert.AreNotEqual(px.RowVersion, rv);




            // Assert

            Assert.AreEqual(px.ProductName, px.ProductName);
            Assert.AreNotEqual(0, px.ProductId);
            Assert.AreEqual(4, px.PriceList.Count);


            


            // Arrange

            Assert.AreNotEqual(0, px.ProductId);
            
            
            /* triggers concurrency exception
            byte[] x = px.RowVersion;
            x[0] += 1;
            px.RowVersion = x;
            */

            var py = new Product
            {
                ProductId = px.ProductId,
                ProductName = "Optimus",
                Category = "Autobotszx",
                MinimumPrice = 7,
                
                RowVersion = px.RowVersion

                // empty list is incompatible with cascade=all-delete-orphan
            };

            
            int pxid = px.ProductId;            
            db.Save(py);
            
            

            



            
            Product pz = db.Get(px.ProductId);
            // throw new Exception(py.ProductId + " " + px.ProductId + " " + pxid);
            Assert.AreEqual(px.ProductId, py.ProductId);
            
            Assert.IsNotNull(pz.PriceList);
            Assert.AreEqual(4, pz.PriceList.Count);
            
            
            

        }


        [TestMethod]
        public void Nh_Can_Fetch_Eager_Load()
        {
            EmptyDatabase();
            IRepository<Question> db = new NH.Repository<Question>(NhModelsMapper.GetSession(connectionString));
            Test_Can_Fetch_Eager_Load_Common(db);


            // Arrange
            EmptyDatabase();
            Question qx = new Question { Text = "42" };
            db.Save(qx);
            db.Evict(qx.QuestionId);

            // Act
            var qq = NH.EagerExtensionHelper.EagerLoad(db.All.Where(x => x.QuestionId == qx.QuestionId), "Answers").SingleOrDefault();

            // Assert
            Assert.AreEqual("42", qq.Text);
           
        }

        [TestMethod]
        public void Ef_Can_Fetch_Eager_Load()
        {
            EmptyDatabase();
            IRepository<Question> db = new EF.Repository<Question>(new EfDbMapper(connectionString));
            Test_Can_Fetch_Eager_Load_Common(db);


            // Arrange
            EmptyDatabase();
            Question qx = new Question { Text = "42" };
            db.Save(qx);
            db.Evict(qx.QuestionId);

            // Act
            var qq = EF.EagerExtensionHelper.EagerLoad(db.All.Where(x => x.QuestionId == qx.QuestionId), "Answers").SingleOrDefault();

            // Assert
            Assert.AreEqual("42", qq.Text);

        }

        public void Test_Can_Fetch_Eager_Load_Common(IRepository<Question> r)
        {
            var q = new Question { Text = "Hello", Answers = new List<Answer>(), Comments = new List<QuestionComment>() };
            var c = new QuestionComment { Question = q, Text = "Hei" };
            var a = new Answer { Question = q, Text = "Yes", Comments = new List<AnswerComment>() };

            q.Comments.Add(c);
                    

            var ac = new AnswerComment { Answer = a, Text = "hahah" };
            a.Comments.Add(ac);

            q.Answers.Add(a);

            r.Save(q);
                                    

            r.Evict(q.QuestionId);


            var g = r.GetEager(q.QuestionId, "Answers", "Comments");



            
            
            Assert.AreEqual("Hello", g.Text);
            // System.Threading.Thread.Sleep(10000);
            Assert.AreEqual("Hei", g.Comments[0].Text);
            Assert.AreEqual("Yes", g.Answers[0].Text);
            Assert.AreEqual("hahah", g.Answers[0].Comments[0].Text);

        }
    






    }


    class Test
    {
        public string[] Names { get; set; }
    }


}
    