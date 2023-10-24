# masterslave

## 1. 项目介绍

## 2.如何启动

* 控制台方式启动

	直接点击编译生成的可执行程序即可


* 服务方式启动

	```
	 NetX.Master.exe start
	 NetX.Master.exe stop
	```

	```
	 NetX.Worker.exe start
	 NetX.Worker.exe stop
	```

## 3.规则

### Master节点配置规则

* Worker服务配置的约定大于配置规则

	1. 配置节点如下
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
	***Master节点配置说明*** 

		Port：master服务端口

		IpWhitelist：允许连接的worker节点ip列表

	***HangfireCredentials节点配置说明*** 

		UserName：hangfire控制台用户名
		Password：hangfire控制台密码

### Worker节点配置规则

* 实现业务的约定大于配置规则

  1. Worker节点必须实现```IJobRunner```接口
  1. 使用```Transient```、```Scoped``` 或```Singleton``` 特性标注声明周期，注入到容器中
 
* Worker服务配置的约定大于配置规则

  1. 配置节点如下：
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
	***WorkerConfig节点配置说明*** 

		Id： 集群唯一
		GrpcServer： master主机地址
