- [GUI for WebApplication Deployment](#gui-for-webapplication-deployment)
- [WebAppConfigCreatorWindow](#webappconfigcreatorwindow)
- [Mainwindow](#mainwindow)
- [PlcRackConfigCreatorWindow](#plcrackconfigcreatorwindow)
- [Mainwindow](#mainwindow-1)
  
![1518F](screens/1518F.png)
# GUI for WebApplication Deployment 
![Deployment](screens/Gif.gif)

On the MainWindow you can select the webapps to deploy and the racks you want to deploy to:

![Deployment](screens/01_Menu.png)
![Deployment](screens/12_select_apps_and_racks.png)

With the Edit Application(s) or create config button you can open the WebAppConfigCreatorWindow

# WebAppConfigCreatorWindow
On the WebAppConfigCreatorWindow 
![Deployment](screens/02_configcreatorwindow.png)

you can select your WebAppdirectory

![Deployment](screens/03_choose_directory.png)

Select the app you want to configure

![Deployment](screens/04_select_app.png)

Do the configuration

![Deployment](screens/05_configure_app.png)

Save it (into a WebAppConfig.json file in the directory of the webapp) and go back to the Mainwindow
# Mainwindow

Or you can also add an already existing configuration file of your WebApplication

![Deployment](screens/docu_addExistingConfigFile.png)

Also you can Open the

# PlcRackConfigCreatorWindow
Where you can add a Plc Rack configuration and edit Plc Rack configurations:

![Deployment](screens/10_select_rack.png)

Save it and go back to the MainWindow.

# Mainwindow
Or you can also add an already existing configuration file of your PlcRackConfiguration

![Deployment](screens/docu_addExistingRackConfigFile.png)

When you are done configuring and selecting the configurations you want to deploy to you can start the Deployment of the Apps to the Racks:

![start_deployment](screens/docu_startDeployment.png)

you'll be prompted for the Login credentials - Credentials of the configured User from TIA Portal (which wont be saved)

![Deployment](screens/14_login.png)

and optionally if the certificate of any plc is not in your trustlist:

![Deployment](screens/docu_ca_not_trusted.png)

You'll have to decide wether you want to connect and trust to the earlier provided IP/DNS or not.

Once the deployment is finished you'll receive a message:

![DeploymentDone](screens/docu_deploymentDone.png)

You can also delete your WebApplication(s):

![DeleteApps](screens/docu_deleteApp.png)

Again once the WebApps are deleted you'll receive a message.