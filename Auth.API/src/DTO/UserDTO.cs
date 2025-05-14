namespace Auth.API.DTO;


public class UserDto
{
    public string name { get; set; }
    public string email { get; set; }
    public string password { get; set; }
}

public class UserResponseDto
{
    public int UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }

    public string Role { get; set; }

    public bool IsTwoFactorEnabled { get; set; }

    public string TwoFactorCode { get; set; }

    public DateTime? TwoFactorExpiresAt { get; set; }
}


public class UserInsertDto
{
    public string name { get; set; }
    public string email { get; set; }
    public string password { get; set; }

}

public class LoginDto
{
    public string email { get; set; }
    public string password { get; set; }

}

public class UserResponseMessageDto
{
    public string Message { get; set; }
}

public class UserRecoveryPasswordResponseDto : UserResponseMessageDto
{
}

public class UserResetPasswordResponseDto : UserResponseMessageDto
{
}



public class UserRecoveryPasswordDto
{
    public string email { get; set; }

}

public class UserResetPasswordDto
{
    public string password { get; set; }
}