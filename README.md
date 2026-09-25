# ULTRAKILL - Stamina Memory Trainer (Reverse Engineering)

An educational reverse engineering project demonstrating static binary inspection and dynamic memory pointer resolution on **ULTRAKILL** (Unity Engine), accompanied by an external C# CLI trainer to achieve infinite stamina.

---

## 📌 Project Overview

This project demonstrates the practical reverse engineering pipeline for Unity Mono games:
1. **Static Binary Decompilation:** Decompiling `Assembly-CSharp.dll` to inspect movement routines and identify internal field names and parameters.
2. **Dynamic Memory Inspection & Pointer Scanning:** Tracking floating-point values in RAM and constructing a resilient multi-level pointer chain.
3. **External Memory Trainer:** Building an external C# CLI tool using Windows Win32 APIs (`OpenProcess`, `ReadProcessMemory`, `WriteProcessMemory`) to dynamically freeze stamina values.

---

## 🛠️ Tools Used & Technical Rationale

| Tool | Purpose & Justification |
| :--- | :--- |
| **dnSpyEx** | Used for static decompilation. ULTRAKILL uses Unity's Mono framework, allowing `Assembly-CSharp.dll` to be reversed into readable C#. This revealed that stamina is stored as `boostCharge` with a default float value of `300f`. |
| **Cheat Engine 7.x** | Used for runtime dynamic memory analysis. It isolated the `boostCharge` address in memory and resolved a 7-level pointer chain back to `UnityPlayer.dll`. |
| **.NET C# (csc.exe)** | Chosen for building an external user-mode trainer via P/Invoke calls to Windows memory management APIs (`kernel32.dll`). |

---

## 📂 Repository Structure

```text
ULTRAKILL-Reverse-Engineering/
├── README.md
├── .gitignore
├── docs/
│   ├── methodology.md
│   ├── dnspy-boostcharge.png
│   └── stamina-pointer-chain.png
├── cheat-engine/
│   ├── stamina.PTR
│   └── stamina_rescan.PTR
└── src/
    └── UltrakillTrainer/
        ├── Program.cs
        ├── GameObjects.cs
        ├── Memory.cs
        ├── build.bat
        └── UltrakillTrainer.csproj