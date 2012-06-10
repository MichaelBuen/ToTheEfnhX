#define UseBuiltInCloningX

// Entity Framework is funny, we have to clone the object
// http://stackoverflow.com/questions/1158422/entity-framework-detach-and-keep-related-object-graph

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;


namespace TestProject.SampleModel
{
#if UseBuiltInCloning
    [Serializable]
#endif
    public class Product
    {
        public virtual int ProductId { get; set; }
        public virtual string ProductName { get; set; }
        public virtual string Category { get; set; }
        public virtual decimal MinimumPrice { get; set; }

        public virtual IList<ProductPrice> PriceList { get; set; }


        [Timestamp]
        public virtual byte[] RowVersion { get; set; }





    }

#if UseBuiltInCloning
    [Serializable]
#endif
    public class ProductPrice
    {
        public virtual Product Product { get; set; }

        public virtual int ProductPriceId { get; set; }
        public virtual DateTime EffectiveDate { get; set; }
        public virtual decimal Price { get; set; }
    }
}


namespace TestProject.SampleModel
{

#if UseBuiltInCloning
    [Serializable]
#endif
    public class Question
    {
        public virtual int QuestionId { get; set; }
        public virtual string Text { get; set; }
        public virtual string Poster { get; set; }

        public virtual IList<QuestionComment> Comments { get; set; }
        public virtual IList<Answer> Answers { get; set; }


        public virtual byte[] RowVersion { get; set; }
    }

#if UseBuiltInCloning
    [Serializable]
#endif
    public class QuestionComment
    {
        public virtual Question Question { get; set; }

        public virtual int QuestionCommentId { get; set; }
        public virtual string Text { get; set; }
        public virtual string Poster { get; set; }
    }

#if UseBuiltInCloning
    [Serializable]
#endif
    public class Answer
    {
        public virtual Question Question { get; set; }

        public virtual int AnswerId { get; set; }
        public virtual string Text { get; set; }
        public virtual string Poster { get; set; }

        public virtual IList<AnswerComment> Comments { get; set; }

    }

#if UseBuiltInCloning
    [Serializable]
#endif
    public class AnswerComment
    {
        public virtual Answer Answer { get; set; }

        public virtual int AnswerCommentId { get; set; }
        public virtual string Text { get; set; }
        public virtual string Poster { get; set; }
    }
}

