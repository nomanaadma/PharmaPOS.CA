using PharmaPOS.Domain.MediaAggregate;

namespace PharmaPOS.Contracts.Responses;

public record MediaUploadResponse(
    Media Media, 
    int Width, 
    int Height
);
