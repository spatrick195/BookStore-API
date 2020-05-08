using BookStore_API.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.DTOs;
using System.Collections.Generic;
using System;
using BookStore_API.Data;

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
        
        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;
            return $"{controller} - {action}";
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
            var location = GetControllerActionNames();

            try
            {
                _logger.LogInfo($"{location}: Attempting to retrieve all author records...");
                var authors = await _authorRepository.FindAll();
                if (authors == null)
                {
                    _logger.LogWarn($"{location}: Attempted to retrieve all authors, but none were found.");
                    return NotFound();
                }
                var response = _mapper.Map<IList<AuthorDTO>>(authors);
                _logger.LogInfo($"{location}: Retrieved authors successfully.");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Returns an author based on Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns an author based on the id given </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthor(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempting to retrieve author record: {id}...");

                if (id < 1)
                {
                    _logger.LogWarn($"{location}: Author retrieval failed due to a bad data request.");
                }
                
                var author = await _authorRepository.FindById(id);
                
                if (author == null)
                {
                    _logger.LogWarn($"{location}: No author records found with Id: {id}.");
                    return NotFound();
                }
                var response = _mapper.Map<AuthorDTO>(author);
                _logger.LogInfo($"{location}: Successfully retrieved author record: {id}.");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
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
            var location = GetControllerActionNames();
            try
            {
                _logger.LogWarn($"{location}: Attempting to create author...");
                if (authorDto == null)
                {
                    _logger.LogWarn($"{location}: Author creation failed due to a bad data request.");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: Author data was incomplete or not valid.");
                    return BadRequest(ModelState);
                }
                var author = _mapper.Map<Author>(authorDto);
                var success = await _authorRepository.Create(author);
                if (!success)
                {
                    return InternalError($"{location}: Author creation failed"); 
                }
                _logger.LogInfo($"{location}: Author created successfully");
                return Created("Create", new { author });
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }
        
        /// <summary>
        /// Updates an author
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
            var location = GetControllerActionNames();

            try
            {
                _logger.LogWarn($"{location}: Attempting to update author record: {id}...");
                if (id < 1 || authorDto == null || id != authorDto.Id)
                {
                    _logger.LogWarn($"{location}: Author update failed due to a bad data request.");
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: Author data was incomplete or not valid.");
                    return BadRequest(ModelState);
                }

                var author = _mapper.Map<Author>(authorDto);
                var success = await _authorRepository.Update(author);
                if (!success)
                {
                    return InternalError($"{location}: Author update failed."); 
                }

                _logger.LogInfo($"{location}: Author updated successfully.");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }
        
        /// <summary>
        /// Removes an author by their associated Id
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
                _logger.LogInfo($"{location}: Attempting to delete Author record: {id}...");
                if (id < 1)
                {
                    _logger.LogWarn($"{location}: Author deletion failed due to a bad data request.");
                    return BadRequest();
                }
                var doesExist = await _authorRepository.DoesExist(id);
                if(!doesExist)
                {
                    _logger.LogWarn($"{location}: Attempted to delete an Author with an id of {id}, but there are no authors with this Id.");
                    return NotFound();
                }
                var author = await _authorRepository.FindById(id);
                var success = await _authorRepository.Delete(author);
                if (!success)
                {
                    return InternalError($"{location}: Author deletion failed.");
                }
                _logger.LogInfo($"{location}: Author deleted successfully.");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }
    }
}