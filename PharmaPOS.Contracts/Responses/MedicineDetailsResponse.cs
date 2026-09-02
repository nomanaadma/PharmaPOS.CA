using PharmaPOS.Domain.MedicineAggregate;

namespace PharmaPOS.Contracts.Responses;

public record MedicineDetailsResponse(
    Medicine Medicine, 
    string CategoryName
);
