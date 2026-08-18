using ShopeeSellerUploader.Core.Models;

namespace ShopeeSellerUploader.Core.Validation;

public static class ProductItemValidator
{
    public static ValidationResult Validate(ProductItem product)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(product.ProductCode))
        {
            result.Errors.Add("Product code is required.");
        }

        if (string.IsNullOrWhiteSpace(product.ProductName))
        {
            result.Errors.Add("Product name is required.");
        }

        if (product.Price <= 0)
        {
            result.Errors.Add("Price must be greater than zero.");
        }

        if (product.Stock < 0)
        {
            result.Errors.Add("Stock must not be negative.");
        }

        if (product.Weight <= 0)
        {
            result.Errors.Add("Weight must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(product.SKU))
        {
            result.Errors.Add("SKU is required.");
        }

        return result;
    }
}
