using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebLibrary.Data;
using WebLibrary.Data.Models;

namespace WebLibrary.Services
{
    public class CheckoutService : ICheckout
    {
        private LibraryContext _context;
        public CheckoutService(LibraryContext context)
        {
            _context = context;
        }

        public void Add(Checkout newCheckout)
        {
            _context.Add(newCheckout);
            _context.SaveChanges();
        }

        public void CheckInItem(int assetId, int libraryCardId)
        {
            var now = DateTime.Now;
            var item = _context.LibraryAssets.FirstOrDefault(a => a.Id == assetId);

            RemoveExistingCheckouts(assetId);
            CloseExistingCheckoutHistory(assetId);

            var currentHolds = _context.Holds
                .Include(h => h.LibraryAsset)
                .Include(h => h.LibraryCard)
                .Where(h => h.LibraryAsset.Id == assetId);

            if (currentHolds.Any())
            {
                CheckoutToEarliestHold(assetId, currentHolds);
            }
            UpdateAssetStatus(assetId, "Available");

            _context.SaveChanges();
        }

        private void CheckoutToEarliestHold(int assetId, IQueryable<Hold> currentHolds)
        {
            var earliestHold = currentHolds.OrderBy(h => h.HoldPlaced).FirstOrDefault();

            var card = earliestHold.LibraryCard;

            _context.Remove(earliestHold);
            _context.SaveChanges();

            CheckOutItem(assetId, card.Id);
        }

        public void CheckOutItem(int assetId, int libraryCardId)
        {
            if (IsCheckedOut(assetId))
            {
                return;
            }

            var item = _context.LibraryAssets.FirstOrDefault(a => a.Id == assetId);
            UpdateAssetStatus(assetId, "Checked Out");
            var libraryCard = _context.LibraryCards.Include(c => c.Checkouts).FirstOrDefault(c => c.Id == libraryCardId);

            var now = DateTime.Now;
            var checkout = new Checkout
            {
                LibraryAsset = item,
                LibraryCard = libraryCard,
                Since = now,
                Until = GetDefaultCheckoutTime(now)
            };

            _context.Add(checkout);

            var checkoutHistory = new CheckoutHistory
            {
                CheckedOut = now,
                LibraryAsset = item,
                LibraryCard = libraryCard
            };

            _context.Add(checkoutHistory);
            _context.SaveChanges();
        }

        private DateTime GetDefaultCheckoutTime(DateTime now)
        {
            return now.AddDays(30);
        }

        private bool IsCheckedOut(int assetId)
        {
            return _context.Checkouts.Where(co => co.LibraryAsset.Id == assetId).Any();
        }

        public IEnumerable<Checkout> GetAll()
        {
            return _context.Checkouts.Include(item => item.LibraryAsset);
        }

        public Checkout GetById(int checkoutId)
        {
            return GetAll().FirstOrDefault(item => item.Id == checkoutId);
        }

        public Checkout GetLatestCheckout(int assetId)
        {
            return GetAll()
                .Where(item => item.LibraryAsset.Id == assetId)
                .OrderByDescending(item => item.Since)
                .FirstOrDefault();
        }

        public IEnumerable<CheckoutHistory> GetCheckoutHistories(int id)
        {
            return _context.CheckoutHistories
                .Include(item => item.LibraryAsset)
                .Include(item => item.LibraryCard)
                .Where(item => item.LibraryAsset.Id == id);
        }

        public string GetCurrentHoldPatronName(int holdId)
        {
            var hold = _context.Holds
                .Include(h => h.LibraryAsset)
                .Include(h => h.LibraryCard)
                .FirstOrDefault(h => h.Id == holdId);

            var cardId = hold?.LibraryCard.Id;
            var patron = _context.Patrons.Include(p => p.LibraryCard).FirstOrDefault(p => p.LibraryCard.Id == cardId);

            return patron?.FirstName + " " + patron?.LastName;
        }

        public DateTime GetCurrentHoldPlaced(int holdId)
        {
            return _context.Holds
                .Include(h => h.LibraryAsset)
                .Include(h => h.LibraryCard)
                .FirstOrDefault(h => h.Id == holdId).HoldPlaced;


        }

        public IEnumerable<Hold> GetCurrentHolds(int id)
        {
            return _context.Holds
                .Include(item => item.LibraryAsset)
                .Where(item => item.LibraryAsset.Id == id);
        }

        public void MarkFound(int assetId)
        {
            UpdateAssetStatus(assetId, "Available");
            RemoveExistingCheckouts(assetId);
            CloseExistingCheckoutHistory(assetId);

            _context.SaveChanges();
        }

        private void UpdateAssetStatus(int assetId, string status)
        {
            var item = _context.LibraryAssets.FirstOrDefault(a => a.Id == assetId);
            if (item != null)
            {
                _context.Update(item);
                item.Status = _context.Statuses.FirstOrDefault(s => s.Name == status);
            }
        }

        private void CloseExistingCheckoutHistory(int assetId)
        {
            var history = _context.CheckoutHistories.FirstOrDefault(h => h.LibraryAsset.Id == assetId && h.CheckedIn == null);
            if (history != null)
            {
                _context.Update(history);
                history.CheckedIn = DateTime.Now;
            }
        }

        private void RemoveExistingCheckouts(int assetId)
        {
            var checkout = _context.Checkouts.FirstOrDefault(co => co.LibraryAsset.Id == assetId);
            if (checkout != null)
            {
                _context.Remove(checkout);
            }
        }

        public void MarkLost(int assetId)
        {
            UpdateAssetStatus(assetId, "Lost");
            _context.SaveChanges();
        }

        public void PlaceHold(int assetId, int libraryCardId)
        {
            var now = DateTime.Now;
            var asset = _context.LibraryAssets.FirstOrDefault(a => a.Id == assetId);
            var card = _context.LibraryCards.FirstOrDefault(c => c.Id == libraryCardId);

            if (asset.Status.Name == "Available")
            {
                UpdateAssetStatus(assetId, "On Hold");
            }

            var hold = new Hold
            {
                HoldPlaced = now,
                LibraryAsset = asset,
                LibraryCard = card
            };
            _context.Add(hold);
            _context.SaveChanges();
        }

        public string GetCurrentCheckoutPatron(int assetId)
        {
            var checkout = GetCheckoutByAssetId(assetId);
            if (checkout == null)
            {
                return "Not checked out.";
            }

            var cardId = checkout.LibraryCard.Id;
            var patron = _context.Patrons
                .Include(p => p.LibraryCard)
                .FirstOrDefault(p => p.LibraryCard.Id == cardId);

            return patron?.FirstName + " " + patron?.LastName;
        }

        private Checkout GetCheckoutByAssetId(int assetId)
        {
            return _context.Checkouts
                .Include(c => c.LibraryAsset)
                .Include(c => c.LibraryCard)
                .FirstOrDefault(c => c.LibraryAsset.Id == assetId);
        }
    }
}
