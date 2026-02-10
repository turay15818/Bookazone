using Bookazone.Domain.Entities.Profile;

namespace Bookazone.Application.DTOs;

public class JwtResponse
{
    public JwtResponse()
    {
    }

    public JwtResponse(string? token, Users? user)
    {
        Token = token;
        User = user;
    }

    public string? Token { get; set; }
    public Users? User { get; set; }
}