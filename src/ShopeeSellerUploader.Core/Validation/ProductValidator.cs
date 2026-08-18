using ShopeeSellerUploader.Core.Models;

namespace ShopeeSellerUploader.Core.Validation;

public static class ProductValidator
{
    public static ValidationResult Validate(ProductRecord product)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(product.ProductCode))
        {
            result.Errors.Add("ProductCode is required.");
        }

        if (string.IsNullOrWhiteSpace(product.ProductName))
        {
            result.Errors.Add("ProductName is required.");
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

        if (product.GetImagePaths().Count == 0)
        {
            result.Errors.Add("At least one image path is required.");
        }

        foreach (var imagePath in product.GetImagePaths())
        {
            if (!File.Exists(imagePath))
            {
                result.Errors.Add($"Image file not found: {imagePath}");
            }
        }

        if (!string.IsNullOrWhiteSpace(product.VariationName) || !string.IsNullOrWhiteSpace(product.VariationOption))
        {
            if (string.IsNullOrWhiteSpace(product.VariationName) || string.IsNullOrWhiteSpace(product.VariationOption))
            {
                result.Errors.Add("VariationName and VariationOption must both be filled.");
            }

            if (product.VariationPrice is <= 0)
            {
                result.Errors.Add("VariationPrice must be greater than zero when using variations.");
            }

            if (product.VariationStock is < 0)
            {
                result.Errors.Add("VariationStock must not be negative.");
            }
        }

        return result;
    }
}
