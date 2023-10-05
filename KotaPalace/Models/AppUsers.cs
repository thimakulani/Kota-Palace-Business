

using System;

namespace KotaPalace.Models
{
    public class AppUsers
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string UserType { get; set; }
        public string Id { get; set; }
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public Guid ConcurrencyStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public object LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public long AccessFailedCount { get; set; }
        public int BusinessId { get; set; }
        public string Url { get; set; }
    }
}
