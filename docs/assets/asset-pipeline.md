# 资产入库与资源分类

资产按用途与类型归入现有目录。Flow Cloud 与其他环境资产采用同一套分类。

| 文件 | 目录 |
| --- | --- |
| flow_cloud_mesh.res、flow_cloud_lod.res | assets/models/environment/clouds/ |
| flow_cloud_noise.jpg 及其 .import | assets/textures/environment/clouds/ |
| flow_cloud.tres、flow_cloud_reference.tres | assets/materials/environment/ |
| flow_cloud.gdshader、cloud_noise_3d.gdshaderinc、flow_cloud_gradient_sky.gdshader | assets/shaders/environment/ |
| flow_cloud.tscn、cloud_field.tscn | scenes/world/environment/ |
| flow_cloud_original.tscn | scenes/debug/ |
| CloudField.cs | scripts/world/ |

Flow Cloud 来源版本为 fe8bb31ed09eebd89f2ff87a8df84354bbb1858f。原网格和纹理沿用下载的内容，资源路径适配当前分类目录。flow_cloud_reference.tres 保存上游参数，flow_cloud.tres 保存世界光照、比例适配和轮廓运动配置。Godot 按新路径重建纹理缓存。

完整原版场景供单独运行比对。世界用的云组件复用原版 CloudModel 的节点变换与资源，然后由生成器控制整体位置、朝向和比例。

校验二进制资源时比较文件哈希。校验文本资源时先还原路径映射，再与下载原文比较。迁移时同时保留 .uid 和 .import 文件，确认所有场景的资源引用有效。

[Flow Cloud 下载源](https://github.com/ffttasd/Didi-Dev-Lab/tree/fe8bb31ed09eebd89f2ff87a8df84354bbb1858f/flow_cloud)

世界云材质需要保留 top_texture 噪声贴图、use_procedural_color、use_reference_space、use_world_lighting 和位移强度。具体配置与排查方法见 docs/environment/flow-clouds.md。

世界使用的 flow_cloud_lod.res 以 ImporterMesh.add_surface 读取原始表面数组，再调用 generate_lods(60, 25, []) 并压缩保存。基础顶点坐标保持原样。完整原版示例使用原网格供比对。
