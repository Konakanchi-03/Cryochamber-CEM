using System;
using System.IO;
using System.Numerics;
using PicoGK;
using Leap71.ShapeKernel;
using CryoChamber.Configuration;
using CryoChamber.Physics;
using CryoChamber.Geometry;

namespace CryoChamber
{
    public class Program
    {
        public static ChamberParameters s_oParams = new ChamberParameters();
        public static bool s_bBatchMode = false;

        public static void Main(string[] args)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("  LEAP 71 Computational Engineering Model (CEM)");
            Console.WriteLine("  Regeneratively Cooled Cryogenic Rocket Thrust Chamber (LOX/LH2)");
            Console.WriteLine("=================================================================");

            // Check if batch mode requested (e.g. for CI or automated export)
            s_bBatchMode = args.Length > 0 && (args[0] == "--batch" || args[0] == "--export");

            string strExportsDir = Path.Combine(Directory.GetCurrentDirectory(), "exports");
            Directory.CreateDirectory(strExportsDir);
            string strLogFile = Path.Combine(strExportsDir, "picogk.log");

            // Print baseline engineering metrics
            Console.WriteLine($"\n[1] Operational Parameters:");
            Console.WriteLine($"    - Chamber Pressure (Pc)      : {s_oParams.m_fChamberPressureBar:F1} bar");
            Console.WriteLine($"    - Thrust                     : {s_oParams.m_fThrustNewton:F0} N");
            Console.WriteLine($"    - Throat Diameter (Dt)       : {s_oParams.m_fThroatRadiusMM * 2:F1} mm");
            Console.WriteLine($"    - Chamber Diameter (Dc)      : {s_oParams.m_fChamberRadiusMM * 2:F1} mm");
            Console.WriteLine($"    - Exit Diameter (De)         : {s_oParams.m_fExitRadiusMM * 2:F1} mm");
            Console.WriteLine($"    - Total Axial Length (L)     : {s_oParams.m_fTotalLengthMM:F1} mm");
            Console.WriteLine($"    - Conformal Channel Count    : {s_oParams.m_nChannelCount}");
            Console.WriteLine($"    - Hot-gas Wall Thickness    : {s_oParams.m_fHotWallThicknessMM:F2} mm");
            Console.WriteLine($"    - Throat Channel Height (ht) : {s_oParams.m_fChannelHeightThroatMM:F2} mm (High Aspect Ratio)");
            Console.WriteLine($"    - Skirt Channel Height (he)  : {s_oParams.m_fChannelHeightExitMM:F2} mm (Low Delta-P)");
            Console.WriteLine($"    - Interactive Voxel Grid     : {s_oParams.m_fVoxelSizeMM:F2} mm resolution");
            Console.WriteLine($"    - Mode                       : {(s_bBatchMode ? "Batch Export (Auto-exit)" : "Interactive 3D Viewer")}\n");

            try
            {
                // Launches PicoGK voxel engine and 3D viewer
                Library.Go(
                    s_oParams.m_fVoxelSizeMM,
                    ExecuteChamberCEM,
                    strLogFile,
                    bEndAppWithTask: s_bBatchMode,
                    strWindowTitle: "LEAP 71 CEM - Cryogenic Regenerative Rocket Thrust Chamber"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[ERROR] Execution failed: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        public static void ExecuteChamberCEM()
        {
            Console.WriteLine("[2] Starting Computational Geometry Synthesis in PicoGK...");

            ThrustChamberModel oModel = new ThrustChamberModel(s_oParams);

            // 1. Build complete monolithic thrust chamber and fluid domain
            (Voxels voxChamber, Voxels voxFluid) = oModel.voxBuildCompleteChamber();

            // 2. Build 90-degree angular cutaway to reveal internal conformal channels
            Console.WriteLine("--> Generating 90-deg Cutaway for internal channel inspection...");
            Voxels voxCutaway = oModel.voxCreateCutaway(voxChamber);

            // 3. Render in PicoGK 3D viewer
            Console.WriteLine("[3] Rendering to PicoGK 3D Viewer...");
            
            // Metallic titanium cutaway body
            Sh.PreviewVoxels(
                voxCutaway,
                Cp.clrRock,
                fTransparency: 0.0f,
                fMetallic: 0.85f,
                fRoughness: 0.35f
            );

            // Cryogenic fluid volume inside the cut section (glowing crystal blue)
            Voxels voxFluidCut = oModel.voxCreateCutaway(voxFluid);
            Sh.PreviewVoxels(
                voxFluidCut,
                Cp.clrCrystal,
                fTransparency: 0.2f,
                fMetallic: 0.1f,
                fRoughness: 0.1f
            );

            // 4. Export mesh deliverables for 3D printing (Additive Manufacturing) and CFD
            Console.WriteLine("[4] Exporting Production Deliverables (STL & OpenVDB)...");
            string strExportsDir = Path.Combine(Directory.GetCurrentDirectory(), "exports");
            Directory.CreateDirectory(strExportsDir);

            string strCutawayStl = Path.Combine(strExportsDir, "CryoChamber_Cutaway.stl");
            string strFluidStl   = Path.Combine(strExportsDir, "CryoChamber_FluidDomain_CFD.stl");
            string strFullVdb    = Path.Combine(strExportsDir, "CryoChamber_Monolith.vdb");

            Mesh mshCutaway = new Mesh(voxCutaway);
            mshCutaway.SaveToStlFile(strCutawayStl);
            Console.WriteLine($"    -> Saved Cutaway Mesh (STL)   : {strCutawayStl}");

            Mesh mshFluid = new Mesh(voxFluid);
            mshFluid.SaveToStlFile(strFluidStl);
            Console.WriteLine($"    -> Saved CFD Fluid Mesh (STL) : {strFluidStl}");

            voxChamber.SaveToVdbFile(strFullVdb);
            Console.WriteLine($"    -> Saved OpenVDB Sparse Grid  : {strFullVdb}");

            Console.WriteLine("\n[SUCCESS] Model successfully synthesized and rendered!");
            if (!s_bBatchMode)
            {
                Console.WriteLine("You can interact with the 3D model in the viewer window:");
                Console.WriteLine("  - Left Mouse Button  : Rotate / Orbit");
                Console.WriteLine("  - Right Mouse Button : Pan");
                Console.WriteLine("  - Scroll Wheel       : Zoom");
            }
        }
    }
}
