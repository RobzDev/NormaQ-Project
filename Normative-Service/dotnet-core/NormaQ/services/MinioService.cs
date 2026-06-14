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


    public async Task<string> SubirArchivoAsync(IFormFile archivo, string rutaCompleta)
    {
        using var stream = archivo.OpenReadStream();
        var request = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = rutaCompleta,
            InputStream = stream,
            ContentType = archivo.ContentType,
            AutoCloseStream = true
        };

        await _s3.PutObjectAsync(request);
        return rutaCompleta; // Retornamos la ruta que se guardará en minio_identifier
    }

    /// <summary>
    /// Recupera el flujo de datos de un objeto en MinIO.
    /// </summary>
    /// <param name="rutaCompleta">La llave (Key) del objeto en el bucket.</param>
    /// <returns>Un Stream con los datos del archivo.</returns>
    public async Task<Stream> ObtenerArchivoAsync(string rutaCompleta)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucket,
                Key = rutaCompleta
            };

            // Ejecución de la petición a MinIO
            GetObjectResponse response = await _s3.GetObjectAsync(request);

            // IMPORTANTE: Devolvemos el ResponseStream. 
            // No usamos el bloque 'using' aquí porque cerraríamos el stream 
            // antes de que el controlador pueda leerlo.
            return response.ResponseStream;
        }
        catch (AmazonS3Exception e)
        {
            // Manejo de errores específicos de S3 (ej. archivo no encontrado)
            throw new Exception($"Error al recuperar archivo de MinIO: {e.Message}");
        }
    }




    public async Task EliminarArchivoAsync(string rutaCompleta)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _bucket,
            Key = rutaCompleta
        };
        await _s3.DeleteObjectAsync(request);
    }

    public async Task CopiarArchivoAsync(string rutaOrigen, string rutaDestino)
    {
        try
        {
            // 1. Ejecución: Configurar la petición de copiado interno
            var request = new CopyObjectRequest
            {
                SourceBucket = _bucket,
                SourceKey = rutaOrigen,
                DestinationBucket = _bucket,
                DestinationKey = rutaDestino
            };

            // 2. Ejecución: S3/MinIO duplica el puntero del archivo internamente
            await _s3.CopyObjectAsync(request);
        }
        catch (AmazonS3Exception ex)
        {
            throw new Exception($"Error operativo en el servidor MinIO al copiar objeto: {ex.Message}");
        }
    }

}