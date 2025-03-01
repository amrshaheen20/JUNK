using ChatApi.server.Context;
using ChatApi.server.Models.DbSet;
using ChatApi.server.Models.Dtos.Response;
using ChatApi.server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ChatApi.server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttachmentsController : MainControllere
    {
        private readonly UserManager<Profile> userManager;
        private readonly DataBaseContext db;
        private readonly AttachmentManger attachmentManger;

        public AttachmentsController(UserManager<Profile> userManager, DataBaseContext db, AttachmentManger attachmentManger)
        {
            this.userManager = userManager;
            this.db = db;
            this.attachmentManger = attachmentManger;
        }

        //let's say this is not the right way
        private static bool TryParseRange(string rangeHeader, long totalLength, out long start, out long end)
        {
            start = 0;
            end = totalLength - 1;

            if (string.IsNullOrEmpty(rangeHeader) || totalLength <= 0)
                return false;

            var match = System.Text.RegularExpressions.Regex.Match(
                rangeHeader,
                @"^bytes\s*=\s*(\d*)\s*-\s*(\d*)$",
                RegexOptions.IgnoreCase
            );

            if (!match.Success)
                return false;

            string startStr = match.Groups[1].Value.Trim();
            string endStr = match.Groups[2].Value.Trim();

            try
            {
                if (!string.IsNullOrEmpty(startStr) && !string.IsNullOrEmpty(endStr))
                {
                    start = long.Parse(startStr);
                    end = long.Parse(endStr);

                    if (start > end)
                        return false;
                }
                else if (!string.IsNullOrEmpty(startStr))
                {
                    start = long.Parse(startStr);
                    end = Math.Min(totalLength - 1, start + 52428800);//50mb
                }
                else if (!string.IsNullOrEmpty(endStr))
                {
                    long suffix = long.Parse(endStr);
                    start = Math.Max(totalLength - suffix, 0);
                    end = Math.Min(totalLength - 1, start + 52428800);//50mb
                }
                else
                {
                    return false;
                }

                start = Math.Max(start, 0);
                end = Math.Min(end, totalLength - 1);

                if (start >= totalLength || end >= totalLength || start > end)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }



        [AllowAnonymous]
        [HttpGet("{attachment_id}/{file_name?}")]
        public async Task<IActionResult> GetFile(string attachment_id, string? file_name, CancellationToken cancellationToken)
        {
            var fileInfo = await db.Attachments.FirstOrDefaultAsync(x => x.Id == attachment_id, cancellationToken);
            if (fileInfo == null)
                return ERROR(NotFound, "File not found.");

            var filePath = Path.Combine(Constants.GetUploadsFolder(), fileInfo.FilePath);
            if (!System.IO.File.Exists(filePath))
                return ERROR(NotFound, "File not found");


            var stream = await attachmentManger.GetFileAsync(attachment_id, cancellationToken);
            if (stream == null)
            {
                return ERROR(InternalServerError, "An error occurred while retrieving the file.");
            }


            if (Request.Headers.ContainsKey(HeaderNames.Range))
            {
                var fileLength = new FileInfo(filePath).Length;
                var rangeHeader = Request.Headers.Range;

                if (TryParseRange(rangeHeader!, fileLength, out long start, out long end))
                {
                    Response.StatusCode = StatusCodes.Status206PartialContent;
                    Response.Headers.ContentRange = $"bytes {start}-{end}/{fileLength}";
                    Response.Headers.AcceptRanges = "bytes";

                    var buffer = new byte[end - start + 1];
                    using var fileStream = System.IO.File.OpenRead(filePath);
                    fileStream.Seek(start, SeekOrigin.Begin);
                    _ = await fileStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    fileStream.Close();

                    return File(buffer, fileInfo.ContentType, file_name ?? fileInfo.Name);
                }


                Response.Headers.ContentRange = $"bytes */{fileLength}";
                return StatusCode(StatusCodes.Status416RangeNotSatisfiable);
            }



            return File(stream, fileInfo.ContentType, file_name ?? fileInfo.Name);
        }

        [Authorize]
        [HttpPost("upload")]
        //we can do that by another way, split file into buffers and transform them :/
        //[DisableRequestSizeLimit]
        //[RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<ActionResult<AttachmentResponseDto>> UploadFile(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return ERROR(NotFound, "No file uploaded or file is empty.");

            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var uploadedFile = await attachmentManger.UploadFileAsync(file, ProfileId, cancellationToken);

            if (uploadedFile == null)
                return ERROR(InternalServerError, "An error occurred while uploading the file.");

            return Created(new AttachmentResponseDto()
            {
                Id = uploadedFile.Id,
                Name = uploadedFile.Name,
                ContentType = uploadedFile.ContentType
            });

        }

        [Authorize]
        [HttpPost("uploads")]

        public async Task<ActionResult<IEnumerable<AttachmentResponseDto>>> UploadFiles(List<IFormFile> files, CancellationToken cancellationToken)
        {
            if (files == null || files.Count == 0)
                return ERROR(BadRequest, "No files uploaded or files are empty.");


            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var uploadedFiles = await attachmentManger.UploadFilesAsync(files, ProfileId, cancellationToken);

            if (uploadedFiles == null)
                return ERROR(InternalServerError, "An error occurred while uploading the file.");


            return Created(uploadedFiles.Select(a => new AttachmentResponseDto()
            {
                Id = a.Id,
                Name = a.Name,
                ContentType = a.ContentType
            }));

        }

        [Authorize]
        [HttpDelete("{attachment_id}")]
        public async Task<ActionResult<AttachmentResponseDto>> DeleteFile(string attachment_id, CancellationToken cancellationToken)
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var roles = await userManager.GetRolesAsync(await userManager.FindByIdAsync(ProfileId));

            var file = await db.Attachments.FirstOrDefaultAsync(x => x.Id == attachment_id, cancellationToken);
            if (file == null)
                return ERROR(NotFound, "File not found");

            if (file.ProfileId != ProfileId && !ProfileRoles.IsSuperAdmin(roles))
                return ERROR(Unauthorized, "You are not authorized to delete this file.");

            var deletedFile = await attachmentManger.DeleteFileAsync(attachment_id, cancellationToken);
            if (!deletedFile)
            {
                return ERROR(InternalServerError, "An error occurred while deleting the file.");
            }

            return Ok(new AttachmentResponseDto()
            {
                Id = file.Id,
                Name = file.Name,
                ContentType = file.ContentType
            });
        }
    }
}
