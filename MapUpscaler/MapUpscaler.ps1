using Namespace System.IO

$CurrentPath = (Get-Location).Path + "\"

# $xBRZNetDll = Get-Item($CurrentPath + "\xBRZNet.dll")
# [Reflection.Assembly]::LoadFile($xBRZNetDll.FullName) | Out-Null
# $xBRZScaler = New-Object xBRZNet.xBRZScaler

$InputPath = $CurrentPath + "Input\"
$OutputPath = $CurrentPath + "Output\"
$TempPath = $CurrentPath + "Temp\"

New-Item -ItemType Directory -Force -Path $OutputPath
New-Item -ItemType Directory -Force -Path $TempPath

$files = Get-ChildItem ($InputPath + "*.png")
foreach ($file in $files) {
    $fileName = [Path]::GetFileNameWithoutExtension($file.FullName);

    $resultFile = $OutputPath + $fileName + ".png"
    $xBRZCommand = ".\ScalerTest -4xBRZ " + $file.FullName + " " + $resultFile

    Invoke-Expression $xBRZCommand

    # $alphaFile = $TempPath + $fileName + "_alpha.png"
    # magick.exe $file.FullName -negate -channel-fx a=>r -channel-fx g=0% -channel-fx b=0% -background "rgb(0,255,0)" -flatten $alphaFile
    
    # $Bitmap = New-Object System.Drawing.Bitmap($alphaFile)
    # $Bitmap = $xBRZScaler.ScaleImage($Bitmap, 4)
    
    # $alphaScaledFile = $TempPath + $fileName + "_alphaScaled.png"
    # $Bitmap.Save($alphaScaledFile)

    # # magick.exe $alphaScaledFile -channel r -negate +channel -channel-fx r=>a -channel a -evaluate subtract 20000 -evaluate multiply 3 $alphaScaledFile
    # magick.exe $alphaScaledFile -channel r -negate +channel -channel-fx r=>a $alphaScaledFile
    
    # $Bitmap = New-Object System.Drawing.Bitmap($file.FullName)
    # $Bitmap = $xBRZScaler.ScaleImage($Bitmap, 4)
    
    # $upscaledFile = $TempPath + $fileName + "_upscaled.png"
    # $Bitmap.Save($upscaledFile)
    
    # $resultFile = $OutputPath + $fileName + ".png"
    # magick.exe $upscaledFile $alphaScaledFile -compose CopyOpacity -composite $resultFile
    
    # Remove-Item $pbmFile
    # Remove-Item $svgFile
    # Remove-Item $pngMonoFile
    # Remove-Item $alphaFile
    # Remove-Item $pngBlurFile
    # Remove-Item $pngFilledFile

    $fileName + " done!"
}
Read-Host "Conversion complete! Press enter to close..."