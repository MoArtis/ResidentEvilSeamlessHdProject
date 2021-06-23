$InputPath = $CurrentPath
$OutputPath = $CurrentPath + "Output\"

New-Item -ItemType Directory -Force -Path $OutputPath

$files = Get-ChildItem ($InputPath + "*.dds")
foreach ($file in $files) {
	$fileName = [System.Io.Path]::GetFileNameWithoutExtension($file.FullName);
	$pngMonoFile = $OutputPath + $fileName + ".png"
	magick.exe $file.FullName $pngMonoFile
}