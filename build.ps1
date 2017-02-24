$ErrorActionPreference = "Stop"

function checkExitCode {
    if ($LastExitCode -ne 0) {
        throw "Non-zero exit code: $LastExitCode"
    }
}

dotnet clean
checkExitCode

dotnet restore
checkExitCode

dotnet build -c Release --version-suffix "dev"
checkExitCode

dotnet test -c Release tests\Freya.Core.Tests\Freya.Core.Tests.fsproj
checkExitCode

dotnet test -c Release tests\Freya.Core.Hopac.Tests\Freya.Core.Hopac.Tests.fsproj
checkExitCode

dotnet pack -c Release --include-symbols --include-source --version-suffix "dev"
