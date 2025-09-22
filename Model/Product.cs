namespace SampleWebApi.Model
{
    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public bool InStock { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime CreatedUtc { get; set; }            // set on insert
        public DateTime UpdatedUtc { get; set; }
        // Soft-delete flag (optional)
        public byte[] RowVersion { get; set; } = [];  // For ETag & concurrency
    }
}
