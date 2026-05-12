using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Clinic.Queries;

public record DoctorSpecializationDto(Guid Id, string Name, int ExperienceYears);

public record DoctorDto(
    Guid Id,
    string FirstName,
    string LastName,
    string MiddleName,
    string Bio,
    string AvatarUrl,
    decimal RatingAvg,
    Guid OfficeId,
    List<DoctorSpecializationDto> Specializations);

public record PaginatedList<T>(List<T> Items, int TotalCount, int PageNumber, int PageSize);

public record GetDoctorsQuery(
    Guid? OfficeId = null,
    Guid? SpecializationId = null,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PaginatedList<DoctorDto>>;

public class GetDoctorsHandler : IRequestHandler<GetDoctorsQuery, PaginatedList<DoctorDto>>
{
    private readonly IDoctorRepository _doctorRepo;

    public GetDoctorsHandler(IDoctorRepository doctorRepo)
    {
        _doctorRepo = doctorRepo;
    }

    public async Task<PaginatedList<DoctorDto>> Handle(GetDoctorsQuery request, CancellationToken cancellationToken)
    {
        var (doctors, totalCount) = await _doctorRepo.GetFilteredDoctorsAsync(
            request.OfficeId,
            request.SpecializationId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var currentYear = DateOnly.FromDateTime(DateTime.UtcNow);

        var doctorDtos = doctors.Select(d => new DoctorDto(
            d.Id,
            d.FirstName,
            d.LastName,
            d.MiddleName ?? "",
            d.Bio,
            d.AvatarUrl,
            d.RatingAvg,
            d.OfficeId,
            d.DoctorSpecializations.Select(ds => new DoctorSpecializationDto(
                ds.SpecializationId,
                ds.Specialization.Name,
                ds.CareerStartDate.HasValue ? currentYear.Year - ds.CareerStartDate.Value.Year : 0
            )).ToList()
        )).ToList();

        return new PaginatedList<DoctorDto>(doctorDtos, totalCount, request.PageNumber, request.PageSize);
    }
}
