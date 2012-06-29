[T4Scaffolding.ControllerScaffolder("SurfaceController", Description = "Adds an Umbraco 5 ASP.NET MVC SurfaceController", SupportsModelType = $true, SupportsDataContextType = $true, SupportsViewScaffolder = $true)][CmdletBinding()]
param(
	[parameter(Mandatory = $true, ValueFromPipelineByPropertyName = $true)][string]$ControllerName,        
    [string]$ModelType,
    [string]$Project,
    [string]$CodeLanguage,
	[string]$DbContextType,
	[string]$Area,
	[string]$ViewScaffolder,
	[alias("MasterPage")]$Layout,
 	[alias("ContentPlaceholderIDs")][string[]]$SectionNames,
	[alias("PrimaryContentPlaceholderID")][string]$PrimarySectionName,
	[switch]$ReferenceScriptLibraries = $true,
	[switch]$PartialView = $true,	# Makes this view a PartialView by default
	[string[]]$ModelProperties,
	[string[]]$TemplateFolders,
	[switch]$Force = $false,
	[string]$ForceMode,
	[switch]$NoAnnotations = $true,
	[switch]$NoTypeWarning = $false,
	[switch]$FixUnderscores = $false,
	[switch]$ScaffoldView = $true
)

# If you haven't specified a model type, we'll guess from the controller name
if ($ModelType) {
	$foundModelType = Get-ProjectType $ModelType -Project $Project -ErrorAction SilentlyContinue
	
	if($foundModelType) {
		Write-Host "Found model:" $foundModelType.FullName
	}
} else {
	$ModelType = $ControllerName
	$ExistingViewModel = $ModelType+"ViewModel"
	Write-Host "Looking for an existing $ExistingViewModel"
	$foundModelType = Get-ProjectType $ExistingViewModel -Project $Project -ErrorAction SilentlyContinue

	#if (!$foundModelType) {
	#	$ModelType = [string](Get-SingularizedWord $ModelType)+"ViewModel"
	#	Write-Host "Looking for $ModelType"
	#	$foundModelType = Get-ProjectType $ModelType -Project $Project -ErrorAction SilentlyContinue
	#}
}

$outputPath = "Controllers\SurfaceControllers\"+$ControllerName+"SurfaceController"
$namespace = (Get-Project $Project).Properties.Item("DefaultNamespace").Value
$renderMethodName = "Render" + $ControllerName

if($ModelType) {
	if(!$foundModelType) {
		#If a ModelType was provided, but not found we should the ModelType + ViewModel convention
		$ModelName = $ModelType + "ViewModel"
	} else {
		#If a ModelType was provided we should use that as our ModelName
		$ModelName = $ModelType
	}
} else {
	#If a ModelType was not provided we should default to our ControllerName
	$ModelName = $ControllerName + "ViewModel"
}

if($foundModelType) {
	$PartToReplace = "."+$ModelType
	$ModelFullName = $foundModelType.FullName
	$ModelNamespaceRef = [System.Text.RegularExpressions.Regex]::Replace($ModelFullName, $PartToReplace, "", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
} else {
	$ModelNamespaceRef =  $namespace + ".ViewModels"
}

Add-ProjectItemViaTemplate $outputPath -Template SurfaceControllerTemplate -Model @{ 
	Namespace = $namespace; 
	ControllerName = $ControllerName;
	RenderMethod = $renderMethodName;
	ModelTypeName = $ModelName;
	ModelNamespaceRef = $ModelNamespaceRef;
} -SuccessMessage "Added SurfaceController output at '{0}'" -TemplateFolders $TemplateFolders -Project $Project -CodeLanguage $CodeLanguage -Force:$Force

#If an existing model was found we go ahead with scaffolding the View for our SurfaceController
$controllerNameWithoutSuffix = [System.Text.RegularExpressions.Regex]::Replace($ControllerName, "Controller$", "", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
if ($foundModelType) {
	Scaffold SurfaceView -Controller $controllerNameWithoutSuffix -ViewName $renderMethodName -ModelType $foundModelType.FullName -Area $Area -Layout $Layout -SectionNames $SectionNames -PrimarySectionName $PrimarySectionName -ReferenceScriptLibraries:$ReferenceScriptLibraries -PartialView:$PartialView -Properties $ModelProperties -Project $Project -CodeLanguage $CodeLanguage -Force:$overwriteFilesExceptController -NoAnnotations:$true -NoTypeWarning:$false -FixUnderscores:$false
}

#If no model was found then we should scaffold a ViewModel and View for our SurfaceController with the supplied ModelProperties
if(!$foundModelType) {
	$hasProperties = [bool]$ModelProperties
	
	if($hasProperties) {
		Scaffold ViewModel -Model $ModelName -Properties $ModelProperties -Folder "ViewModels" -ControllerName $controllerNameWithoutSuffix -Project $Project -CodeLanguage $CodeLanguage -TemplateFolders $TemplateFolders -Force:$Force -NoAnnotations:$NoAnnotations -NoTypeWarning:$NoTypeWarning -FixUnderscores:$FixUnderscores -ScaffoldView:$ScaffoldView
	} else {
		Write-Warning "No properties was supplied so the ViewModel and View cannot be Scaffolded."
	}
}