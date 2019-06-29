$CurrentPath = ".\"
$InputPath = $CurrentPath + "Input\"
$OutputPath = $CurrentPath + "Output\"
$TempPath = $CurrentPath + "Temp\"

$Potrace = ($CurrentPath + 'potrace\potrace.exe ')

New-Item -ItemType Directory -Force -Path $OutputPath
New-Item -ItemType Directory -Force -Path $TempPath

$files = Get-ChildItem ($InputPath + "*.png")
foreach ($file in $files) {
    $fileName = [System.Io.Path]::GetFileNameWithoutExtension($file.FullName);

    $pngMonoFile = $TempPath + $fileName + "_mono.png"
    magick.exe $file.FullName -channel RGB -fx blue -background "rgb(0,0,0)"-flatten -filter point -resize 31.25% $pngMonoFile # 25%

    $pbmFile = $TempPath + $fileName + ".pbm"
    $svgFile = $TempPath + $fileName + ".svg"

    # http://potrace.sourceforge.net/potrace.1.html
    $potraceCommand = $Potrace + $pbmFile + (" -o " + $svgFile) + " -b svg -r 30 -t 0 -a 0.8 -O 0 -i -n -u 10 -z ran -C '#0000ff'" # 24
    
    magick.exe $pngMonoFile $pbmFile
    Invoke-Expression $potraceCommand 
    
    $alphaFile = $TempPath + $fileName + "_alpha.png"
    magick.exe $svgFile -channel rgb -negate -channel-fx g=>alpha -channel-fx r=0% -channel-fx g=0% -channel-fx b=100% $alphaFile

    $pngBlurFile = $TempPath + $fileName + "_blur.png"
    magick.exe $file.FullName -blur 0x1 -alpha off $pngBlurFile

    $pngFilledFile = $TempPath + $fileName + "_Filled.png"
    magick.exe $file.FullName $pngBlurFile -compose Dst_Over -composite $pngFilledFile

    magick.exe $pngFilledFile -background "rgb(0,255,0)" -flatten $alphaFile -compose CopyOpacity -composite ($OutputPath + $fileName + ".png")

    Remove-Item $pbmFile
    Remove-Item $svgFile
    Remove-Item $pngMonoFile
    Remove-Item $alphaFile
    Remove-Item $pngBlurFile
    Remove-Item $pngFilledFile

    $fileName + " done!"
}
Read-Host "Conversion complete! Press enter to close..."