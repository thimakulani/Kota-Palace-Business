using System.ComponentModel;

namespace KotaPalace.Models
{
    public class Business
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PhoneNumber { get; set; }
        public string ImgUrl { get; set; }
        public string OwnerId { get; set; }
        public string Status { get; set; }
        public string Online { get; set; }
        public virtual AppUsers Owner { get; set; }
        public virtual Address Address { get; set; }
    }
    public class Address
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int BusinessId { get; set; }
    }
}
