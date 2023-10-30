@echo off

REM Laptop
SET PdfSharpCoreRootDir="C:\Users\ja\Documents\GitHub\PdfSharpCore"
SET DestinationDir="C:\Users\ja\Documents\GitHub\FarmOrganizer\localdependencies"

REM PC - TODO

COPY /Y %PdfSharpCoreRootDir%\MigraDocCore.DocumentObjectModel\bin\Release\*.nupkg %DestinationDir%
COPY /Y %PdfSharpCoreRootDir%\MigraDocCore.Rendering\bin\Release\*.nupkg %DestinationDir%
COPY /Y %PdfSharpCoreRootDir%\PdfSharpCore\bin\Release\*.nupkg %DestinationDir%
COPY /Y %PdfSharpCoreRootDir%\PdfSharpCore.Charting\bin\Release\*.nupkg %DestinationDir%

PAUSE