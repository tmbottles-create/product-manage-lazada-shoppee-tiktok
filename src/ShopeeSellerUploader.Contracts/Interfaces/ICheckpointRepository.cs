using ShopeeSellerUploader.Core.Models;

namespace ShopeeSellerUploader.Contracts.Interfaces;

public interface ICheckpointRepository
{
    Task<CheckpointState?> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(CheckpointState state, CancellationToken cancellationToken = default);
}
