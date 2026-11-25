# 昼夜、天空与地平线

WorldClock 默认从 08:30 开始自动运行。DayDurationSeconds 为 1800，代表现实时间 30 分钟经过完整游戏日。时间每帧增加 24 × delta / DayDurationSeconds，75 秒经过一个游戏小时，午夜回绕到 00:00。

编辑器选择 World/Environment/SkyRig 调节 Preview Hour。该预览值独立于运行时钟。运行时的开场时刻、播放状态和一天长度分别由 WorldClock 的 StartHour、StartRunning、DayDurationSeconds 决定。

## 日出与落日

太阳由 SunFrame/Orbit/Sun 节点控制，06:00 与 18:00 经过水平线。SkyRig 按太阳高度连续调整日光和环境光。天空着色器在靠近日出落日方向的地平线混合橙红与紫色，颜色随高度向高空蓝色渐变。

太阳盘的视角半径默认为 0.85 度。太阳盘输出 HDR 辐射，WorldEnvironment 的 Glow 在阈值 1.2 以上产生辉光。盘外有近晕和更宽的空气散射。靠近地平线时降低太阳盘强度并增加暖色，盘面按观察方向的地平线遮挡，能观察到半轮太阳升起和落下。云和建筑的深度遮挡可挡住盘面，辉光由剩余可见亮部产生。

soft_sky 材质可调整 sun_radius、sun_intensity、halo_strength 与 star_intensity。

## 地平线渐隐

SkyRig 的 HorizonFadeStart 默认 2200 米，HorizonFadeEnd 默认 9000 米。它们驱动 WorldEnvironment 的深度雾，曲线为 1.6。远处几何逐渐融入背景。天空使用自身的渐变。海面平面扩展为 40 × 40 千米，让可见边界落在远景渐隐之后；水材质的 UV 比例同步放大四倍以保留原有世界空间纹理尺度。

FogAerialPerspective 为 1，远景雾色从天空辐射立方体采样，朝向日落与背离日落方向都能得到对应天空色。天空的辐射图使用实时模式和 256 尺寸。反射仍由项目环境设置决定，空气透视使用该辐射图。

## 夜晚星空

星点在球面经纬网格上按稳定哈希分布，有大小、亮度和冷暖色差，采用屏幕导数平滑边缘。太阳低于地平线后星点渐入，地平线附近受空气层遮淡。轻微闪烁沿固定星位变化，天空还包含很淡的星带与示意月亮。

## 检查方法

从 Main 运行，确认时钟自动推进。编辑器依次检查 06:00、06:18、17:42、18:00、18:24 与 00:00，确认半轮太阳、辉光、晚霞到夜空的过渡以及地平线颜色。正午可抬头观察太阳亮核。

[Godot 环境与后处理](https://docs.godotengine.org/en/stable/classes/class_environment.html)
