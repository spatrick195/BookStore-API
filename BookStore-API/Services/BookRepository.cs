using System.Collections.Generic;
using System.Threading.Tasks;
using BookStore_API.Contracts;
using BookStore_API.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStore_API.Services
{
    public class BookRepository : IBookRepository
    {
        private readonly ApplicationDbContext _db;

        public BookRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        
        public async Task<IList<Book>> FindAll()
        {
            var authors = await _db.Books.ToListAsync();
            return authors;
        }

        public async Task<Book> FindById(int id)
        {
            var author = await _db.Books.FindAsync(id);
            return author;
        }

        public async Task<bool> Create(Book entity)
        {
            await _db.Books.AddAsync(entity);
            return await Save();
        }

        public async Task<bool> Update(Book entity)
        {   
            _db.Books.Update(entity);
            return await Save();
        }

        public async Task<bool> DoesExist(int id)
        {
            return await _db.Books.AnyAsync(b => b.Id == id);
        }

        public async Task<bool> Delete(Book entity)
        {
            _db.Books.Remove(entity);
            return await Save();
        }

        public async Task<bool> Save()
        {
            var changes = await _db.SaveChangesAsync();
            return changes > 0;        
        }
    }
}