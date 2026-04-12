using System.ComponentModel.DataAnnotations;

namespace DotCast.Infrastructure.UrlBuilder;

public class UrlBuilderOptions
{
    [Required]
    public string BaseUrl { get; set; } = null!;
}
