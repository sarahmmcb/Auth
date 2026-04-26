using Auth.Contracts;

namespace AuthDAL.Models;
public class UserDTO
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public AccountStatus AccountStatus { get; set; }
    public DateTimeOffset CreateDate { get; set; }
    public int RoleId { get; set; }
    public required string RoleName { get; set; }
}
