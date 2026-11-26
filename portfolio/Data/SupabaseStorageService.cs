using System.Net.Http.Headers;

namespace portfolio.Data
{
    public class SupabaseStorageService
    {
        private readonly HttpClient _httpClient;
        private readonly string _supabaseUrl;
        private readonly string _serviceRoleKey;
        private readonly string _bucketName;

        public SupabaseStorageService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _supabaseUrl = config["Supabase:Url"]!.TrimEnd('/');
            _serviceRoleKey = config["Supabase:ServiceRoleKey"]!;
            _bucketName = config["Supabase:BucketName"] ?? "portfolio-images";
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            // Generate a unique filename so uploads never overwrite each other
            string extension = Path.GetExtension(file.FileName);
            string fileName = $"{Guid.NewGuid()}{extension}";

            string uploadEndpoint = $"{_supabaseUrl}/storage/v1/object/{_bucketName}/{fileName}";

            using var stream = file.OpenReadStream();
            using var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            using var request = new HttpRequestMessage(HttpMethod.Post, uploadEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _serviceRoleKey);
            request.Headers.Add("apikey", _serviceRoleKey);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Supabase Storage upload failed: {errorBody}");
            }

            // Return the direct public URL for the uploaded image
            return $"{_supabaseUrl}/storage/v1/object/public/{_bucketName}/{fileName}";
        }

        public async Task DeleteImageAsync(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return;
            }

            string publicPrefix = $"{_supabaseUrl}/storage/v1/object/public/{_bucketName}/";

            // Skip deletion if the image is an external URL rather than stored in your Supabase bucket
            if (!imageUrl.StartsWith(publicPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string fileName = imageUrl.Substring(publicPrefix.Length);
            string deleteEndpoint = $"{_supabaseUrl}/storage/v1/object/{_bucketName}/{fileName}";

            using var request = new HttpRequestMessage(HttpMethod.Delete, deleteEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _serviceRoleKey);
            request.Headers.Add("apikey", _serviceRoleKey);

            await _httpClient.SendAsync(request);
        }
    }
}
