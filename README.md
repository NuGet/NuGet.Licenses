# NuGet.Licenses

NuGet feed-agnostic license display (or redirect) endpoint. To be used by the NuGet client to present the license text to
the user given the SPDX identifier of the license. The primary website for this app is https://licenses.nuget.org/.

## Updating license text

We fetch the license text from the https://github.com/spdx/license-list-data repository.

Execute the following commands to update the license content. This will also validate that the fetched HTML has expected attributes and CSS class names.

```
.\src\NuGet.Licenses\license-text-extractor.ps1
```

Then make a PR with the changes!

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft trademarks or logos is subject to and must follow Microsoft’s Trademark & Brand Guidelines. Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship. Any use of third-party trademarks or logos are subject to those third-party’s policies.