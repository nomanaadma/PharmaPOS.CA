namespace PharmaPOS.Contracts.Responses;

public record MedicinesWithStockResponse(
    List<MedicineStockResponse> StockList, 
    Dictionary<int, string> MedicineNames
);
