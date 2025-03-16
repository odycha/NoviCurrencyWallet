using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NoviCurrencyWallet.Core.Exceptions;
using System.Net;

namespace NoviCurrencyWallet.Core.Middleware;

public class GlobalExceptionHandlingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

	public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"❌Something Went wrong while processing {context.Request.Path}");
			await HandleExceptionAsync(context, ex);
		}
	}

	private Task HandleExceptionAsync(HttpContext context, Exception ex)
	{
		context.Response.ContentType = "application/json";
		HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
		var errorDetails = new ErrorDetails
		{
			ErrorType = "Failure",
			ErrorMessage = ex.Message,
		};

		switch (ex)
		{
			case NotFoundException notFoundException:
				statusCode = HttpStatusCode.NotFound;
				errorDetails.ErrorType = "Not Found";
				break;
			case BadRequestException badRequestException:
				statusCode = HttpStatusCode.BadRequest;
				errorDetails.ErrorType = "Bad Request";
				break;
			case TaskCanceledException when (ex.InnerException is TimeoutException):
				statusCode = HttpStatusCode.RequestTimeout;
				errorDetails.ErrorType = "Timeout";
				errorDetails.ErrorMessage = "The request to the ECB API timed out.";
				break;
			case HttpRequestException:
				statusCode = HttpStatusCode.ServiceUnavailable;
				errorDetails.ErrorType = "External Service Failure";
				errorDetails.ErrorMessage = "Failed to fetch exchange rates from ECB.";
				break;
			default:
				break;
		}

		string response = JsonConvert.SerializeObject(errorDetails);

		//HTTP response being sent back to the client - HttpStatusCode is an enum, it needs to be explicitly cast to an integer
		context.Response.StatusCode = (int)statusCode;

		return context.Response.WriteAsync(response);
	}
}






