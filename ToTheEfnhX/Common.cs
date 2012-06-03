using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
 

namespace Ienablemuch.ToTheEfnhX
{


    // http://stackoverflow.com/questions/6700045/sql-nomenclature-query-or-command-for-insert-update-delete
    // Rational for using the word Changes is used instead of Update. Update is an ambiguous word.
    // In fact, prior to EntityFramework's DbUpdateConcurrencyException, ADO.NET have chosen a plain name DbConcurrencyException 
    [Serializable]
    public class DbChangesConcurrencyException : Exception
    {
        public DbChangesConcurrencyException() : base("Row has been modified or deleted since you last loaded it") { }
        public DbChangesConcurrencyException( string message ) : base( message ) { }
        public DbChangesConcurrencyException( string message, Exception inner ) : base( message, inner ) { }
        protected DbChangesConcurrencyException( 
	    System.Runtime.Serialization.SerializationInfo info, 
	    System.Runtime.Serialization.StreamingContext context ) : base( info, context ) { }
    }


    [Serializable]
    public class IdParameterIsInvalidTypeException : Exception
    {
        public IdParameterIsInvalidTypeException() { }
        public IdParameterIsInvalidTypeException(string message) : base(message) { }
        public IdParameterIsInvalidTypeException(string message, Exception inner) : base(message, inner) { }
        protected IdParameterIsInvalidTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }



    

}