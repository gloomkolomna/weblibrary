using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebLibrary.Data;
using WebLibrary.Data.Models;

namespace WebLibrary.Services
{
    public class LibraryAssetService : ILibraryAsset
    {
        private readonly LibraryContext _context;
        public LibraryAssetService(LibraryContext context)
        {
            _context = context;
        }

        public void Add(LibraryAsset newAsset)
        {
            _context.Add(newAsset);
            _context.SaveChanges();
        }

        public IEnumerable<LibraryAsset> GetAll()
        {
            return _context.LibraryAssets
                .Include(asset => asset.Status)
                .Include(asset => asset.Location);
        }

        public string GetAuthorOrDirector(int id)
        {
            var isBook = _context.LibraryAssets.OfType<Book>().Where(asset => asset.Id == id).Any();
            var isVideo = _context.LibraryAssets.OfType<Video>().Where(asset => asset.Id == id).Any();

            return isBook ?
                _context.Books.FirstOrDefault(b => b.Id == id)?.Author :
                _context.Videos.FirstOrDefault(v => v.Id == id)?.Director
                ?? "Неизвестно";
        }

        public LibraryAsset GetById(int id)
        {
            return GetAll().FirstOrDefault(asset => asset.Id == id);
        }

        public LibraryBranch GetCurrentLocation(int id)
        {
            return GetById(id)?.Location;
        }

        public string GetDeweyIndex(int id)
        {
            if (_context.Books.Any(book => book.Id == id))
            {
                return _context.Books.FirstOrDefault(book => book.Id == id)?.DeweyIndex;
            }

            return string.Empty;
        }

        public string GetIsbn(int id)
        {
            if (_context.Books.Any(book => book.Id == id))
            {
                return _context.Books.FirstOrDefault(book => book.Id == id)?.ISBN;
            }

            return string.Empty;
        }

        public string GetTitle(int id)
        {
            return _context.LibraryAssets.FirstOrDefault(asset => asset.Id == id)?.Title;
        }

        public string GetType(int id)
        {
            var book = _context.LibraryAssets.OfType<Book>().Where(b => b.Id == id);
            return book.Any() ? "Книга" : "Видео";
        }
    }
}
