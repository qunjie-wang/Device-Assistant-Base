# IMR Desktop Assistant

Windows 托盘设备助手，采用一个主程序和一个可选洗手间状态扩展。

## 功能

### 基础版（全员安装）

- 系统托盘常驻
- 单击托盘图标，在右下角显示设备信息
- 计算机名
- 当前活动网卡 IPv4 地址
- 当前活动网卡 MAC 地址
- 当前用户登录自启动
- 不需要管理员权限
- 不显示在任务栏和 Alt+Tab

### 洗手间状态扩展（按需安装）

- 在同一个弹窗顶部增加四个状态圆点
- 绿色：空闲
- 红色：占用
- 灰色：离线或接口异常
- 默认每 3 秒请求一次接口
- 不增加第二个托盘图标或后台进程

默认接口：

```text
http://192.168.4.196:3002/api/wc_status
```

## 员工安装方式

### 基础版

1. 解压 `IMR-Device-Assistant-Base.zip`
2. 双击 `install.cmd`
3. 安装目录为 `%LOCALAPPDATA%\IMRDesktopAssistant`

### 洗手间状态扩展

1. 先安装基础版
2. 解压 `IMR-Washroom-Plugin.zip`
3. 双击 `install-plugin.cmd`

卸载扩展不会影响基础设备信息功能。

## 开发和构建

需要 Windows 和 .NET 8 SDK：

```powershell
.\build-release.ps1
```

输出：

```text
artifacts\IMR-Device-Assistant-Base.zip
artifacts\IMR-Washroom-Plugin.zip
```

也可以把仓库推送到 GitHub，然后在 Actions 中运行 `Build Windows packages`，下载构建产物。

## 接口兼容格式

支持根数组：

```json
[
  { "stallId": "1", "status": "occupied" },
  { "stallId": "2", "status": "idle" }
]
```

也支持 `stalls`、`data`、`result`、`items` 或 `list` 包装字段。

状态映射：

- 空闲：`idle`、`free`、`off`、`false`、`0`、`clear`、`无人`、`空闲`
- 占用：`occupied`、`busy`、`on`、`true`、`1`、`presence`、`有人`、`占用`
- 其他：离线

## 修改扩展接口

安装扩展后编辑：

```text
%LOCALAPPDATA%\IMRDesktopAssistant\Plugins\WashroomStatus\washroom.json
```

修改完成后，从托盘菜单退出并重新启动程序。
