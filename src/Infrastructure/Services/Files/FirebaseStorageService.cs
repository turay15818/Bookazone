using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace Bookazone.Infrastructure.Services.Files;

public class FirebaseStorageService
{
    private readonly ILogger<FirebaseStorageService> _logger;
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;
    public FirebaseStorageService(
        IConfiguration configuration,
        ILogger<FirebaseStorageService> logger, 
        StorageClient storageClient)
    {
        _logger = logger;
        _storageClient = storageClient ?? throw new ArgumentNullException(nameof(storageClient), "StorageClient must be provided");
        _bucketName = configuration["Firebase:BucketName"] ?? "bookazone-7391a.firebasestorage.app";
        _logger.LogInformation("Firebase Storage bucket name: {BucketName}", _bucketName);
        if (FirebaseApp.DefaultInstance == null)
        {
            try
            {
                var serviceAccountPath = configuration["Firebase:CredentialPath"];
                if (!string.IsNullOrEmpty(serviceAccountPath))
                {
                    var fullPath = Path.IsPathRooted(serviceAccountPath)
                        ? serviceAccountPath
                        : Path.Combine(Directory.GetCurrentDirectory(), serviceAccountPath);

                    if (File.Exists(fullPath))
                    {
                        FirebaseApp.Create(new AppOptions
                        {
                            Credential = GoogleCredential.FromFile(fullPath),
                            ProjectId = configuration["Firebase:ProjectId"]
                        });
                        _logger.LogInformation("Firebase Admin SDK initialized successfully for Storage");
                    }
                    else
                    {
                        _logger.LogError("Firebase service account file not found at: {Path}", fullPath);
                    }
                }
                else
                {
                    _logger.LogWarning("Firebase service account path not configured. Storage operations disabled.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Firebase Admin SDK");
            }
        }

        _logger.LogInformation("Using injected Google Cloud Storage client");
    }

    
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            _logger.LogInformation("Testing Firebase Storage connection...");
            var buckets = new List<Google.Apis.Storage.v1.Data.Bucket>();
            await foreach (var bucket in _storageClient.ListBucketsAsync(_bucketName.Split('.')[0]))
            {
                buckets.Add(bucket);
            }
            _logger.LogInformation("Firebase connection successful. Found {Count} buckets.", buckets.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Firebase connection failed.");
            return false;
        }
    }

    
    public async Task<string?> UploadFileAsync(IFormFile file, string folder)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("File is null or empty");
                return null;
            }
            _logger.LogInformation("Uploading file {FileName} of type {ContentType} to folder {Folder}", 
                file.FileName, file.ContentType, folder);
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var objectName = $"{folder}/{fileName}";
            _logger.LogInformation("Generated object name: {ObjectName}", objectName);
            using var stream = file.OpenReadStream();
            var uploadOptions = new UploadObjectOptions
            {
                PredefinedAcl = PredefinedObjectAcl.PublicRead
            };
            _logger.LogInformation("Uploading to bucket: {BucketName}", _bucketName);
            var uploadedObject = await _storageClient.UploadObjectAsync(
                _bucketName,
                objectName,
                file.ContentType,
                stream,
                options: uploadOptions);
            _logger.LogInformation("File uploaded successfully to Firebase Storage: {ObjectName}", objectName);
            var downloadUrl = $"https://storage.googleapis.com/{_bucketName}/{objectName}";
            _logger.LogInformation("Generated download URL: {DownloadUrl}", downloadUrl);
            return downloadUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName} to Firebase Storage", file?.FileName);
            return null;
        }
    }



    public async Task<bool> MakeObjectPublicAsync(string objectPath)
    {
        try
        {
            var obj = await _storageClient.GetObjectAsync(_bucketName, objectPath);
            await _storageClient.UpdateObjectAsync(obj, new UpdateObjectOptions
            {
                PredefinedAcl = PredefinedObjectAcl.PublicRead
            });

            _logger.LogInformation("Updated ACL for object {ObjectPath} to PublicRead", objectPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ACL for object {ObjectPath}", objectPath);
            return false;
        }
    }
    
    public async Task<string?> UpdateFileAsync(IFormFile newFile, string oldFilePath, string folder)
    {
        if (!string.IsNullOrWhiteSpace(oldFilePath))
            await DeleteFileAsync(oldFilePath);

        return await UploadFileAsync(newFile, folder);
    }

    public async Task<bool> DeleteFileAsync(string objectPath)
    {
        try
        {
            await _storageClient.DeleteObjectAsync(_bucketName, objectPath);
            _logger.LogInformation("Deleted file from Firebase Storage: {ObjectPath}", objectPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {ObjectPath}", objectPath);
            return false;
        }
    }


    public async Task<int> MakeFolderContentsPublicAsync(string folder)
    {
        try
        {
            int count = 0;
            var objects = _storageClient.ListObjectsAsync(_bucketName, folder);
            await foreach (var obj in objects)
            {
                if (await MakeObjectPublicAsync(obj.Name))
                {
                    count++;
                }
            }
            _logger.LogInformation("Made {Count} objects in folder {Folder} publicly readable", count, folder);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making folder contents public: {Folder}", folder);
            return 0;
        }
    }
}
