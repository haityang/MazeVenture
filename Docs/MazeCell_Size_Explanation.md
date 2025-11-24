# MazeCell 大小说明

## 📋 核心答案

**是的，每个MazeCell的大小是一样的（如果使用相同的Prefab）。**

但是，项目中有**两种不同的Prefab类型**，它们可能大小不同：
1. `cellPrefab` - 普通格子
2. `cellTreePrefab` - 带树的格子

---

## 🔍 代码分析

### 1. MazeCell的创建过程

```csharp
// Maze.cs - CreateCell方法
private MazeCell CreateCell(
    IntVector2 coordinates,
    int anAltitude,
    MazeCell aCellPrefab)  // 传入Prefab
{
    // 1. 实例化Prefab（大小由Prefab决定）
    MazeCell newCell = Instantiate(aCellPrefab) as MazeCell;
    
    // 2. 设置属性
    newCell.coordinates = coordinates;
    newCell.altitude = anAltitude;
    newCell.name = "Maze Cell " + coordinates.x + ", " + coordinates.z;
    
    // 3. 设置父对象
    newCell.transform.parent = transform;
    
    // 4. 设置位置（注意：只设置位置，不修改大小）
    newCell.transform.localPosition = new Vector3(
        coordinates.x - size.x * 0.5f + 0.5f,  // X位置
        newCell.altitude * 1.078f,              // Y位置（高度）
        coordinates.z - size.z * 0.5f + 0.5f); // Z位置
    
    // ❌ 没有设置 localScale（缩放）
    // ✅ 所有格子使用Prefab的原始大小
    
    return newCell;
}
```

### 2. 关键发现

**代码中没有修改格子大小的逻辑：**
- ❌ 没有 `transform.localScale` 的设置
- ❌ 没有动态缩放
- ✅ 所有格子直接使用Prefab的原始大小

### 3. 位置计算分析

```csharp
// 位置计算公式
localPosition.x = coordinates.x - size.x * 0.5f + 0.5f
localPosition.z = coordinates.z - size.z * 0.5f + 0.5f
```

**分析：**
- `coordinates.x - size.x * 0.5f`：将格子中心对齐到网格中心
- `+ 0.5f`：每个格子占据**1个单位**的空间
- 这意味着：**所有格子应该占据相同的空间（1x1单位）**

---

## 📐 格子大小规范

### 1. 标准大小

**理论上，所有MazeCell应该：**
- 占据 **1x1 单位**的空间（在X和Z轴上）
- 高度由 `altitude` 决定（Y轴偏移）
- 大小由Prefab的原始设置决定

### 2. 两种Prefab类型

```csharp
// Maze.cs中的Prefab引用
public MazeCell cellPrefab;        // 普通格子Prefab
public MazeCell cellTreePrefab;    // 带树的格子Prefab
```

**使用场景：**
```csharp
// 92-98行：随机选择使用哪个Prefab
if (null != cellTreePrefab && 0 == Random.Range(0, 77)) {
    // 1/77的概率使用带树的格子
    neighbor = CreateCell(coordinates, 0, cellTreePrefab);
} else {
    // 其他情况使用普通格子
    neighbor = CreateCell(coordinates, 0, cellPrefab);
}
```

**注意：**
- 两个Prefab可能大小不同（取决于Prefab的配置）
- 但代码中**没有动态调整大小**
- 如果两个Prefab大小不同，会导致网格不整齐

---

## 🎯 实际大小确定方式

### 1. Prefab决定大小

**MazeCell的大小由以下因素决定：**
1. **Prefab的原始大小**（在Unity Editor中设置）
2. **Prefab中GameObject的Scale**
3. **Prefab中Mesh的大小**

### 2. 代码中的假设

**代码假设所有格子大小一致：**
```csharp
// 位置计算假设每个格子占据1单位空间
localPosition.x = coordinates.x - size.x * 0.5f + 0.5f
//                                    ↑
//                              假设格子大小为1
```

### 3. 网格对齐

**所有格子按网格对齐：**
```
格子 (0,0) 位置: (-size.x/2 + 0.5, altitude, -size.z/2 + 0.5)
格子 (1,0) 位置: (-size.x/2 + 1.5, altitude, -size.z/2 + 0.5)
格子 (0,1) 位置: (-size.x/2 + 0.5, altitude, -size.z/2 + 1.5)
...
```

**每个格子之间的间距 = 1单位**

---

## ⚠️ 潜在问题

### 1. Prefab大小不一致

**如果两个Prefab大小不同：**
- `cellPrefab` 可能是 1x1 单位
- `cellTreePrefab` 可能是 1.2x1.2 单位（如果树更大）
- **结果：** 网格不整齐，格子可能重叠或间隙

### 2. 解决方案

**确保所有Prefab大小一致：**
1. 在Unity Editor中检查Prefab的Scale
2. 确保所有Prefab的根GameObject Scale = (1, 1, 1)
3. 通过Mesh大小控制实际尺寸，而不是Scale

---

## 📊 大小相关代码总结

### 代码中涉及大小的部分

| 代码位置 | 作用 | 是否修改大小 |
|---------|------|------------|
| `CreateCell()` | 创建格子 | ❌ 否 |
| `transform.localPosition` | 设置位置 | ❌ 否 |
| `transform.localScale` | 设置缩放 | ❌ 不存在 |
| Prefab实例化 | 创建对象 | ✅ 使用Prefab大小 |

### 高度处理

```csharp
// 高度由altitude决定，不是大小
newCell.altitude = anAltitude;
transform.localPosition.y = altitude * 1.078f;  // 高度偏移
```

**注意：** `altitude` 影响**位置**，不影响**大小**

---

## 🔧 如何检查格子大小

### 方法1：在Unity Editor中检查

1. 打开 `Assets/Prefabs/MazeCellA.prefab`
2. 查看根GameObject的Transform
3. 检查Scale是否为 (1, 1, 1)
4. 检查Mesh的大小

### 方法2：运行时检查

```csharp
// 可以在CreateCell后添加调试代码
Debug.Log($"Cell size: {newCell.transform.localScale}");
Debug.Log($"Cell bounds: {GetComponent<Renderer>().bounds.size}");
```

### 方法3：查看Prefab文件

检查Prefab的YAML文件，查看Transform组件：
```yaml
Transform:
  m_LocalScale: {x: 1, y: 1, z: 1}  # 应该是1,1,1
```

---

## 💡 最佳实践

### 1. 统一Prefab大小

**确保所有MazeCell Prefab：**
- Scale = (1, 1, 1)
- 实际大小通过Mesh控制
- 占据1x1单位空间

### 2. 如果需要不同大小

**如果确实需要不同大小的格子：**
```csharp
// 可以在CreateCell中添加
if (aCellPrefab == cellTreePrefab) {
    newCell.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
}
```

**但要注意：**
- 需要调整位置计算
- 可能影响边缘对齐
- 需要重新计算网格间距

---

## 📝 总结

### 核心结论

1. ✅ **相同Prefab创建的格子大小相同**
2. ⚠️ **不同Prefab可能大小不同**（取决于Prefab配置）
3. ✅ **代码中没有动态修改大小**
4. ✅ **位置计算假设格子大小为1单位**

### 关键点

- **大小由Prefab决定**，不是代码
- **位置由坐标计算**，假设格子大小为1
- **高度由altitude决定**，不影响大小
- **两个Prefab类型**可能大小不同

### 建议

**在Unity Editor中：**
1. 检查所有MazeCell Prefab的Scale
2. 确保它们都是 (1, 1, 1)
3. 通过Mesh大小控制实际尺寸
4. 确保所有格子占据相同的空间

---

## 🔍 相关代码位置

- **创建格子**：`Assets/Scripts/Maze.cs` 第112-129行
- **Prefab引用**：`Assets/Scripts/Maze.cs` 第10-12行
- **位置计算**：`Assets/Scripts/Maze.cs` 第123-126行
- **Prefab文件**：
  - `Assets/Prefabs/MazeCellA.prefab`
  - `Assets/Prefabs/MazeCellTreeA.prefab`

