[T4Scaffolding.Scaffolder(Description = "Scaffolder for an Rebel Property Editor")][CmdletBinding()]
param(        
	[parameter(Mandatory = $true, ValueFromPipelineByPropertyName = $true)][string]$Name,
	[parameter(Mandatory = $true, ValueFromPipelineByPropertyName = $true)][string]$Alias,
	[switch]$IsParameterEditor,
	[switch]$HasPreValueEditor,
	[switch]$UseCustomEditorView,
	[switch]$UseCustomPreValuesView,
    [string]$Project,
	[string]$CodeLanguage,
	[string[]]$TemplateFolders,
	[switch]$Force = $false
)

# TODO: Need to add validation to the PropertyEditorAlias value as it can only be alphanumeric, no spaces, start with a capital, etc...

$propertyEditorPath = '{0}\{0}Editor' -f $Alias
$editorModelPath = '{0}\{0}EditorModel' -f $Alias
$preValueModelPath = '{0}\{0}PreValueModel' -f $Alias
$editorViewPath = 'Views\EditorTemplates\{0}EditorModel' -f $Alias
$preValuesViewPath = 'Views\EditorTemplates\{0}PreValueModel' -f $Alias

$namespace = (Get-Project $Project).Properties.Item("DefaultNamespace").Value
$assemblyName = (Get-Project $Project).Properties.Item("AssemblyName").Value

Add-ProjectItemViaTemplate "Views\Web" -Template RazorWebConfig `
	-Model @{ PropertyEditorAlias = $Alias; } `
	-SuccessMessage "Added razor web.config output at {0}" `
	-TemplateFolders $TemplateFolders -Project $Project -CodeLanguage "config" -Force:$Force

Add-ProjectItemViaTemplate $propertyEditorPath -Template PropertyEditorTemplate `
	-Model @{ Namespace = $namespace; PropertyEditorAlias = $Alias; PropertyEditorName = $Name; IsParameterEditor = $IsParameterEditor.IsPresent; HasPreValueEditor = $HasPreValueEditor.IsPresent } `
	-SuccessMessage "Added Property Editor output at {0}" `
	-TemplateFolders $TemplateFolders -Project $Project -CodeLanguage $CodeLanguage -Force:$Force

Add-ProjectItemViaTemplate $editorModelPath -Template EditorModelTemplate `
	-Model @{ Namespace = $namespace; PropertyEditorAlias = $Alias; HasPreValueEditor = $HasPreValueEditor.IsPresent; UseCustomEditorView = $UseCustomEditorView.IsPresent; AssemblyName = $AssemblyName } `
	-SuccessMessage "Added Editor Model output at {0}" `
	-TemplateFolders $TemplateFolders -Project $Project -CodeLanguage $CodeLanguage -Force:$Force

if ($HasPreValueEditor.IsPresent) 
{
	Add-ProjectItemViaTemplate $preValueModelPath -Template PreValueModelTemplate `
	-Model @{ Namespace = $namespace; PropertyEditorAlias = $Alias; UseCustomPreValuesView = $UseCustomPreValuesView.IsPresent; AssemblyName = $AssemblyName } `
	-SuccessMessage "Added Pre Value Model output at {0}" `
	-TemplateFolders $TemplateFolders -Project $Project -CodeLanguage $CodeLanguage -Force:$Force
}

if ($UseCustomEditorView.IsPresent) 
{
	Add-ProjectItemViaTemplate $editorViewPath -Template EditorViewTemplate `
	-Model @{ Namespace = $namespace; PropertyEditorAlias = $Alias; } `
	-SuccessMessage "Added Editor Model View output at {0}" `
	-TemplateFolders $TemplateFolders -Project $Project -CodeLanguage $CodeLanguage -Force:$Force
}

if ($UseCustomPreValuesView.IsPresent) 
{
	Add-ProjectItemViaTemplate $preValuesViewPath -Template PreValuesViewTemplate `
	-Model @{ Namespace = $namespace; PropertyEditorAlias = $Alias; } `
	-SuccessMessage "Added Pre Value View output at {0}" `
	-TemplateFolders $TemplateFolders -Project $Project -CodeLanguage $CodeLanguage -Force:$Force
}