using System;
using System.Collections.Generic;
using WebLibrary.Data.Models;

namespace WebLibrary.Data
{
    public interface ICheckout
    {
        IEnumerable<Checkout> GetAll();
        IEnumerable<Hold> GetCurrentHolds(int id);
        IEnumerable<CheckoutHistory> GetCheckoutHistories(int id);

        Checkout GetById(int checkoutId);
        Checkout GetLatestCheckout(int assetId);        

        void Add(Checkout newCheckout);
        void CheckOutItem(int assetId, int libraryCardId);
        void CheckInItem(int assetId, int libraryCardId);
        void PlaceHold(int assetId, int libraryCardId);
        void MarkLost(int assetId);
        void MarkFound(int assetId);               

        string GetCurrentCheckoutPatron(int assetId);
        string GetCurrentHoldPatronName(int holdId);
        DateTime GetCurrentHoldPlaced(int holdId);     
    }
}
