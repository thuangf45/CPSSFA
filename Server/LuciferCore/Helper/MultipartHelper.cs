using LuciferCore.Core;
using LuciferCore.Extra;
using LuciferCore.Manager;
using LuciferCore.NetCoreServer;
using System.Text;

public static class MultipartHelper
{
    private static int IndexOf(byte[] haystack, byte[] needle, int startIndex)
    {
        if (haystack == null || needle == null || startIndex < 0 || startIndex >= haystack.Length)
        {
            return -1;
        }

        for (int i = startIndex; i <= haystack.Length - needle.Length; i++)
        {
            bool found = true;
            for (int j = 0; j < needle.Length; j++)
            {
                if (haystack[i + j] != needle[j])
                {
                    found = false;
                    break;
                }
            }
            if (found) return i;
        }
        return -1;
    }

    public static string GetHeader(HttpRequest request, string name)
    {
        for (int i = 0; i < request.Headers; i++)
        {
            var (key, value) = request.Header(i);
            if (key.Equals(name, StringComparison.OrdinalIgnoreCase))
                return value;
        }
        return null;
    }

    public static List<UploadedFile> ParseFiles(HttpRequest request)
    {
        var files = new List<UploadedFile>();
        var contentType = GetHeader(request, "Content-Type");
        if (string.IsNullOrEmpty(contentType) || !contentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase))
            return files;

        var boundaryIndex = contentType.IndexOf("boundary=", StringComparison.OrdinalIgnoreCase);
        if (boundaryIndex < 0) return files;

        var boundaryClean = contentType.Substring(boundaryIndex + 9).Trim().Trim('"');
        var boundary = "--" + boundaryClean;

        var boundaryBytes = Encoding.UTF8.GetBytes(boundary);
        var headerSeparatorBytes = Encoding.UTF8.GetBytes("\r\n\r\n");
        var bodyBytes = request.BodyBytes;

        Simulation.GetModel<LogManager>().Log($"BodyBytes Length: {bodyBytes.Length}");
        Simulation.GetModel<LogManager>().Log($"Boundary: {boundary}");

        int pos = IndexOf(bodyBytes, boundaryBytes, 0);
        if (pos == -1) return files;
        Simulation.GetModel<LogManager>().Log($"Found first boundary at pos: {pos}");

        pos += boundaryBytes.Length; // Di chuyển con trỏ qua boundary đầu tiên

        while (pos < bodyBytes.Length)
        {
            // Bỏ qua CRLF sau boundary nếu có
            if (pos + 2 <= bodyBytes.Length && bodyBytes[pos] == '\r' && bodyBytes[pos + 1] == '\n')
                pos += 2;

            // Kiểm tra xem đã đến end boundary chưa
            if (pos + 2 <= bodyBytes.Length && bodyBytes[pos] == '-' && bodyBytes[pos + 1] == '-')
            {
                Simulation.GetModel<LogManager>().Log("Reached end boundary. Stop parsing.");
                break;
            }

            // Tìm vị trí kết thúc header
            int headerEndPos = IndexOf(bodyBytes, headerSeparatorBytes, pos);
            if (headerEndPos < 0) break;
            Simulation.GetModel<LogManager>().Log($"Found header end at pos: {headerEndPos}");

            // Trích xuất và parse header
            var headerStr = Encoding.UTF8.GetString(bodyBytes, pos, headerEndPos - pos);
            pos = headerEndPos + 4; // Di chuyển con trỏ qua \r\n\r\n

            string name = null;
            string fileName = null;
            string partContentType = null;

            foreach (var line in headerStr.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.StartsWith("Content-Disposition", StringComparison.OrdinalIgnoreCase))
                {
                    var nameMatch = System.Text.RegularExpressions.Regex.Match(line, @"name=""([^""]*)""");
                    if (nameMatch.Success) name = nameMatch.Groups[1].Value;

                    var fileNameMatch = System.Text.RegularExpressions.Regex.Match(line, @"filename=""([^""]*)""");
                    if (fileNameMatch.Success) fileName = fileNameMatch.Groups[1].Value;
                }
                else if (line.StartsWith("Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    partContentType = line.Split(':')[1].Trim();
                }
            }

            if (string.IsNullOrEmpty(fileName))
            {
                // Nếu không phải file, tìm boundary tiếp theo và bỏ qua
                int nextBoundaryPos = IndexOf(bodyBytes, boundaryBytes, pos);
                if (nextBoundaryPos < 0) break;
                pos = nextBoundaryPos;
            }
            else
            {
                // Đây là file, tìm boundary tiếp theo để lấy dữ liệu file
                int nextBoundaryPos = IndexOf(bodyBytes, boundaryBytes, pos);
                if (nextBoundaryPos < 0) break;
                Simulation.GetModel<LogManager>().Log($"Found next boundary at pos: {nextBoundaryPos}");

                // Lấy dữ liệu file (loại bỏ CRLF trước boundary)
                int fileDataLength = nextBoundaryPos - pos;
                if (fileDataLength >= 2 && bodyBytes[nextBoundaryPos - 2] == '\r' && bodyBytes[nextBoundaryPos - 1] == '\n')
                {
                    fileDataLength -= 2;
                }

                var fileData = new byte[fileDataLength];
                Array.Copy(bodyBytes, pos, fileData, 0, fileDataLength);

                files.Add(new UploadedFile
                {
                    Name = name,
                    FileName = fileName,
                    ContentType = partContentType,
                    Content = fileData
                });

                Simulation.GetModel<LogManager>().Log($"File '{fileName}' extracted. Raw Length: {nextBoundaryPos - pos}, Clean Length: {fileDataLength}");
                pos = nextBoundaryPos;
            }
        }
        return files;
    }
}