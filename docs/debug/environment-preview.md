# 环境的编辑器预览

运行中的 World 仅包含环境。时间由 WorldClock 自动推进，当前游戏天为现实 30 分钟。开发用 environment_debug_panel.tscn 和 environment_preview.tscn 保存在 scenes/debug，当前 Main 与 World 都未实例化它们。

在编辑器打开 scenes/Main.tscn 或 scenes/world/world.tscn。选择 World/Environment/SkyRig，调节 Preview Hour 检查天空、太阳与云的场景光照。选择 CloudField 可调节布局、高度和显示距离。编辑器会使用主 3D 视口的相机位置，预览当前位置附近的云。

默认云体在 3 千米内完整显示，随后 1.2 千米渐隐，另有 600 米预加载距离。实例的入场淡入默认为 1.2 秒，首次进入和远距离跳转都经过渐显。

原版 Flow Cloud 可单独打开 scenes/debug/flow_cloud_original.tscn 按 F6 比对。
