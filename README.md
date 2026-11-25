# SkyCrew

SkyCrew 是由 **OilHook Studios** 开发的飞机题材派对游戏，采用柔和的卡通画风和单世界结构。当前主世界包含昼夜天空、基于 Flow Cloud 网格、三层噪声和场景光照联动的固定位置云场。游戏时间自动流逝，一天为现实时间 30 分钟。

## 打开项目

1. 用 **Godot 4.7.2 .NET（C#）** 导入本目录的 `project.godot`。渲染器使用 **Forward+**，C# 目标框架为 .NET 8.0。本机 Godot 位于 `D:\Godot\GodotSharp\4.7.2`。
2. 按 **F5** 进入世界。默认视口和独立游戏窗口均为 **1920 × 1080**。当前入口为 scenes/Main.tscn，World 提供环境，玩家相机提供游戏视角。
3. 在 `world.tscn` 的 3D 视口选择 `Environment/CloudField`，调节 Inspector 的 `Minimum Altitude`、`Cell Spacing`、`Cell Occupancy`、`Seed` 等属性。位置由种子固定，Render Distance 控制完整显示距离，Fade Distance 控制其后的渐隐宽度，Preload Margin 控制提前加载范围。最低高度默认 450 米，约束云体底部。
4. 选择 `Environment/SkyRig` 调整 `Preview Hour` 可预览天空时刻，游戏开场时刻由 `WorldClock/StartHour` 决定。视口菜单的「查看环境」应开启。
5. 原版完整示例位于 `scenes/debug/flow_cloud_original.tscn`，可单独打开并按 F6 比对。世界云组件位于 `scenes/world/environment/flow_cloud.tscn`，模型、材质、纹理和着色器均按类型存入 `assets/`。

命令行编译可运行 `dotnet build SkyCrew.sln`。

## 目录分类

本目录是 Godot 的 `res://`。用文件的职责决定目录，用文件类型决定它属于 `scenes/`、`scripts/`、`assets/` 还是 `data/`。例如，机场触发区域的节点场景进 `scenes/gameplay/triggers/`，触发行为脚本进 `scripts/gameplay/triggers/`，可供多个触发器读取的规则资源进 `data/gameplay/`。

### 场景与脚本

| 目录 | 放置内容 |
| --- | --- |
| `scenes/world/` | 唯一的世界入口 `world.tscn` |
| `scenes/world/environment/` | 天空、灯光、云、雾和其他环境组件 |
| `scenes/world/regions/` | 世界内的地形、机场、岛屿和其他可分区实例化的地图组件 |
| `scenes/world/props/` | 可放入地图的静态布景组件 |
| `scenes/actors/aircraft/`、`characters/` | 飞机、角色以及各自可独立编辑的节点组件 |
| `scenes/gameplay/interactables/`、`triggers/`、`objectives/` | 交互物、触发区域、关卡目标的节点场景 |
| `scenes/ui/menus/`、`hud/` | 正式菜单、游戏内 HUD |
| `scenes/debug/` | 调试面板、预览镜头和开发用场景组件 |
| `scripts/world/` | 世界时钟、光照同步、未来的地图分区管理 |
| `scripts/actors/aircraft/`、`characters/` | 飞行操控、角色控制与对应的专属行为 |
| `scripts/gameplay/interactables/`、`triggers/`、`objectives/` | 交互逻辑、触发条件、目标和计分逻辑 |
| `scripts/ui/`、`scripts/debug/` | 正式界面行为、调试和预览行为 |
| `scripts/core/` | 多个系统确实共用的基础类型与服务 |

### 资源与数据

| 目录 | 放置内容 |
| --- | --- |
| `assets/models/aircraft/`、`characters/`、`environment/clouds/`、`props/` | 模型、网格源文件和烘焙后的网格资源。环境模型按类型再分目录 |
| `assets/textures/environment/clouds/`、`terrain/`、`actors/`、`props/`、`ui/` | 云噪声、地形贴图、角色和飞机贴图、布景贴图、界面图像 |
| `assets/materials/environment/`、`actors/`、`props/` | 环境、可动实体、布景的共享 Godot 材质 |
| `assets/shaders/environment/`、`actors/`、`vfx/`、`ui/` | 环境着色器、实体着色器、特效和 UI 着色器及其包含文件 |
| `assets/animations/environment/`、`actors/` | 昼夜、环境预览和实体动画用的 `AnimationLibrary` 或共享动画资源 |
| `assets/audio/music/`、`ambience/`、`sfx/`、`voice/` | 音乐、环境音、音效、语音 |
| `assets/fonts/` | 正式 UI 或其他用途的字体资源 |
| `data/world/`、`aircraft/`、`gameplay/`、`localization/` | 世界配置、机型参数、玩法数据、本地化文本和翻译资源 |

目录和 Godot 资源文件使用 `snake_case`，场景树节点和 C# 类型使用 `PascalCase`。C# 文件名与对应类型名一致。保留资源旁的 `.uid` 与 `.import` 配置并一同提交，导入缓存 `.godot/` 由编辑器生成。空目录用 `.gitkeep` 保留。项目配置、C# 工程文件和项目图标位于根目录。

## 实现文档

| 目录 | 内容 |
| --- | --- |
| `docs/architecture/` | [单世界结构、场景树和职责边界](docs/architecture/world-scene.md) |
| `docs/environment/` | [昼夜、天空和光照](docs/environment/day-night.md) · [Flow Cloud 和穿云](docs/environment/flow-clouds.md) |
| `docs/debug/` | [原生调试控件和预览镜头](docs/debug/environment-preview.md) |
| `docs/performance/` | [LOD、裁剪与大地图验证方法](docs/performance/world-rendering.md) |
| `docs/assets/` | [资产入库与资源复用](docs/assets/asset-pipeline.md) |
| `docs/gameplay/` | 预留飞机操控、触发器、交互、派对规则和计分文档 |

## 文档维护

实现变化直接同步到对应的技术文档。README 维护项目介绍、运行方法和目录分类，技术文档记录当前原理、参数、使用方法与验证条件。
