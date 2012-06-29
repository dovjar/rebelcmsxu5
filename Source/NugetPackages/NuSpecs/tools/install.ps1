param($rootPath, $toolsPath, $package, $project)

# TODO - Set Visual Studio Project property to enable IISExpress
if ($project) {
	# Try to delete App_Start folder
	$projectPath = Split-Path $project.FullName -Parent
	$projectPathAppStart = Join-Path $projectPath "App_Start"
	$AppStartFolderExists = Test-Path $projectPathAppStart
	if($AppStartFolderExists)
	{
		$project.ProjectItems | ForEach { if ($_.Name -eq "App_Start") { $_.Remove() } }
		Remove-Item $projectPathAppStart -Recurse -Force
	}
	# Try to delete license.txt file
	$project.ProjectItems | ForEach { if ($_.Name -eq "license.txt") { $_.Delete() } }
	
	# Remove WebActivator assembly reference
	$project.Object.References | Where-Object { $_.Name -eq 'WebActivator' } | ForEach-Object { $_.Remove() }
	
	# Remove Examine plugins folder if it exists
	$examinePluginPath = Join-Path (Split-Path $project.FullName -Parent) "\App_Plugins\Packages\Examine"
	$examinePluginPathExists = Test-Path $examinePluginPath
	if($examinePluginPathExists)
	{
        #Removes the assemblies from the bin folder
        Get-ChildItem -path $examinePluginPath -Recurse |
    	   Where -filterscript {($_.Name.EndsWith("dll"))} | Foreach-Object {
    	    $fileToRemove = $_.FullName.replace("App_Plugins\Packages\Examine\lib","bin")
            if(Test-Path $fileToRemove) { 
                Remove-Item $fileToRemove
                }
    	   }
        #Removes the Examine folder
        Remove-Item $examinePluginPath -Recurse -Force
	}
	
	# Create a backup of extisting umbraco config files
	$configPath = Join-Path (Split-Path $project.FullName -Parent) "\App_Data\Umbraco\Config"
	Get-ChildItem -path $configPath |
	   Where -filterscript {($_.Name.EndsWith("config"))} | Foreach-Object {
	    $newFileName = $_.FullName.replace(".config",".config.backup")
	    Copy-Item $_.FullName $newFileName
	   }
		
	# Create a backup of original web.config
	$projectDestinationPath = Split-Path $project.FullName -Parent
	$webConfigSource = Join-Path $projectDestinationPath "web.config"
	$webConfigDestination = Join-Path $projectDestinationPath "web.config.backup"
	Copy-Item $webConfigSource $webConfigDestination
	
	# Copy umbraco files from package to project folder
	$umbracoFilesPath = Join-Path $rootPath "UmbracoFiles\*"
	Copy-Item $umbracoFilesPath $projectDestinationPath -recurse -force
	
	# Open readme.txt file
	$DTE.ItemOperations.OpenFile($toolsPath + '\Readme.txt')
}