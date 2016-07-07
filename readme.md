# KF.ORM

## Usages

需在Web.config文件中configuration.connectionStrings中注册数据库，如:

```xml
<configuration>
	<connectionStrings>
		<add name="[database name]" connectionString="[database connection string]" providerName="[database type]"/>
		...
	</connectionStrings>
</configuration>
```

可注册映射实体类，类中需引用KF.ORM.Entity，需在类名上标注属性[Database(Name = "【数据库名称】"), Table(Name = "【表名】")]，成员上标注主键属性[Column(Name = "【列名】", PrimaryKey = true, NotSaved = true)]或非主键属性[Column(Name = "【列名】")]:

```csharp
using System;
using KF.ORM.Entity;

namespace [domain namespace]
{
	[Database(Name = "[database name]]"), Table(Name = "[table name]]")]
	public class [domain name]
	{
		[Column(Name = "[column name]", PrimaryKey = true, AutoId = true, NotSaved = true)]
		public [member type]] [member name] { get; set; }

		[Column(Name = "[column name]")]
		public [member type]] [member name] { get; set; }

		...
	}
}
```

调用时，需引用KF.ORM，调用接口为Database，如：

```csharp
Database.Instance()...
```

## Examples

Web.config:

```xml
<configuration>
	<connectionStrings>
		<add name="BCAPMP" connectionString="Data Source=|DataDirectory|\BCAPMP.db;" providerName="SQLITE"/>
		<add name="KFWF" connectionString="Server=.;Database=KFWF;Trusted_Connection=Yes;Connect Timeout=90" providerName="SQLSERVER"/>
		<!-- 利用OLEDB访问Excel，若使用Microsoft.ACE.OLEDB.12.0，则需安装AccessDatabaseEngine(http://download.microsoft.com/download/7/0/3/703ffbcb-dc0c-4e19-b0da-1463960fdcdb/AccessDatabaseEngine.exe) -->
		<add name="ExcelTest" connectionString="Provider=Microsoft.ACE.OLEDB.12.0;Data Source=|DataDirectory|\ExcelTest.xlsx;Extended Properties='Excel 12.0 Xml;HDR=No';" providerName="OLEDB"/>
	</connectionStrings>
</configuration>
```