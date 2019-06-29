using Namespace System.IO

$currentPath = (Get-Location).Path + "\"

$upscalePath = $currentPath + "Upscaled\"
$originalPath = $currentPath + "Original\"

$outputPath = $currentPath + "Output\"
$tempPath = $currentPath + "Temp\"

New-Item -ItemType Directory -Force -Path $outputPath
New-Item -ItemType Directory -Force -Path $tempPath

$upscaledFiles = Get-ChildItem ($upscalePath + "*.png")
foreach ($upscaledFile in $upscaledFiles) {

    $filename = [Path]::GetFileNameWithoutExtension($upscaledFile);

    $originalFile = Get-Item ($originalPath + $filename + ".png")

    $alphaFile = $tempPath + $fileName + "_alpha.png"
    $xBRZCommand = ".\ScalerTest -4xBRZ " + $originalFile + " " + $alphaFile

    $blurFile = $TempPath + $fileName + "_blur.png"
    $filledFile = $TempPath + $fileName + "_Filled.png"
    $resultFile = $OutputPath + $fileName + ".png"
    
    Invoke-Expression $xBRZCommand | 
    magick.exe $upscaledFile.FullName -blur 0x1 -alpha off $blurFile |
    magick.exe $upscaledFile.FullName $blurFile -compose Dst_Over -composite $filledFile |
    magick.exe $filledFile -background "rgb(0,255,0)" -flatten $alphaFile -compose CopyOpacity -composite $resultFile

    $fileName + " done!"
}

#Remove-Item $tempPath -Recurse -Force

Read-Host "Conversion complete! Press enter to close..."