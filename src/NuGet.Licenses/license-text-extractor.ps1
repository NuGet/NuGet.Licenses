# Copyright (c) .NET Foundation. All rights reserved.
# Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

# This script retrieves the SPDX approved license list from their Github repo (https://github.com/spdx/license-list-data)
# and generates the list of accepted licenses

[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12;

Write-Host "Fetching license IDs..."
$licenseResponseData = Invoke-RestMethod "https://raw.githubusercontent.com/spdx/license-list-data/main/json/licenses.json"
$licenseIds = $licenseResponseData.licenses | Where-Object { (-not $_.isDeprecatedLicenseId) -and ($_.isOsiApproved -or $_.isFsfLibre) } | Select-Object -ExpandProperty licenseId | Sort-Object
Write-Host "Found $($licenseIds.Count) licenses."

# The expression analyzer is a bit sensitive for unknown license exceptions, so we'll use specific source until client gets support.
# The SPDX version that client uses is here: https://github.com/NuGet/NuGet.Client/blob/release-6.4.x/src/NuGet.Core/NuGet.Packaging/Licenses/NuGetLicenseData.cs
# Make note of the release label in the NuGet.Client URL above! This should match the version of NuGet.Packaging that NuGet.Licenses.csproj depends on.
Write-Host "Fetching exception IDs..."
$exceptionResponseData = Invoke-RestMethod "https://raw.githubusercontent.com/spdx/license-list-data/main/json/exceptions.json"
$exceptionIds = $exceptionResponseData.exceptions | Where-Object { -not $_.isDeprecatedLicenseId } | Select-Object -ExpandProperty licenseExceptionId | Sort-Object
Write-Host "Found $($exceptionIds.Count) exceptions."

$outDir = Join-Path $PSScriptRoot "App_Data\Licenses"

if (Test-Path $outDir) {
    Remove-Item -Force -Recurse $outDir
}
New-Item -ItemType Directory $outDir | Out-Null

function Validate-HtmlTags($docId, $content) {
    $html = New-Object -ComObject "HTMLFile"
    $html.IHTMLDocument2_write($content)

    foreach ($el in $html.all) {
        $tagName = $el.tagName
        $attributes = $el.attributes | Where-Object { $_.specified }
        if ($tagName -in ("HTML", "HEAD", "TITLE", "BODY", "P", "LI", "BR")) {
            foreach ($attr in $attributes) {
                Write-Error "'$docId': unexpected $tagName attribute name $($attr.name)"
                exit
            }
        }
        elseif ($tagName -in ("DIV", "VAR", "UL")) {
            foreach ($attr in $attributes) {
                $attrName = $attr.name
                if ($attrName -eq "class") {
                    if ($attr.textContent -in ("optional-license-text", "replaceable-license-text")) {
                        # allowed
                    }
                    else {
                        Write-Error "'$docId': unexpected $tagName attribute value $attrName = $($attr.textContent)"
                        exit
                    }
                }
                elseif ($attrName -eq "style") {
                    if ($attr.textContent -in ("LIST-STYLE-TYPE: none")) {
                        # allowed
                    }
                    else {
                        Write-Error "'$docId': unexpected $tagName attribute value $attrName = $($attr.textContent)"
                        exit
                    }
                }
                else {
                    Write-Error "'$docId': unexpected $tagName) attribute name $attrName"
                    exit
                }
            }
        }
        else {
            Write-Error "'$docId': unexpected tag name $tagName"
            exit
        }
    }
}

### parse all licenses ###
foreach ($licenseId in ($licenseIds)) {
    $licenseDetailsUrl = "https://raw.githubusercontent.com/spdx/license-list-data/main/json/details/$licenseId.json"

    Write-Host "Fetching license '$licenseId'..."
    $licenseDetails = Invoke-RestMethod $licenseDetailsUrl

    Validate-HtmlTags $licenseId $licenseDetails.licenseTextHtml
    if ($licenseDetails.standardLicenseHeaderHtml) {
        Validate-HtmlTags $licenseId $licenseDetails.standardLicenseHeaderHtml
    }

    $details = @{
        isException = $false;
        name = $licenseDetails.name;
        html = $licenseDetails.licenseTextHtml;
        headerHtml = $licenseDetails.standardLicenseHeaderHtml;
        comments = $licenseDetails.licenseComments
    }

    $details | ConvertTo-Json -Depth 100 | Out-File -Encoding utf8 "$outDir\$licenseId.json" -NoClobber;
}

foreach ($exceptionId in ($exceptionIds)) {
    $exceptionUrl = "https://raw.githubusercontent.com/spdx/license-list-data/main/json/exceptions/$exceptionId.json";

    Write-Host "Fetching exception '$exceptionId'..."
    $exceptionDetails = Invoke-RestMethod $exceptionUrl
    
    Validate-HtmlTags $exceptionId $exceptionDetails.exceptionTextHtml

    $details = @{
        isException = $true;
        name = $exceptionDetails.name;
        html = $exceptionDetails.exceptionTextHtml;
        headerHtml = $null;
        comments = $exceptionDetails.licenseComments
    }

    $details | ConvertTo-Json -Depth 100 | Out-File -Encoding utf8 "$outDir\$exceptionId.json" -NoClobber;
}

(Get-Date).ToUniversalTime().ToString("O") | Out-File -Encoding utf8 "$outDir\last-updated.txt" -NoClobber;
