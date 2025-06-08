using System.Net;

namespace mcqTestBackend.Model
{
    public class ApiResponse
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public bool IsSuccess { get; set; } = true;
        public object Result { get; set; }
        public List<string>? ErrorMessage { get; set; } = new();
    }
}
