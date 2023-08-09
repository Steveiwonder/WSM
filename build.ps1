Write-Host "Building WSM.Server"
$verison = $args[0]
Write-Host "Version number $version"
docker build -t steveiwonder/wsm.server:$version -t steveiwonder/wsm.server:latest --no-cache --progress=plain -f dockerfile.server .
Write-Host "Publishing WSM.Client"
dotnet publish "./WSM.Client/WSM.Client.csproj" -o ./build/wsm.client/ -c Debug
Write-Host "Removing client appsettings.json"
Remove-Item -Path ./build/wsm.client/appsettings.json -ErrorAction Ignore
Write-Host "Copying install-service.ps1"
Copy-Item -Path ./WSM.Client/install-service.ps1 ./build/wsm.client/install-service.ps1
Write-Host "Copying uninstall-service.ps1"
Copy-Item -Path ./WSM.Client/uninstall-service.ps1 ./build/wsm.client/uninstall-service.ps1
Write-Host "Creating wsm.client.zip"
Compress-Archive -Path ./build/wsm.client -DestinationPath ./build/wsm.client.zip -Force
Write-Host "Done"
