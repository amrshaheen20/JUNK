using ChatApi.server.Models.Dtos.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace ChatApi.server.Services
{
    public static class Swagger
    {
        public class AuthResponsesOperationFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                var declaringType = context.MethodInfo.DeclaringType;
                var methodAttributes = context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>();

                IEnumerable<AuthorizeAttribute> classAttributes = declaringType != null
                    ? declaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>()
                    : Enumerable.Empty<AuthorizeAttribute>();

                var authAttributes = methodAttributes.Union(classAttributes);

                //Console.WriteLine($"Checking authorization for {context.MethodInfo.Name}");
                //foreach (var attr in authAttributes)
                //{
                //    Console.WriteLine($"Found AuthorizeAttribute: {attr}");
                //}

                if (authAttributes.Any())
                {
                    operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });

                    var securityScheme = new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    };

                    operation.Security = new List<OpenApiSecurityRequirement>
                    {
                        new OpenApiSecurityRequirement
                        {
                            [securityScheme] = new List<string>()
                        }
                    };
                }


                if (context.MethodInfo.GetParameters().Length > 0)
                {
                    operation.Responses.TryAdd("400", new OpenApiResponse
                    {
                        Description = "Bad Request",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new OpenApiMediaType
                            {
                                Schema = context.SchemaGenerator.GenerateSchema(typeof(ResponseErrorBlock), context.SchemaRepository)
                            }
                        }
                    });
                }

            }

        }



        public static void AddSwaggerSupport(this IServiceCollection services)
        {


            services.AddSwaggerGen(swagger =>
            {
                swagger.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = Assembly.GetExecutingAssembly().GetName().Name,
                    Description = ""
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                swagger.IncludeXmlComments(xmlPath);

                // To Enable authorization using Swagger (JWT)    
                swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
                });
                //swagger.AddSecurityRequirement(new OpenApiSecurityRequirement()
                //{
                //    {
                //         new OpenApiSecurityScheme()
                //         {
                //             Reference = new OpenApiReference
                //             {
                //                 Type = ReferenceType.SecurityScheme,
                //                 Id = "Bearer"
                //             }
                //         },
                //         Array.Empty<string>()
                //    }
                //});


                swagger.AddSignalRSwaggerGen();



                swagger.TagActionsBy(api =>
                {
                    if (api.GroupName != null)
                    {
                        return new[] { api.GroupName };
                    }

                    if (api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                    {
                        return new[] { controllerActionDescriptor.ControllerName };
                    }

                    throw new InvalidOperationException("Unable to determine tag for endpoint.");
                });

                swagger.DocInclusionPredicate((name, api) => true);
                swagger.OperationFilter<AuthResponsesOperationFilter>();

            });
        }









    }
}
