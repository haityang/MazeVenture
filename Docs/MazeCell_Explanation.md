# MazeCell 作用详解

## 📋 核心概念

**MazeCell（迷宫单元格）** 是迷宫系统中最基础、最核心的数据结构，代表迷宫网格中的一个**单元格单元**。

---

## 🎯 主要作用

### 1. **迷宫的基本单元（Building Block）**

MazeCell是构成迷宫的**最小单位**，类似于：
- 棋盘上的一个格子
- 地图上的一个区块
- 网格系统中的一个单元格

```
迷宫结构：
┌─────┬─────┬─────┐
│Cell │Cell │Cell │
├─────┼─────┼─────┤
│Cell │Cell │Cell │
├─────┼─────┼─────┤
│Cell │Cell │Cell │
└─────┴─────┴─────┘
```

---

## 🔧 核心功能

### 1. **位置信息管理**

```csharp
public IntVector2 coordinates;  // 单元格在迷宫中的坐标 (x, z)
public int altitude = 0;        // 单元格的高度（用于多层迷宫）
```

**作用：**
- 标识单元格在迷宫中的位置
- 支持多层迷宫（通过altitude）
- 用于计算相邻单元格的位置

**示例：**
```csharp
// 在Maze.cs中创建单元格时设置坐标
newCell.coordinates = coordinates;  // 例如：(5, 3)
newCell.altitude = anAltitude;      // 例如：0（地面层）或1（第二层）
```

---

### 2. **边缘管理（Edge Management）**

```csharp
private MazeCellEdge[] edges = new MazeCellEdge[MazeDirections.Count];  // 4个方向的边缘
```

**作用：**
- 管理单元格**四个方向**的边缘（东、南、西、北）
- 每个边缘可以是：
  - **MazeWall**（墙） - 不可通行
  - **MazePassage**（通道） - 可通行
  - **MazeDoor**（门） - 可开关的通道

**关键方法：**
```csharp
public MazeCellEdge GetEdge(MazeDirection direction)  // 获取指定方向的边缘
public void SetEdge(MazeDirection direction, MazeCellEdge edge)  // 设置边缘
```

**示例：**
```csharp
// 玩家移动时检查边缘是否可通行
MazeCellEdge edge = cell.GetEdge(direction);
if (edge is MazePassage) {
    // 可以通行，移动到相邻单元格
}
```

---

### 3. **房间归属管理**

```csharp
private int roomNumber;  // 单元格所属的房间号

public int GetRoomNumber()  // 获取房间号
public void Initialize(int aRoomNumber)  // 初始化房间号
```

**作用：**
- 标识单元格属于哪个房间
- 支持房间系统（不同房间可以有不同的材质、设置）
- 用于判断两个单元格是否在同一房间

**示例：**
```csharp
// 在迷宫生成时判断是否在同一房间
if (currentCell.GetRoomNumber() == neighbor.GetRoomNumber()) {
    // 同一房间，可以创建通道
    CreatePassageInSameRoom(currentCell, neighbor, direction);
}
```

---

### 4. **附件管理（Accessory）**

```csharp
public MazeCellAccessory accessory = null;  // 单元格附件（如楼梯）
```

**作用：**
- 支持在单元格上放置特殊元素
- 例如：**MazeStairs**（楼梯）- 用于连接不同高度的单元格

**示例：**
```csharp
// 检查附件是否是障碍物
if (null != accessory && accessory.IsObstacle()) {
    result += 1;  // 增加障碍物计数
}
```

---

### 5. **迷宫生成支持**

```csharp
public bool IsFullyInitialized {  // 是否所有边缘都已初始化
    get {
        return initializedEdgeCount == MazeDirections.Count;
    }
}

public MazeDirection RandomUninitializedDirection {  // 随机获取未初始化的方向
    // 用于迷宫生成算法
}
```

**作用：**
- 支持迷宫生成算法（如Prim算法）
- 跟踪哪些方向的边缘还未初始化
- 随机选择未初始化的方向进行扩展

**生成流程：**
```csharp
// 在Maze.cs的生成算法中
MazeCell currentCell = activeCells[currentIndex];
if (currentCell.IsFullyInitialized) {
    // 所有边缘都已初始化，从活动列表中移除
    activeCells.RemoveAt(currentIndex);
    return;
}

// 随机选择一个未初始化的方向
MazeDirection direction = currentCell.RandomUninitializedDirection;
```

---

### 6. **玩家交互事件**

```csharp
public void OnPlayerEntered()   // 玩家进入单元格时调用
public void OnPlayerExited()    // 玩家离开单元格时调用
```

**作用：**
- 处理玩家进入/离开单元格的事件
- 通知所有边缘和附件玩家状态变化
- 可用于触发视觉效果、音效等

**事件传播：**
```csharp
public void OnPlayerEntered() {
    // 通知所有边缘
    for (int i = 0; i < edges.Length; i++) {
        edges[i].OnPlayerEntered();
    }
    // 通知附件（如楼梯）
    if(null != accessory) {
        accessory.OnPlayerEntered();
    }
}
```

**使用示例：**
```csharp
// 在Player.cs中
public void TeleportToCell(MazeCell cell) {
    if (GetCurrentCell() != null) {
        GetCurrentCell().OnPlayerExited();  // 离开旧单元格
    }
    route.Add(cell);
    transform.position = cell.transform.position;
    GetCurrentCell().OnPlayerEntered();  // 进入新单元格
}
```

---

## 📊 数据结构关系图

```
MazeCell (单元格)
├── coordinates (坐标)
├── altitude (高度)
├── roomNumber (房间号)
├── edges[] (边缘数组，4个方向)
│   ├── edges[0] → MazeWall / MazePassage / MazeDoor
│   ├── edges[1] → MazeWall / MazePassage / MazeDoor
│   ├── edges[2] → MazeWall / MazePassage / MazeDoor
│   └── edges[3] → MazeWall / MazePassage / MazeDoor
└── accessory (附件)
    └── MazeStairs (楼梯)
```

---

## 🔄 在游戏流程中的作用

### 1. **迷宫生成阶段**

```
Maze.Generate()
    ↓
CreateCell() → 创建MazeCell实例
    ↓
SetEdge() → 设置边缘（墙/通道/门）
    ↓
Initialize() → 初始化房间号
```

### 2. **玩家移动阶段**

```
Player.MoveDirection()
    ↓
GetDestinationCell() → 获取目标MazeCell
    ↓
GetEdge() → 检查边缘是否可通行
    ↓
MoveToCell() → 添加到路径
    ↓
OnPlayerEntered() / OnPlayerExited() → 触发事件
```

---

## 💡 设计亮点

### 1. **组件化设计**
- MazeCell本身是MonoBehaviour，可以挂载到GameObject
- 通过组合边缘和附件，实现灵活的功能扩展

### 2. **职责单一**
- 只负责管理单元格的状态和连接关系
- 不直接处理游戏逻辑，通过事件系统解耦

### 3. **支持扩展**
- 通过MazeCellEdge继承体系支持不同类型的边缘
- 通过MazeCellAccessory支持不同类型的附件

---

## 🎮 实际使用示例

### 示例1：检查单元格是否可通行

```csharp
// 在Player.cs中
public void MoveDirection(MazeDirection direction) {
    MazeCell cellToMoveFrom = GetDestinationCell();
    MazeCellEdge edge = cellToMoveFrom.GetEdge(direction);
    
    if (edge is MazePassage) {  // 检查是否是通道（可通行）
        // 可以移动
        MoveToCell(edge.otherCell);
    }
    // 如果是MazeWall，则不能移动
}
```

### 示例2：获取相邻单元格

```csharp
// 在Maze.cs中
MazeDirection direction = currentCell.RandomUninitializedDirection;
IntVector2 coordinates = currentCell.coordinates + direction.ToIntVector2();
MazeCell neighbor = GetCell(coordinates);
```

### 示例3：判断房间关系

```csharp
// 在Maze.cs中
if (currentCell.GetRoomNumber() == neighbor.GetRoomNumber() &&
    doorProbability > 0 && 
    currentCell.altitude == neighbor.altitude) {
    // 同一房间，可以创建通道
    CreatePassageInSameRoom(currentCell, neighbor, direction);
}
```

---

## 📝 关键属性总结

| 属性/方法 | 类型 | 作用 |
|----------|------|------|
| `coordinates` | IntVector2 | 单元格在迷宫中的坐标位置 |
| `altitude` | int | 单元格的高度（支持多层） |
| `roomNumber` | int | 单元格所属的房间号 |
| `edges[]` | MazeCellEdge[] | 四个方向的边缘数组 |
| `accessory` | MazeCellAccessory | 单元格附件（如楼梯） |
| `GetEdge()` | MazeCellEdge | 获取指定方向的边缘 |
| `SetEdge()` | void | 设置指定方向的边缘 |
| `OnPlayerEntered()` | void | 玩家进入事件 |
| `OnPlayerExited()` | void | 玩家离开事件 |
| `IsFullyInitialized` | bool | 是否所有边缘都已初始化 |
| `RandomUninitializedDirection` | MazeDirection | 随机未初始化方向 |

---

## 🔍 与其他类的关系

### MazeCell 的依赖关系

```
MazeCell
├── 使用 IntVector2 (坐标)
├── 使用 MazeDirection (方向)
├── 包含 MazeCellEdge[] (边缘)
├── 包含 MazeCellAccessory (附件)
└── 关联 MazeRoom (通过roomNumber)
```

### 使用 MazeCell 的类

```
Maze
├── 创建和管理 MazeCell 数组
├── 在生成算法中使用 MazeCell
└── 通过坐标获取 MazeCell

Player
├── 跟踪当前所在的 MazeCell
├── 通过 MazeCell 检查移动可能性
└── 触发 MazeCell 的事件

MazeCellEdge
├── 引用两个 MazeCell (cell, otherCell)
└── 连接两个单元格
```

---

## 🎯 总结

**MazeCell的核心作用：**

1. ✅ **数据结构** - 存储单元格的位置、高度、房间等信息
2. ✅ **连接管理** - 管理四个方向的边缘（墙/通道/门）
3. ✅ **事件处理** - 处理玩家进入/离开事件
4. ✅ **生成支持** - 支持迷宫生成算法
5. ✅ **扩展支持** - 支持附件（楼梯等）和房间系统

**简单来说：**
> **MazeCell是迷宫的"积木块"，每个单元格管理自己的位置、连接关系和状态，共同构成了整个迷宫结构。**

---

## 💻 代码位置

- **定义**：`Assets/Scripts/MazeCell.cs`
- **使用**：`Assets/Scripts/Maze.cs`（生成）、`Assets/Scripts/Player.cs`（移动）
- **相关**：`Assets/Scripts/MazeCellEdge.cs`、`Assets/Scripts/MazeCellAccessory.cs`

