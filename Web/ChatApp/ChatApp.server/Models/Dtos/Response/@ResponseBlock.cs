namespace ChatApi.server.Models.Dtos.Response
{
    //public class _ResponseBlock<T>
    //{
    //    public bool Succeed => true;
    //    public string? Message { get; set; }
    //    public T? Data { get; set; }

    //    public _ResponseBlock() { }

    //    public _ResponseBlock(T data)
    //    {
    //        this.Data = data;
    //    }

    //}

    //public class ResponseBlock
    //{
    //    public bool Succeed => true;
    //    public string? Message { get; set; }

    //    public ResponseBlock() { }

    //    public ResponseBlock(string message) 
    //    {
    //        Message = message;
    //    }
    //}

    public class FieldError
    {
        public string Field { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }
    public class ResponseErrorBlock
    {
        public bool Succeed => false;
        public string? Message { get; set; }
        public List<FieldError> Errors { get; set; } = new();

        public ResponseErrorBlock() { }

        public ResponseErrorBlock(string message)
        {
            Message = message;
        }

        public ResponseErrorBlock(string message, List<FieldError> errors)
        {
            Message = message;
            Errors = errors;
        }
    }


}
