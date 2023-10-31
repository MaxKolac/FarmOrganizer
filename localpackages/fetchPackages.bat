@echo off

REM Laptop
SET PdfSharpCoreRootDir="C:\Users\ja\Documents\GitHub\PdfSharpCore"
SET DestinationDir="C:\Users\ja\Documents\GitHub\FarmOrganizer\localpackages"

REM PC - TODO

COPY /Y %PdfSharpCoreRootDir%\MigraDocCore.DocumentObjectModel\bin\Release\net7.0\MigraDocCore.DocumentObjectModel.dll %DestinationDir%
COPY /Y %PdfSharpCoreRootDir%\MigraDocCore.Rendering\bin\Release\net7.0\MigraDocCore.Rendering.dll %DestinationDir%
COPY /Y %PdfSharpCoreRootDir%\PdfSharpCore\bin\Release\net7.0\\PdfSharpCore.dll %DestinationDir%
COPY /Y %PdfSharpCoreRootDir%\PdfSharpCore.Charting\bin\Release\net7.0\\PdfSharpCore.Charting.dll %DestinationDir%

PAUSE