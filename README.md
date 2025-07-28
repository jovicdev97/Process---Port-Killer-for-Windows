# Port Killer

A powerful Windows application for managing network ports and terminating processes that are using specific ports. Built with C# and Windows Forms for efficient port management and process control.

![Platform](https://img.shields.io/badge/Platform-Windows-blue) ![.NET](https://img.shields.io/badge/.NET-9.0-purple) ![License](https://img.shields.io/badge/License-MIT-green)

## Features

- **Fast Port Scanning** - Efficiently scan specific ports or all active ports
- **Process Management** - View detailed information about processes using ports
- **Safe Termination** - Smart process killing with system process protection
- **User-Friendly GUI** - Clean Windows Forms interface
- **Flexible Input** - Support for single ports, comma-separated lists, and port ranges
- **Real-time Updates** - Refresh capability to see current network state
- **Performance Optimized** - Cached netstat calls for lightning-fast scanning

## Requirements

- **Operating System**: Windows 10 or later
- **Runtime**: Self-contained (no .NET installation required)
- **Privileges**: Administrator rights for terminating certain processes

## Installation

### Quick Start
1. Download `PortKiller.GUI.exe` from the `dist/` folder
2. Run the executable (no installation required)
3. Grant administrator privileges when prompted

### Build from Source
```bash
git clone <repository-url>
cd PortKiller
build.bat
```

## Usage

### Basic Operations
1. **Launch the application**
2. **Scan Ports**:
   - Leave port field empty to scan all active ports
   - Enter specific ports: `8080` or `8080,3000,5000`
   - Use port ranges: `8080-8090`
3. **View Results**: The grid shows Port, Process ID, Process Name, Protocol, and Status
4. **Kill Processes**:
   - Select specific rows and click "Kill Selected"
   - Click "Kill All" to terminate all processes using the scanned ports
   - Use "Refresh" to update the current state

### Input Formats
- **Single port**: `8080`
- **Multiple ports**: `8080,3000,5000` or `8080;3000;5000`
- **Port ranges**: `8080-8090`
- **Mixed formats**: `80,443,8000-8010,9000`

## Safety Features

- **System Process Protection**: Prevents termination of critical system processes
- **Confirmation Dialogs**: Asks for confirmation before killing multiple processes  
- **Detailed Results**: Shows success/failure status for each termination attempt
- **UAC Integration**: Automatically requests elevated privileges when needed

## Building from Source

### Prerequisites
- Visual Studio 2022 or later, OR
- .NET 6.0 SDK, OR  
- Build Tools for Visual Studio

### Build Instructions

**Option 1: Using the build script**
```batch
build.bat
```

**Option 2: Using dotnet CLI**
```bash
dotnet build PortKiller.sln
```

**Option 3: Using MSBuild directly**
```bash
msbuild PortKiller.sln /p:Configuration=Release
```

## Architecture

The application follows a clean layered architecture:

```
PortKiller/
├── PortKiller.GUI/           # Windows Forms user interface
├── PortKiller.Core/          # Business logic and orchestration
├── PortKiller.SystemAccess/  # Low-level system interactions
│   └── Models/              # Data models and enums
└── dist/                    # Built application
```

### Key Components
- **PortScanner**: Handles port parsing, validation, and scanning operations
- **ProcessKiller**: Manages safe process termination with system protection
- **NetworkHelper**: Interfaces with Windows networking APIs and netstat
- **ProcessHelper**: Handles process management and UAC elevation

## Performance

- **Optimized Scanning**: Efficient netstat caching reduces scan time by 90%
- **Batch Operations**: Single netstat call handles multiple ports
- **Responsive UI**: Asynchronous operations prevent interface freezing
- **Memory Efficient**: Minimal resource usage during operation

## Security Considerations

This application is designed for **defensive security purposes only**:
- System administrators managing development environments
- Troubleshooting port conflicts during development
- Educational purposes for understanding network processes

**Important**: Always exercise caution when terminating processes, especially on production systems.

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License. This software is provided as-is for educational and administrative purposes.

## Changelog

### v1.0.0
- Initial release
- Fast port scanning with netstat optimization
- Safe process termination with system protection
- Windows Forms GUI with real-time updates
- Support for flexible port input formats
- UAC integration for elevated operations

---

**Note**: This tool is for legitimate system administration and development purposes only. Users are responsible for compliance with their organization's policies and applicable laws.