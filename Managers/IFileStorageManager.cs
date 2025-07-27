namespace GBES.Managers
{
    public interface IFileStorageManager
    {
        /// <summary>
        /// File(blob) Upload
        /// </summary>
        /// <returns>New FileName</returns>
        Task<string> UploadAsync(byte[] bytes, string fileName, string folderPath, bool overwrite);
        Task<string> UploadAsync(Stream stream, string fileName, string folderPath, bool overwrite);

        /// <summary>
        /// File(blob) Download
        /// </summary>
        /// <returns>File(blob)</returns>
        Task<byte[]> DownloadAsync(string fileName, string folderPath);

        /// <summary>
        /// File(blob) Delete
        /// </summary>
        /// <returns>true of false</returns>
        Task<bool> DeleteAsync(string fileName, string folderPath);

        /// <summary>
        /// Get Sub Folder with string   동적으로 폴더 만들기
        /// </summary>
        string GetFolderPath(string ownerType, string ownerId, string fileType);
        /// <summary>
        /// Get Sub Folder with long
        /// </summary>
        string GetFolderPath(string ownerType, long ownerId, string fileType);
        /// <summary>
        /// Get Sub Folder with int
        /// </summary>
        string GetFolderPath(string ownerType, int ownerId, string fileType);
    }
}
