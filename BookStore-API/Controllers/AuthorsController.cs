using BookStore_API.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.DTOs;
using System.Collections.Generic;
using System;
using BookStore_API.Data;
using NLog.Fluent;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Endpoint used to interact with the authors in the database
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;

        public AuthorsController(IAuthorRepository authorRepository, ILoggerService logger, IMapper mapper)
        {
            this._authorRepository = authorRepository;
            this._mapper = mapper;
            this._logger = logger;
        }

        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, message);
        }

        /// <summary>
        /// Get all authors from the repository
        /// </summary>
        /// <returns>List of authors</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            try
            {
                _logger.LogInfo("Attempting to get all authors");
                var authors = await _authorRepository.FindAll(); // Use repository method to find all authors

                if (authors == null)
                {
                    _logger.LogWarn($"Attempted to get authors, but there are no authors in the database.");
                    return NotFound();
                }

                var response = _mapper.Map<IList<AuthorDTO>>(authors); // Mapping Data

                _logger.LogInfo("Getting authors was successful");
                return Ok(response); // returns status 200, and returns payload
            }
            catch (Exception e) // catch the error
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Returns an author by their Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns an authors' record based on the id given </returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuthor(int id)
        {
            try
            {
                _logger.LogInfo($"Attempted to get author with id: {id}");
                var author = await _authorRepository.FindById(id);
                if (author == null)
                {
                    _logger.LogWarn($"Attempted to get author with id: {id} - returned not found");
                    return NotFound();
                }
                var response = _mapper.Map<AuthorDTO>(author);
                _logger.LogInfo($"Successfully retrieved author with id: {id}");
                return Ok(response); // returns status 200, and returns payload
            }
            catch (Exception e) // catch the error
            {
                return InternalError($"{e.Message} - {e.InnerException}"); // use our InternalError method
            }
        }
        
        /// <summary>
        /// Creates an author
        /// </summary>
        /// <param name="authorDto"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAuthor([FromBody] AuthorCreateDTO authorDto)
        {
            try
            {
                _logger.LogWarn($"Attempting to create author");
                if (authorDto == null)
                {
                    _logger.LogWarn("Empty request attempted");
                    return BadRequest(ModelState);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn("Author data not completed");
                    return BadRequest(ModelState);
                }

                var author = _mapper.Map<Author>(authorDto);
                var success = await _authorRepository.Create(author);
                
                if (!success)
                {
                    return InternalError("Author creation failed"); 
                }
                _logger.LogInfo("Author created successfully");
                return Created("Create", new { author }); // returns status 200, and returns payload
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}"); // use our InternalError method
            }
        }
        
        /// <summary>
        /// Updates a user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="author"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] AuthorUpdateDTO authorDto)
        {
            try
            {
                _logger.LogInfo($"Attempting to update author with Id: {id}");
                if (id < 1 || authorDto == null || id != authorDto.Id)
                {
                    _logger.LogWarn("Bad Request: Payload null");
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn("Model State not valid");
                    return BadRequest(ModelState);
                }

                var author = _mapper.Map<Author>(authorDto); // src is whatever is coming through authorDto
                var success = await _authorRepository.Update(author);
                if (!success)
                {
                    return InternalError("Update operation failed"); // use our InternalError method
                }
                // 204 request
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}"); // use our InternalError method
            }
        }
        
        /// <summary>
        /// Removes an author by their associated  Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInfo($"Attempting to delete Author ID: {id}");
                if (id < 1)
                {
                    _logger.LogWarn($"Author deletion failed due to bad data request");
                    return BadRequest();
                }
                
                var doesExist = await _authorRepository.doesExist(id);
                if(!doesExist)
                {
                    _logger.LogWarn($"Attempted to delete Author {id}, but no author exists with the corresponding Id.");
                    return NotFound();
                }
                
                var author = await _authorRepository.FindById(id);
                var success = await _authorRepository.Delete(author);
                
                if (!success)
                {
                    return InternalError("Author deletion failed.");
                }
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}"); // use our InternalError method
            }
        }
    }
}