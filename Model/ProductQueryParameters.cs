using FluentValidation;

namespace SampleWebApi.Model
{
    public sealed class ProductQueryParameters
    {
        public string? Search { get; set; }          // name contains
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? InStock { get; set; }

        public string? SortBy { get; set; } = "name"; // name|price
        public bool Desc { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public sealed record ProductDto(int Id, string Name, decimal Price, bool InStock, string ETag);
    public sealed record CreateProductDto(string Name, decimal Price, bool InStock);
    public sealed record UpdateProductDto(string Name, decimal Price, bool InStock);
    public class CreateProductValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0m);
        }
    }
    public class UpdateProductValidator : AbstractValidator<UpdateProductDto>
    {
        public UpdateProductValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0m);
        }
    }

}
