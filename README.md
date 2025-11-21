# MazeVenture
## Erik M. Buck's 2016 Make-IT-Wright project
### (Implemented start to finish in 24 hours)
This is a simple "world exploration" game created with the Unity 3D Game Engine using a mix of 3D models and C# code created by Erik M. Buck specifically for this project and free assets from the Unity Asset Store. The player controls a 3D avatar and explores a procedurally generated maze with walls, rooms, doors, trees, stairs, and more. The maze is never the same twice, and it provides an "endless" world. When the edge of existing content is reached, more maze is generated.

Procedural maze generation was inspired by the Catlike Coding Unity tutorial: http://catlikecoding.com/unity/tutorials/maze/. The Maze generation has been extended in numerous ways beyond the tutorial including multi-level mazes, trees, stairs, wall types, railings and banisters, physics, etc.

![Image of MazeVenture](http://cdn.rawgit.com/erikbuck/MazeVenture/master/MazeVenture.png)

## 如何开始游戏

### 在 Unity 编辑器中运行

#### 第一步：设置场景
1. 打开 Unity 编辑器（Unity 6000.1.6f1 或更高版本）
2. 打开场景文件：`Assets/Scene.unity`
3. 确保场景中有一个标记为 "MainCamera" 的相机：
   - 如果场景中没有相机，在 Hierarchy 窗口中右键点击 → `GameObject` → `Camera`
   - 选中相机，在 Inspector 窗口的 Tag 下拉菜单中选择 "MainCamera"

#### 第二步：创建 GameManager 对象
如果场景中没有 GameManager 对象，请按以下步骤创建：

1. **创建空游戏对象**：
   - 在 Hierarchy 窗口中右键点击
   - 选择 `GameObject` → `Create Empty`
   - 将对象重命名为 "GameManager"

2. **添加 GameManager 脚本**：
   - 选中 GameManager 对象
   - 在 Inspector 窗口中点击 `Add Component`
   - 搜索 "GameManager" 并添加该组件

3. **配置预制体引用**：
   - 在 Inspector 窗口的 GameManager 组件中，找到以下字段：
     - **Maze Prefab**：将 `Assets/Prefabs/Maze.prefab` 拖拽到此字段
     - **Player Prefab**：将 `Assets/Prefabs/Player.prefab` 拖拽到此字段
   - 相机设置（可选调整）：
     - **Camera Follow Player**：保持勾选（默认 true）
     - **Camera Height**：相机高度（默认 15）
     - **Camera Distance**：相机距离（默认 10）

#### 第三步：运行游戏
1. 点击 Unity 编辑器顶部的 **Play** 按钮（▶️）
2. 游戏会自动开始：
   - 迷宫会自动生成
   - 玩家角色会出现在迷宫中心
   - 相机会自动定位到玩家上方，俯视迷宫

### 游戏操作说明

**移动控制：**
- **W** 或 **↑**：向前移动（朝向当前面向的方向）
- **S** 或 **↓**：向后移动（转向相反方向并移动）
- **A** 或 **←**：向左转并移动
- **D** 或 **→**：向右转并移动
- **Q**：向左转（不移动，仅改变朝向）
- **E**：向右转（不移动，仅改变朝向）

**其他操作：**
- **空格键 (Space)**：重新开始游戏（生成新的随机迷宫）

### 游戏特点
- 🎮 程序化生成的迷宫，每次游戏都不同
- 🏰 包含房间、门、楼梯、树木等元素
- 🎨 多层次的迷宫结构
- 🎯 探索无尽的迷宫世界

### 系统要求
- Unity 6000.1.6f1 或更高版本
- 需要 Unity UI 包（com.unity.ugui）

### 快速设置检查清单
在运行游戏前，请确保：
- ✅ 场景中有标记为 "MainCamera" 的相机
- ✅ 场景中有 GameManager 对象
- ✅ GameManager 的 Maze Prefab 字段已设置为 `Assets/Prefabs/Maze.prefab`
- ✅ GameManager 的 Player Prefab 字段已设置为 `Assets/Prefabs/Player.prefab`

### 常见问题

**Q: 场景中没有 GameManager 对象怎么办？**  
A: 请按照上面的"第二步：创建 GameManager 对象"步骤手动创建。

**Q: 看不到迷宫？**  
A: 检查以下几点：
- 场景中是否有标记为 "MainCamera" 的相机
- GameManager 的 Maze Prefab 和 Player Prefab 是否正确配置
- 查看 Console 窗口是否有错误信息

**Q: 游戏运行时出现空引用错误？**  
A: 确保 GameManager 的 Maze Prefab 和 Player Prefab 字段都已正确设置。

### 注意事项
- 游戏启动时会自动生成迷宫，可能需要几秒钟
- 相机会自动跟随玩家移动
- 如果看不到迷宫，请检查场景中是否有标记为 "MainCamera" 的相机
