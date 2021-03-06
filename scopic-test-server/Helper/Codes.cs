using System.ComponentModel;

namespace scopic_test_server.Helper
{
    public static class Codes
    {
        public enum BidCode
        {
            Success,
            [Description("You can't make a bid if you're the current highest bidder.")]
            HighestBid,//Currently the highest bidder
            [Description("Your bid price is lower than or equal to the current bid price.")]
            PriceTooLow,//Bid price lower than highest bidder
            [Description("Something went wrong. Please try again.")]
            Null,//
        }

        public enum ProductCode
        {
            [Description("Product has been edited.")]

            Success,
            [Description("Expiry date time must be set greater than the current date time")]
            InvalidDate,
            [Description("Product is already awarded.")]
            AlreadyAwarded,
            [Description("Product could not be found. Please refresh your browser.")]
            Null
        }
    }

}