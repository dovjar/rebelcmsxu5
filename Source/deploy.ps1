include .\_powerup\deploy\combos\PsakeCombos\StandardSettingsAndRemoteExec.ps1

task deploy {
	run deploywebsite ${web.servers}
}

task deploywebsite  {
	import-module websitecombos
	import-module powerupfilesystem

	if (${website.domain}.GetType().FullName -eq "System.String")
	{
		$domains = ${website.domain}.split('|');
	} else {
		$domains = ${website.domain};
	}
	
	$bindings = @();
	$domains| % {
		
		$bindings += @(
					@{url = $_;}
					);
	}
	$comboOptions = @{
		websitename = ${website.name};
		sourcefolder = "WebApp";
		copywithoutmirror = 1;
		destinationfolder = ${website.name};
		webroot = ${deployment.root};
		bindings = $bindings;
		siteid = ${siteid};
	}	

	invoke-combo-standardwebsite($comboOptions)
}