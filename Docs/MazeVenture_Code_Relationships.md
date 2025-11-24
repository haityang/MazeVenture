# MazeVenture 代码关系图

## 类继承关系

```mermaid
classDiagram
    class MazeCellEdge {
        <<abstract>>
        MazeCell cell
        MazeCell otherCell
        MazeDirection direction
        virtual void Initialize()
    }
    
    class MazeWall {
        override void Initialize()
    }
    
    class MazePassage {
        override void Initialize()
    }
    
    class MazeDoor {
        HingeJoint hinge
        override void Initialize()
    }
    
    class MazeCellAccessory {
        <<abstract>>
        MazeCell cell
        virtual bool IsPassable()
    }
    
    class MazeStairs {
        override bool IsPassable()
    }
    
    MazeCellEdge <|-- MazeWall
    MazeCellEdge <|-- MazePassage
    MazePassage <|-- MazeDoor
    MazeCellAccessory <|-- MazeStairs
```

## 主要类依赖关系

```mermaid
graph TD
    GameManager --> Maze
    GameManager --> Player
    Maze --> MazeCell
    Maze --> MazeCellEdge
    Maze --> MazeWall
    Maze --> MazePassage
    Maze --> MazeDoor
    Maze --> MazeStairs
    Maze --> MazeRoom
    Maze --> MazeRoomSettings
    Player --> MazeCell
    Player --> MazeCellEdge
    Player --> MazeDirection
    MazeCell --> MazeCellEdge
    MazeCell --> MazeCellAccessory
    MazeCell --> MazeRoom
    MazeCellEdge --> MazeCell
    MazeCellEdge --> MazeDirection
    MazePassage --> MazeCellEdge
    MazeWall --> MazeCellEdge
    MazeDoor --> MazePassage
    MazeCellAccessory --> MazeCell
    MazeStairs --> MazeCellAccessory
    MazeDirection --> IntVector2
    MazeCell --> IntVector2
    Maze --> IntVector2
```

## 游戏流程调用链

```mermaid
graph LR
    GameManager.Start --> GameManager.BeginGame
    GameManager.BeginGame --> Maze.Generate
    Maze.Generate --> Maze.CreateCell
    Maze.Generate --> Maze.CreatePassage
    Maze.Generate --> Maze.CreateWall
    GameManager.BeginGame --> Player.TeleportToCell
    Player.MoveDirection --> MazeCell.GetEdge
    MazeCell.GetEdge --> MazeCellEdge
    MazeCellEdge --> MazePassage
    Player.MoveToCell --> MazeCell.OnPlayerEntered
    Player.MoveToCell --> MazeCell.OnPlayerExited
```

## 核心组件说明

1. **基础组件**
   - `IntVector2`: 用于表示二维网格坐标
   - `MazeDirection`: 提供方向枚举及相关工具方法

2. **迷宫结构组件**
   - `MazeCell`: 迷宫单元格，包含坐标、房间号等信息
   - `MazeCellEdge`: 单元格边缘基类，派生出墙体、通道和门
   - `MazeWall`: 不可通行的墙体
   - `MazePassage`: 可通行的通道
   - `MazeDoor`: 可开关的门

3. **迷宫装饰组件**
   - `MazeCellAccessory`: 单元格附件基类
   - `MazeStairs`: 楼梯组件

4. **房间系统**
   - `MazeRoom`: 房间管理类
   - `MazeRoomSettings`: 房间设置类

5. **游戏控制组件**
   - `Maze`: 迷宫生成和管理核心类
   - `Player`: 玩家控制类
   - `GameManager`: 游戏管理器，负责初始化游戏和重启游戏