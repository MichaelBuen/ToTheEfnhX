using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Automapping;
using TestProject.SampleModel;
using FluentNHibernate.Conventions.Instances;
using FluentNHibernate.Conventions;

namespace TestProject
{
    internal static class NhModelsMapper
    {
        public static ISession GetSession(string connectionString)
        {
            return GetSessionFactory(connectionString).OpenSession();
        }


        static ISessionFactory _sf = null;
        private static ISessionFactory GetSessionFactory(string connectionString)
        {
            if (_sf != null) return _sf;

            



            var fc = Fluently.Configure()
                    .Database(MsSqlConfiguration.MsSql2008.ConnectionString(connectionString))
                    .Mappings
                    (m =>


                            m.AutoMappings.Add
                            (
                                AutoMap.AssemblyOf<Product>(new CustomConfiguration())

                                // .Conventions.Add(ForeignKey.EndsWith("Id"))                                
                               .Conventions.Add<CustomForeignKeyConvention>()

                               

                               .Conventions.Add<HasManyConvention>()
                               .Conventions.Add<RowversionConvention>()

                               .Override<Product>(x => x.HasMany(z => z.PriceList).KeyColumn("Product_ProductId"))
                               .Override<Product>(x => x.Version(z => z.RowVersion))
                               
                               // .Override<Product>(x => x.Id(z => z.ProductId).GeneratedBy.Identity())

                               .Override<Question>(x => x.LazyLoad())
                               

                               .Override<Question>(x => x.HasMany(z => z.Answers).KeyColumn("Question_QuestionId") /*.Not.LazyLoad() */)
                               .Override<Question>(x => x.HasMany(z => z.Comments).KeyColumn("Question_QuestionId") /*.Not.LazyLoad()*/)
                               .Override<Answer>(x => x.HasMany(z => z.Comments).KeyColumn("Answer_AnswerId")/*.Not.LazyLoad()*/)                                                              
                            )


           );



            _sf = fc.BuildSessionFactory();
            return _sf;
        }

        


        class CustomConfiguration : DefaultAutomappingConfiguration
        {
            IList<Type> _objectsToMap = new List<Type>()
            {
                // whitelisted objects to map
                typeof(Product), typeof(ProductPrice), typeof(Question), typeof(QuestionComment), typeof(Answer), typeof(AnswerComment)
            };
            public override bool ShouldMap(Type type) { return _objectsToMap.Any(x => x == type); }
            public override bool IsId(FluentNHibernate.Member member) { return member.Name == member.DeclaringType.Name + "Id"; }

            public override bool IsVersion(FluentNHibernate.Member member) { return member.Name == "RowVersion"; }
        }




        public class CustomForeignKeyConvention : ForeignKeyConvention
        {
            protected override string GetKeyName(FluentNHibernate.Member property, Type type)
            {
                if (property == null)
                    return type.Name + "Id";


                // make foreign key compatible with Entity Framework
                return type.Name + "_" + property.Name + "Id";
            }
        }


        class HasManyConvention : IHasManyConvention
        {

            public void Apply(IOneToManyCollectionInstance instance)
            {
                instance.Inverse();

                // good for n-tier
                instance.Cascade.AllDeleteOrphan();
            }


        }

        class RowversionConvention : IVersionConvention
        {
            public void Apply(IVersionInstance instance) { instance.Generated.Always(); }
        }

    }//ModelsMapper



}
