using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using BookStore_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Interacts with the books table
    /// </summary>
    [Route("api/controller")]
    [ApiController]
    public class BooksController : Controller
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public BooksController(IBookRepository bookRepository, ILoggerService logger, IMapper mapper)
        {
            _bookRepository = bookRepository;
            _logger = logger;
            _mapper = mapper;
        }

        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, message);
        }

        /// <summary>
        /// Retrieves all books from the database
        /// </summary>
        /// <returns>List of books</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBooks()
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo("Attempting to retrieve all book records...");
                var books = await _bookRepository.FindAll();
                if (books == null)
                {
                    _logger.LogWarn("Attempted to retrieve all books, but none were found");
                    return NotFound();
                }

                var response = _mapper.Map<IList<BookDTO>>(books);

                _logger.LogInfo($"{location}: Retrieved books successfully.");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Retrieves a book matching the Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns a book based on the Id given</returns>
        [HttpGet("id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBook(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempting to retrieve Book record: {id}...");
                if (id < 1)
                {
                    _logger.LogWarn($"{location}: Book retrieval failed due to a bad data request.");
                    return BadRequest();
                }
                var book = await _bookRepository.FindById(id);

                if (book == null)
                {
                    _logger.LogWarn($"{location}: No book records found with Id: {id}.");
                    return NotFound();
                }

                var response = _mapper.Map<BookDTO>(book);

                _logger.LogInfo($"{location}: Successfully retrieved book record: {id}.");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }
        
        /// <summary>
        /// Creates a book
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] BookCreateDTO bookDTO)
        {
            var location = GetControllerActionNames();
            
            try
            {
                _logger.LogWarn($"{location}: Attempting to create book record...");
                if (bookDTO == null)
                {
                    _logger.LogWarn($"{location}: Book creation failed due to a bad data request.");
                    return BadRequest(ModelState);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: Book data was incomplete or not valid.");
                    return BadRequest(ModelState);
                }

                var book = _mapper.Map<Book>(bookDTO);
                var success = await _bookRepository.Create(book);
                if (!success)
                {
                    return InternalError($"{location}: Book creation failed");
                }
                _logger.LogInfo($"{location}: Book created successfully");
                return Created("Create", new { book });
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }
        
        /// <summary>
        /// Updates an author based on the Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="book"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] BookUpdateDTO bookDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogWarn($"{location}: Attempting to update book record: {id}...");
                if (id < 1 || bookDTO == null || id != bookDTO.Id)
                {
                    _logger.LogWarn($"{location}: Book update failed due to a bad data request.");
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: Book data was incomplete or not valid.");
                    return BadRequest(ModelState);
                }
                var book = _mapper.Map<Book>(bookDTO);
                var success = await _bookRepository.Update(book);
                if (!success)
                {
                    return InternalError($"{location}: Book update failed.");
                }
                _logger.LogInfo($"{location}: Book updated successfully.");
                return NoContent();
            }
            catch (Exception e)
            {                
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }
        
        /// <summary>
        /// Deletes a book by their associated Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempting to delete book record: {id}");
                if (id < 1)
                {
                    _logger.LogWarn($"{location}: Book deletion failed due to a bad data request.");
                    return BadRequest();
                }
                var doesExist = await _bookRepository.DoesExist(id);
                if(!doesExist)
                {
                    _logger.LogWarn($"{location}: Attempted to delete a book with an id of {id}, but there are no books with this Id");
                    return NotFound();
                }
                var book = await _bookRepository.FindById(id);
                var success = await _bookRepository.Delete(book);
                if (!success)
                {
                    return InternalError($"{location}: Book deletion failed");
                }
                _logger.LogInfo($"{location}: Book deleted successfully");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }
    }
}