namespace ChatApi.server.Models.Dtos.Response
{
    public class PageResponseDto<T>
    {
        public int TotalNumber { get; set; }
        public int Count => Data.Count();
        public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();

        public PageResponseDto() { }

        public PageResponseDto(int totalNumber, IEnumerable<T>? data)
        {
            TotalNumber = totalNumber;

            if (data != null)
                Data = data;
        }
    }
}
