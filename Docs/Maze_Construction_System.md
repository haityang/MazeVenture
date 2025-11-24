# 迷宫构建系统完整解析

## 📋 目录
1. [构建迷宫需要的核心元素](#1-构建迷宫需要的核心元素)
2. [各元素的作用和特性](#2-各元素的作用和特性)
3. [元素之间的交互关系](#3-元素之间的交互关系)
4. [迷宫格子和房间的关系](#4-迷宫格子和房间的关系)
5. [MazeEdge的作用详解](#5-mazeedge的作用详解)
6. [迷宫生成流程](#6-迷宫生成流程)

---

## 1. 构建迷宫需要的核心元素

### 1.1 基础元素层次结构

```
Maze (迷宫)
├── MazeCell (迷宫格子) - 基础单元
│   ├── MazeCellEdge[] (边缘数组，4个方向)
│   │   ├── MazeWall (墙) - 不可通行
│   │   ├── MazePassage (过道) - 可通行
│   │   └── MazeDoor (门) - 可开关的过道
│   └── MazeCellAccessory (附件)
│       └── MazeStairs (楼梯) - 连接不同高度
└── MazeRoom (房间) - 格子集合
    └── MazeRoomSettings (房间设置)
```

### 1.2 核心元素清单

| 元素 | 类型 | 作用 |
|------|------|------|
| **Maze** | 管理器 | 整个迷宫的管理和生成 |
| **MazeCell** | 基础单元 | 迷宫的最小单位，一个格子 |
| **MazeCellEdge** | 边缘基类 | 连接两个格子的边缘 |
| **MazeWall** | 边缘实现 | 不可通行的墙 |
| **MazePassage** | 边缘实现 | 可通行的过道 |
| **MazeDoor** | 边缘实现 | 可开关的门（继承自MazePassage） |
| **MazeCellAccessory** | 附件基类 | 格子上的特殊元素 |
| **MazeStairs** | 附件实现 | 楼梯，连接不同高度 |
| **MazeRoom** | 房间 | 多个格子的集合 |
| **MazeDirection** | 方向枚举 | 东、南、西、北四个方向 |

---

## 2. 各元素的作用和特性

### 2.1 MazeCell（迷宫格子）

**作用：** 迷宫的基本单元，类似棋盘上的一个格子

**核心属性：**
```csharp
public IntVector2 coordinates;      // 坐标位置 (x, z)
public int altitude = 0;             // 高度（支持多层迷宫）
public int roomNumber;               // 所属房间号
private MazeCellEdge[] edges;        // 四个方向的边缘
public MazeCellAccessory accessory;  // 附件（如楼梯）
```

**关键功能：**
- 管理四个方向的边缘（东、南、西、北）
- 存储位置和高度信息
- 处理玩家进入/离开事件
- 支持迷宫生成算法

---

### 2.2 MazeCellEdge（边缘系统）

**作用：** 连接两个相邻格子的边缘，决定是否可通行

**继承体系：**
```
MazeCellEdge (抽象基类)
├── MazeWall (墙) - IsPassable() = false
├── MazePassage (过道) - IsPassable() = true
└── MazeDoor (门) - IsPassable() = 动态（根据门的角度）
```

#### 2.2.1 MazeWall（墙）

**特性：**
- ❌ **不可通行** (`IsPassable() = false`)
- ✅ **是障碍物** (`IsObstacle() = true`)
- 用于分隔格子，阻止玩家移动

**使用场景：**
```csharp
// 在Maze.cs中创建墙
private void CreateWall(MazeCell cell, MazeCell otherCell, MazeDirection direction)
{
    MazeWall wall = Instantiate(wallPrefabs[...]) as MazeWall;
    wall.Initialize(cell, otherCell, direction);
}
```

#### 2.2.2 MazePassage（过道）

**特性：**
- ✅ **可通行** (`IsPassable() = true`)
- ❌ **不是障碍物** (`IsObstacle() = false`)
- 连接两个格子，允许玩家自由通过

**使用场景：**
```csharp
// 创建普通过道（同一房间内）
private void CreatePassageInSameRoom(MazeCell cell, MazeCell otherCell, MazeDirection direction)
{
    MazePassage passage = Instantiate(passagePrefab) as MazePassage;
    passage.Initialize(cell, otherCell, direction);
}
```

#### 2.2.3 MazeDoor（门）

**特性：**
- ✅ **可通行**（但需要打开）
- ✅ **是障碍物**（关闭时）
- **动态通行性**：根据门的角度判断
  ```csharp
  public override bool IsPassable() {
      return (currentAngle > 70);  // 门打开角度>70度时可通行
  }
  ```
- **自动开关**：玩家进入时打开，离开时关闭
  ```csharp
  public override void OnPlayerEntered() {
      targetAngle = 90f;  // 打开门
      StartCoroutine("UpdateDoorAngle");
  }
  ```

**使用场景：**
```csharp
// 在不同房间之间创建门
if (Random.value < doorProbability) {
    prefab = doorPrefab;  // 使用门而不是普通过道
    otherCell.Initialize(cell.GetRoomNumber() + 1);  // 新房间
}
```

---

### 2.3 MazeCellAccessory（附件系统）

**作用：** 在格子上放置特殊元素

#### 2.3.1 MazeStairs（楼梯）

**特性：**
- ✅ **可通行** (`IsPassable() = true`)
- ✅ **是障碍物** (`IsObstacle() = true`)
- **连接不同高度**：从低层（altitude=0）到高层（altitude=1）

**楼梯结构：**
```
低着陆点 (lowLandingCell) - altitude=0
    ↓
楼梯所在格子 (cell) - 有MazeStairs附件
    ↓
高着陆点 (highLandingCell) - altitude=1
```

**创建条件：**
```csharp
private bool CanCreateStairs(MazeCell cell, MazeCell otherCell, MazeDirection direction)
{
    // 1. 必须有楼梯Prefab
    // 2. 当前格子必须在altitude=0
    // 3. 低着陆点必须在同一房间
    // 4. 高着陆点位置必须为空
}
```

---

### 2.4 MazeRoom（房间）

**作用：** 将多个格子组织成房间，可以有不同的外观设置

**核心属性：**
```csharp
public int settingsIndex;           // 房间设置索引
public MazeRoomSettings settings;    // 房间外观设置（材质等）
private List<MazeCell> cells;       // 房间包含的格子列表
```

**功能：**
- 管理一组格子
- 可以合并房间（`Assimilate`）
- 可以显示/隐藏房间（`Show`/`Hide`）

---

## 3. 元素之间的交互关系

### 3.1 整体关系图

```
┌─────────────────────────────────────────┐
│              Maze (迷宫)                 │
│  ┌───────────────────────────────────┐  │
│  │    MazeCell[,] (格子二维数组)      │  │
│  │                                   │  │
│  │  ┌──────────┐  ┌──────────┐     │  │
│  │  │ MazeCell │  │ MazeCell │     │  │
│  │  │          │  │          │     │  │
│  │  │ edges[]  │  │ edges[]  │     │  │
│  │  │ [N,E,S,W]│  │ [N,E,S,W]│     │  │
│  │  │          │  │          │     │  │
│  │  │ ┌──────┐ │  │ ┌──────┐ │     │  │
│  │  │ │Edge  │─┼──┼─│Edge  │ │     │  │
│  │  │ │Wall/ │ │  │ │Pass/ │ │     │  │
│  │  │ │Door  │ │  │ │Door  │ │     │  │
│  │  │ └──────┘ │  │ └──────┘ │     │  │
│  │  │          │  │          │     │  │
│  │  │accessory │  │          │     │  │
│  │  │Stairs    │  │          │     │  │
│  │  └──────────┘  └──────────┘     │  │
│  │                                   │  │
│  └───────────────────────────────────┘  │
│                                         │
│  ┌───────────────────────────────────┐  │
│  │    MazeRoom[] (房间数组)           │  │
│  │  Room 0: [Cell1, Cell2, ...]     │  │
│  │  Room 1: [Cell5, Cell6, ...]     │  │
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘
```

### 3.2 边缘连接关系

**关键点：** 一个边缘连接**两个**格子

```csharp
public class MazeCellEdge {
    public MazeCell cell;        // 边缘所属的格子
    public MazeCell otherCell;   // 边缘连接的另一个格子
    public MazeDirection direction;  // 边缘的方向
}
```

**初始化过程：**
```csharp
public virtual void Initialize(MazeCell cell, MazeCell otherCell, MazeDirection direction)
{
    this.cell = cell;
    this.otherCell = otherCell;
    this.direction = direction;
    
    // 在cell上设置这个边缘
    cell.SetEdge(direction, this);
    
    // 在otherCell上设置相反方向的边缘（同一个边缘对象）
    if(null != otherCell) {
        otherCell.SetEdge(direction.GetOpposite(), this);
    }
}
```

**示例：**
```
格子A (0,0)                   格子B (1,0)
┌─────────┐                  ┌─────────┐
│         │                  │         │
│    A    │───MazePassage───→│    B    │
│         │   (East方向)     │         │
└─────────┘                  └─────────┘
     ↑                            ↑
     │                            │
  edges[East]                  edges[West]
  (同一个MazePassage对象)
```

### 3.3 玩家移动时的交互

```
玩家移动流程：
1. Player.MoveDirection(direction)
   ↓
2. GetDestinationCell() → 获取当前目标格子
   ↓
3. cell.GetEdge(direction) → 获取边缘
   ↓
4. if (edge is MazePassage) → 检查是否可通行
   ↓
5. MoveToCell(otherCell) → 移动到相邻格子
   ↓
6. cell.OnPlayerExited() → 触发离开事件
   ↓
7. newCell.OnPlayerEntered() → 触发进入事件
   ↓
8. edge.OnPlayerEntered() → 边缘处理（如门自动打开）
```

---

## 4. 迷宫格子和房间的关系

### 4.0 Maze.cells 数组详解

**重要问题：Maze里存的cell，是全部房间的全部Cell吗？**

**答案：是的！** `Maze.cells` 数组包含**所有已创建的房间的所有格子**。

#### 4.0.1 cells数组的结构

```csharp
// Maze.cs
private MazeCell[,] cells;  // 二维数组，按坐标索引

// 初始化
cells = new MazeCell[size.x, size.z];  // 大小为 size.x * size.z

// 存储格子
cells[coordinates.x, coordinates.z] = newCell;  // 按坐标存储

// 获取格子
public MazeCell GetCell(IntVector2 coordinates) {
    return cells[coordinates.x, coordinates.z];
}
```

#### 4.0.2 存储方式

**Maze.cells 是按坐标索引的二维数组：**
- ✅ **包含所有已创建的格子**（无论属于哪个房间）
- ✅ **按坐标位置索引**：`cells[x, z]` = 坐标(x,z)的格子
- ⚠️ **可能包含null**：不是所有坐标位置都有格子（取决于生成算法）
- ✅ **数组索引 = 坐标值**：坐标(3,4)的格子存储在`cells[3, 4]`（直接对应，不减1）

**重要说明：**
- **数组大小**：在程序开始时就知道（size.x * size.z）
- **实际格子数**：未知，取决于生成算法（通常 < 数组大小）
- **生成方式**：逐渐生成（渐进式），不是一次性全部生成

**示例：**
```csharp
// 假设迷宫大小是 5x5
cells = new MazeCell[5, 5];

// 创建格子时存储
cells[2, 3] = newCell;  // 坐标(2,3)的格子
cells[1, 1] = newCell;  // 坐标(1,1)的格子

// 获取格子
MazeCell cell = cells[2, 3];  // 获取坐标(2,3)的格子
```

#### 4.0.3 与房间的关系

**关键点：**
1. **Maze.cells** = 按坐标索引的所有格子（包含所有房间）
2. **MazeRoom.cells** = 按房间组织的格子列表（每个房间有自己的列表）

**两种存储方式：**
```
Maze.cells[,] (按坐标索引)
├── cells[0,0] → Cell (Room 0)
├── cells[0,1] → Cell (Room 0)
├── cells[1,0] → Cell (Room 1)  ← 不同房间
├── cells[1,1] → Cell (Room 1)
└── ...

MazeRoom.cells[] (按房间组织)
├── Room 0: [Cell(0,0), Cell(0,1), ...]
└── Room 1: [Cell(1,0), Cell(1,1), ...]
```

**注意：**
- `Maze.cells` 是**主存储**，包含所有格子
- `MazeRoom.cells` 是**辅助列表**，用于房间管理
- 同一个格子对象同时存在于两个地方（不同的数据结构）

#### 4.0.4 为什么需要两种存储？

**Maze.cells（按坐标索引）：**
- ✅ 快速通过坐标查找格子：`GetCell(coordinates)`
- ✅ 检查相邻格子是否存在
- ✅ 支持网格遍历

**MazeRoom.cells（按房间组织）：**
- ✅ 快速访问房间内的所有格子
- ✅ 批量操作（显示/隐藏房间）
- ✅ 房间合并操作

### 4.1 关系概述

**MazeCell（格子）和 MazeRoom（房间）是多对一的关系：**

```
MazeRoom (房间)
├── MazeCell 1 (格子1)
├── MazeCell 2 (格子2)
├── MazeCell 3 (格子3)
└── ... (更多格子)
```

### 4.2 房间号系统

**每个格子都有一个房间号：**
```csharp
public class MazeCell {
    private int roomNumber;  // 所属房间号
    
    public int GetRoomNumber() {
        return roomNumber;
    }
    
    public void Initialize(int aRoomNumber) {
        roomNumber = aRoomNumber;
    }
}
```

### 4.3 房间的创建规则

#### 规则1：普通过道 - 同一房间
```csharp
// 如果两个格子在同一房间，创建普通过道
if (currentCell.GetRoomNumber() == neighbor.GetRoomNumber()) {
    CreatePassageInSameRoom(currentCell, neighbor, direction);
    // 两个格子保持在同一房间
}
```

#### 规则2：门 - 不同房间
```csharp
// 如果创建门，新格子属于新房间
if (Random.value < doorProbability) {
    prefab = doorPrefab;
    otherCell.Initialize(cell.GetRoomNumber() + 1);  // 新房间号
}
```

#### 规则3：楼梯 - 特殊房间
```csharp
// 楼梯连接不同高度，高着陆点属于新房间
if (CanCreateStairs(...)) {
    highLandingCell.Initialize(otherCell.GetRoomNumber() + 1);
    // 高着陆点属于新房间
}
```

### 4.4 房间的作用

1. **视觉区分**：不同房间可以使用不同的材质
   ```csharp
   public class MazeRoomSettings {
       public Material floorMaterial;  // 地板材质
       public Material wallMaterial;   // 墙壁材质
   }
   ```

2. **逻辑分组**：将相关格子组织在一起

3. **生成控制**：控制迷宫的生成规则
   - 同一房间内的格子更容易连通
   - 不同房间之间通过门连接

---

## 5. MazeEdge的作用详解

### 5.1 核心作用

**MazeCellEdge（边缘）是连接两个格子的"桥梁"，决定了玩家能否在两个格子之间移动。**

### 5.2 三个关键功能

#### 功能1：通行性判断
```csharp
public virtual bool IsPassable() {
    return true;  // 基类默认可通行
}

// MazeWall重写
public override bool IsPassable() {
    return false;  // 墙不可通行
}

// MazeDoor重写
public override bool IsPassable() {
    return (currentAngle > 70);  // 门打开时可通行
}
```

**使用场景：**
```csharp
// 在Player.cs中
MazeCellEdge edge = cell.GetEdge(direction);
if (edge is MazePassage) {  // 或者 edge.IsPassable()
    // 可以移动
    MoveToCell(edge.otherCell);
}
```

#### 功能2：双向连接
```csharp
// 一个边缘对象同时属于两个格子
edge.Initialize(cellA, cellB, direction);
// cellA.edges[East] = edge
// cellB.edges[West] = edge
// 两个格子共享同一个边缘对象
```

**优势：**
- 节省内存（一个边缘对象连接两个格子）
- 保证一致性（修改一个边缘，两个格子都能感知）

#### 功能3：事件响应
```csharp
public virtual void OnPlayerEntered() {}
public virtual void OnPlayerExited() {}
```

**实际应用：**
```csharp
// MazeDoor自动开关
public override void OnPlayerEntered() {
    targetAngle = 90f;  // 打开门
    StartCoroutine("UpdateDoorAngle");
}

public override void OnPlayerExited() {
    targetAngle = 0f;   // 关闭门
    StartCoroutine("UpdateDoorAngle");
}
```

### 5.3 边缘的初始化流程

```
创建边缘流程：
1. Maze.CreatePassage(cell, otherCell, direction)
   ↓
2. Instantiate(passagePrefab) → 创建边缘对象
   ↓
3. passage.Initialize(cell, otherCell, direction)
   ↓
4. cell.SetEdge(direction, passage)
   ↓
5. otherCell.SetEdge(oppositeDirection, passage)
   ↓
6. 边缘对象成为cell的子对象
   ↓
7. 设置边缘的位置和旋转
```

### 5.4 边缘类型对比

| 特性 | MazeWall | MazePassage | MazeDoor |
|------|----------|-------------|----------|
| **可通行** | ❌ 否 | ✅ 是 | ✅ 是（打开时） |
| **是障碍物** | ✅ 是 | ❌ 否 | ✅ 是 |
| **自动交互** | ❌ 无 | ❌ 无 | ✅ 自动开关 |
| **使用场景** | 分隔格子 | 连接格子 | 连接不同房间 |
| **继承关系** | 直接继承Edge | 直接继承Edge | 继承Passage |

---

## 6. 迷宫生成流程

### 6.1 完整生成流程

```
1. Maze.Generate() 开始
   ↓
2. 创建格子二维数组 cells[,] (按坐标索引，包含所有房间的所有格子)
   - 数组大小 = size.x * size.z (已知)
   - 但实际格子数 < 数组大小 (未知，取决于生成算法)
   - 初始状态：所有位置都是null
   ↓
3. DoFirstGenerationStep()
   - 随机选择一个位置创建第一个格子
   - 初始化房间号为0
   - 加入活动列表
   - 注意：格子是逐渐生成的，不是一次性全部生成
   ↓
4. DoNextGenerationStep() (循环，逐步生成)
   ├─ 从活动列表取一个格子
   ├─ 检查是否所有边缘都已初始化
   │  ├─ 是 → 从活动列表移除
   │  └─ 否 → 继续
   ├─ 随机选择一个未初始化的方向
   ├─ 计算相邻格子坐标
   ├─ 检查相邻位置
   │  ├─ 在迷宫范围内？
   │  │  ├─ 是 → 检查相邻格子是否存在
   │  │  │  ├─ 不存在 → 创建新格子
   │  │  │  │  └─ 创建过道连接
   │  │  │  └─ 存在 → 检查房间关系
   │  │  │     ├─ 同一房间 → 创建过道
   │  │  │     └─ 不同房间 → 创建墙
   │  │  └─ 否 → 创建边界墙
   │  └─ 重复直到活动列表为空
   ↓
5. 生成完成
```

### 6.2 边缘创建决策树

```
需要创建边缘？
├─ 相邻格子不存在
│  └─ 创建新格子 + 创建过道
│
├─ 相邻格子存在
│  ├─ 同一房间？
│  │  ├─ 是 → 创建过道（CreatePassageInSameRoom）
│  │  └─ 否 → 创建墙
│  │
│  └─ 不同房间？
│     ├─ 随机创建门？
│     │  ├─ 是 → 创建门（MazeDoor）
│     │  └─ 否 → 创建墙
│     │
│     └─ 可以创建楼梯？
│        └─ 是 → 创建楼梯（MazeStairs）
│
└─ 边界外
   └─ 创建边界墙
```

### 6.3 楼梯创建的特殊流程

```
创建楼梯流程：
1. 检查CanCreateStairs()
   - 当前格子altitude=0
   - 低着陆点在迷宫内且在同一房间
   - 高着陆点位置为空
   ↓
2. 创建楼梯附件
   - cell.accessory = Instantiate(stairsPrefab)
   - 附加到当前格子
   ↓
3. 创建高着陆点格子
   - 位置：otherCell坐标 + direction
   - 高度：altitude=1
   - 房间号：otherCell房间号 + 1
   ↓
4. 创建高着陆点过道
   - 连接otherCell和高着陆点
   ↓
5. 创建支撑柱和栏杆
   - 在otherCell上创建柱子
   - 在高着陆点上创建栏杆
```

---

## 7. 实际使用示例

### 7.1 玩家移动示例

```csharp
// Player.cs
public void MoveDirection(MazeDirection direction) {
    // 1. 获取目标格子
    MazeCell cellToMoveFrom = GetDestinationCell();
    
    // 2. 获取边缘
    MazeCellEdge edge = cellToMoveFrom.GetEdge(direction);
    
    // 3. 检查是否可通行
    if (edge is MazePassage) {
        // 4. 确定目标格子
        if (cellToMoveFrom == edge.otherCell) {
            MoveToCell(edge.cell);
        } else {
            MoveToCell(edge.otherCell);
        }
    }
    // 如果是MazeWall，则不能移动
}
```

### 7.2 门自动开关示例

```csharp
// MazeDoor.cs
public override void OnPlayerEntered() {
    StopCoroutine("UpdateDoorAngle");
    targetAngle = 90f;  // 目标：打开90度
    StartCoroutine("UpdateDoorAngle");  // 开始动画
}

public override bool IsPassable() {
    return (currentAngle > 70);  // 打开超过70度时可通行
}
```

### 7.3 房间判断示例

```csharp
// Maze.cs
if (currentCell.GetRoomNumber() == neighbor.GetRoomNumber() &&
    doorProbability > 0 && 
    currentCell.altitude == neighbor.altitude) {
    // 同一房间，创建普通过道
    CreatePassageInSameRoom(currentCell, neighbor, direction);
} else {
    // 不同房间或不同高度，创建墙
    CreateWall(currentCell, neighbor, direction);
}
```

---

## 8. 总结

### 8.1 核心概念

1. **MazeCell（格子）** = 迷宫的基本单元
2. **MazeCellEdge（边缘）** = 连接两个格子的桥梁
3. **MazeRoom（房间）** = 多个格子的逻辑分组
4. **MazeCellAccessory（附件）** = 格子上的特殊元素

### 8.2 关键关系

- **格子 ↔ 边缘**：一个格子有4个边缘（4个方向）
- **边缘 ↔ 格子**：一个边缘连接2个格子（双向）
- **格子 ↔ 房间**：多个格子属于一个房间（多对一）
- **格子 ↔ 附件**：一个格子可以有1个附件（如楼梯）

### 8.3 设计亮点

1. **组件化设计**：通过边缘和附件系统实现灵活扩展
2. **双向连接**：边缘同时属于两个格子，保证一致性
3. **多态设计**：通过继承实现不同类型的边缘和附件
4. **事件驱动**：通过OnPlayerEntered/Exited实现交互

### 8.4 快速记忆

```
迷宫 = 格子网格
格子 = 4个边缘 + 可选附件
边缘 = 墙/过道/门（决定是否可通行）
房间 = 多个格子的集合（视觉和逻辑分组）
附件 = 楼梯等特殊元素（连接不同高度）
```

---

**记住：边缘是连接格子的关键，决定了迷宫的连通性和玩家的移动路径！**

