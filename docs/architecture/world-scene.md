# 世界与程序化云场

游戏入口为 scenes/Main.tscn，组装 World、玩家、机场与水面。World 的 Environment 分支包含 SkyRig 和 CloudField。游戏视角来自玩家的 Camera3D。

SkyRig 保存太阳、天空和 WorldEnvironment 节点，WorldClock 控制时间。CloudField 按固定种子预先计算云的位置、朝向和比例，再按相机距离实例化 scenes/world/environment/flow_cloud.tscn。完整原版示例保存在 scenes/debug/flow_cloud_original.tscn。

CloudField 是 [Tool] 节点。生成实例不设置 Owner，场景只保存参数。编辑器按主 3D 视口相机位置加载附近云，运行时使用当前游戏相机。所有云的位置在本次布局中固定，相机移动只决定实例的加载与释放。

修改布局参数后编辑器自动重建，也可点击「重新生成云」。同一种子与参数产生同一组位置，离开再返回时能恢复同一朵云。World 中的环境调试面板和预览相机已从运行树移除。
