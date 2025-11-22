using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceCoza.BLL.Services
{
    public class FileService
    {
        public async Task<string> GenerateFile(IFormFile file, string filePath)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is null or empty", nameof(file));

            var fileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}-{Guid.NewGuid()}-{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(filePath, fileName);

            Directory.CreateDirectory(filePath);

            var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
            stream.Close();

            return fileName;
        }

        public bool IsImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var contentType = file.ContentType.ToLowerInvariant();

            return contentType.StartsWith("image/");
        }
    }
}
