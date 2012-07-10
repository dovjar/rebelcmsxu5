[T4Scaffolding.Scaffolder(Description = "Creates a ViewModel with properties. You can specify property types or use conventions.")][CmdletBinding()]
param(
	[parameter(Mandatory = $true, ValueFromPipelineByPropertyName = $true)][string]$Model,
	[string[]]$Properties,
	[string]$Folder = "ViewModels",
	[string]$ControllerName,
    [string]$Project,
	[string]$CodeLanguage,
	[string[]]$TemplateFolders,
	[switch]$Force = $false,
	[switch]$NoAnnotations = $true,
	[switch]$NoTypeWarning = $false,
	[switch]$FixUnderscores = $false,
	[switch]$ScaffoldView = $false
)

#Ensure ViewModel naming convetion
$modelNameWithoutSuffix = [System.Text.RegularExpressions.Regex]::Replace($Model, "ViewModel$", "", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
$ViewModelName = $modelNameWithoutSuffix + "ViewModel"

Scaffold Model -Model $ViewModelName -Properties $Properties -Folder $Folder -ControllerName $ControllerName -Project $Project -CodeLanguage $CodeLanguage -TemplateFolders $TemplateFolders -Force:$Force -NoAnnotations:$NoAnnotations -NoTypeWarning:$NoTypeWarning -FixUnderscores:$FixUnderscores -ScaffoldView:$ScaffoldView