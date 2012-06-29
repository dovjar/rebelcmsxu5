[T4Scaffolding.Scaffolder(Description = "Creates a Task")][CmdletBinding()]
param(        
	[parameter(Mandatory = $true, ValueFromPipelineByPropertyName = $true, Position = 0)][string]$TaskClassName,
	[parameter(Mandatory = $true, ValueFromPipelineByPropertyName = $true, Position = 1)][string]$Template,
	[string]$OutputPath = "Tasks",
    [string]$Project,
	[string]$CodeLanguage,
	[string[]]$TemplateFolders,
	[switch]$Force = $false
)

#Generates a unique guid, which is used in the generated class
$guidForTask = [System.Guid]::NewGuid().ToString()
#Current namespace
$namespace = (Get-Project $Project).Properties.Item("DefaultNamespace").Value

$outputFilePath = Join-Path $OutputPath $TaskClassName

# Render the T4 template, adding the output to the Visual Studio project
Add-ProjectItemViaTemplate $outputFilePath -Template $Template -Model @{
	TaskGuid = $guidForTask;
	ClassName = $TaskClassName;
	Namespace = $namespace;
} -SuccessMessage "Added $TaskClassName at '{0}'" -TemplateFolders $TemplateFolders -Project $Project -CodeLanguage $CodeLanguage -Force:$Force