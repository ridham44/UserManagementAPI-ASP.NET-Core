using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.Models
{
    /// <summary>
    /// Represents a user in the system.
    /// </summary>
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        [RegularExpression("^(Admin|User|Moderator)$", ErrorMessage = "Role must be Admin, User, or Moderator.")]
        public string Role { get; set; } = "User";

        [Range(18, 120, ErrorMessage = "Age must be between 18 and 120.")]
        public int Age { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for creating a new user (no Id required).
    /// </summary>
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        [RegularExpression("^(Admin|User|Moderator)$", ErrorMessage = "Role must be Admin, User, or Moderator.")]
        public string Role { get; set; } = "User";

        [Range(18, 120, ErrorMessage = "Age must be between 18 and 120.")]
        public int Age { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing user.
    /// </summary>
    public class UpdateUserRequest
    {
        [StringLength(50, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 50 characters.")]
        public string? FirstName { get; set; }

        [StringLength(50, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 50 characters.")]
        public string? LastName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string? Email { get; set; }

        [RegularExpression("^(Admin|User|Moderator)$", ErrorMessage = "Role must be Admin, User, or Moderator.")]
        public string? Role { get; set; }

        [Range(18, 120, ErrorMessage = "Age must be between 18 and 120.")]
        public int? Age { get; set; }

        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// Standard API response wrapper.
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Success") =>
            new() { Success = true, Message = message, Data = data };

        public static ApiResponse<T> Fail(string message, List<string>? errors = null) =>
            new() { Success = false, Message = message, Errors = errors };
    }
}
