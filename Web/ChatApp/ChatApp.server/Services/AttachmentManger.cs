using ChatApi.server.Context;
using ChatApi.server.Models.DbSet;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace ChatApi.server.Services
{
    public class AttachmentManger
    {
        private readonly DataBaseContext db;

        public AttachmentManger(DataBaseContext db)
        {
            this.db = db;
        }

        public async Task<Stream?> GetFileAsync(string attachment_id, CancellationToken cancellationToken)
        {
            var fileInfo = await db.Attachments.FirstOrDefaultAsync(x => x.Id == attachment_id, cancellationToken);
            if (fileInfo == null)
                return null;

            var filePath = Path.Combine(Constants.GetUploadsFolder(), fileInfo.FilePath);
            if (!File.Exists(filePath))
                return null;


            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }

        public async Task<Attachment?> UploadFileAsync(IFormFile? file, string profileId, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return null;


            try
            {
                Attachment attachment = new();

                var uploadsFolder = Constants.GetUploadsFolder();
                var fileName = attachment.Id;
                var filePath = Path.Combine(uploadsFolder, fileName);

                var md5Hash = await FileHasher.ComputeHashAsync(file.OpenReadStream(), HashAlgorithmName.MD5);

                var existingFile = await db.Attachments.FirstOrDefaultAsync(x => x.Md5Hash == md5Hash, cancellationToken);

                var FileIsExist = existingFile != null && File.Exists(Path.Combine(uploadsFolder, existingFile.FilePath));


                if (existingFile != null && FileIsExist)
                {
                    attachment.ProfileId = profileId;
                    attachment.Name = file.FileName;
                    attachment.FilePath = existingFile.FilePath;
                    attachment.ContentType = existingFile.ContentType;
                    attachment.Length = existingFile.Length;
                    attachment.Md5Hash = existingFile.Md5Hash;

                }
                else
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await file.CopyToAsync(stream, cancellationToken);

                    attachment.ProfileId = profileId;
                    attachment.Name = file.FileName;
                    attachment.FilePath = fileName;
                    //TODO: Need to correct this
                    attachment.ContentType = file.ContentType;
                    attachment.Length = file.Length;
                    attachment.Md5Hash = md5Hash;
                }


                db.Attachments.Add(attachment);
                await db.SaveChangesAsync(cancellationToken);

                return attachment;
            }
            catch
            {
                return null;
            }
        }

        public async Task<ICollection<Attachment>?> UploadFilesAsync(ICollection<IFormFile>? files, string profileId, CancellationToken cancellationToken)
        {

            if (files == null || files.Count == 0 || files.Sum(x => x.Length) == 0)
                return null;
            using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var attachments = new List<Attachment>();

                foreach (var file in files)
                {
                    var attachment = await UploadFileAsync(file, profileId, cancellationToken);
                    if (attachment != null)
                        attachments.Add(attachment);
                }

                await transaction.CommitAsync(cancellationToken);
                return attachments.ToArray();
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                return null;
            }
        }

        public async Task<bool> DeleteFileAsync(string attachmentId, CancellationToken cancellationToken)
        {
            try
            {
                var uploadFolder = Constants.GetUploadsFolder();
                var file = await db.Attachments.FirstOrDefaultAsync(x => x.Id == attachmentId, cancellationToken);
                if (file == null)
                    return true;

                var NotDuplicate = db.Attachments.Where(x => x.Md5Hash == file.Md5Hash).Count() == 1;

                if (NotDuplicate)
                {
                    var filePath = Path.Combine(uploadFolder, file.FilePath);
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }

                db.Attachments.Remove(file);
                await db.SaveChangesAsync(cancellationToken);


                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
