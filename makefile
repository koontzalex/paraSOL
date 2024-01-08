compile:
	csc server.cs

server:
	server.exe

weatherRequest:
	powershell -command "& {Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope Process}; .\openweatherapiget.ps1"

geoRequest:
	powershell -command "& {Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope Process}; .\geoipget.ps1"

rerouteHostFile:
	server.exe RerouteHostFile

spinupServer:
	server.exe SpinupServer

unrouteHostFile:
	server.exe CleanupHostFile