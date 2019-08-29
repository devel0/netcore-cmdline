#!/bin/bash

exdir=$(dirname `readlink -f "$0"`)

dotnet build /p:CopyLocalLockFileAssemblies=true
docpal --proptable --mgtable --mgspace -out "$exdir"/tmp-api-doc "$exdir"/netcore-cmdline/bin/Debug/netstandard2.1/netcore-cmdline.dll

rm -fr "$exdir"/doc/api
mkdir -p "$exdir"/doc/api
mv "$exdir"/tmp-api-doc/docs/SearchAThing "$exdir"/doc/api
