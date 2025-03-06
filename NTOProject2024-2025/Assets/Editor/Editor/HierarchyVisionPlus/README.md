# HierarchyVision Plus v1.0.1

A powerful Unity editor extension package that provides enhanced Hierarchy and Project view functionality.

## Features

### Hierarchy View Enhancement
- Advanced filtering and search functionality
- Object color management
- Quick material preview and renaming
- Custom node icons (disabled by default)
- Tree connection line visualization
- Material preview master switch (New)
- Performance optimization and memory management (Enhanced)

### Project View Enhancement
- Custom folder colors (Using GUID storage, supports renaming)
- Folder connection line visualization (Optimized rendering performance)
- Preset color picker (Integrated into unified color picker)

## Version Requirements
- Unity 2020.3.0f1 or higher
- Tested on the following versions:
  - Unity 2020.3 LTS
  - Unity 2021.3 LTS
  - Unity 2022.3 LTS

## Installation Steps
1. In Unity Editor, select Assets > Import Package > Custom Package
2. Select the downloaded EditorExtension.unitypackage file
3. Ensure all files are selected and click "Import"

## Feature Details

### 1. Component Icons in Hierarchy
- **Default State**: Disabled
- **How to Enable**: 
  - Via Menu: Tools > Hierarchy > Show Component Icons
  - Via Shortcut: Ctrl + Shift + ~ (tilde)
- **Description**: Displays component icons next to GameObjects in the Hierarchy window
- **Performance**: Automatically caches component information for better performance

### 2. Hierarchy Filter Mode
- **Shortcut**: `~` (tilde)
- **Function**: Toggle Hierarchy filter mode (Hierarchy window shows only selected nodes)

### 3. Scene Filter Mode
- **Shortcut**: `Alt + ~`
- **Function**: Toggle Scene filter mode (Scene window shows only selected nodes)

### 4. Color Management
- **Node Colors**: Hold `Alt` + click node icon in Hierarchy window
- **Folder Colors**: Hold `Alt` + click folder icon in Project window
- **Color Storage**: Using GUID storage, supports folder renaming (New)

### 5. Material Operations
- **Feature Toggle**: Tools > Hierarchy > Enable Material Preview (New)
- **Locate**: Left-click on material color block
- **Preview**: Hold `Ctrl` + hover over material color block
- **Rename**: Double-click material color block
- **Performance Optimization**:
  - Smart caching system
  - Memory usage optimization
  - Dynamic performance monitoring

### 6. Performance Settings (New)
- **Path**: Tools > Hierarchy > Material Preview Settings
- **Configurable Items**:
  - Cache size
  - Performance warning thresholds
  - Memory usage limits
  - Auto cleanup intervals

## Performance Optimization
- Object pooling to reduce GC
- Smart caching mechanism
- Automatic unused resource cleanup
- Performance monitoring and warning system
- Cache is automatically cleared when:
  - Scenes are loaded/unloaded
  - Entering/Exiting Play Mode
  - Cleanup threshold is reached
- All features can be toggled individually

## Technical Support
- Email: [455471553@qq.com]
- Documentation: [https://jxaytl13.github.io]
- Issue Tracking: [issues-url]

## Copyright
© 2024 [T·L]. All Rights Reserved.

## Changelog


### v1.0.3
- Material preview window style adjustment
- Performance optimization

### v1.0.1
- Added material preview master switch
- Optimized folder color storage mechanism using GUID to maintain colors after renaming
- Improved performance monitoring and caching system
- Integrated color picker functionality
- Added detailed performance settings options
- Optimized memory usage and GC

### v1.0.0
- Initial release
- Added component icon feature with caching system
- Implemented all basic features
- Supports Unity 2020.3.0f1 and above