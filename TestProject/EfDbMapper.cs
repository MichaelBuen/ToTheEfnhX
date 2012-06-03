using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using TestProject.SampleModel;

namespace TestProject
{
    internal class EfDbMapper : DbContext
    {

        

        public EfDbMapper(string connectionString) :base(connectionString)
        {
            // this.Configuration.ProxyCreationEnabled = false; // putting this here, causes an error on Ef_Can_Update in PriceList[0].ProductId
        }

        

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            
            
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();


            // modelBuilder.Entity<Product>(); // /* the commented code is not needed, we followed closely EF conventions */  .HasMany(x => x.PriceList).WithRequired(x => x.Product).Map(x => x.MapKey("Product_ProductId"));

            
            modelBuilder.Entity<Product>().HasMany(x => x.PriceList).WithRequired(x => x.Product).Map(x => x.MapKey("Product_ProductId"));

            

            modelBuilder.Entity<Question>().Property(x => x.RowVersion).IsRowVersion();

            modelBuilder.Entity<Question>().HasMany(x => x.Answers).WithRequired(x => x.Question).Map(x => x.MapKey("Question_QuestionId"));
            modelBuilder.Entity<Question>().HasMany(x => x.Comments).WithRequired(x => x.Question).Map(x => x.MapKey("Question_QuestionId"));

            modelBuilder.Entity<Answer>().HasMany(x => x.Comments).WithRequired(x => x.Answer).Map(x => x.MapKey("Answer_AnswerId"));

            

            
            

            base.OnModelCreating(modelBuilder);

           
        }
    }
}
