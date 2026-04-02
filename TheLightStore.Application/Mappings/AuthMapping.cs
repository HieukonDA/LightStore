using System;
using TheLightStore.Infrastructure.Persistence.SysEntities;
using static TheLightStore.Application.Dtos.AuthDto;

namespace TheLightStore.Application.Mappings;

public static class AuthMapping
{
    public static MeDto EntityToVModel(Users entity)
    {
        return new MeDto
        {
            UserId = entity.Id,
            UserName = entity.UserName,
            Email = entity.Email ?? "",
            FirstName = entity.FirstName ?? "",
            LastName = entity.LastName ?? "",
            Sex = entity.Sex.HasValue ? (entity.Sex.Value ? "MALE" : "FEMALE") : "",
            Birthday = entity.Birthday.HasValue ? entity.Birthday.Value.ToString("dd/MM/yyyy") : "",
            Address = entity.Address ?? "",
            Note = entity.Note ?? "",
            CreatedBy = entity.CreatedBy,
            CreatedDate = entity.CreatedDate,
            UpdatedBy = entity.UpdatedBy,
            UpdatedDate = entity.UpdatedDate,
            IsActive = entity.IsActive,
            RoleType = entity.Role?.Name
        };
    }
    public static LoginResponseSlim EntityToLoginResponseVModel(string AccessToken, string RefreshToken, Users entity)
    {
        return new LoginResponseSlim
        {
            AccessToken = AccessToken,
            RefreshToken = RefreshToken,
            Email = entity.Email,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
        };
    }
}
