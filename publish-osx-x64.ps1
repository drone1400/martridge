# note, if project was cloned from github, make sure to run this first:
# git submodule update --init --recursive

#publish osx-x64
dotnet clean ./citrus/src/Citrus.Avalonia/Citrus.Avalonia.csproj --configuration Release -p:Platform="Any CPU" --framework netstandard2.0
dotnet clean ./citrus/src/Citrus.Avalonia.DataGrid/Citrus.Avalonia.DataGrid.csproj --configuration Release -p:Platform="Any CPU" --framework netstandard2.0
dotnet clean ./sharpcompress/src/SharpCompress/SharpCompress.csproj --configuration Release -p:Platform="Any CPU" --framework net10.0
dotnet clean ./martridge/src/martridge.csproj --configuration Release -p:Platform="Any CPU" --framework net10.0
dotnet publish ./martridge/src/martridge.csproj --output ./publish/net10.0_osx-x64/martridge/ --configuration Release -p:Platform="Any CPU" --framework net10.0 --runtime osx-x64 --self-contained true -p:PublishSingleFile=true