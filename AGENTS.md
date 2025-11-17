# SchneidMaschine - Industrial Cutting Machine Control System

## Project Overview

- **Purpose**: Industrial control application for automated cutting machine processing material strips for envelope/packaging production
- **Language**: C# with WPF (Windows Presentation Foundation)
- **Framework**: .NET Framework 4.7.2
- **Hardware Integration**: Dual ESP32/Arduino controllers via USB serial communication
- **Database**: SQL Server LocalDB for production statistics and configuration
- **Version**: v1.0.0 (Copyright 2020)

## Key Architecture Components

### Technology Stack
- WPF frontend with German language interface for industrial environment
- Multi-threaded serial communication for real-time hardware control
- SQL Server LocalDB for persistent data storage
- Arduino/ESP32 firmware for stepper motor and cutting operations
- Custom communication protocol between C# application and microcontrollers

### Hardware Components
- **Schneidmaschine**: Main cutting unit with stepper motor control
- **Rollenzentrierung**: Roll centering and material feed system
- **Dual ESP32 Controllers**: Separate USB serial connections for each subsystem

## Project Structure

### Core Application Files
- `MainWindow.xaml/.cs` - Primary UI and USB communication logic
- `App.xaml/.cs` - Application entry point and configuration
- `App.config` - Database connection strings and .NET configuration
- `SchneidMaschine.csproj` - Project dependencies and build configuration

### Business Logic Modules
- `model/DataModel.cs` - Central data management and ESP32 communication coordinator
- `model/CommandLine.cs` - Command protocol implementation for hardware communication
- `model/Statistik.cs` - Production statistics tracking and calculations

### User Interface Pages
- `pages/Home.xaml/.cs` - Main dashboard with production statistics
- `pages/Auto.xaml/.cs` - Automated cutting operations
- `pages/HalbAuto.xaml/.cs` - Semi-automatic mode operations
- `pages/EinzelSchritt.xaml/.cs` - Manual single-step operations
- `pages/SchnittModus.xaml/.cs` - Cutting mode selection interface
- `pages/Streifen40.xaml/.cs` - 40mm strip cutting configuration
- `pages/Streifen70.xaml/.cs` - 70mm strip cutting configuration
- `pages/Wartung.xaml/.cs` - Maintenance and calibration interface

### Threading and Communication
- `threads/Thread_Con_Schneidmaschine.cs` - Cutting machine communication thread
- `threads/Thread_Con_Rollenzentrierung.cs` - Roll centering communication thread
- `threads/Thread_Port_Scanner.cs` - USB port detection and management

### Database Layer
- `db/DBHandler.cs` - SQL Server LocalDB operations and data persistence
- `db/Database.mdf` - LocalDB database file with production statistics

### Arduino/ESP32 Integration
- `IoT/sketche/SchneidMaschine/` - Cutting machine ESP32 firmware
- `IoT/sketche/Rollenzentrierung/` - Roll centering ESP32 firmware
- `IoT/Beispiele/` - Reference implementations and test sketches
- `IoT/fritzing/` - Circuit diagrams and hardware schematics

## Development Guidelines

### Serial Communication Protocol
- **App to Arduino**: Start char `%`, End char `#`
- **Arduino to App**: Start char `~`, End char `@`
- **Command Format**: `COMMAND_parameter_DIRECTION`
- **Error Handling**: Always wrap serial operations in try-catch blocks
- **Connection Management**: Use separate StringBuilder instances for each device

### Thread Management Best Practices
- **Critical Rule**: Set thread references to `null` after `Thread.Abort()`
- Threads cannot be restarted once aborted - create new instances for reconnection
- Use `Dispatcher.Invoke()` for UI updates from background threads
- Implement proper cleanup in connection disconnect scenarios

```csharp
// Correct thread cleanup pattern
if (threadCheckConnection_Schneidmaschine != null && threadCheckConnection_Schneidmaschine.IsAlive)
{
    threadCheckConnection_Schneidmaschine.Abort();
    threadCheckConnection_Schneidmaschine = null; // Essential for reconnection
}
```

### TextBox Output Separation
- **Critical**: Use separate StringBuilder instances for each ESP32 device
- `sbRollenzentrierung` for roll centering output
- `sbSchneidmaschine` for cutting machine output
- Implement line limiting (100 lines max) to prevent memory issues

### Data Models and Enums

#### Strip Types (STREIFEN enum)
- `C4_40_Schachtel_KURZ = 320` - C4 40mm short box strips
- `C4_40_Schachtel_LANG = 700` - C4 40mm long box strips
- `C4_70_Deckel = 650` - C4 70mm lid strips
- `C5_40_Deckel = 400` - C5 40mm lid strips

#### Cutting Machine Commands (COMMAND_Schneidmaschine)
- `stepperStart/stepperStop` - Motor control operations
- `schneidenStart/schneidenBeendet` - Cutting cycle control
- `handradOn/handradOff` - Manual control mode toggle
- `allesStop/allesGestoppt` - Emergency stop functionality
- `resetIstWert` - Position value reset

#### Motor Configuration
- **Step-to-Millimeter Ratio**: 12.7 steps = 1mm
- **Direction Control**: `forward/backward` enum values
- **Position Tracking**: Convert between steps and millimeters using `mmToSteps()` and `stepsToMM()`

### Database Operations

#### Statistics Categories
- **Heute (Today)**: Daily production counters, reset automatically at midnight
- **Rolle (Roll)**: Per-roll material consumption and strip counts
- **Langzeit (Long-term)**: Historical production data accumulation

#### Key Database Operations
- Use parameterized queries to prevent SQL injection
- Implement automatic daily reset functionality
- Track strip counts per box type for inventory management
- Monitor roll length consumption for material planning

### Error Handling Patterns

#### Serial Port Exceptions
- Display user-friendly German error messages
- Implement graceful degradation when hardware disconnects
- Provide clear connection status indicators (Green=Connected, Red=Disconnected)

#### Threading Safety
- Always use try-catch around thread operations
- Handle ThreadAbortException gracefully during shutdown
- Implement timeout mechanisms for hardware communication

### UI/UX Conventions

#### German Industrial Interface
- Use consistent German terminology throughout interface
- Implement clear visual status indicators for machine state
- Provide immediate feedback for user actions and system state changes

#### Production Workflow
- Guide users through cutting modes with clear navigation
- Display real-time production statistics and material consumption
- Implement emergency stop functionality accessible from all modes

## Testing and Debugging

### Hardware Communication Testing
- Test each ESP32 connection independently before dual operation
- Verify command protocol compatibility with Arduino firmware
- Monitor TextBox outputs to ensure proper message routing to correct displays

### Connection Reliability Testing
- Test disconnect/reconnect scenarios extensively
- Verify thread cleanup prevents "thread already running" errors
- Ensure proper device identification and automatic reconnection

### Production Data Validation
- Validate numeric inputs for strip lengths and cutting quantities
- Test edge cases for production counters and statistics calculations
- Verify database operations handle concurrent access properly

## Deployment Configuration

### Database Setup Requirements
- Configure SQL Server LocalDB with proper file permissions
- Verify connection string in App.config matches deployment environment
- Ensure database auto-creation and daily reset functionality works

### Hardware Requirements
- Two dedicated USB ports for ESP32 controllers
- Verify ESP32 firmware compatibility and upload latest versions
- Calibrate machine settings for accurate step-to-millimeter conversion

### Security Considerations
- Configure USB device access permissions for industrial user accounts
- Secure database files with appropriate file system permissions
- Implement backup procedures for production statistics data

## Maintenance Guidelines

### Regular Maintenance Tasks
- Monitor database file size growth and implement archiving strategy
- Update production statistics displays and verify calculation accuracy
- Check ESP32 firmware versions and update when necessary
- Clean up TextBox output logs to prevent memory accumulation

### Performance Optimization
- Limit TextBox line counts to maintain responsive UI
- Optimize database queries for statistics dashboard updates
- Monitor thread resource usage during extended production runs
- Implement log rotation for long-running operations

### Troubleshooting Common Issues
- **Thread Restart Errors**: Ensure proper thread cleanup with null assignment
- **Mixed TextBox Output**: Verify separate StringBuilder usage for each device
- **Connection Failures**: Check USB cable integrity and driver installation
- **Database Errors**: Verify LocalDB service status and file permissions

---
*This documentation reflects the SchneidMaschine industrial control system architecture. Update when making structural changes or adding new operational modes.*