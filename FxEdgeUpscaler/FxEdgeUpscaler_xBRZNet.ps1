using Namespace System.IO

$currentPath = (Get-Location).Path + "\"
$inputPath = $currentPath + "Input\"
$outputPath = $currentPath + "Output\"
$tempPath = $currentPath + "Temp\"

$xBRZNetDll = Get-Item($currentPath + "xBRZNet.dll")
[Reflection.Assembly]::LoadFile($xBRZNetDll.FullName) | Out-Null
$xBRZScaler = New-Object xBRZNet.xBRZScaler

New-Item -ItemType Directory -Force -Path $outputPath
New-Item -ItemType Directory -Force -Path $tempPath

$files = Get-ChildItem ($inputPath + "*.png")
foreach ($file in $files) {
    $fileName = [Path]::GetFileNameWithoutExtension($file.FullName);

    $alphaDownscaledFile = $TempPath + $fileName + "_alphaDownscaled.png"
    magick.exe $file.FullName -filter point -resize 25% -negate -channel-fx a=>r -channel-fx g=0% -channel-fx b=0% -background "rgb(0,255,0)" -flatten $alphaDownscaledFile
    
    $bitmap = New-Object System.Drawing.Bitmap($alphaDownscaledFile)
    $bitmap = $xBRZScaler.ScaleImage($bitmap, 4)
    
    $alphaFile = $tempPath + $fileName + "_alpha.png"
    $bitmap.Save($alphaFile)
    $bitmap.Dispose | Out-Null

    magick.exe $alphaFile -channel r -negate -channel-fx r=>a $alphaFile
    
    $blurFile = $TempPath + $fileName + "_blur.png"
    magick.exe $file.FullName -blur 0x1 -alpha off $blurFile

    $filledFile = $TempPath + $fileName + "_Filled.png"
    magick.exe $file.FullName $blurFile -compose Dst_Over -composite $filledFile

    $resultFile = $OutputPath + $fileName + ".png"
    magick.exe $filledFile -background "rgb(0,255,0)" -flatten $alphaFile -compose CopyOpacity -composite $resultFile

    $fileName + " done!"
}

#Remove-Item $tempPath -Recurse -Force

Read-Host "Conversion complete! Press enter to close..."