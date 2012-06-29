[T4Scaffolding.Scaffolder(Description = "Creates a Model with properties. You can specify property types or use conventions.")][CmdletBinding()]
param(
	[parameter(Mandatory = $true, ValueFromPipelineByPropertyName = $true)][string]$Model,
	[string[]]$Properties,
	[string]$Folder = "Models",
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

# Parses names like Name[99]? to {Name="Name"; MaxLength=99; Required=$false}
function ParseName([string]$name) {
	$result = @{Name = $name; MaxLength = 0; Required = $true; Type = ""; Reference=""}
	# parse reference if any
	if ($result.Name.EndsWith("+")) {
		$result.Name = $result.Name.Substring(0, $result.Name.Length - 1)
		$result.Reference = "!";
	}
	
	# parse nullable if any
	if ($result.Name.EndsWith("?"))	{
		$result.Name = $result.Name.Substring(0, $result.Name.Length - 1)
		$result.Required = $false;
	}
	
	[int]$start = 0
	# parse length if any
	if ($result.Name.EndsWith("]")) {
		$start = $result.Name.IndexOf("[")
		if ($start -gt 0) {
			$lengthPart = $result.Name.Substring($start + 1, $result.Name.Length - $start - 2)
			$result.MaxLength = [System.Convert]::ToInt32($lengthPart)
			$result.Name = $result.Name.Substring(0, $start)
		}
	}
	# parse type if any
	$start = $result.Name.IndexOf(":")
	if ($start -gt 0) {
		$result.Type = $result.Name.Substring($start + 1, $result.Name.Length - $start - 1)
		$result.Name = $result.Name.Substring(0, $start)
	}
	
	if ($result.Reference) {
		if ($result.Name -imatch '^.*id$') {
			$result.Reference = $result.Name.Substring(0, $result.Name.Length-2)
			if ($result.Reference.EndsWith("_")) {
				$result.Reference = $result.Name.Substring(0, $result.Name.Length-1)
			}
		}
		else {
			$result.Reference = ""
			Write-Warning "Cannot extract reference property for $name"
		}	
	}
	
	($result)
}

$patterns = @()
$defaultProjectLanguage = Get-ProjectLanguage

try { 
	$patternsFile = "ModelPatterns." + $defaultProjectLanguage + ".t4"
	$patternsPath = Join-Path $TemplateFolders[0] $patternsFile
	Write-Verbose "Trying to load $patternsFile ..."
	
	Get-Content $patternsPath | Foreach-Object { 
		$items = $_.Split(' ')
		$type = $items[0]
		Write-Verbose "Processing pattern type: $type"

		$typeInfo = ParseName($type)

		if ($items.Length -gt 1) {
			for ($i = 1; $i -lt $items.Length; $i++) {
				$patterns += @{ Type = $typeInfo.Name; Pattern = '^' + $items[$i] + '$'; MaxLength = $typeInfo.MaxLength; Reference = $typeInfo.Reference }
				# Write-Verbose "	Processed pattern: $($items[$i])"
			}
		}
	}
}
catch { Write-Warning "Model patterns was not loaded: $($_.Exception.Message)" }

$defaultSpace = (Get-Project $Project).Properties.Item("DefaultNamespace").Value

if ($Properties -eq $null) {$Properties = @("Id", "Name")}

if (!$Folder) {
	$outputPath = $Model
	$space = $defaultSpace
}
else {
	$outputPath = Join-Path $Folder $Model
	$space = $defaultSpace + "." + $Folder.Replace("\", ".")
}

$props = @()
[int]$typedCount = 0

foreach ($property in $Properties) {
	$nameInfo = ParseName($property)
	$type = $nameInfo.Type
	
	# try to find some attributes from ModelPatterns
	if ($type.Length -eq 0) {
		for ($i = 0; $i -lt $patterns.Length; $i++) {
			$p = $patterns[$i]
			if ($nameInfo.Name -cmatch $p.Pattern) {
				$type = $p.Type
				if ($nameInfo.MaxLength -eq 0 ) { $nameInfo.MaxLength = $p.MaxLength }
				if (!$nameInfo.Reference) { $nameInfo.Reference = $p.Reference }
				break
			}
		}
	}
	else {
		$typedCount++
	}

	if (!$type) { $type = "string" }
	
	# create reference class if not any
	$referenceType = ""
	if ($nameInfo.Reference) {
		$reference = Get-ProjectType $nameInfo.Reference 2>null
		
		if (!$reference) {
			$idType = $nameInfo.Type.ToLower()
			Scaffold TypeScaffolding.Type -Model $nameInfo.Reference Id:$idType,Name -Folder $Folder -Project $Project -CodeLanguage $CodeLanguage `
				-Force:$Force -NoAnnotations:$NoAnnotations -NoTypeWarning
			$referenceType = $space + "." + $nameInfo.Reference
		}
		else {
			$refNamespace = $reference.Namespace.Name
			if ($space -ne $refNamespace -and !$space.StartsWith($refNamespace + ".")) {
				if ($refNamespace.StartsWith($space + ".")) {
					$refNamespace = $refNamespace.Substring($space.Length + 1)
				}
				$referenceType = $refNamespace + "." + $reference.Name
			}
		}
		
	}
	# try to fix underscores
	$nameInfo.PropertyName = $nameInfo.Name
	if ($FixUnderscores) {
		for ($i = 0; $i -lt $nameInfo.PropertyName.Length; $i++) {
			if ($i -eq 0 -or $nameInfo.PropertyName[$i-1] -eq '_') {
				Write-Verbose $nameInfo.PropertyName[$i]
				$nameInfo.PropertyName = $nameInfo.PropertyName.Substring(0, $i) + [System.Char]::ToUpper($nameInfo.PropertyName[$i]) + $nameInfo.PropertyName.Substring($i+1)
			}
		}
		$nameInfo.PropertyName = $nameInfo.PropertyName.Replace("_", "")
	}
	
	# add processed property
	$props += @{Name = $nameInfo.Name; PropertyName = $nameInfo.PropertyName; Type = $type; MaxLength = $nameInfo.MaxLength; Required = $nameInfo.Required; Reference = $nameInfo.Reference; ReferenceType = $referenceType}
}

if ($typedCount -gt 0 -and $typedCount -lt $Properties.Length -and !$NoTypeWarning) {
	Write-Warning "Types were not specified for all properties. Types for such properties were assigned automatically."
	}

Add-ProjectItemViaTemplate $outputPath -Template Model `
	-Model @{ Namespace = $space; TypeName = $Model; Properties = $props; Annotations = !$NoAnnotations } `
	-SuccessMessage "Added $Model output at {0}" `
	-TemplateFolders $TemplateFolders -Project $Project -CodeLanguage $CodeLanguage -Force:$Force

#Scaffold SurfaceView
if($ScaffoldView) {
	$renderMethodName = "Render" + $ControllerName
	$modelNamespaceFull = $space + "." + $Model
	Scaffold SurfaceView -Controller $ControllerName -ViewName $renderMethodName -ModelType $space -ReferenceScriptLibraries:$true -PartialView:$true -Properties $Properties -ModelNamespace $modelNamespaceFull -Project $Project -CodeLanguage $CodeLanguage -Force:$Force -NoAnnotations:$NoAnnotations -NoTypeWarning:$NoTypeWarning -FixUnderscores:$FixUnderscores
}