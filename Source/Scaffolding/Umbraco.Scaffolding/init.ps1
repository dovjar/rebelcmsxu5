param($rootPath, $toolsPath, $package, $project)

# Bail out if scaffolding is disabled (probably because you're running an incompatible version of T4Scaffolding.dll)
if (-not (Get-Module T4Scaffolding)) { return }

# Enable tab expansion
if (!$global:scaffolderTabExpansion) { $global:scaffolderTabExpansion = @{ } }

# Enable MVC 3 Tools Update "Add Controller" dialog integration
. (Join-Path $toolsPath "registerWithMvcTooling.ps1")

function CountSolutionFilesByExtension($extension) {
	$files = (Get-Project).DTE.Solution `
		| ?{ $_.FileName } `
		| %{ [System.IO.Path]::GetDirectoryName($_.FileName) } `
		| %{ [System.IO.Directory]::EnumerateFiles($_, "*." + $extension, [System.IO.SearchOption]::AllDirectories) }
	($files | Measure-Object).Count
}

# Ensure you've got some default settings for each of the included scaffolders
Set-DefaultScaffolder -Name Model -Scaffolder UmbracoScaffolding.Model -SolutionWide -DoNotOverwriteExistingSetting
Set-DefaultScaffolder -Name SurfaceController -Scaffolder UmbracoScaffolding.SurfaceController -SolutionWide -DoNotOverwriteExistingSetting
Set-DefaultScaffolder -Name SurfaceView -Scaffolder UmbracoScaffolding.SurfaceView -SolutionWide -DoNotOverwriteExistingSetting
Set-DefaultScaffolder -Name ViewModel -Scaffolder UmbracoScaffolding.ViewModel -SolutionWide -DoNotOverwriteExistingSetting
Set-DefaultScaffolder -Name UmbracoView -Scaffolder UmbracoScaffolding.UmbracoViews -SolutionWide -DoNotOverwriteExistingSetting
Set-DefaultScaffolder -Name Task -Scaffolder UmbracoScaffolding.Tasks -SolutionWide -DoNotOverwriteExistingSetting