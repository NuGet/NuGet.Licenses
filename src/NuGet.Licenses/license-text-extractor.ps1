# Copyright (c) .NET Foundation. All rights reserved.
# Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

# This script retrieves the SPDX approved license list from their Github repo (https://github.com/spdx/license-list-data)
# and generates the list of accepted licenses

[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12;

$licenseResponseData = Invoke-RestMethod "https://raw.githubusercontent.com/spdx/license-list-data/master/json/licenses.json"
$licenseIds = $licenseResponseData.licenses | Where-Object {(-not $_.isDeprecatedLicenseId) -and ($_.isOsiApproved -or $_.isFsfLibre) } | Select-Object -ExpandProperty licenseId | Sort-Object
$deprecatedLicenseIds = $licenseResponseData.licenses | Where-Object {($_.isDeprecatedLicenseId)} | Select-Object -ExpandProperty licenseId | Sort-Object

# expression analyzer is a bit sensitive for unknown license exceptions, so we'll use specific source until client gets support.
# $exceptionResponseData = Invoke-RestMethod "https://raw.githubusercontent.com/spdx/license-list-data/master/json/exceptions.json"
$exceptionResponseData = Invoke-RestMethod "https://raw.githubusercontent.com/spdx/license-list-data/45d10da0366f5fa931f60f3931fd23d5fb708de5/json/exceptions.json"
$exceptionIds = $exceptionResponseData.exceptions | Where-Object {-not $_.isDeprecatedLicenseId} | Select-Object -ExpandProperty licenseExceptionId | Sort-Object
$deprecatedExceptionIds = $exceptionResponseData.exceptions | Where-Object {$_.isDeprecatedLicenseId} | Select-Object -ExpandProperty licenseExceptionId | Sort-Object

# placeholder for "<<var>>" items
$placeholder = "_" * 5

function Remove-TemplateTokens($LicenseTemplate)
{
    $temp = $LicenseTemplate -replace "<<beginOptional>>|<<endOptional>>",""
    $output = "";
    $varStart = -1;

    for ($i = 0; $i -lt $temp.Length; ++$i)
    {
        if ($temp.IndexOf("<<var;", $i) -eq $i)
        {
            $varStart = $i;
            continue;
        }
        if (($varStart -ge 0) -and ($temp.IndexOf(">>", $i) -eq $i))
        {
          $varBody = $temp.Substring($varStart + 2, $i - $varStart - 2);
          
          $i += 1;

          $varElements = $varBody -split ";";

          $nameElement = $varElements | Where-Object { $_.StartsWith("name=") };

          $name = $nameElement.Substring(6, $nameElement.IndexOf("`"", 6) - 6);

          if ($name -eq "bullet")
          {
            $originalElement = $varElements | Where-Object {$_.StartsWith("original=")}
            $original = $originalElement.Substring(10, $originalElement.IndexOf("`"", 10) - 10);
            $output += $original
          } else
          {
            $output += $placeholder
          }

          $varStart = -1;
          continue;
        }
        if ($varStart -ge 0)
        {
            continue;
        }

        $output += $temp.Substring($i, 1);
    }

    return $output;
}

function New-LicenseClass($LicenseList, $ExceptionList)
{
    "// "
    ""
    "namespace NuGet.Licenses"
    "{"
    "    public static class LicenseData"
    "    {"
    "        public static Dictionary<string, string> Licenses = new Dictionary<string, string>"
    "        {"
    foreach ($license in $LicenseList)
    {
        $licenseText = $license.LicenseText;
        $licenseText = $licenseText.Replace("`r", "\r");
        $licenseText = $licenseText.Replace("`n", "\n");
        $licenseText = $licenseText.Replace("`"", "\`"");
        $licenseText = $licenseText.Replace("`t", "\t");
    "            {`"$($license.LicenseId)`", `"$($licenseText)`"},"
    }
    "        };"
    ""
    "        public static Dictionary<string, string> Exceptions = new Dictionary<string, string>"
    "        {"
    foreach ($exception in $ExceptionList)
    {
        $exceptionText = $exception.ExceptionText;
        $exceptionText = $exceptionText.Replace("`r","\r");
        $exceptionText = $exceptionText.Replace("`n","\n");
        $exceptionText = $exceptionText.Replace("`"", "\`"");
        $exceptionText = $exceptionText.Replace("`t", "\t");
    "            {`"$($exception.ExceptionId)`", `"$($exceptionText)`"},"
    }
    "        };"
    "    }"
    "}"
}

$Licenses = new-object System.Collections.ArrayList;
$Exceptions = new-object System.Collections.ArrayList;

$outDir = "Licenses"

new-item -ItemType Directory -Force $outDir

### parse all licenses ###
foreach ($licenseId in ($licenseIds))
{
    $licenseDetailsUrl = "https://raw.githubusercontent.com/spdx/license-list-data/master/json/details/$licenseId.json"

    $licenseDetails = Invoke-RestMethod $licenseDetailsUrl

    $licenseTemplate = $licenseDetails.standardLicenseTemplate

    $transformed = Remove-TemplateTokens -LicenseTemplate $licenseTemplate

    $transformed | Out-File -Encoding utf8 "$outDir\$licenseId.txt";
}

foreach ($exceptionId in ($exceptionIds))
{
    $exceptionUrl = "https://raw.githubusercontent.com/spdx/license-list-data/master/json/exceptions/$exceptionId.json";

    $exceptionDetails = Invoke-RestMethod $exceptionUrl

    $exceptionText = $exceptionDetails.licenseExceptionText;

    $exceptionText | Out-File -Encoding utf8 "$outDir\$exceptionId.txt";
}


#New-LicenseClass $Licenses $Exceptions | Out-File -Encoding utf8 "C:\Users\zhaoyu\Desktop\licenses.cs"