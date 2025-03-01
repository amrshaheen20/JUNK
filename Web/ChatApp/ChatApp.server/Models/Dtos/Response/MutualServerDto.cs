using ChatApi.server.Models.DbSet;

namespace ChatApi.server.Models.Dtos.Response
{
    public class MutualServerDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? ImageId { get; set; }

        public MutualServerDto(string id, string name, string? imageId)
        {
            Id = id;
            Name = name;
            ImageId = imageId;
        }

        public MutualServerDto(Server server)
        {
            Id = server.Id;
            Name = server.Name;
            ImageId = server.ImageId;
        }
    }
}
