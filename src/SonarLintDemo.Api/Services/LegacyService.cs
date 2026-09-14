namespace SonarLintDemo.Api.Services;

public class LegacyService
{
    // S107: Method has too many parameters
    public string ProcessOrder(
        string customerName,
        string customerEmail,
        string productName,
        decimal productPrice,
        int quantity,
        string shippingAddress,
        string billingAddress,
        string couponCode,
        bool isExpressShipping)
    {
        // S1854: Remove this useless assignment to local variable 'result'.
        var result = "processed";

        // S109: Assign this magic number to a named constant.
        var total = productPrice * quantity * 1.21m;

        result = $"Order for {customerName}: {productName} x {quantity} = {total:C}";
        return result;
    }

    // S3776: Refactor this method to reduce its Cognitive Complexity
    public int CalculateShipping(string country, decimal weight)
    {
        if (country == "US")
        {
            if (weight < 1)
            {
                return 5;
            }
            else if (weight < 5)
            {
                return 10;
            }
            else
            {
                return 25;
            }
        }
        else if (country == "CA")
        {
            if (weight < 1)
            {
                return 6;
            }
            else if (weight < 5)
            {
                return 12;
            }
            else
            {
                return 30;
            }
        }
        else
        {
            if (weight < 1)
            {
                return 10;
            }
            else if (weight < 5)
            {
                return 20;
            }
            else
            {
                return 50;
            }
        }
    }
}
