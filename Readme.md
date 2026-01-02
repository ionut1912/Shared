publish a new version of lib

dotnet build -c Release

publish

dotnet nuget push "bin/Release/YourPackage.version.nupkg" --api-key YOUR_GITHUB_PAT --source "github"
