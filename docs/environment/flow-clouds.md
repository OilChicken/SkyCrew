# Flow Cloud 与生成规则

游戏通过 scenes/Main.tscn 加载世界，云组件为 scenes/world/environment/flow_cloud.tscn。CloudModel 保留原网格，使用 assets/materials/environment/flow_cloud.tres。原版对照材质为 flow_cloud_reference.tres，原版演示位于 scenes/debug/flow_cloud_original.tscn。

## 空间、动画与光照

三层 simplex 噪声各有独立的频率、方向、速度和强度。世界材质开启 use_reference_space，将顶点还原到上游示例的 1000 倍建模比例与 -4 米偏移后，再计算噪声、高度和表面纹理。噪声位移随实例比例放大，因此云升到高空或放大后仍保有高度层次和轮廓运动。

片元按 flow_size 缩放参考坐标，按 flow_speed 推进表面采样。噪声图通过三平面投影融合，由 3D 与 2D 噪声共同调制表面颜色。

use_world_lighting 将颜色接入 SkyRig 发布的太阳方向、日光颜色和强度、环境光颜色和能量。高度渐变与包裹式法线响应形成亮面和暗面，Fresnel 形成轮廓亮边。白天云顶偏白，低太阳高度和夜间跟随场景光照变色。云体层次采用材质内的体积明暗近似。

## 顺光、侧光的漫画明暗

世界材质的 Toon Lighting 分组控制色块。基础明暗仍由高度和光照法线计算，再通过两个阈值映射到暗面、中间调和亮面三档。每档保留窄范围的柔和过渡。亮面颜色限制在可显示范围内，侧光时能保留较大且连续的中间调与暗面。

着色器使用观察方向与太阳方向的夹角平滑调整分档权重。顺光和侧光使用三档明暗，接近背光时逐步回到原来的连续明暗与轮廓光。分档权重同时减弱表面噪声对颜色的调制，让碎小灰影不覆盖大色块。顶点噪声与表面纹理的时间推进继续运行。

toon_strength 控制分档效果的混合强度，toon_band_softness 控制分界处的柔和宽度，两个 threshold 控制暗面和亮面的覆盖范围。材质 Inspector 可直接调节这些属性。

## 世界材质配置

| 属性 | 数值或资源 |
| --- | --- |
| top_texture | assets/textures/environment/clouds/flow_cloud_noise.jpg |
| use_procedural_color | true |
| use_reference_space / use_world_lighting | true / true |
| noise_size_a / b / c | 0.835 / 2.055 / 10.0 |
| noise_strength_a / b / c | 0.24 / 0.11 / 0.04 |
| speed_a / b / c | 1.44 / 1.0 / 10.36 |
| direction_a | Vector3(1.5, 0, 0) |
| direction_b | Vector3(1.48, -0.86, -0.36) |
| direction_c | Vector3(1, 0, 0) |
| procedural_displacement_strength | 1.55 |
| procedural_vertical_lift | 0 |
| flow_size / flow_speed / flow_strength | 1 / 1.85 / 1 |
| texture_detail | 1 |
| tiling | Vector2(0.04, 0.08) |
| rim_color | Color(0.07, 0.07, 0.07, 1) |
| toon_strength | 0.95 |
| toon_band_softness | 0.045 |
| toon_shadow_threshold / toon_light_threshold | 0.40 / 0.72 |

材质资源同时保存参数和贴图绑定。着色器默认值服务于原版对照分支，世界材质使用上表的显式配置。关闭 use_procedural_color 会同时停用三层顶点位移与体积明暗，位移强度为 0 会停止边缘运动，清空 top_texture 会使表面流动失去噪声细节。

## 固定分布与高度

云的位置由 CloudField 的 Seed、CoverageSize、CellSpacing、CellOccupancy 决定。默认覆盖 24 × 24 千米，格宽约 850 米，占用率 0.72，种子 128 得到 563 个位置。格内偏移不超过宽度的 18%，实例比例为 8–13 倍。位置固定，进入和离开区域只加载与释放对应实例。

MinimumAltitude 默认约束最低云底为 450 米，HeightBand 默认为 300 米。网格边界、节点偏移、缩放和顶点最大位移共同决定云心高度。当前参考空间法线位移余量为 0.6045 米，按实例比例换算。

RenderDistance 默认 3000 米，FadeDistance 为 1200 米，PreloadMargin 为 600 米。use_distance_fade 打开世界材质的距离渐隐，每实例 cloud_fade_start、cloud_fade_end 和 cloud_spawn_visibility 分别控制距离区间与入场进度。实际范围管理与 LOD 说明见 docs/performance/world-rendering.md。

验证时从 Main 运行，沿相机移动路线穿过渐隐区并返回，检查同一位置和逐渐显现。暂停镜头后，边缘与表面仍由着色器时间推进。日出日落与夜间检查可在编辑器 SkyRig 的 Preview Hour 中完成。