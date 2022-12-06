$openweatherapikey = Get-Content ./private/openweatherkey.txt

$Parameters = @{
    lat=33.44
    lon=-94.04
    appid=$openweatherapikey
}

Invoke-WebRequest -Uri 'https://api.openweathermap.org/data/2.5/weather' -Body $Parameters -Method Get -UseBasicParsing