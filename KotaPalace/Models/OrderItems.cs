using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KotaPalace.Models
{
    public class OrderItems
    {
        public int Id { get; set; }
        public string Quantity { get; set; } //
        public int MenuId { set; get; }//product id
        public string ItemName { set; get; }
        public decimal Price { get; set; }
        public string Extras { get; set; } 
    }
    public class OrderItemsExtras
    {
        public int Id { set; get; } 

        public int ExtrasID { get; set; } 
    }
}
