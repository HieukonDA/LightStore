namespace TheLightStore.Dtos.Products;

public record ProductImageDto(
    int Id,
    string ImageUrl,
    string AltText,
    bool IsPrimary,
    int SortOrder
);
