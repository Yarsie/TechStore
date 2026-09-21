using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TechStore.Api
{
    public class RemoveODataContentTypesOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Remove OData-specific content types from all responses
            foreach (var response in operation.Responses.Values)
            {
                var keysToRemove = new List<string>();
                foreach (var contentType in response.Content.Keys)
                {
                    if (contentType.Contains("odata", StringComparison.OrdinalIgnoreCase))
                    {
                        keysToRemove.Add(contentType);
                    }
                }

                foreach (var key in keysToRemove)
                {
                    response.Content.Remove(key);
                }
            }

            // Remove OData content types from request body
            if (operation.RequestBody != null)
            {
                var keysToRemove = new List<string>();
                foreach (var contentType in operation.RequestBody.Content.Keys)
                {
                    if (contentType.Contains("odata", StringComparison.OrdinalIgnoreCase))
                    {
                        keysToRemove.Add(contentType);
                    }
                }

                foreach (var key in keysToRemove)
                {
                    operation.RequestBody.Content.Remove(key);
                }
            }
        }
    }
}
