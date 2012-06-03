using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace TestProject.SampleModel
{
        
    
    public class Question
    {
        public virtual int QuestionId { get; set; }
        public virtual string Text { get; set; }
        public virtual string Poster { get; set; }

        public virtual IList<QuestionComment> Comments { get; set; }
        public virtual IList<Answer> Answers{ get; set; }

       
        public virtual byte[] RowVersion { get; set; }
    }

    
    public class QuestionComment
    {
        public virtual Question Question { get; set; }        

        public virtual int QuestionCommentId { get; set; }
        public virtual string Text { get; set; }
        public virtual string Poster { get; set; }
    }


    
    public class Answer
    {
        public virtual Question Question { get; set; }

        public virtual int AnswerId { get; set; }
        public virtual string Text { get; set; }
        public virtual string Poster { get; set; }

        public virtual IList<AnswerComment> Comments { get; set; }
        
    }


    
    public class AnswerComment
    {
        public virtual Answer Answer { get; set; }

        public virtual int AnswerCommentId { get; set; }
        public virtual string Text { get; set; }
        public virtual string Poster { get; set; }
    }
}
