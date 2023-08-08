docker build -t steveiwonder/wsm.server --no-cache --progress=plain -f dockerfile.server .
dotnet publish "./WSM.Client/WSM.Client.csproj" -o ./build/wsm.client/ -c Debug
Remove-Item -Path ./build/wsm.client/appsettings.json -ErrorAction Ignore
Copy-Item -Path ./WSM.Client/install-service.ps1 ./build/wsm.client/install-service.ps1
Copy-Item -Path ./WSM.Client/uninstall-service.ps1 ./build/wsm.client/uninstall-service.ps1
Compress-Archive -Path ./build/wsm.client -DestinationPath ./build/wsm.client.zip -Force