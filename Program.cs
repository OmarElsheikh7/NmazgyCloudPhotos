using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

var builder = WebApplication.CreateBuilder(args);

// Allow CORS so other systems can use this API later if needed
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Tell .NET to serve our HTML/CSS from the wwwroot folder
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors();

// Initialize Cloudinary using the keys from appsettings.json
var cloudinaryConfig = builder.Configuration.GetSection("Cloudinary");
var account = new Account(
    cloudinaryConfig["CloudName"],
    cloudinaryConfig["ApiKey"],
    cloudinaryConfig["ApiSecret"]
);
var cloudinary = new Cloudinary(account);

// Create the Upload Route
app.MapPost("/upload", async (IFormFile photo) =>
{
    if (photo == null || photo.Length == 0)
    {
        return Results.BadRequest(new { error = "No image provided" });
    }

    // Stream the file directly to Cloudinary
    using var stream = photo.OpenReadStream();
    var uploadParams = new ImageUploadParams()
    {
        File = new FileDescription(photo.FileName, stream),
        Folder = "Nmazgy"
    };

    try
    {
        var uploadResult = await cloudinary.UploadAsync(uploadParams);

        // Add this check! If Cloudinary rejects it, show the real reason.
        if (uploadResult.Error != null)
        {
            return Results.Problem($"Cloudinary Error: {uploadResult.Error.Message}");
        }

        return Results.Ok(new
        {
            message = "Upload successful!",
            imageUrl = uploadResult.SecureUrl.ToString()
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Failed to upload image: {ex.Message}");
    }
})
.DisableAntiforgery();

app.Run();