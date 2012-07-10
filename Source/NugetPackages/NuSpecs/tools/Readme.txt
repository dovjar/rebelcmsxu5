A note about running Rebel v5 from Visual Studio.

Before running or debugging the project you need to configure the web application project to use IIS Express.
Right click on the web application project and select Properties -> Click Web tab and check the "Use Local IIS Web Server" radiobutton and either "Use IIS Express" or "Override application root URL" checkbox.

It is not possible running Rebel v5 from Visual Studio with the built-in Cassini.

If you don't have IIS Express installed we recommend you install it or set up a new site in IIS that point to the web applications project folder.

Upgrade notice:
If you are upgrading from an earlier version of Rebel a backup of your web.config file is placed in the root of your projects directory. A backup of the rebel specific config files found in 
App_Data\Rebel\Config has also been made, so if you have added any entries to these files please make sure to copy them back.

If you are upgrading from versions prior to 5.1 you need to upgrade User storage by going to this url and following the instructions:
http://yourdevdomain.com/install/upgrade

Please make sure to check http://rebel.codeplex.com for release notes.

- Rebel