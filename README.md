Building the simulator for use:

  **docker build -t timesheetmodulesimulator:latest .**


Creating an image attached to the simulator:

  **docker run --name optional_module_name_here -p 5000:80 -v "D:\OutputFolder:/app/wwwroot/modules/UserModule" -d timesheetmodulesimulator:latest**