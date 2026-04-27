using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Data;
using UserManagementAPI.Models;

namespace UserManagementAPI.Controllers
{
    /// <summary>
    /// Controller for managing users via a RESTful API.
    /// Provides CRUD operations: GET, POST, PUT, DELETE.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserRepository repository, ILogger<UsersController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/users
        // Returns all users, with optional role filter.
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<User>>), StatusCodes.Status200OK)]
        public IActionResult GetAll([FromQuery] string? role = null, [FromQuery] bool? isActive = null)
        {
            var users = _repository.GetAll();

            if (!string.IsNullOrWhiteSpace(role))
                users = users.Where(u => u.Role.Equals(role, StringComparison.OrdinalIgnoreCase));

            if (isActive.HasValue)
                users = users.Where(u => u.IsActive == isActive.Value);

            _logger.LogInformation("GET /api/users returned {Count} users.", users.Count());
            return Ok(ApiResponse<IEnumerable<User>>.Ok(users, $"Retrieved {users.Count()} user(s)."));
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/users/{id}
        // Returns a single user by ID.
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<User>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<User>), StatusCodes.Status404NotFound)]
        public IActionResult GetById(int id)
        {
            var user = _repository.GetById(id);
            if (user == null)
            {
                _logger.LogWarning("GET /api/users/{Id} - user not found.", id);
                return NotFound(ApiResponse<User>.Fail($"User with ID {id} was not found."));
            }

            return Ok(ApiResponse<User>.Ok(user));
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /api/users
        // Creates a new user. Validates model + email uniqueness.
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<User>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<User>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<User>), StatusCodes.Status409Conflict)]
        public IActionResult Create([FromBody] CreateUserRequest request)
        {
            // Model validation (data annotations on CreateUserRequest)
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<User>.Fail("Validation failed.", errors));
            }

            // Business-rule validation: email must be unique
            if (_repository.EmailExists(request.Email))
            {
                return Conflict(ApiResponse<User>.Fail($"A user with email '{request.Email}' already exists."));
            }

            var user = new User
            {
                FirstName = request.FirstName,
                LastName  = request.LastName,
                Email     = request.Email,
                Role      = request.Role,
                Age       = request.Age,
            };

            var created = _repository.Create(user);
            _logger.LogInformation("POST /api/users - created user ID {Id}.", created.Id);

            return CreatedAtAction(nameof(GetById), new { id = created.Id },
                ApiResponse<User>.Ok(created, "User created successfully."));
        }

        // ─────────────────────────────────────────────────────────────────────
        // PUT /api/users/{id}
        // Updates an existing user (partial update — only provided fields change).
        // ─────────────────────────────────────────────────────────────────────
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<User>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<User>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<User>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<User>), StatusCodes.Status409Conflict)]
        public IActionResult Update(int id, [FromBody] UpdateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<User>.Fail("Validation failed.", errors));
            }

            // If the request tries to change the email, ensure uniqueness
            if (request.Email != null && _repository.EmailExists(request.Email, excludeId: id))
            {
                return Conflict(ApiResponse<User>.Fail($"A user with email '{request.Email}' already exists."));
            }

            var updated = _repository.Update(id, request);
            if (updated == null)
            {
                _logger.LogWarning("PUT /api/users/{Id} - user not found.", id);
                return NotFound(ApiResponse<User>.Fail($"User with ID {id} was not found."));
            }

            _logger.LogInformation("PUT /api/users/{Id} - user updated.", id);
            return Ok(ApiResponse<User>.Ok(updated, "User updated successfully."));
        }

        // ─────────────────────────────────────────────────────────────────────
        // DELETE /api/users/{id}
        // Deletes a user by ID.
        // ─────────────────────────────────────────────────────────────────────
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public IActionResult Delete(int id)
        {
            var deleted = _repository.Delete(id);
            if (!deleted)
            {
                _logger.LogWarning("DELETE /api/users/{Id} - user not found.", id);
                return NotFound(ApiResponse<object>.Fail($"User with ID {id} was not found."));
            }

            _logger.LogInformation("DELETE /api/users/{Id} - user deleted.", id);
            return Ok(ApiResponse<object>.Ok(null, $"User with ID {id} was deleted successfully."));
        }
    }
}
