using UserManagementAPI.Models;

namespace UserManagementAPI.Data
{
    /// <summary>
    /// Interface defining the contract for user data operations.
    /// </summary>
    public interface IUserRepository
    {
        IEnumerable<User> GetAll();
        User? GetById(int id);
        User? GetByEmail(string email);
        User Create(User user);
        User? Update(int id, UpdateUserRequest request);
        bool Delete(int id);
        bool EmailExists(string email, int? excludeId = null);
    }

    /// <summary>
    /// In-memory implementation of IUserRepository.
    /// Stores users in a thread-safe dictionary for demo/testing purposes.
    /// </summary>
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly Dictionary<int, User> _users = new();
        private int _nextId = 1;
        private readonly object _lock = new();

        public InMemoryUserRepository()
        {
            // Seed with sample data
            SeedData();
        }

        private void SeedData()
        {
            var seedUsers = new[]
            {
                new User { FirstName = "Alice",   LastName = "Smith",   Email = "alice@example.com",   Role = "Admin",     Age = 30 },
                new User { FirstName = "Bob",     LastName = "Johnson", Email = "bob@example.com",     Role = "User",      Age = 25 },
                new User { FirstName = "Carol",   LastName = "White",   Email = "carol@example.com",   Role = "Moderator", Age = 28 },
            };

            foreach (var user in seedUsers)
            {
                user.Id = _nextId++;
                _users[user.Id] = user;
            }
        }

        public IEnumerable<User> GetAll()
        {
            lock (_lock)
            {
                return _users.Values.ToList();
            }
        }

        public User? GetById(int id)
        {
            lock (_lock)
            {
                _users.TryGetValue(id, out var user);
                return user;
            }
        }

        public User? GetByEmail(string email)
        {
            lock (_lock)
            {
                return _users.Values.FirstOrDefault(u =>
                    u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            }
        }

        public User Create(User user)
        {
            lock (_lock)
            {
                user.Id = _nextId++;
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                _users[user.Id] = user;
                return user;
            }
        }

        public User? Update(int id, UpdateUserRequest request)
        {
            lock (_lock)
            {
                if (!_users.TryGetValue(id, out var user))
                    return null;

                // Only update fields that were provided (partial update / PATCH-style PUT)
                if (request.FirstName != null) user.FirstName = request.FirstName;
                if (request.LastName  != null) user.LastName  = request.LastName;
                if (request.Email     != null) user.Email     = request.Email;
                if (request.Role      != null) user.Role      = request.Role;
                if (request.Age       != null) user.Age       = request.Age.Value;
                if (request.IsActive  != null) user.IsActive  = request.IsActive.Value;

                user.UpdatedAt = DateTime.UtcNow;
                return user;
            }
        }

        public bool Delete(int id)
        {
            lock (_lock)
            {
                return _users.Remove(id);
            }
        }

        public bool EmailExists(string email, int? excludeId = null)
        {
            lock (_lock)
            {
                return _users.Values.Any(u =>
                    u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                    (excludeId == null || u.Id != excludeId));
            }
        }
    }
}
