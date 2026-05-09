# UGUI Visual Hotspot System - 可视化热区交互系统

![Unity Version](https://img.shields.io/badge/Unity-2021.3%2B-blue.svg) ![License](https://img.shields.io/badge/License-MIT-green.svg)

为 Unity UGUI 设计的终极热区解决方案。无需编码，即可在任意 UI 图形上可视化地创建和管理交互区域。

---

## 简介 (Introduction)

`UGUI Visual Hotspot System` 是一个强大、直观的编辑器扩展工具，它彻底改变了在 `Unity UI` 中创建交互区域的方式。忘记繁琐的脚本和坐标计算吧！现在，设计师和开发者可以直接在 `Scene` 视图中“绘制”出点击区域，像操作普通 `GameObject` 一样轻松，并为每个区域无缝绑定自定义交互事件。它旨在极大提升开发效率，为复杂的 `UI` 交互提供一个优雅而高效的工作流。

## 核心特性 (Core Features)

*   **直观的场景内编辑 (Intuitive In-Scene Editing)**
    告别繁琐的坐标调整。直接在 `Scene` 视图中通过拖拽、缩放和移动控制柄来创建和定义热区，实现真正的所见即所得。

*   **广泛的组件支持 (Broad Component Support)**
    完美兼容 `Unity UI` 的 `Image` 和 `RawImage` 组件，并能智能处理 `Sprite Atlas（图集）`中的精灵，确保热区定位在任何情况下都精准无误。

*   **高性能运行时 (High-Performance Runtime)**
    经过优化的轻量级点击检测机制，确保在运行时对性能的影响降至最低，即使在复杂界面和多热区场景下依然流畅。

*   **强化的 Inspector 面板 (Enhanced Inspector Panel)**
    提供清晰、高效的热区管理界面。支持一键添加、复制和删除，并使用可折叠列表来组织，即使管理大量热区也能保持井然有序。

*   **无缝的事件集成 (Seamless Event Integration)**
    每个热区都暴露了独立的 `UnityEvent`，让你可以像使用 Button 组件一样，在 Inspector 中轻松拖拽绑定任何对象的任何公共函数，无需编写一行代码即可实现复杂的交互逻辑。

*   **完整的编辑器工作流 (Full Editor Workflow)**
    所有在编辑器中的操作——包括创建、删除、复制、移动和缩去——均完全支持标准的撤销与重做（`Undo/Redo`），保障了安全、无忧的编辑流程。

## 适用场景 (Use Cases)

*   **交互式地图：** 在世界地图或区域地图上创建可点击的城市、国家或兴趣点。
*   **角色装备界面：** 为角色预览图的不同部位（如头盔、盔甲、武器）添加点击区域，以查看或更换装备。
*   **复杂的仪表盘/控制台：** 在一张背景图上制作多个可交互的按钮或显示区域。
*   **益智解谜游戏：** 如“找不同”或“隐藏物品”游戏中，快速定义目标物品的点击范围。
*   **全景图/展厅导览：** 在 360 度全景图上标记出可点击的导航点或信息点。

## 安装 (Installation)

1.  下载本项目。
2.  将 `VisualHotspotSystem.cs` 和 `VisualHotspotSystemEditor.cs` 文件导入到你的 Unity 项目中。
3.  **重要：** 确保 `VisualHotspotSystemEditor.cs` 文件位于项目中的 `Editor` 文件夹下。如果你的项目没有这个文件夹，请在 Assets 根目录下创建一个。

## 如何使用 (How to Use)

1.  在你的 `Canvas` 下创建一个 `Image` 或 `RawImage` 对象。
2.  将 `VisualHotspotSystem` 组件添加到该对象上。组件会自动尝试获取 `Image` 或 `RawImage` 作为 `Target Graphic`。
3.  在 `VisualHotspotSystem` 组件的 `Inspector` 面板中，点击 "添加新热区" 按钮。
4.  切换到 `Scene` 视图，你会看到一个默认的热区矩形。
5.  **编辑热区：**
    *   拖动矩形的**四个角点**可以调整其大小和形状。
    *   拖动矩形的**黄色中心手柄**可以整体移动该热区。
6.  回到 `Inspector` 面板，展开你创建的热区，可以在 `OnClick` 事件列表中绑定点击后需要执行的函数。
7.  运行游戏，点击你定义的热区，观察绑定的事件是否被触发！

## 许可证 (License)

本项目采用 [MIT License](LICENSE.md)。
