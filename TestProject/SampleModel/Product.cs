using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace TestProject.SampleModel
{
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

    public class ProductPrice
    {
        public virtual Product Product { get; set; }

        public virtual int ProductPriceId { get; set; }
        public virtual DateTime EffectiveDate { get; set; }
        public virtual decimal Price { get; set; }
    }
}
