using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;
using RoleType = Domain.Enums.RoleType;

namespace Application.Features.Profiles.Commands;

public record UpdateAvatarCommand(Guid AccountId, RoleType Role, string AvatarUrl) : IRequest<bool>;

public class UpdateAvatarHandler : IRequestHandler<UpdateAvatarCommand, bool>
{
    private readonly IGenericRepository<Patient> _patientRepo;
    private readonly IGenericRepository<Doctor> _doctorRepo;
    private readonly IGenericRepository<Registrar> _registrarRepo;
    private readonly IGenericRepository<Domain.Models.Admin> _adminRepo;

    public UpdateAvatarHandler(
        IGenericRepository<Patient> patientRepo,
        IGenericRepository<Doctor> doctorRepo,
        IGenericRepository<Registrar> registrarRepo,
        IGenericRepository<Domain.Models.Admin> adminRepo)
    {
        _patientRepo = patientRepo;
        _doctorRepo = doctorRepo;
        _registrarRepo = registrarRepo;
        _adminRepo = adminRepo;
    }

    public async Task<bool> Handle(UpdateAvatarCommand request, CancellationToken cancellationToken)
    {
        switch (request.Role)
        {
            case RoleType.patient:
                var patient = await _patientRepo.FirstOrDefaultAsync(p => p.AccountId == request.AccountId);
                if (patient == null) return false;
                patient.AvatarUrl = request.AvatarUrl;
                _patientRepo.Update(patient);
                await _patientRepo.SaveChangesAsync();
                break;

            case RoleType.doctor:
                var doctor = await _doctorRepo.FirstOrDefaultAsync(d => d.AccountId == request.AccountId);
                if (doctor == null) return false;
                doctor.AvatarUrl = request.AvatarUrl;
                _doctorRepo.Update(doctor);
                await _doctorRepo.SaveChangesAsync();
                break;

            case RoleType.registrar:
                var registrar = await _registrarRepo.FirstOrDefaultAsync(r => r.AccountId == request.AccountId);
                if (registrar == null) return false;
                registrar.AvatarUrl = request.AvatarUrl;
                _registrarRepo.Update(registrar);
                await _registrarRepo.SaveChangesAsync();
                break;
            
            default:
                throw new NotImplementedException();
        }

        return true;
    }
}
