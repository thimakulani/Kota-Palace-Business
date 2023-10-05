using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace KotaPalace.Models
{
    public class Extras
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int MenuId { get; set; }
    }
}
