using Microsoft.AspNetCore.Mvc;
using MVC.Models;

namespace MVC.Controllers;

public abstract class BaseController : Controller
{

    protected ApiResponse ModelStateErrorResponse(string? message = null)
    {
        var errors = new Dictionary<string, string>();

        foreach (var key in ModelState.Keys)
        {
            var state = ModelState[key];
            if (state!.Errors.Count > 0)
            {
                errors[key] = string.Join(" ", state.Errors.Select(e => e.ErrorMessage));
            }
        }

        return new ApiResponse
        {
            Message = message!,
            Success = false,
            Errors = errors,
            Notify = true
        };
    }

    protected ApiResponse CreateResponse(bool success, string message = null!,
        Dictionary<string, string> errors = null!, string redirectUrl = null!, bool notify = false)
    {
        return new ApiResponse
        {
            Success = success,
            Message = message,
            Errors = errors,
            RedirectUrl = redirectUrl,
            Notify = notify
        };
    }
}

