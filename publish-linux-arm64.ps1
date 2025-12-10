# note, if project was cloned from github, make sure to run this first:
# git submodule update --init --recursive

#publish linux-x64
dotnet clean ./citrus/src/Citrus.Avalonia/Citrus.Avalonia.csproj --configuration Release -p:Platform="Any CPU" --framework netstandard2.0
dotnet clean ./citrus/src/Citrus.Avalonia.DataGrid/Citrus.Avalonia.DataGrid.csproj --configuration Release -p:Platform="Any CPU" --framework netstandard2.0
dotnet clean ./sharpcompress/src/SharpCompress/SharpCompress.csproj --configuration Release -p:Platform="Any CPU" --framework net10.0
dotnet clean ./martridge/src/martridge.csproj --configuration Release -p:Platform="linux-arm64" --framework net10.0
dotnet publish ./martridge/src/martridge.csproj --output ./publish/net10.0_linux-arm64/martridge/ --configuration Release -p:Platform="linux-arm64" --framework net10.0 --runtime linux-arm64 --self-contained true -p:PublishSingleFile=true