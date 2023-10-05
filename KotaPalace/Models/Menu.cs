using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KotaPalace.Models
{
    public class Menu
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string Url { get; set; }
        public bool Status { get; set; }
        public int BusinessId { get; set; }
        public List<Extras> Extras { get; set; }
    }
}
