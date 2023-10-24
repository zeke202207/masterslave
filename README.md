# masterslave

## 1. ��Ŀ����

## 2.�������

* ����̨��ʽ����

	ֱ�ӵ���������ɵĿ�ִ�г��򼴿�


* ����ʽ����

	```
	 NetX.Master.exe start
	 NetX.Master.exe stop
	```

	```
	 NetX.Worker.exe start
	 NetX.Worker.exe stop
	```

## 3.����

### Master�ڵ����ù���

* Worker�������õ�Լ���������ù���

	1. ���ýڵ�����
	```
	{
	"Logging": {
	"LogLevel": {
		"Default": "Information",
		"Microsoft.AspNetCore": "Warning"
	}
	},
	"AllowedHosts": "*",
	"Master": {
	"Port": 5600,
	"IpWhitelist": [ "127.0.0.1:5600", "192.168.7.26:5600" ]
	},
	"HangfireCredentials": {
	"UserName": "admin",
	"Password": "admin@123"
	}
	}

	```
	***Master�ڵ�����˵��*** 

		Port��master����˿�

		IpWhitelist���������ӵ�worker�ڵ�ip�б�

	***HangfireCredentials�ڵ�����˵��*** 

		UserName��hangfire����̨�û���
		Password��hangfire����̨����

### Worker�ڵ����ù���

* ʵ��ҵ���Լ���������ù���

  1. Worker�ڵ����ʵ��```IJobRunner```�ӿ�
  1. ʹ��```Transient```��```Scoped``` ��```Singleton``` ���Ա�ע�������ڣ�ע�뵽������
 
* Worker�������õ�Լ���������ù���

  1. ���ýڵ����£�
	```
	{
	"Logging": {
	"LogLevel": {
		"Default": "Information",
		"Microsoft.AspNetCore": "Warning"
	}
	},
	"AllowedHosts": "*",
	"WorkerConfig": {
	"Id": "001",
	"GrpcServer": "http://127.0.0.1:5600"
	}
	}
	```
	***WorkerConfig�ڵ�����˵��*** 

		Id�� ��ȺΨһ
		GrpcServer�� master������ַ
