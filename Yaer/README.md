# 游戏打包
1. 构建AB包
2. 点击菜单项Game Framework/Scenes in Build Settings/Default Scenes
3. 点击菜单项File/Build Settings, 然后点击Build
4. 需要继续开发时，点击菜单项Game Framework/Scenes in Build Settings/All Scenes
<br>
<br>
# Asset Bundles
## Asset Bundles构建
项目使用GameFramework提供的工具构建Asset Bundles.
### 配置文件
相关配置文件放在Assets/Editor/GFAssetBundleSettings文件夹中
### 构建方式
1. 所有需要构建到ab包的资源都放在Assets/GameRes文件夹中。
2. 点击UnityEditor的菜单Game Framework/Resource Tools/Resource Editor中，将新加入的资源添加进ab包中
3. 如果有无法识别的资源类型，在Assets/Editor/GFAssetBundleSettings/xml/ResourceEditor.xml中添加对应类型
4. 按需添加新的ab包
5. 点击菜单Game Framework/Resource Tools/Resource Builder，构建ab包
6. 在ab包的导出文件夹中，将目标平台的ab包复制进StreamingAssets
<br>
<br>
# 动态资源加载
游戏中的动态资源使用Asset Bundles加载

