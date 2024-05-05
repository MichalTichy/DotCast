# Use the official ASP.NET Core runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the pre-built application artifacts from the build environment to the container
COPY ./ .

# Set the environment variable to your application's DLL
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "DotCast.App.dll"]