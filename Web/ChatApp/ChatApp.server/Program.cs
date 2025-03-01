using ChatApi.server.Context;
using ChatApi.server.Hubs;
using ChatApi.server.Middleware;
using ChatApi.server.Models.DbSet;
using ChatApi.server.Models.Dtos.Response;
using ChatApi.server.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
internal class Program
{
    private static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddDbContext<DataBaseContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

#if DEBUG
        //if you want to kill your server do this :)
        builder.WebHost.ConfigureKestrel(x =>
        {
            x.Limits.MaxRequestBodySize = long.MaxValue;
            x.Limits.MaxRequestBufferSize = long.MaxValue;
        });
#endif

        builder.Services.AddIdentity<Profile, IdentityRole>(o =>
        {
            o.SignIn.RequireConfirmedAccount = true;
            o.User.RequireUniqueEmail = true;
#if DEBUG
            o.Password.RequireDigit = false;
            o.Password.RequireLowercase = false;
            o.Password.RequireUppercase = false;
            o.Password.RequireNonAlphanumeric = false;
#endif
            o.Password.RequiredLength = 8;
            o.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;

        }).AddEntityFrameworkStores<DataBaseContext>().AddDefaultTokenProviders();


        builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromHours(1);
        });


        builder.Services.AddCors(options =>
        {
            options.AddPolicy("MyPolicy",
                              policy =>
                               policy.SetIsOriginAllowed(origin => true)
                              .AllowAnyMethod()
                              .AllowCredentials()
                              .AllowAnyHeader());

        });

        builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
        builder.Services.AddSingleton<EmailSender>();
        builder.Services.AddScoped<AttachmentManger>();
        builder.Services.AddScoped<ChatHub>();
   


#if RELEASE
        builder.Services.AddHostedService<OptimizeFiles>();
#endif


       builder.Services.AddSignalR(hubOptions =>
            {
                hubOptions.EnableDetailedErrors = true;
                //hubOptions.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                //hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(15);
                //hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(10);
                //hubOptions.MaximumParallelInvocationsPerClient = int.MaxValue;
                hubOptions.MaximumParallelInvocationsPerClient = int.MaxValue;
            });


        builder.Services.AddControllers().AddJsonOptions(x =>
                        x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

        builder.Services.AddJwtConfiguration(builder);

        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(ms => ms.Value != null && ms.Value.Errors.Count > 0)
                    .SelectMany(ms => ms.Value!.Errors.Select(error => new FieldError
                    {
                        Field = ms.Key,
                        Error = error.ErrorMessage
                    }))
                    .ToList();

                var response = new ResponseErrorBlock
                {
                    Message = "Request Validation Failed",
                    Errors = errors
                };

                return new BadRequestObjectResult(response);
            };
        });




        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerSupport();

      


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.ConfigObject.AdditionalItems.Add("persistAuthorization", "true");
                c.ConfigObject.AdditionalItems.Add("deepLinking", "true");
            });
        }


        app.UseMiddleware<ExceptionHandlingMiddleware>();


        //app.UseStaticFiles(new StaticFileOptions
        //{
        //    FileProvider = new PhysicalFileProvider(
        //        Path.Combine(Directory.GetCurrentDirectory(), "Upload")),
        //    RequestPath = "/Upload"
        //});

        app.UseHttpsRedirection();

        app.UseCors("MyPolicy");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        // app.UseWebSockets();

        app.MapHub<ChatHub>("api/socket/Chathub", options =>
        {
            //  options.LongPolling.PollTimeout = TimeSpan.FromSeconds(120);
        });


        app.Run();
    }
}

