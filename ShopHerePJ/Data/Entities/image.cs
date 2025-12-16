namespace ShopHerePJ.Data.Entities
{
    public partial class image
    {
        public int id { get; set; }
        public string object_type { get; set; } = null!;
        public int object_id { get; set; }
        public string? file_name { get; set; }
        public string content_type { get; set; } = null!;
        public int file_size { get; set; }
        public byte[] data { get; set; } = null!;
        public bool is_primary { get; set; }
        public DateTime created_at { get; set; }
    }
}
