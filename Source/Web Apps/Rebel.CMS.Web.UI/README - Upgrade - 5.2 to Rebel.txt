Thanks for downloading Rebel 1.0!

These are some notes if you are upgrading from Umbraco 5.2 to Rebel

Configuration changes
---------------------
Be sure to copy over the contents of App_Data/Umbraco/Config from the release zip
There is a new element in the web.config for general Framework configuration, either copy over the /web.config from the release zip, 
or if you have made modifications, the new lines are:

To be added to /configuration/configSections/sectionGroup name="rebel":
	<section name="framework" type="Rebel.Framework.Configuration.General, Rebel.Framework" requirePermission="false"/>

To be added to /rebel:
	<framework configSource="App_Data\Rebel\Config\rebel.framework.config" />

** Important **

The files /App_Data/Umbraco/Config/log4net.config and /App_Data/Umbraco/Config/umbraco.cms.system.config contain important changes
that affect performance. Please ensure these are copied over or merged and renamed to rebel if you have made any changes.

Default packages - remove Examine
---------------------------------
Be sure to remove the Examine folder from App_Plugins/Packages which may have been left from a previous 5.0 or 5.1 installation. 
5.2 includes a reference to a newer Lucene.dll; if the Examine package is left in the older Lucene.dll will get copied to the bin
folder over the top and cause the application to fail to compile. Note that for the moment the Examine package is not actually used
and is safe to delete.

First app startup for Rebel on a database created earlier than 5.2
----------------------------------------------------------------
Rebel has a new database table called AggregateNodeStatus. When the app first starts up, if the table doesn't exist, it will try to create 
this table. If it can't create it, for example if your database user does not have dbo rights, it will log a warning message together
with the T-SQL that it attempted to run, so that if you prefer not to change your db user permissions, you can run the script manually.

Permissions & slow performance
------------------------------
It's vital that the application has read/write/modify permissions to the following folders in order to start up properly.

/bin/*
/App_Plugins/*
/App_Data/*

Regarding the first two: if the application doesn't have write permissions to these folders you will receive a YSOD on application startup.

A note about source control: some source control providers may mark files as readonly if you check them in. This can cause problems
in Rebel 1.0's plugin manager when it attempts to write a cache of which assemblies it has copied to the bin folder, if the file cannot
be written to then a YSOD will be produced on application startup. 
Please avoid checking the following folders/files into source control or otherwise marking files as readonly:

/bin/rebel-plugins.list
/App_Plugins/Cache/*


Thanks again!
The Rebel team

Issues and feature requests can always be logged at http://youtrack.rebelcms.com