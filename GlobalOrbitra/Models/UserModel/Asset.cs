namespace GlobalOrbitra.Models.UserModel
{
    public class Asset
    {
        public int Id { get; set; }
        public string Symbol { get; set; } = null!; 
        public string Name { get; set; } = null!;
        public string? ImageUrl { get; set; }
    }

}
