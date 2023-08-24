$path = "${pwd}\WSM.Client.exe"
$serviceName = "Windows Server Monitor"
sc.exe create "$serviceName" binpath="$path"
sc.exe start "$serviceName"