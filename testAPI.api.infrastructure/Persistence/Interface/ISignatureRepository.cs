using testAPI.api.domain.Entities;
using static testAPI.api.infrastructure.Persistence.Interface.IRepo;

namespace testAPI.api.infrastructure.Persistence.Interface
{
    public interface ISignatureRepository : IRepo<Signature>
    {
        Task<Signature?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<bool> UserHasSignatureAsync(int userId, CancellationToken cancellationToken = default);
        Task<Signature?> GetByIdWithTrackingAsync(int id, CancellationToken cancellationToken = default);
        Task UpdateSignatureAsync(Signature signature, CancellationToken cancellationToken = default);
    }
}
