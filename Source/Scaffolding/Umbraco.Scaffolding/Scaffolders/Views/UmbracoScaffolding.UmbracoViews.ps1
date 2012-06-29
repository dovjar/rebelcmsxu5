[T4Scaffolding.ViewScaffolder("Razor", Description = "Adds an Umbraco 5 ASP.NET MVC View or Partial View for using the Razor view engine", IsRazorType = $true, LayoutPageFilter = "*.cshtml|*.cshtml")][CmdletBinding()]
param(        
	[parameter(Mandatory = $true, ValueFromPipelineByPropertyName = $true, Position = 0)][string]$Template,
	[parameter(Mandatory = $true, ValueFromPipelineByPropertyName = $true, Position = 1)][string]$ViewName,
	[string]$Folder = "Views",
	[string]$Area,
	[string]$Layout,	# If not set, we'll use the default layout
 	[string[]]$SectionNames,
	[string]$PrimarySectionName,
	[switch]$ReferenceScriptLibraries = $false,
	[switch]$PartialView = $false,
    [string]$Project,
	[string]$CodeLanguage,
	[string[]]$TemplateFolders,
	[switch]$Force = $false
)

$outputFolderName = $Folder

# If we are creating a partial view it should be located in the standard \Views\Partials folder
if($Template -eq "PartialView") {
	$Folder = "Views\Partials"
	$PartialView = $true
}

# Render the T4 template, adding the output to the Visual Studio project
$outputPath = Join-Path $Folder $ViewName
Add-ProjectItemViaTemplate $outputPath -Template $Template -Model @{
	IsContentPage = [bool]$Layout;
	IsPartialView = [bool]$PartialView;
	Layout = $Layout;
	SectionNames = $SectionNames;
	PrimarySectionName = $PrimarySectionName;
	ReferenceScriptLibraries = $ReferenceScriptLibraries.ToBool();
	ViewName = $ViewName;
	PrimaryKeyName = $primaryKeyName;
	ViewDataType = [MarshalByRefObject]$foundModelType;
	ViewDataTypeName = $foundModelType.Name;
	RelatedEntities = $relatedEntities;
} -SuccessMessage "Added $ViewName view at '{0}'" -TemplateFolders $TemplateFolders -Project $Project -CodeLanguage $CodeLanguage -Force:$Force