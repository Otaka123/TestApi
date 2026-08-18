using testAPI.api.domain.Entities;
using testAPI.api.infrastructure.Data;
using testAPI.api.infrastructure.Persistence.Interface;
using Application.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static testAPI.api.infrastructure.Persistence.Repo;

namespace testAPI.api.infrastructure.Persistence
{
    public class SignatureRepository : Repository<Signature>, ISignatureRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<SignatureRepository> _logger;

        public SignatureRepository(AppDbContext db, ILogger<SignatureRepository> logger) : base(db)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Signature?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _db.Signatures.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        }

        public async Task<bool> UserHasSignatureAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _db.Signatures.AnyAsync(s => s.UserId == userId, cancellationToken);
        }

        public async Task<Signature?> GetByIdWithTrackingAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _db.Signatures.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task UpdateSignatureAsync(Signature signature, CancellationToken cancellationToken = default)
        {
            try
            {
                var existingSignature = await _db.Signatures.FirstOrDefaultAsync(s => s.Id == signature.Id, cancellationToken);

                if (existingSignature == null)
                    throw new ArgumentException($"Signature with ID {signature.Id} not found");

                _db.Entry(existingSignature).CurrentValues.SetValues(signature);
                existingSignature.UpdatedAt = AppDubaiTime.Now;

                _db.Signatures.Update(existingSignature);
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating signature");
                throw;
            }
        }
    }
}
