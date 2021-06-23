#$RegbtkPath = "D:\Gamedev\Projects\Tools\REBGTK\REBGTK\"
$RegbtkPath = ".\"
$AlphaChannelsPath = $RegbtkPath + "AlphaChannels\"

$Potrace = ($RegbtkPath + 'potrace\potrace.exe ')

New-Item -ItemType Directory -Force -Path ($AlphaChannelsPath + "Recolorized\")
$RecolorizedPath = ($AlphaChannelsPath + "Recolorized\")

$files = Get-ChildItem ($AlphaChannelsPath + "*.png")
foreach ($file in $files) {
    $fileName = $AlphaChannelsPath + [System.Io.Path]::GetFileNameWithoutExtension($file.FullName);

    magick.exe $file -channel rgb -channel-fx g=>alpha -channel-fx r=0% -channel-fx g=0% -channel-fx b=100% ($RecolorizedPath + $file.Name)

    $fileName + " done!"
}
Read-Host "Conversion complete! Press enter to close..."