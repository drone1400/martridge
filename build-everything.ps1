# note, if project was cloned from github, make sure to run this first:
# git submodule update --init --recursive

#publish win-x86
dotnet clean ./martridge/martridge.sln --configuration Release -p:Platform="x86" --framework net8.0
dotnet publish ./martridge/src/martridge.csproj --output ./publish/net8.0_win-x86/martridge/ --configuration Release -p:Platform="x86" --framework net8.0 --runtime win-x86 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

#publish win-x64
dotnet clean ./martridge/martridge.sln --configuration Release -p:Platform="x64" --framework net8.0
dotnet publish ./martridge/src/martridge.csproj --output ./publish/net8.0_win-x64/martridge/ --configuration Release -p:Platform="x64" --framework net8.0 --runtime win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

#publish linux-x64
dotnet clean ./martridge/martridge.sln --configuration Release -p:Platform="Any CPU" --framework net8.0
dotnet publish ./martridge/src/martridge.csproj --output ./publish/net8.0_linux-x64/martridge/ --configuration Release -p:Platform="Any CPU" --framework net8.0 --runtime linux-x64 --self-contained true -p:PublishSingleFile=true

#publish osx-x64
dotnet clean ./martridge/martridge.sln --configuration Release -p:Platform="Any CPU" --framework net8.0
dotnet publish ./martridge/src/martridge.csproj --output ./publish/net8.0_osx-x64/martridge/ --configuration Release -p:Platform="Any CPU" --framework net8.0 --runtime osx-x64 --self-contained true -p:PublishSingleFile=true

#publish osx-arm64
dotnet clean ./martridge/martridge.sln --configuration Release -p:Platform="Any CPU" --framework net8.0
dotnet publish ./martridge/src/martridge.csproj --output ./publish/net8.0_osx-arm64/martridge/ --configuration Release -p:Platform="Any CPU" --framework net8.0 --runtime osx-arm64 --self-contained true -p:PublishSingleFile=true