using Grpc.Core;

public class GrpcErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public GrpcErrorHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if the request is a gRPC request
        if (context.Request.ContentType != "application/grpc")
        {
            await _next(context);
            return;
        }

        try
        {
            await _next(context);
        }
        catch (RpcException rpcEx)
        {
            // Handle gRPC exceptions
            Console.WriteLine($"gRPC Exception: {rpcEx.StatusCode} - {rpcEx.Message}");
            // You can also log this to your logging infrastructure here
            // Optionally, modify the response or rethrow the exception
            // throw;
        }
        catch (Exception ex)
        {
            // Handle other exceptions
            Console.WriteLine($"Exception in gRPC request: {ex.Message}");
            // Log the exception and return a gRPC error status
            // throw new RpcException(new Status(StatusCode.Internal, "An unexpected error occurred"));
        }
    }
}
