# Hit the root

$base = 'http://liveqna.alexandrebrisebois.com' # 'http://localhost:8080' #

$result = Invoke-RestMethod -Method Get -Uri $($base + '/api')

$root = $result

# register

$Body = @{
    name = "Alexandre Brisebois"
    email = "alexandre.brisebois@microsoft.com"
} | ConvertTo-Json 

$result = Invoke-RestMethod -Method Post -Uri $($base + $root.links[0].uri) -Body $Body -ContentType 'application/json'

$registered = $result 

# create session

$Body = @{
    name = "A Practical Overview of Actors in Service Fabric"
    start = Get-Date -Date '2016/07/06 09:00:00' -Format 'yyyy/MM/dd HH:mm:ss'
    end = Get-Date -Date '2016/07/06 10:00:00' -Format 'yyyy/MM/dd HH:mm:ss'
} | ConvertTo-Json

$result = Invoke-RestMethod -Method Post -Uri $($base + $registered.links[0].uri) -Body $Body -ContentType 'application/json'

$created = $result 

# list sessions

$result = Invoke-RestMethod -Method Get -Uri $($base + $registered.links[0].uri)

$sessions = $result

# view session details

$result = Invoke-RestMethod -Method Get -Uri $($base + $created.links[1].uri)

$session = $result

# register attendee

$Body = @{
    name = "Maxime Rouiller"
    email = "brisebois@outlook.com"
} | ConvertTo-Json 

$aresult = Invoke-RestMethod -Method Post -Uri $($base + $root.links[0].uri) -Body $Body -ContentType 'application/json'

$attendee = $aresult 

# list sessions

$aresult = Invoke-RestMethod -Method Get -Uri $($base + $attendee.links[0].uri)

$asessions = $aresult

# join session

$aresult = Invoke-RestMethod -Method Post -Uri $($base + $asessions.sessions[0].links[0].uri)

$joinedSession = $aresult

$joinedSession

# view session

$aresult = Invoke-RestMethod -Method Get -Uri $($base + $joinedSession.links[0].uri)

$asession = $aresult
$asession

$s = Invoke-RestMethod -Method Post -Uri $($base + '/api/4380ef93-f823-51a8-8a3e-a74351a293ad/sessions/-9077483017110054082/start')
