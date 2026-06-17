using MediatR;

namespace HealthCare.Application.Family.Commands.InviteMember;

/// <summary>SRS §2.1 — Mời thành viên qua email.</summary>
public record InviteFamilyMemberCommand(string Email) : IRequest;
