#$RegbtkPath = "D:\Gamedev\Projects\Tools\REBGTK\REBGTK\"
$RegbtkPath = ".\"
$AlphaChannelsPath = $RegbtkPath + "AlphaChannels\"

$Potrace = ($RegbtkPath + 'potrace\potrace.exe ')

New-Item -ItemType Directory -Force -Path ($AlphaChannelsPath + "Vectorized\")
$VectorizedPath = ($AlphaChannelsPath + "Vectorized\")

$files = Get-ChildItem ($AlphaChannelsPath + "*.png")
foreach ($file in $files) 
{
$fileName = $AlphaChannelsPath + [System.Io.Path]::GetFileNameWithoutExtension($file.FullName);

$pbmFile = $fileName + ".pbm"
$svgFile = $fileName + ".svg"
$potraceCommand = $Potrace + $pbmFile + (" -o " + $svgFile)  + " -b svg -r 24 -t 0 -a 0.9 -O 0 -i -C '#0000ff'"

magick.exe $file.FullName $pbmFile
Invoke-Expression $potraceCommand 
magick.exe $svgFile -channel rgb -negate -channel-fx g=>alpha -channel-fx r=0% -channel-fx g=0% -channel-fx b=100% ($VectorizedPath + $file.Name)

Remove-Item $pbmFile
Remove-Item $svgFile

$fileName + " done!"
}
Read-Host "Conversion complete! Press enter to close..."