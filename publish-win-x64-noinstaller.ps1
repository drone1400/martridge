# note, if project was cloned from github, make sure to run this first:
# git submodule update --init --recursive

#publish win-x64
dotnet clean ./citrus/src/Citrus.Avalonia/Citrus.Avalonia.csproj --configuration Release -p:Platform="x64" --framework netstandard2.0
dotnet clean ./citrus/src/Citrus.Avalonia.DataGrid/Citrus.Avalonia.DataGrid.csproj --configuration Release -p:Platform="x64" --framework netstandard2.0
dotnet clean ./sharpcompress/src/SharpCompress/SharpCompress.csproj --configuration Release -p:Platform="x64" --framework netstandard2.0
dotnet clean ./martridge/src/martridge.csproj --configuration Release -p:Platform="x64" --framework net10.0
dotnet publish ./martridge/src/martridge.csproj --output ./publish/net10.0_win-x64-noinstaller/martridge/ --configuration Release -p:Platform="x64" --framework net10.0 --runtime win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DisableFeatureDinkInstaller=true