namespace WebAPI_2.DAL.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public bool IsAdmin { get; set; }
        public string Name { get; set; }
        public string NickName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<Book> SavedBooks { get; set; }
    }
}
