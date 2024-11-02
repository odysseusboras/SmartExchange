param(
    [string]$ServiceName = "SmartExchangeService",
    [string]$DisplayName = "Smart Exchange Service",
    [string]$RelativeServiceExePath = ".\bin\Release\net8.0\SmartExchange.exe"
)

# Get the absolute path of the service executable
$ServiceExePath = (Resolve-Path $RelativeServiceExePath).Path

# Function to check if the service exists
function ServiceExists {
    param ([string]$name)
    return Get-Service -Name $name -ErrorAction SilentlyContinue
}

# If the service exists, stop and remove it
if (ServiceExists $ServiceName) {
    Write-Host "Service $ServiceName exists. Stopping and removing..."
    Stop-Service -Name $ServiceName -Force
    sc.exe delete $ServiceName

    # Wait and check if the service was successfully removed
    $maxRetries = 10
    $retryInterval = 5 # seconds
    $retryCount = 0

    while (ServiceExists $ServiceName -and $retryCount -lt $maxRetries) {
        Write-Host "Waiting for service $ServiceName to be removed..."
        Start-Sleep -Seconds $retryInterval
        $retryCount++
    }

    if (ServiceExists $ServiceName) {
        Write-Host "Failed to remove service $ServiceName after $($retryCount * $retryInterval) seconds."
        exit 1
    } else {
        Write-Host "Service $ServiceName successfully removed."
    }
} else {
    Write-Host "Service $ServiceName does not exist."
}

# Install the service with automatic start type
Write-Host "Installing $ServiceName..."
$escapedServiceExePath = "`"$ServiceExePath`""  # Properly escape the path
sc.exe create $ServiceName binPath= $escapedServiceExePath displayname= "$DisplayName" start= auto

# Start the service
Start-Service -Name $ServiceName
Write-Host "$ServiceName started successfully."
