using HealthCare.Application.Family.DTOs;
using MediatR;

namespace HealthCare.Application.Family.Commands.CreateFamilyGroup;

public record CreateFamilyGroupCommand(string Name) : IRequest<FamilyGroupDto>;
