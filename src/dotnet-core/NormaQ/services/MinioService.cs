using Amazon.S3;
using Amazon.S3.Model;

namespace NormaQ.Services;

public class MinioService
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucket;

    public MinioService(IAmazonS3 s3, IConfiguration config)
    {
        _s3 = s3;
        _bucket = config["MinIO:BucketName"]!;
    }

    public async Task EnsureBucketExistsAsync()
    {
        try
        {
            await _s3.EnsureBucketExistsAsync(_bucket);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al verificar/crear bucket MinIO: {ex.Message}");
        }
    }
}