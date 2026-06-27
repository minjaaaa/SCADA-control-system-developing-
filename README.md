# SCADA Control System

A **SCADA (Supervisory Control and Data Acquisition)** application developed in **C#** using **WPF** for the user interface and **Entity Framework** for database management. The system enables real-time monitoring and control of industrial processes through configurable I/O tags, alarms, historical data logging, and reporting.

---

## 🚀 Features

### 🔄 Real-Time Monitoring
- Multithreaded architecture for independent scanning and reading of I/O tags from a PLC simulator.
- Continuous acquisition and processing of analog and digital signals.

### 🚨 Alarm Management
- High and Low limit alarm detection.
- Real-time alarm visualization.
- Alarm acknowledgment (ACK) support.

### 📊 Historical Data & Reports
- Storage of analog input (AI) history.
- Advanced filtering and search capabilities.
- Export filtered results to **.txt** reports.

### ⚙️ Configuration Management
- Export complete system configuration to **JSON**.
- Import configuration for quick migration and backup.

### 📝 Trace & Logging System
- Configurable application logging.
- Category-based logging using bitmask configuration.
- Event tracking through `system.log`.

---

## 🏗️ System Architecture

The solution consists of three main components:

### **DataConcentrator**
Class library containing:
- Tag management
- Alarm processing
- Multithreading logic
- PLC communication

### **ScadaGUI**
WPF desktop application responsible for:
- User interface
- Configuration
- Visualization
- Alarm monitoring
- Historical data review

### **Database**
SQL LocalDB database managed with Entity Framework for storing:
- System configuration
- Alarm history
- Tag values
- Historical readings

---

## 🛠️ Technologies

- **Language:** C#
- **Framework:** .NET, WPF
- **ORM:** Entity Framework
- **Database:** SQL LocalDB
- **Serialization:** JSON (Newtonsoft.Json)

### Design Patterns
- Singleton (PLC Simulator)
- Observer (Alarm Events)

---

## 📂 Project Structure

```text
ScadaSolution
│
├── DataConcentrator/      # Business logic
├── ScadaGUI/              # WPF application
├── Database/              # Entity Framework & LocalDB
└── ScadaSolution.sln
```

---

## ▶️ Getting Started

### Prerequisites

- Visual Studio 2022 (or newer)
- .NET Framework / .NET version required by the project
- SQL LocalDB
- NuGet Package Manager

### Installation

1. Clone the repository

```bash
git clone https://github.com/yourusername/yourrepository.git
```

2. Open

```text
ScadaSolution.sln
```

3. Restore NuGet packages

Required packages include:

- Entity Framework
- Newtonsoft.Json

4. Set **ScadaGUI** as the Startup Project.

5. Build and run the application.

---

## 📋 Logging Categories

The application stores logs inside:

```text
system.log
```

Logging categories are configured using bitmask values.

| Bit | Category |
|-----|----------|
| 0 | Application Startup |
| 1 | Alarm Activation / Acknowledgment |
| 2 | Tag Management |
| 3 | Configuration Import / Export |
| 4 | Errors & Exceptions |

---

## 📈 Main Functionalities

- Real-time PLC communication
- Analog and Digital I/O tags
- Configurable scan intervals
- Alarm management
- Historical data storage
- Search and filtering
- TXT report generation
- JSON configuration import/export
- Configurable logging system

---

## 🎓 Academic Project

This project was developed as part of the **Software Design in Control Systems** course.

---

## 📄 License

This project is intended for educational purposes.