$wc = New-Object System.Net.WebClient
$exit = 0

$logFileName = "$env:APPVEYOR_BUILD_FOLDER\nunit_core2_results.xml"
dotnet test Tests\LinqToDB.EntityFrameworkCore.Tests\ -f netcoreapp2.0 --logger:"trx;LogFileName=$logFileName" -c Release
if ($LastExitCode -ne 0) { $exit = $LastExitCode }
$wc.UploadFile("https://ci.appveyor.com/api/testresults/mstest/$env:APPVEYOR_JOB_ID", "$logFileName")

$host.SetShouldExit($exit)
