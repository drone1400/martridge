# note, if project was cloned from github, make sure to run this first:
# git submodule update --init --recursive

#publish win-x64
dotnet clean ./citrus/src/Citrus.Avalonia/Citrus.Avalonia.csproj --configuration Release -p:Platform="x64" --framework netstandard2.0
dotnet clean ./citrus/src/Citrus.Avalonia.DataGrid/Citrus.Avalonia.DataGrid.csproj --configuration Release -p:Platform="x64" --framework netstandard2.0
dotnet clean ./sharpcompress/src/SharpCompress/SharpCompress.csproj --configuration Release -p:Platform="x64" --framework netstandard2.0
dotnet clean ./martridge/src/martridge.csproj --configuration Release -p:Platform="x64" --framework net8.0
dotnet publish ./martridge/src/martridge.csproj --output ./publish/net8.0_win-x64/martridge/ --configuration Release -p:Platform="x64" --framework net8.0 --runtime win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true