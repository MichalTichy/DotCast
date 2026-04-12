var builder = DistributedApplication.CreateBuilder(args);

var dataRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", ".aspire"));
var audiobooksPath = Path.Combine(dataRoot, "audiobooks");
var audiobooksZipPath = Path.Combine(dataRoot, "audiobooks-zip");

Directory.CreateDirectory(audiobooksPath);
Directory.CreateDirectory(audiobooksZipPath);

var database = builder.AddPostgres("postgres")
    .WithDataVolume()
    .AddDatabase("dotcast");

builder.AddProject<Projects.DotCast_App>("dotcast-app", launchProfileName: "DotCast.App")
    .WithReference(database, "DotCast")
    .WaitFor(database)
    .WithEnvironment("StorageOptions__AudioBooksLocation", audiobooksPath)
    .WithEnvironment("StorageOptions__ZippedAudioBooksLocation", audiobooksZipPath)
    .WithEnvironment("UrlBuilderOptions__BaseUrl", "https://localhost:7062")
    .WithEnvironment("PresignedUrlOptions__SecretKey", "development-only-secret-key");

builder.Build().Run();
