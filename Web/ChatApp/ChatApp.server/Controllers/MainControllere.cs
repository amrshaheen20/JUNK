using ChatApi.server.Models.Dtos.Response;
using Microsoft.AspNetCore.Mvc;

namespace ChatApi.server.Controllers
{
    //[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public class MainControllere : ControllerBase
    {
        [NonAction]
        public virtual ActionResult Created(object? value) => StatusCode(StatusCodes.Status201Created, value);


        [NonAction]
        public virtual ActionResult Created() => StatusCode(StatusCodes.Status201Created);

        [NonAction]
        public virtual ActionResult Forbid(object? value) => StatusCode(StatusCodes.Status403Forbidden, value);

        [NonAction]
        public virtual ActionResult InternalServerError(object? value) => StatusCode(StatusCodes.Status500InternalServerError, value);

        [NonAction]
        public virtual ActionResult InternalServerError() => StatusCode(StatusCodes.Status500InternalServerError);

        [NonAction]
        public virtual ActionResult ERROR(Func<object, ActionResult> func, string message)
        {
            return func(new ResponseErrorBlock(message));
        }
    }
}
