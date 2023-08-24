$serviceName = "Windows Server Monitor"
sc.exe stop "$serviceName"
sc.exe delete "$serviceName"