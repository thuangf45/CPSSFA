using LuciferCore.Extra;

public static class ImageFileHelper
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

    public static (bool isValid, string extension, string error) Validate(UploadedFile file)
    {
        if (file == null)
            return (false, null, "Không có file!");

        if (string.IsNullOrEmpty(file.ContentType) || !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return (false, null, "File không phải ảnh!");

        string extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(extension))
        {
            extension = file.ContentType switch
            {
                "image/png" => ".png",
                "image/jpeg" => ".jpg",
                "image/gif" => ".gif",
                _ => ".dat"
            };
        }

        if (!AllowedExtensions.Contains(extension.ToLower()))
            return (false, null, "Định dạng ảnh không hợp lệ!");

        // Kiểm tra header file
        if (!IsValidImage(file.Content, extension))
            return (false, null, "Dữ liệu file ảnh không hợp lệ!");

        return (true, extension, null);
    }

    private static bool IsValidImage(byte[] content, string extension)
    {
        if (content.Length < 4)
            return false;

        switch (extension.ToLower())
        {
            case ".jpg":
            case ".jpeg":
                return content[0] == 0xFF && content[1] == 0xD8; // JPEG: Bắt đầu bằng FF D8
            case ".png":
                return content[0] == 0x89 && content[1] == 0x50 && content[2] == 0x4E && content[3] == 0x47; // PNG: 89 50 4E 47
            case ".gif":
                return content[0] == 0x47 && content[1] == 0x49 && content[2] == 0x46; // GIF: 47 49 46
            default:
                return false;
        }
    }
}
