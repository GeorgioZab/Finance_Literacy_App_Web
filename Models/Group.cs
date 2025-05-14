namespace Finance_Literacy_App_Web.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? InviteCode { get; set; }
        public List<User> Users { get; set; } = new List<User>();
    }
}