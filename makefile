compile:
	csc server.cs

openweatherrequest:
	powershell -command "& {Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope Process}; .\openweatherapiget.ps1"