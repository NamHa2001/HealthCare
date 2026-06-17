using HealthCare.Application.Admin.DTOs;
using HealthCare.Application.Common.Models;
using MediatR;

namespace HealthCare.Application.Admin.Queries.GetUsers;

public record GetUsersQuery(
    string? Search = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<AdminUserDto>>;
