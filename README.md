# CryoChamber-CEM 🚀
### Autonomous Computational Engineering Model for a 3D-Printed Cryogenic Rocket Thrust Chamber

[![PicoGK](https://img.shields.io/badge/Geometry%20Kernel-PicoGK%202.3-blue.svg)](https://picogk.org)
[![ShapeKernel](https://img.shields.io/badge/Framework-LEAP71__ShapeKernel-green.svg)](https://github.com/leap71/LEAP71_ShapeKernel)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-yellow.svg)](LICENSE)

An open-source **Computational Engineering Model (CEM)** that algorithmically designs a flight-weight, regeneratively cooled liquid rocket thrust chamber ($LOX / LH_2$) from first-principles aerothermodynamics and Additive Manufacturing (DfAM) rules.

Built using **[PicoGK](https://picogk.org)** (open-source voxel geometry kernel by **[LEAP 71](https://leap71.com)**) and the **[LEAP 71 ShapeKernel](https://github.com/leap71/LEAP71_ShapeKernel)**.

---

## 💡 The Philosophy: Code Instead of CAD

Traditional Computer-Aided Design (CAD) relies on manual surface modeling (B-Rep / NURBS). When engineering a rocket chamber with **64 conformal, variable-aspect-ratio cooling channels, bifurcations, and toroidal distribution manifolds**, traditional CAD tools frequently freeze, crash, or fail on Boolean operations.

**CryoChamber-CEM** replaces manual drawing with an executable algorithmic model:
* **Physics-Driven:** Geometry is computed directly from combustion pressure, gas-side heat flux, and coolant mass flow rates.
* **Voxel & Signed Distance Fields (OpenVDB):** Boolean subtractions (carving 64 intricate 3D channels out of a monolithic metal alloy shell) are mathematically infallible and compute in **under 30 seconds on a standard laptop**.
* **Dual Deliverables:** Automatically synthesizes both the **metal 3D-printable solid part** and the **CFD fluid volume mesh** in a single run.

---

## 🔬 Aerothermal & Fluid Dynamic Foundations

### 1. Gas-Side Heat Flux Distribution ($q''(z)$)
The model implements a continuous aerothermal estimation based on the **Bartz Equation**:
$$q''(z) = h_g(z) \cdot \left( T_{aw}(z) - T_{wg}(z) \right)$$
At a nominal chamber pressure of $P_c = 35\,\text{bar}$, the peak heat flux at the throat reaches $\sim 28\,\text{MW/m}^2$, necessitating extreme cooling effectiveness.

### 2. Variable-Aspect-Ratio Conformal Channels
To balance heat dissipation with coolant pressure loss ($\Delta P$):
* **Nozzle Throat (Peak Heat Flux):** Channels morph into a **high-aspect-ratio** profile ($h \approx 3.8\,\text{mm}$, narrow width) to maximize coolant flow velocity and wetted perimeter, substantially boosting the convective heat transfer coefficient:
  $$h_c \propto \frac{\dot{m}^{0.8}}{D_h^{0.2} \cdot A_c^{0.8}}$$
* **Nozzle Skirt (Low Heat Flux):** Channels expand into shallow profiles ($h \approx 1.4\,\text{mm}$) to curb friction and minimize parasitic pressure loss.

### 3. Integrated Manifold Dynamics
* **Inlet Torus:** Positioned at the nozzle skirt exit, evenly feeding cryogenic Liquid Hydrogen ($LH_2$) into the base of all 64 channels in a counter-flow configuration.
* **Outlet Torus:** Encircles the injector dome interface, collecting heated, expanding hydrogen gas to route into the injector face.

---

## 🏭 Design for Additive Manufacturing (DfAM)

Designed specifically for **Laser Powder Bed Fusion (LPBF)** in high-conductivity copper alloys (e.g., GRCop-42 / GRCop-84) or nickel superalloys (Inconel 718):
* **Minimum Printable Rib Thickness:** Rib walls are dynamically clamped ($t_{\text{rib}} \ge 0.8\,\text{mm}$) to ensure structural integrity against chamber burst pressure.
* **Conformal Conduction Wall:** Continuous $1.2\,\text{mm}$ hot-gas liner wall thickness maintained through all curves and transitions.
* **Powder Removal Ports:** Continuous fluid channels designed for trapped powder evacuation post-print.

---

## 📐 Chamber Architecture & Specifications

| Parameter | Value | Description |
| :--- | :--- | :--- |
| **Propellants** | $LOX / LH_2$ | Liquid Oxygen / Liquid Hydrogen |
| **Nominal Thrust** | $7,500\,\text{N}$ | Upper-stage / lander scale |
| **Chamber Pressure ($P_c$)** | $35.0\,\text{bar}$ | |
| **Throat Diameter ($D_t$)** | $36.0\,\text{mm}$ | Sonic choking constriction |
| **Chamber Diameter ($D_c$)** | $72.0\,\text{mm}$ | Contraction ratio $\epsilon_c = 4.0$ |
| **Exit Diameter ($D_e$)** | $108.0\,\text{mm}$ | Expansion ratio $\epsilon_e = 9.0$ |
| **Total Length** | $180.0\,\text{mm}$ | Injector to skirt exit |
| **Cooling Channels** | $64$ | Conformal variable-aspect-ratio |
| **Hot Wall Thickness** | $1.20\,\text{mm}$ | Conduction liner |
| **Voxel Resolution** | $0.50\,\text{mm}$ | Real-time interactive preview |

---

## 📂 Repository Structure

Following LEAP 71 architectural standards and Hungarian notation (`f`, `d`, `i`, `vec`, `vox`, `msh`):

```text
CryoChamber/
├── CryoChamber.sln                 <-- Visual Studio / .NET Solution
├── README.md                       <-- Engineering documentation & showcase
├── libs/
│   └── LEAP71_ShapeKernel/         <-- Git submodule: LEAP 71 ShapeKernel library
└── src/
    └── CryoChamber/
        ├── Configuration/
        │   └── ChamberParameters.cs    // Parameter definitions (thermal, geometric, DfAM)
        ├── Physics/
        │   └── ThermalHydraulics.cs    // Bartz heat flux & variable aspect-ratio math
        ├── Geometry/
        │   ├── NozzleContour.cs        // Rao 80% parabolic bell & contraction profile
        │   ├── CoolingChannelGenerator.cs // Procedural conformal 64-channel array
        │   ├── ManifoldBuilder.cs      // Inlet & outlet toroidal distribution rings
        │   └── ThrustChamberModel.cs   // Master assembly, voxel carver, and cutaways
        ├── Program.cs                  // Application entry point (GLFW 3D Viewer & Exporter)
        └── CryoChamber.csproj          // C# .NET 9 project referencing PicoGK
```

---

## 🚀 Quick Start & How to Run

### Prerequisites
* [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
* Windows (64-bit) or macOS (Apple Silicon)
* GPU supporting OpenGL 4.1+

### 1. Clone the Repository
```bash
git clone --recurse-submodules https://github.com/YOUR_USERNAME/CryoChamber-CEM.git
cd CryoChamber-CEM
```

### 2. Launch Interactive 3D Viewer
Run the computational engine and launch the real-time GLFW visualizer:
```bash
dotnet run --project src/CryoChamber
```
* **Left Click + Drag:** Orbit / Rotate the 3D model
* **Right Click + Drag:** Pan camera
* **Mouse Wheel:** Zoom

### 3. Headless / Batch Production Export
To generate the manufacturing meshes without keeping the viewer open:
```bash
dotnet run --project src/CryoChamber -- --batch
```

This writes the following production deliverables into `src/CryoChamber/exports/`:
* `CryoChamber_Cutaway.stl` — High-resolution 90° cutaway mesh showcasing internal channels.
* `CryoChamber_FluidDomain_CFD.stl` — Complete internal flow volume for ANSYS Fluent / OpenFOAM mesh generation.
* `CryoChamber_Monolith.vdb` — Native OpenVDB sparse volumetric distance field.

---

## ⚖️ License & Acknowledgments

This project is licensed under the [Apache 2.0 License](LICENSE).

Special thanks to **Lin Kayser** and **Josefine Lissner** at **[LEAP 71](https://leap71.com)** for pioneering the field of Computational Engineering and providing the open-source **[PicoGK](https://picogk.org)** geometry kernel.
